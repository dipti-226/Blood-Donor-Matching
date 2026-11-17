using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.DonorRequests
{
    public class DonorRequestResponse
    {
        public Guid Id { get; set; }
        public Guid BloodRequestId { get; set; }
        public Guid DonorReference { get; set; }
        public DonorRequestStatus Status { get; set; }
        public BloodGroup? VerifiedBloodGroup { get; set; }
        public string City { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}