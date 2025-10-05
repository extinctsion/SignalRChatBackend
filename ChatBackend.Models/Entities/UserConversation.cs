using ChatBackend.Models.Enums;

namespace ChatBackend.Models.Entities
{
    public class UserConversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public Guid ConversationId { get; set; }

        public ConversationRole Role { get; set; } = ConversationRole.Member;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LeftAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Last read message timestamp for read receipts
        public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public Conversation Conversation { get; set; } = null!;
    }
}
