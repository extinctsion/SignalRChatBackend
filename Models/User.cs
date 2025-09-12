using System.ComponentModel.DataAnnotations;

namespace ChatBackend.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? AvatarUrl { get; set; }
        
        public UserStatus Status { get; set; } = UserStatus.Offline;
        
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<UserConversation> UserConversations { get; set; } = new List<UserConversation>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<MessageStatus> MessageStatuses { get; set; } = new List<MessageStatus>();
    }
    
    public enum UserStatus
    {
        Online,
        Away,
        Busy,
        Offline
    }
}
