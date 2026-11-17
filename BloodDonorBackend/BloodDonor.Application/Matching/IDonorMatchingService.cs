namespace BloodDonor.Application.Matching
{
    public interface IDonorMatchingService
    {
        Task<DonorMatchListResult> GetMatchesAsync(string userId, Guid bloodRequestId);
    }
}