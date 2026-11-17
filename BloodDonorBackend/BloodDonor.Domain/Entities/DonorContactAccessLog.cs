namespace BloodDonor.Domain.Entities
{
    public class DonorContactAccessLog
    {
        public Guid Id { get; set; }
        public Guid DonorRequestId { get; set; }
        public string AccessedByUserId { get; set; } = string.Empty;
        public DateTime AccessedAt { get; set; }
    }
}