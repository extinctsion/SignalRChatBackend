using ChatBackend.Models.Enums;

namespace ChatBackend.Models.DTOs.Message
{
    public class MessageStatusDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DeliveryStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
