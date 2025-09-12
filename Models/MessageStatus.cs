namespace ChatBackend.Models
{
    public class MessageStatus
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid MessageId { get; set; }
        
        public Guid UserId { get; set; }
        
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Sent;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Message Message { get; set; } = null!;
        public User User { get; set; } = null!;
    }
    
    public enum DeliveryStatus
    {
        Sent,
        Delivered,
        Read
    }
}
