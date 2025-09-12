using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ChatBackend.Data;
using ChatBackend.Models;
using ChatBackend.DTOs;
using ChatBackend.Services;
using System.Security.Claims;

namespace ChatBackend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _context;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatHub> _logger;
        
        public ChatHub(
            ChatDbContext context, 
            IUserService userService,
            IMessageService messageService,
            ILogger<ChatHub> logger)
        {
            _context = context;
            _userService = userService;
            _messageService = messageService;
            _logger = logger;
        }
        
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "Authentication failed");
                Context.Abort();
                return;
            }
            
            _logger.LogInformation("User {UserId} connected to chat hub", userId);
            
            // Update user status to online
            await _userService.UpdateUserStatusAsync(userId.Value, UserStatus.Online);
            
            // Join user to their conversation groups
            var conversations = await _context.UserConversations
                .Where(uc => uc.UserId == userId && uc.IsActive)
                .Select(uc => uc.ConversationId.ToString())
                .ToListAsync();
                
            foreach (var conversationId in conversations)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
            }
            
            // Notify other users that this user is online
            await Clients.Others.SendAsync("UserStatusChanged", new
            {
                UserId = userId,
                Status = UserStatus.Online,
                LastSeen = DateTime.UtcNow
            });
            
            await base.OnConnectedAsync();
        }
        
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                _logger.LogInformation("User {UserId} disconnected from chat hub", userId);
                
                // Update user status to offline
                await _userService.UpdateUserStatusAsync(userId.Value, UserStatus.Offline);
                
                // Notify other users that this user is offline
                await Clients.Others.SendAsync("UserStatusChanged", new
                {
                    UserId = userId,
                    Status = UserStatus.Offline,
                    LastSeen = DateTime.UtcNow
                });
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        
        public async Task SendMessage(CreateMessageDto messageDto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "Authentication failed");
                return;
            }
            
            try
            {
                // Validate user is member of the conversation
                var isMember = await _context.UserConversations
                    .AnyAsync(uc => uc.UserId == userId && 
                                   uc.ConversationId == messageDto.ConversationId && 
                                   uc.IsActive);
                
                if (!isMember)
                {
                    await Clients.Caller.SendAsync("Error", "You are not a member of this conversation");
                    return;
                }
                
                var message = await _messageService.CreateMessageAsync(userId.Value, messageDto);
                
                // Send message to all members of the conversation
                await Clients.Group(messageDto.ConversationId.ToString())
                    .SendAsync("ReceiveMessage", message);
                
                _logger.LogInformation("Message {MessageId} sent by user {UserId} to conversation {ConversationId}", 
                    message.Id, userId, messageDto.ConversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from user {UserId}", userId);
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }
        
        public async Task JoinConversation(Guid conversationId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "Authentication failed");
                return;
            }
            
            // Validate user is member of the conversation
            var isMember = await _context.UserConversations
                .AnyAsync(uc => uc.UserId == userId && 
                               uc.ConversationId == conversationId && 
                               uc.IsActive);
            
            if (isMember)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
                _logger.LogInformation("User {UserId} joined conversation {ConversationId}", userId, conversationId);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "You are not a member of this conversation");
            }
        }
        
        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
            
            var userId = GetUserId();
            _logger.LogInformation("User {UserId} left conversation {ConversationId}", userId, conversationId);
        }
        
        public async Task StartTyping(Guid conversationId)
        {
            var userId = GetUserId();
            if (userId == null) return;
            
            await Clients.GroupExcept(conversationId.ToString(), Context.ConnectionId)
                .SendAsync("UserStartedTyping", new { UserId = userId, ConversationId = conversationId });
        }
        
        public async Task StopTyping(Guid conversationId)
        {
            var userId = GetUserId();
            if (userId == null) return;
            
            await Clients.GroupExcept(conversationId.ToString(), Context.ConnectionId)
                .SendAsync("UserStoppedTyping", new { UserId = userId, ConversationId = conversationId });
        }
        
        public async Task MarkMessageAsRead(Guid messageId)
        {
            var userId = GetUserId();
            if (userId == null) return;
            
            try
            {
                await _messageService.UpdateMessageStatusAsync(messageId, userId.Value, DeliveryStatus.Read);
                
                // Notify sender about read receipt
                var message = await _context.Messages
                    .Include(m => m.Sender)
                    .FirstOrDefaultAsync(m => m.Id == messageId);
                    
                if (message != null)
                {
                    await Clients.User(message.SenderId.ToString())
                        .SendAsync("MessageStatusUpdated", new
                        {
                            MessageId = messageId,
                            UserId = userId,
                            Status = DeliveryStatus.Read,
                            UpdatedAt = DateTime.UtcNow
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read by user {UserId}", messageId, userId);
            }
        }
        
        public async Task UpdateStatus(UserStatus status)
        {
            var userId = GetUserId();
            if (userId == null) return;
            
            try
            {
                await _userService.UpdateUserStatusAsync(userId.Value, status);
                
                // Notify other users about status change
                await Clients.Others.SendAsync("UserStatusChanged", new
                {
                    UserId = userId,
                    Status = status,
                    LastSeen = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for user {UserId}", userId);
            }
        }
        
        private Guid? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
