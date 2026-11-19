using BloodDonor.Application.BloodRequests;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Notifications;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.BloodRequests
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly ApplicationDbContext _dbContext;

        public BloodRequestService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BloodRequestResult> CreateAsync(string userId, CreateBloodRequestRequest request)
        {
            var hospitalId = await ActiveHospitalResolver.GetActiveHospitalIdAsync(_dbContext, userId);

            if (hospitalId == null)
            {
                return NotAuthorizedResult();
            }

            var now = DateTime.UtcNow;

            var bloodRequest = new BloodRequest
            {
                Id = Guid.NewGuid(),
                HospitalId = hospitalId.Value,
                CreatedByUserId = userId,
                PatientBloodGroup = request.PatientBloodGroup!.Value,
                Component = request.Component!.Value,
                UnitsRequired = request.UnitsRequired,
                Urgency = request.Urgency!.Value,
                Status = BloodRequestStatus.Open,
                CreatedAt = now,
                UpdatedAt = now
            };

            _dbContext.BloodRequests.Add(bloodRequest);
            await _dbContext.SaveChangesAsync();

            return BloodRequestResult.Success(BloodRequestMapping.Map(bloodRequest));
        }

        public Task<BloodRequestResult> FulfillAsync(string userId, Guid bloodRequestId)
        {
            return CloseAsync(userId, bloodRequestId, BloodRequestStatus.Fulfilled, null);
        }

        public Task<BloodRequestResult> CancelAsync(string userId, Guid bloodRequestId, CancelBloodRequestRequest request)
        {
            return CloseAsync(userId, bloodRequestId, BloodRequestStatus.Cancelled, request.Reason.Trim());
        }

        private async Task<BloodRequestResult> CloseAsync(string userId,Guid bloodRequestId,BloodRequestStatus newStatus,string? cancellationReason)
        {
            var hospitalId = await ActiveHospitalResolver.GetActiveHospitalIdAsync(_dbContext, userId);

            if (hospitalId == null)
            {
                return NotAuthorizedResult();
            }

            var now = DateTime.UtcNow;
            DateTime? closedAt = now;
            string? closedBy = userId;
            var ownHospitalId = hospitalId.Value;

            // The close and the donors' notifications commit or roll back together.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Atomic conditional update: only an Open request of the caller's own hospital can be closed,
            // so two staff acting at the same time cannot both succeed.
            var affectedRows = await _dbContext.BloodRequests
                .Where(r => r.Id == bloodRequestId
                            && r.HospitalId == ownHospitalId
                            && r.Status == BloodRequestStatus.Open)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Status, newStatus)
                    .SetProperty(r => r.ClosedAt, closedAt)
                    .SetProperty(r => r.ClosedByUserId, closedBy)
                    .SetProperty(r => r.CancellationReason, cancellationReason)
                    .SetProperty(r => r.UpdatedAt, now));

            if (affectedRows == 0)
            {
                // A request belonging to another hospital is reported as not found,
                // so its existence is never revealed.
                var existsInOwnHospital = await _dbContext.BloodRequests
                    .AnyAsync(r => r.Id == bloodRequestId && r.HospitalId == ownHospitalId);

                if (!existsInOwnHospital)
                {
                    return BloodRequestResult.Failure(
                        BloodRequestErrorType.RequestNotFound,
                        "No blood request exists with the specified id.");
                }

                return BloodRequestResult.Failure(
                    BloodRequestErrorType.RequestNotOpen,
                    "This blood request is no longer open.");
            }

            // Tell donors who still have a Pending or Accepted request that the hospital no longer needs them.
            var donorTargets = await (
                from r in _dbContext.DonorRequests.AsNoTracking()
                where r.BloodRequestId == bloodRequestId
                      && (r.Status == DonorRequestStatus.Pending || r.Status == DonorRequestStatus.Accepted)
                join p in _dbContext.DonorProfiles.AsNoTracking() on r.DonorProfileId equals p.Id
                select new { DonorRequestId = r.Id, p.UserId }
            ).ToListAsync();

            if (donorTargets.Count > 0)
            {
                var hospitalName = await _dbContext.Hospitals
                    .AsNoTracking()
                    .Where(h => h.Id == ownHospitalId)
                    .Select(h => h.Name)
                    .FirstAsync();

                var message = NotificationBuilder.BloodRequestClosed(hospitalName);

                foreach (var target in donorTargets)
                {
                    _dbContext.Notifications.Add(NotificationBuilder.Create(
                        target.UserId,
                        NotificationType.BloodRequestClosed,
                        message,
                        bloodRequestId,
                        target.DonorRequestId));
                }

                await _dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            var response = await _dbContext.BloodRequests
                .AsNoTracking()
                .Where(r => r.Id == bloodRequestId)
                .Select(BloodRequestMapping.ToResponse(_dbContext)).FirstAsync();

            return BloodRequestResult.Success(response);
        }

        private static BloodRequestResult NotAuthorizedResult()
        {
            return BloodRequestResult.Failure(
                BloodRequestErrorType.NotAuthorized,
                "You are not authorized to manage blood requests.");
        }
    }
}