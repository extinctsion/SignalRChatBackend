using ChatBackend.Models.Enums;

namespace ChatBackend.Models.DTOs.Message
{
    public class UpdateMessageStatusDto
    {
        public DeliveryStatus Status { get; set; }
    }
}
