using ChatBackend.Models;

namespace ChatBackend.DTOs
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ConversationType Type { get; set; }
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<UserConversationDto> Members { get; set; } = new();
        public MessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
    
    public class CreateConversationDto
    {
        public string Name { get; set; } = string.Empty;
        public ConversationType Type { get; set; } = ConversationType.Group;
        public string? Description { get; set; }
        public List<Guid> MemberIds { get; set; } = new();
    }
    
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
