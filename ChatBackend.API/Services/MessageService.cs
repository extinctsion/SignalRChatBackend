using Microsoft.EntityFrameworkCore;
using ChatBackend.Data;
using ChatBackend.Models.Entities;
using ChatBackend.Models.DTOs.Message;
using ChatBackend.Models.Enums;

namespace ChatBackend.Services
{
    public class MessageService : IMessageService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<MessageService> _logger;
        
        public MessageService(ChatDbContext context, ILogger<MessageService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<MessageDto> CreateMessageAsync(Guid senderId, CreateMessageDto messageDto)
        {
            var message = new Message
            {
                ConversationId = messageDto.ConversationId,
                SenderId = senderId,
                Type = messageDto.Type,
                Content = messageDto.Content,
                ReplyToMessageId = messageDto.ReplyToMessageId
            };
            
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            
            // Create message status for all conversation members
            var conversationMembers = await _context.UserConversations
                .Where(uc => uc.ConversationId == messageDto.ConversationId && uc.IsActive)
                .Select(uc => uc.UserId)
                .ToListAsync();
            
            var messageStatuses = conversationMembers
                .Where(userId => userId != senderId) // Don't create status for sender
                .Select(userId => new MessageStatus
                {
                    MessageId = message.Id,
                    UserId = userId,
                    Status = DeliveryStatus.Sent
                }).ToList();
            
            if (messageStatuses.Any())
            {
                _context.MessageStatuses.AddRange(messageStatuses);
                await _context.SaveChangesAsync();
            }
            
            // Load complete message for return
            var completeMessage = await GetMessageByIdAsync(message.Id);
            
            _logger.LogInformation("Message created with ID: {MessageId}", message.Id);
            
            return completeMessage!;
        }
        
        public async Task<List<MessageDto>> GetMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                .ThenInclude(rm => rm!.Sender)
                .Include(m => m.MessageStatuses)
                .ThenInclude(ms => ms.User)
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return messages.Select(MapToDto).ToList();
        }
        
        public async Task<MessageDto?> GetMessageByIdAsync(Guid messageId)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                .ThenInclude(rm => rm!.Sender)
                .Include(m => m.MessageStatuses)
                .ThenInclude(ms => ms.User)
                .FirstOrDefaultAsync(m => m.Id == messageId);
                
            return message != null ? MapToDto(message) : null;
        }
        
        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null || message.SenderId != userId)
                return false;
                
            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
            message.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Message deleted: {MessageId} by user: {UserId}", messageId, userId);
            return true;
        }
        
        public async Task UpdateMessageStatusAsync(Guid messageId, Guid userId, DeliveryStatus status)
        {
            var messageStatus = await _context.MessageStatuses
                .FirstOrDefaultAsync(ms => ms.MessageId == messageId && ms.UserId == userId);
                
            if (messageStatus != null)
            {
                messageStatus.Status = status;
                messageStatus.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        
        private static MessageDto MapToDto(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderUsername = message.Sender.Username,
                SenderAvatarUrl = message.Sender.AvatarUrl,
                Type = message.Type,
                Content = message.Content,
                FileUrl = message.FileUrl,
                FileName = message.FileName,
                ContentType = message.ContentType,
                FileSize = message.FileSize,
                ReplyToMessageId = message.ReplyToMessageId,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.UpdatedAt,
                IsDeleted = message.IsDeleted,
                MessageStatuses = message.MessageStatuses.Select(ms => new MessageStatusDto
                {
                    UserId = ms.UserId,
                    Username = ms.User.Username,
                    Status = ms.Status,
                    UpdatedAt = ms.UpdatedAt
                }).ToList(),
                ReplyToMessage = message.ReplyToMessage != null ? new MessageDto
                {
                    Id = message.ReplyToMessage.Id,
                    SenderId = message.ReplyToMessage.SenderId,
                    SenderUsername = message.ReplyToMessage.Sender.Username,
                    Type = message.ReplyToMessage.Type,
                    Content = message.ReplyToMessage.Content,
                    CreatedAt = message.ReplyToMessage.CreatedAt
                } : null
            };
        }
    }
}
