using ChatBackend.Models.Enums;
using System.ComponentModel.DataAnnotations;


namespace ChatBackend.Models.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public ConversationType Type { get; set; } = ConversationType.Group;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        public Guid CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // For soft delete - messages will be kept for 1 year
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public User CreatedByUser { get; set; } = null!;
        public ICollection<UserConversation> UserConversations { get; set; } = new List<UserConversation>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
