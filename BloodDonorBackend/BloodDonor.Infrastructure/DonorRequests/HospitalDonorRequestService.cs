using BloodDonor.Application.Compatibility;
using BloodDonor.Application.DonorRequests;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.BloodRequests;
using BloodDonor.Infrastructure.Matching;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.DonorRequests
{
    public class HospitalDonorRequestService : IHospitalDonorRequestService
    {
        private const int MaxDonorRequestsPerBloodRequest = 100;

        private readonly ApplicationDbContext _dbContext;
        private readonly IBloodCompatibilityService _compatibilityService;

        public HospitalDonorRequestService(ApplicationDbContext dbContext, IBloodCompatibilityService compatibilityService)
        {
            _dbContext = dbContext;
            _compatibilityService = compatibilityService;
        }

        public async Task<DonorRequestResult<IReadOnlyList<DonorRequestResponse>>> SendAsync(
            string userId, Guid bloodRequestId, SendDonorRequestsRequest request)
        {
            var hospitalId = await ActiveHospitalResolver.GetActiveHospitalIdAsync(_dbContext, userId);

            if (hospitalId == null)
            {
                return Fail<IReadOnlyList<DonorRequestResponse>>(
                    DonorRequestErrorType.NotAuthorized, "You are not authorized to manage donor requests.");
            }

            var ownHospitalId = hospitalId.Value;

            var bloodRequest = await _dbContext.BloodRequests
                .AsNoTracking()
                .Where(r => r.Id == bloodRequestId && r.HospitalId == ownHospitalId)
                .Select(r => new { r.PatientBloodGroup, r.Component, r.Status })
                .FirstOrDefaultAsync();

            if (bloodRequest == null)
            {
                return Fail<IReadOnlyList<DonorRequestResponse>>(
                    DonorRequestErrorType.BloodRequestNotFound, "No blood request exists with the specified id.");
            }

            if (bloodRequest.Status != BloodRequestStatus.Open)
            {
                return Fail<IReadOnlyList<DonorRequestResponse>>(
                    DonorRequestErrorType.BloodRequestNotOpen, "Donor requests can only be sent for open blood requests.");
            }

            var references = request.DonorReferences.Distinct().ToList();

            var hospitalCity = await _dbContext.Hospitals
                .AsNoTracking()
                .Where(h => h.Id == ownHospitalId)
                .Select(h => h.City)
                .FirstAsync();

            var cityKey = hospitalCity.Trim().ToLowerInvariant();

            var compatibleGroups = _compatibilityService
                .GetCompatibleDonorBloodGroups(bloodRequest.PatientBloodGroup, bloodRequest.Component)
                .ToList();

            // Never trust the client's list: every reference must be a CURRENT match for this request.
            var matchedReferences = await MatchCandidateQuery
                .Build(_dbContext, compatibleGroups, cityKey)
                .Where(c => references.Contains(c.ProfileId))
                .Select(c => c.ProfileId)
                .ToListAsync();

            if (matchedReferences.Count != references.Count)
            {
                return Fail<IReadOnlyList<DonorRequestResponse>>(
                    DonorRequestErrorType.DonorsNotEligible,
                    "One or more donor references are not current matches for this blood request.");
            }

            var alreadyRequested = await _dbContext.DonorRequests
                .AnyAsync(r => r.BloodRequestId == bloodRequestId && references.Contains(r.DonorProfileId));

            if (alreadyRequested)
            {
                return Fail<IReadOnlyList<DonorRequestResponse>>(
                    DonorRequestErrorType.DonorAlreadyRequested,
                    "One or more donors have already been sent a request for this blood request.");
            }

            var existingCount = await _dbContext.DonorRequests
                .CountAsync(r => r.BloodRequestId == bloodRequestId);

            if (existingCount + references.Count > MaxDonorRequestsPerBloodRequest)
            {
                return Fail<IReadOnlyList<DonorRequestResponse>>(
                    DonorRequestErrorType.TooManyDonorRequests,
                    $"A blood request can have at most {MaxDonorRequestsPerBloodRequest} donor requests.");
            }

            var now = DateTime.UtcNow;

            var newRequests = references
                .Select(donorProfileId => new DonorRequest
                {
                    Id = Guid.NewGuid(),
                    BloodRequestId = bloodRequestId,
                    DonorProfileId = donorProfileId,
                    Status = DonorRequestStatus.Pending,
                    CreatedByUserId = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                })
                .ToList();

            try
            {
                _dbContext.DonorRequests.AddRange(newRequests);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // The unique index caught a concurrent duplicate. Nothing was saved.
                return Fail<IReadOnlyList<DonorRequestResponse>>(
                    DonorRequestErrorType.DonorAlreadyRequested,
                    "One or more donors have already been sent a request for this blood request.");
            }

            var createdIds = newRequests.Select(r => r.Id).ToList();

            var created = await BuildHospitalView(bloodRequestId)
                .Where(x => createdIds.Contains(x.Id))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return DonorRequestResult<IReadOnlyList<DonorRequestResponse>>.Success(created);
        }

        public async Task<DonorRequestResult<IReadOnlyList<DonorRequestResponse>>> ListAsync(
            string userId, Guid bloodRequestId, DonorRequestStatus? status)
        {
            var hospitalId = await ActiveHospitalResolver.GetActiveHospitalIdAsync(_dbContext, userId);

            if (hospitalId == null)
            {
                return Fail<IReadOnlyList<DonorRequestResponse>>(
                    DonorRequestErrorType.NotAuthorized, "You are not authorized to view donor requests.");
            }

            var bloodRequestStatus = await GetOwnBloodRequestStatusAsync(hospitalId.Value, bloodRequestId);

            if (bloodRequestStatus == null)
            {
                return Fail<IReadOnlyList<DonorRequestResponse>>(
                    DonorRequestErrorType.BloodRequestNotFound, "No blood request exists with the specified id.");
            }

            var query = BuildHospitalView(bloodRequestId);

            if (status != null)
            {
                var statusFilter = status.Value;
                query = query.Where(x => x.Status == statusFilter);
            }

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return DonorRequestResult<IReadOnlyList<DonorRequestResponse>>.Success(items);
        }

        public async Task<DonorRequestResult<DonorRequestResponse>> CancelAsync(
            string userId, Guid bloodRequestId, Guid donorRequestId)
        {
            var hospitalId = await ActiveHospitalResolver.GetActiveHospitalIdAsync(_dbContext, userId);

            if (hospitalId == null)
            {
                return Fail<DonorRequestResponse>(
                    DonorRequestErrorType.NotAuthorized, "You are not authorized to manage donor requests.");
            }

            var bloodRequestStatus = await GetOwnBloodRequestStatusAsync(hospitalId.Value, bloodRequestId);

            if (bloodRequestStatus == null)
            {
                return Fail<DonorRequestResponse>(
                    DonorRequestErrorType.BloodRequestNotFound, "No blood request exists with the specified id.");
            }

            var now = DateTime.UtcNow;
            string? clearedContact = null;

            // Atomic: only a Pending or Accepted request can be cancelled. The shared contact number is erased.
            var affectedRows = await _dbContext.DonorRequests
                .Where(r => r.Id == donorRequestId
                            && r.BloodRequestId == bloodRequestId
                            && (r.Status == DonorRequestStatus.Pending || r.Status == DonorRequestStatus.Accepted))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Status, DonorRequestStatus.Cancelled)
                    .SetProperty(r => r.ContactNumber, clearedContact)
                    .SetProperty(r => r.UpdatedAt, now));

            if (affectedRows == 0)
            {
                var exists = await _dbContext.DonorRequests
                    .AnyAsync(r => r.Id == donorRequestId && r.BloodRequestId == bloodRequestId);

                if (!exists)
                {
                    return Fail<DonorRequestResponse>(
                        DonorRequestErrorType.DonorRequestNotFound, "No donor request exists with the specified id.");
                }

                return Fail<DonorRequestResponse>(
                    DonorRequestErrorType.InvalidTransition, "This donor request can no longer be cancelled.");
            }

            var response = await BuildHospitalView(bloodRequestId)
                .Where(x => x.Id == donorRequestId)
                .FirstAsync();

            return DonorRequestResult<DonorRequestResponse>.Success(response);
        }

        public async Task<DonorRequestResult<DonorRequestContactResponse>> GetContactAsync(
            string userId, Guid bloodRequestId, Guid donorRequestId)
        {
            var hospitalId = await ActiveHospitalResolver.GetActiveHospitalIdAsync(_dbContext, userId);

            if (hospitalId == null)
            {
                return Fail<DonorRequestContactResponse>(
                    DonorRequestErrorType.NotAuthorized, "You are not authorized to view donor contact details.");
            }

            var bloodRequestStatus = await GetOwnBloodRequestStatusAsync(hospitalId.Value, bloodRequestId);

            if (bloodRequestStatus == null)
            {
                return Fail<DonorRequestContactResponse>(
                    DonorRequestErrorType.BloodRequestNotFound, "No blood request exists with the specified id.");
            }

            if (bloodRequestStatus != BloodRequestStatus.Open)
            {
                return Fail<DonorRequestContactResponse>(
                    DonorRequestErrorType.BloodRequestNotOpen, "Contact details are only available for open blood requests.");
            }

            var row = await (
                from r in _dbContext.DonorRequests.AsNoTracking()
                where r.Id == donorRequestId && r.BloodRequestId == bloodRequestId
                join p in _dbContext.DonorProfiles.AsNoTracking() on r.DonorProfileId equals p.Id
                select new { r.Status, r.ContactNumber, p.FullName }
            ).FirstOrDefaultAsync();

            if (row == null)
            {
                return Fail<DonorRequestContactResponse>(
                    DonorRequestErrorType.DonorRequestNotFound, "No donor request exists with the specified id.");
            }

            if (row.Status != DonorRequestStatus.Accepted || row.ContactNumber == null)
            {
                return Fail<DonorRequestContactResponse>(
                    DonorRequestErrorType.DonorRequestNotAccepted, "The donor has not accepted this request.");
            }

            // Fail closed: the access is logged first, and the contact is only returned
            // if the log row was saved successfully.
            _dbContext.DonorContactAccessLogs.Add(new DonorContactAccessLog
            {
                Id = Guid.NewGuid(),
                DonorRequestId = donorRequestId,
                AccessedByUserId = userId,
                AccessedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();

            return DonorRequestResult<DonorRequestContactResponse>.Success(new DonorRequestContactResponse
            {
                DonorRequestId = donorRequestId,
                FullName = row.FullName,
                ContactNumber = row.ContactNumber
            });
        }

        // Hospital view: donor reference, verified group, city, area and status only. Never a name or contact.
        private IQueryable<DonorRequestResponse> BuildHospitalView(Guid bloodRequestId)
        {
            return
                from r in _dbContext.DonorRequests.AsNoTracking()
                where r.BloodRequestId == bloodRequestId
                join p in _dbContext.DonorProfiles.AsNoTracking() on r.DonorProfileId equals p.Id
                join v in _dbContext.DonorBloodGroupVerifications.AsNoTracking() on p.UserId equals v.UserId
                select new DonorRequestResponse
                {
                    Id = r.Id,
                    BloodRequestId = r.BloodRequestId,
                    DonorReference = p.Id,
                    Status = r.Status,
                    VerifiedBloodGroup = v.VerifiedBloodGroup,
                    City = p.City,
                    Area = p.Area,
                    CreatedAt = r.CreatedAt,
                    RespondedAt = r.RespondedAt
                };
        }

        private Task<BloodRequestStatus?> GetOwnBloodRequestStatusAsync(Guid hospitalId, Guid bloodRequestId)
        {
            return _dbContext.BloodRequests
                .AsNoTracking()
                .Where(r => r.Id == bloodRequestId && r.HospitalId == hospitalId)
                .Select(r => (BloodRequestStatus?)r.Status)
                .FirstOrDefaultAsync();
        }

        private static DonorRequestResult<T> Fail<T>(DonorRequestErrorType errorType, string message)
        {
            return DonorRequestResult<T>.Failure(errorType, message);
        }
    }
}