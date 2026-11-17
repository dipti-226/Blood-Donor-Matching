using BloodDonor.Application.BloodRequests;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.BloodRequests
{
    public class BloodRequestQueryService : IBloodRequestQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public BloodRequestQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BloodRequestListResult> GetListAsync(string userId, BloodRequestStatus status)
        {
            var hospitalId = await ActiveHospitalResolver.GetActiveHospitalIdAsync(_dbContext, userId);

            if (hospitalId == null)
            {
                return BloodRequestListResult.Failure(
                    BloodRequestErrorType.NotAuthorized,
                    "You are not authorized to view blood requests.");
            }

            var ownHospitalId = hospitalId.Value;

            var items = await _dbContext.BloodRequests
                .AsNoTracking()
                .Where(r => r.HospitalId == ownHospitalId && r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .Select(BloodRequestMapping.ToResponse)
                .ToListAsync();

            return BloodRequestListResult.Success(items);
        }

        public async Task<BloodRequestResult> GetByIdAsync(string userId, Guid bloodRequestId)
        {
            var hospitalId = await ActiveHospitalResolver.GetActiveHospitalIdAsync(_dbContext, userId);

            if (hospitalId == null)
            {
                return BloodRequestResult.Failure(
                    BloodRequestErrorType.NotAuthorized,
                    "You are not authorized to view blood requests.");
            }

            var ownHospitalId = hospitalId.Value;

            var response = await _dbContext.BloodRequests
                .AsNoTracking()
                .Where(r => r.Id == bloodRequestId && r.HospitalId == ownHospitalId)
                .Select(BloodRequestMapping.ToResponse)
                .FirstOrDefaultAsync();

            if (response == null)
            {
                return BloodRequestResult.Failure(
                    BloodRequestErrorType.RequestNotFound,
                    "No blood request exists with the specified id.");
            }

            return BloodRequestResult.Success(response);
        }
    }
}