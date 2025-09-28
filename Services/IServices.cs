using ChatBackend.Models;
using ChatBackend.DTOs;

namespace ChatBackend.Services
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserDto userDto);
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 50);
        Task<UserDto> UpdateUserStatusAsync(Guid userId, UserStatus status);
        Task<bool> DeleteUserAsync(Guid userId);
    }
    
    public interface IMessageService
    {
        Task<MessageDto> CreateMessageAsync(Guid senderId, CreateMessageDto messageDto);
        Task<List<MessageDto>> GetMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50);
        Task<MessageDto?> GetMessageByIdAsync(Guid messageId);
        Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
        Task UpdateMessageStatusAsync(Guid messageId, Guid userId, DeliveryStatus status);
    }
    
    public interface IConversationService
    {
        Task<ConversationDto> CreateConversationAsync(Guid createdByUserId, CreateConversationDto conversationDto);
        Task<List<ConversationDto>> GetUserConversationsAsync(Guid userId);
        Task<ConversationDto?> GetConversationByIdAsync(Guid conversationId, Guid userId);
        Task<bool> AddUserToConversationAsync(Guid conversationId, Guid userId, ConversationRole role = ConversationRole.Member);
        Task<bool> RemoveUserFromConversationAsync(Guid conversationId, Guid userId);
        Task<bool> UpdateUserRoleAsync(Guid conversationId, Guid userId, ConversationRole role);
        Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId);
    }
    
    public interface IFileService
    {
        Task<FileUploadDto> UploadFileAsync(IFormFile file, string bucketName = "chat-files");
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<string> GetPresignedUrlAsync(string fileKey, TimeSpan expiration);
    }



    public interface IAppLogger
    {
        void Info(string message, object? context = null);
        void Warn(string message, object? context = null);
        void Error(string message, Exception? ex = null, object? context = null);
        void Debug(string message, object? context = null);
        void LogBusinessEvent(string message, object? context = null);
        void TrackExecutionTime(string operation, TimeSpan duration, object? context = null);
    }
}
