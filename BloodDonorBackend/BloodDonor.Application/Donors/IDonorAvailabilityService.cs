namespace BloodDonor.Application.Donors
{
    public interface IDonorAvailabilityService
    {
        Task<DonorAvailabilityResult> GetAsync(string userId);

        Task<DonorAvailabilityResult> SetAsync(string userId, SetDonorAvailabilityRequest request);
    }
}