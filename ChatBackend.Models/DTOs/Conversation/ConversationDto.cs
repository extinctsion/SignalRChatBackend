using ChatBackend.Models.Enums;
using ChatBackend.Models.DTOs.Message;

namespace ChatBackend.Models.DTOs.Conversation
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
}
