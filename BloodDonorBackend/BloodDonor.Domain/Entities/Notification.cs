namespace BloodDonor.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string RecipientUserId { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? BloodRequestId { get; set; }
        public Guid? DonorRequestId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}