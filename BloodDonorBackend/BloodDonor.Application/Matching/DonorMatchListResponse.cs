namespace BloodDonor.Application.Matching
{
    public class DonorMatchListResponse
    {
        public Guid BloodRequestId { get; set; }
        public int TotalCount { get; set; }
        public IReadOnlyList<DonorMatchResponse> Matches { get; set; } = Array.Empty<DonorMatchResponse>();
        public string Notice { get; set; } =
            "Based on blood group compatibility, verification, availability and city only. " +
            "This is not a medical eligibility decision. Qualified staff must assess every donor.";
    }
}