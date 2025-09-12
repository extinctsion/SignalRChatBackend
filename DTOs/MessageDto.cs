using ChatBackend.Models;

namespace ChatBackend.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderAvatarUrl { get; set; }
        public MessageType Type { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public List<MessageStatusDto> MessageStatuses { get; set; } = new();
        public MessageDto? ReplyToMessage { get; set; }
    }
    
    public class CreateMessageDto
    {
        public Guid ConversationId { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        public string? Content { get; set; }
        public Guid? ReplyToMessageId { get; set; }
    }
    
    public class MessageStatusDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DeliveryStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
