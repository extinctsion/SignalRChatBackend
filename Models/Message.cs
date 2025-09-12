using System.ComponentModel.DataAnnotations;

namespace ChatBackend.Models
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid ConversationId { get; set; }
        
        public Guid SenderId { get; set; }
        
        public MessageType Type { get; set; } = MessageType.Text;
        
        [StringLength(4000)]
        public string? Content { get; set; }
        
        // For file uploads (S3 URLs)
        [StringLength(1000)]
        public string? FileUrl { get; set; }
        
        [StringLength(255)]
        public string? FileName { get; set; }
        
        [StringLength(100)]
        public string? ContentType { get; set; }
        
        public long? FileSize { get; set; }
        
        // For message threading/replies
        public Guid? ReplyToMessageId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // For soft delete
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
        
        // Navigation properties
        public Conversation Conversation { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public Message? ReplyToMessage { get; set; }
        public ICollection<Message> Replies { get; set; } = new List<Message>();
        public ICollection<MessageStatus> MessageStatuses { get; set; } = new List<MessageStatus>();
    }
    
    public enum MessageType
    {
        Text,
        Image,
        File,
        Emoji,
        System // For system messages like "User joined", "User left", etc.
    }
}
