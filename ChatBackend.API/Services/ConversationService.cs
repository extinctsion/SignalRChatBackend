using Microsoft.EntityFrameworkCore;
using ChatBackend.Data;
using ChatBackend.Models.Enums;
using ChatBackend.Models.Entities;
using ChatBackend.Models.DTOs.Conversation;
using ChatBackend.Models.DTOs.Message;

namespace ChatBackend.Services
{
    public class ConversationService : IConversationService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<ConversationService> _logger;
        
        public ConversationService(ChatDbContext context, ILogger<ConversationService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<ConversationDto> CreateConversationAsync(Guid createdByUserId, CreateConversationDto conversationDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var conversation = new Conversation
                {
                    Name = conversationDto.Name,
                    Type = conversationDto.Type,
                    Description = conversationDto.Description,
                    CreatedByUserId = createdByUserId
                };
                
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
                
                // Add creator as owner
                var creatorMembership = new UserConversation
                {
                    UserId = createdByUserId,
                    ConversationId = conversation.Id,
                    Role = ConversationRole.Owner,
                    IsActive = true
                };
                
                _context.UserConversations.Add(creatorMembership);
                
                // Add other members
                var memberMemberships = conversationDto.MemberIds
                    .Where(id => id != createdByUserId)
                    .Select(userId => new UserConversation
                    {
                        UserId = userId,
                        ConversationId = conversation.Id,
                        Role = ConversationRole.Member,
                        IsActive = true
                    }).ToList();
                
                if (memberMemberships.Any())
                {
                    _context.UserConversations.AddRange(memberMemberships);
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Conversation created with ID: {ConversationId}", conversation.Id);
                
                return (await GetConversationByIdAsync(conversation.Id, createdByUserId))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<List<ConversationDto>> GetUserConversationsAsync(Guid userId)
        {
            var conversations = await _context.UserConversations
                .Include(uc => uc.Conversation)
                .ThenInclude(c => c.CreatedByUser)
                .Include(uc => uc.Conversation)
                .ThenInclude(c => c.UserConversations)
                .ThenInclude(uc => uc.User)
                .Where(uc => uc.UserId == userId && uc.IsActive && !uc.Conversation.IsDeleted)
                .Select(uc => uc.Conversation)
                .ToListAsync();
                
            var conversationDtos = new List<ConversationDto>();
            
            foreach (var conversation in conversations)
            {
                var dto = await MapToDto(conversation, userId);
                conversationDtos.Add(dto);
            }
            
            return conversationDtos.OrderByDescending(c => c.UpdatedAt).ToList();
        }
        
        public async Task<ConversationDto?> GetConversationByIdAsync(Guid conversationId, Guid userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.CreatedByUser)
                .Include(c => c.UserConversations)
                .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted);
                
            if (conversation == null)
                return null;
                
            // Check if user is member
            var isMember = conversation.UserConversations.Any(uc => uc.UserId == userId && uc.IsActive);
            if (!isMember)
                return null;
                
            return await MapToDto(conversation, userId);
        }
        
        public async Task<bool> AddUserToConversationAsync(Guid conversationId, Guid userId, ConversationRole role = ConversationRole.Member)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null || conversation.IsDeleted)
                return false;
                
            var existingMembership = await _context.UserConversations
                .FirstOrDefaultAsync(uc => uc.ConversationId == conversationId && uc.UserId == userId);
                
            if (existingMembership != null)
            {
                if (!existingMembership.IsActive)
                {
                    existingMembership.IsActive = true;
                    existingMembership.JoinedAt = DateTime.UtcNow;
                    existingMembership.LeftAt = null;
                    existingMembership.Role = role;
                }
                else
                {
                    return false; // Already a member
                }
            }
            else
            {
                var membership = new UserConversation
                {
                    UserId = userId,
                    ConversationId = conversationId,
                    Role = role,
                    IsActive = true
                };
                
                _context.UserConversations.Add(membership);
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> RemoveUserFromConversationAsync(Guid conversationId, Guid userId)
        {
            var membership = await _context.UserConversations
                .FirstOrDefaultAsync(uc => uc.ConversationId == conversationId && uc.UserId == userId && uc.IsActive);
                
            if (membership == null)
                return false;
                
            membership.IsActive = false;
            membership.LeftAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> UpdateUserRoleAsync(Guid conversationId, Guid userId, ConversationRole role)
        {
            var membership = await _context.UserConversations
                .FirstOrDefaultAsync(uc => uc.ConversationId == conversationId && uc.UserId == userId && uc.IsActive);
                
            if (membership == null)
                return false;
                
            membership.Role = role;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.UserConversations)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
                
            if (conversation == null)
                return false;
                
            // Check if user is owner
            var userMembership = conversation.UserConversations
                .FirstOrDefault(uc => uc.UserId == userId && uc.IsActive);
                
            if (userMembership == null || userMembership.Role != ConversationRole.Owner)
                return false;
                
            conversation.IsDeleted = true;
            conversation.DeletedAt = DateTime.UtcNow;
            conversation.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Conversation deleted: {ConversationId} by user: {UserId}", conversationId, userId);
            return true;
        }
        
        private async Task<ConversationDto> MapToDto(Conversation conversation, Guid currentUserId)
        {
            // Get last message
            var lastMessage = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversation.Id && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();
                
            // Get unread count for current user
            var unreadCount = await _context.Messages
                .Where(m => m.ConversationId == conversation.Id && 
                           m.SenderId != currentUserId && 
                           !m.IsDeleted)
                .CountAsync(m => !m.MessageStatuses.Any(ms => ms.UserId == currentUserId && 
                                                             (ms.Status == DeliveryStatus.Read)));
            
            return new ConversationDto
            {
                Id = conversation.Id,
                Name = conversation.Name,
                Type = conversation.Type,
                Description = conversation.Description,
                AvatarUrl = conversation.AvatarUrl,
                CreatedByUserId = conversation.CreatedByUserId,
                CreatedByUsername = conversation.CreatedByUser.Username,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                Members = conversation.UserConversations
                    .Where(uc => uc.IsActive)
                    .Select(uc => new UserConversationDto
                    {
                        UserId = uc.UserId,
                        Username = uc.User.Username,
                        Email = uc.User.Email,
                        AvatarUrl = uc.User.AvatarUrl,
                        Status = uc.User.Status,
                        Role = uc.Role,
                        JoinedAt = uc.JoinedAt,
                        LastReadAt = uc.LastReadAt,
                        IsActive = uc.IsActive
                    }).ToList(),
                LastMessage = lastMessage != null ? new MessageDto
                {
                    Id = lastMessage.Id,
                    ConversationId = lastMessage.ConversationId,
                    SenderId = lastMessage.SenderId,
                    SenderUsername = lastMessage.Sender.Username,
                    Type = lastMessage.Type,
                    Content = lastMessage.Content,
                    CreatedAt = lastMessage.CreatedAt
                } : null,
                UnreadCount = unreadCount
            };
        }
    }
}
