using ChatBackend.Models.Enums;

namespace ChatBackend.Models.DTOs.Conversation
{
    public class UserConversationDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public UserStatus Status { get; set; }
        public ConversationRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime LastReadAt { get; set; }
        public bool IsActive { get; set; }
    }
}
