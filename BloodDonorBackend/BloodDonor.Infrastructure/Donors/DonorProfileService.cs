using BloodDonor.Application.Donors;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Donors
{
    public class DonorProfileService : IDonorProfileService
    {
        private readonly ApplicationDbContext _dbContext;

        public DonorProfileService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DonorProfileResult> CreateProfileAsync(string userId, CreateDonorProfileRequest request)
        {
            var existingProfile = await _dbContext.DonorProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (existingProfile != null)
            {
                return DonorProfileResult.Failure(
                    DonorProfileErrorType.ProfileAlreadyExists,
                    "A donor profile already exists for this account.");
            }

            var now = DateTime.UtcNow;

            var profile = new DonorProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                City = request.City,
                Area = request.Area,
                Pincode = request.Pincode,
                CreatedAt = now,
                UpdatedAt = now
            };

            try
            {
                _dbContext.DonorProfiles.Add(profile);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return DonorProfileResult.Failure(
                    DonorProfileErrorType.ProfileAlreadyExists,
                    "A donor profile already exists for this account.");
            }

            return DonorProfileResult.Success(MapToResponse(profile));
        }

        public async Task<DonorProfileResult> GetOwnProfileAsync(string userId)
        {
            var profile = await _dbContext.DonorProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (profile == null)
            {
                return DonorProfileResult.Failure(
                    DonorProfileErrorType.ProfileNotFound,
                    "No donor profile exists for this account.");
            }

            return DonorProfileResult.Success(MapToResponse(profile));
        }

        private static DonorProfileResponse MapToResponse(DonorProfile profile)
        {
            return new DonorProfileResponse
            {
                Id = profile.Id,
                FullName = profile.FullName,
                DateOfBirth = profile.DateOfBirth,
                Gender = profile.Gender,
                Address = profile.Address,
                City = profile.City,
                Area = profile.Area,
                Pincode = profile.Pincode,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
    }
}