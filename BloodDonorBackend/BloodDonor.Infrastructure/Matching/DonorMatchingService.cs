using BloodDonor.Application.Compatibility;
using BloodDonor.Application.Matching;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.BloodRequests;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Matching
{
    public class DonorMatchingService : IDonorMatchingService
    {
        private const int MaxMatches = 100;

        private readonly ApplicationDbContext _dbContext;
        private readonly IBloodCompatibilityService _compatibilityService;

        public DonorMatchingService(ApplicationDbContext dbContext, IBloodCompatibilityService compatibilityService)
        {
            _dbContext = dbContext;
            _compatibilityService = compatibilityService;
        }

        public async Task<DonorMatchListResult> GetMatchesAsync(string userId, Guid bloodRequestId)
        {
            var hospitalId = await ActiveHospitalResolver.GetActiveHospitalIdAsync(_dbContext, userId);

            if (hospitalId == null)
            {
                return DonorMatchListResult.Failure(
                    DonorMatchErrorType.NotAuthorized,
                    "You are not authorized to view donor matches.");
            }

            var ownHospitalId = hospitalId.Value;

            // Another hospital's request is reported as not found, so its existence is never revealed.
            var bloodRequest = await _dbContext.BloodRequests
                .AsNoTracking()
                .Where(r => r.Id == bloodRequestId && r.HospitalId == ownHospitalId)
                .Select(r => new { r.PatientBloodGroup, r.Component, r.Status })
                .FirstOrDefaultAsync();

            if (bloodRequest == null)
            {
                return DonorMatchListResult.Failure(
                    DonorMatchErrorType.RequestNotFound,
                    "No blood request exists with the specified id.");
            }

            if (bloodRequest.Status != BloodRequestStatus.Open)
            {
                return DonorMatchListResult.Failure(
                    DonorMatchErrorType.RequestNotOpen,
                    "Donor matches are only available for open blood requests.");
            }

            var hospital = await _dbContext.Hospitals
                .AsNoTracking()
                .Where(h => h.Id == ownHospitalId)
                .Select(h => new { h.City, h.Area })
                .FirstAsync();

            var cityKey = hospital.City.Trim().ToLowerInvariant();
            var areaKey = hospital.Area.Trim().ToLowerInvariant();
            var patientGroup = bloodRequest.PatientBloodGroup;

            var compatibleGroups = _compatibilityService
                .GetCompatibleDonorBloodGroups(patientGroup, bloodRequest.Component)
                .ToList();

            var candidates = MatchCandidateQuery.Build(_dbContext, compatibleGroups, cityKey);

            var totalCount = await candidates.CountAsync();

            // Operational ordering only (not medical): exact blood group first,
            // then same area as the hospital, then earliest verification.
            var rows = await candidates
                .OrderByDescending(x => x.VerifiedBloodGroup == patientGroup)
                .ThenByDescending(x => x.Area.Trim().ToLower() == areaKey)
                .ThenBy(x => x.VerifiedAt)
                .ThenBy(x => x.ProfileId)
                .Take(MaxMatches)
                .ToListAsync();

            var matches = rows
                .Select(r => new DonorMatchResponse
                {
                    DonorReference = r.ProfileId,
                    VerifiedBloodGroup = r.VerifiedBloodGroup!.Value,
                    City = r.City,
                    Area = r.Area,
                    IsExactBloodGroupMatch = r.VerifiedBloodGroup.Value == patientGroup
                })
                .ToList();

            return DonorMatchListResult.Success(new DonorMatchListResponse
            {
                BloodRequestId = bloodRequestId,
                TotalCount = totalCount,
                Matches = matches
            });
        }
    }
}