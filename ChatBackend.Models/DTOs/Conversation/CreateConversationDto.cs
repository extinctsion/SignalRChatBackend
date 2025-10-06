using ChatBackend.Models.Enums;

namespace ChatBackend.Models.DTOs.Conversation
{
    public class CreateConversationDto
    {
        public string Name { get; set; } = string.Empty;
        public ConversationType Type { get; set; } = ConversationType.Group;
        public string? Description { get; set; }
        public List<Guid> MemberIds { get; set; } = new();
    }
}
