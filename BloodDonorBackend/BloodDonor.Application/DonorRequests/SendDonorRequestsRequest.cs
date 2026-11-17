using System.ComponentModel.DataAnnotations;

namespace BloodDonor.Application.DonorRequests
{
    public class SendDonorRequestsRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(20)]
        public List<Guid> DonorReferences { get; set; } = new();
    }
}