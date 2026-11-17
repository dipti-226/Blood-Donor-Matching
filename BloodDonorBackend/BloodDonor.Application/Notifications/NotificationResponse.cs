using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.Notifications
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? BloodRequestId { get; set; }
        public Guid? DonorRequestId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}