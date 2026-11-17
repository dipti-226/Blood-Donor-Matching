namespace BloodDonor.Application.DonorRequests
{
    public class DonorRequestContactResponse
    {
        public Guid DonorRequestId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }
}