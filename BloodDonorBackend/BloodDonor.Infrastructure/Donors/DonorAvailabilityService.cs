using BloodDonor.Application.Donors;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Donors
{
    public class DonorAvailabilityService : IDonorAvailabilityService
    {
        private readonly ApplicationDbContext _dbContext;

        public DonorAvailabilityService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DonorAvailabilityResult> GetAsync(string userId)
        {
            var isAvailable = await _dbContext.DonorProfiles
                .AsNoTracking()
                .Where(d => d.UserId == userId)
                .Select(d => (bool?)d.IsAvailableForRequests)
                .FirstOrDefaultAsync();

            if (isAvailable == null)
            {
                return ProfileNotFound();
            }

            return DonorAvailabilityResult.Success(new DonorAvailabilityResponse
            {
                IsAvailable = isAvailable.Value
            });
        }

        public async Task<DonorAvailabilityResult> SetAsync(string userId, SetDonorAvailabilityRequest request)
        {
            var newValue = request.IsAvailable!.Value;
            var now = DateTime.UtcNow;

            var affectedRows = await _dbContext.DonorProfiles
                .Where(d => d.UserId == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(d => d.IsAvailableForRequests, newValue)
                    .SetProperty(d => d.UpdatedAt, now));

            if (affectedRows == 0)
            {
                return ProfileNotFound();
            }

            return DonorAvailabilityResult.Success(new DonorAvailabilityResponse
            {
                IsAvailable = newValue
            });
        }

        private static DonorAvailabilityResult ProfileNotFound()
        {
            return DonorAvailabilityResult.Failure(
                DonorAvailabilityErrorType.ProfileNotFound,
                "No donor profile exists for this account. Create a profile first.");
        }
    }
}