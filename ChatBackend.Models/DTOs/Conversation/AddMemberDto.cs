using ChatBackend.Models.Enums;

namespace ChatBackend.Models.DTOs.Conversation
{
    public class AddMemberDto
    {
        public Guid UserId { get; set; }
        public ConversationRole Role { get; set; } = ConversationRole.Member;
    }
}
