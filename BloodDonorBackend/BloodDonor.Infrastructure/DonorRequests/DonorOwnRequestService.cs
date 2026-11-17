using BloodDonor.Application.DonorRequests;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Notifications;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.DonorRequests
{
    public class DonorOwnRequestService : IDonorOwnRequestService
    {
        private readonly ApplicationDbContext _dbContext;

        public DonorOwnRequestService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DonorRequestResult<IReadOnlyList<DonorOwnRequestResponse>>> ListAsync(string donorUserId)
        {
            // Only Pending or Accepted requests whose blood request is still Open.
            var rows = await BuildOwnQuery(donorUserId)
                .Where(x => x.BloodRequestStatus == BloodRequestStatus.Open
                            && (x.Status == DonorRequestStatus.Pending || x.Status == DonorRequestStatus.Accepted))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            IReadOnlyList<DonorOwnRequestResponse> items = rows.Select(ToResponse).ToList();

            return DonorRequestResult<IReadOnlyList<DonorOwnRequestResponse>>.Success(items);
        }

        public Task<DonorRequestResult<DonorOwnRequestResponse>> AcceptAsync(
            string donorUserId, Guid donorRequestId, AcceptDonorRequestRequest request)
        {
            return RespondAsync(
                donorUserId,
                donorRequestId,
                DonorRequestStatus.Accepted,
                allowFromAccepted: false,
                contactNumber: request.ContactNumber.Trim());
        }

        public Task<DonorRequestResult<DonorOwnRequestResponse>> DeclineAsync(string donorUserId, Guid donorRequestId)
        {
            return RespondAsync(
                donorUserId,
                donorRequestId,
                DonorRequestStatus.Declined,
                allowFromAccepted: true,
                contactNumber: null);
        }

        private async Task<DonorRequestResult<DonorOwnRequestResponse>> RespondAsync(
            string donorUserId,
            Guid donorRequestId,
            DonorRequestStatus newStatus,
            bool allowFromAccepted,
            string? contactNumber)
        {
            var now = DateTime.UtcNow;
            DateTime? respondedAt = now;

            // The response and the hospital's notifications commit or roll back together.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Atomic: the request must belong to this donor, still be in an allowed state,
            // and its blood request must still be Open. Declining erases the shared number.
            var affectedRows = await _dbContext.DonorRequests
                .Where(r => r.Id == donorRequestId
                            && (r.Status == DonorRequestStatus.Pending
                                || (allowFromAccepted && r.Status == DonorRequestStatus.Accepted))
                            && _dbContext.DonorProfiles.Any(p => p.Id == r.DonorProfileId && p.UserId == donorUserId)
                            && _dbContext.BloodRequests.Any(b => b.Id == r.BloodRequestId && b.Status == BloodRequestStatus.Open))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Status, newStatus)
                    .SetProperty(r => r.ContactNumber, contactNumber)
                    .SetProperty(r => r.RespondedAt, respondedAt)
                    .SetProperty(r => r.UpdatedAt, now));

            var row = await BuildOwnQuery(donorUserId)
                .Where(x => x.DonorRequestId == donorRequestId)
                .FirstOrDefaultAsync();

            if (affectedRows == 0)
            {
                // Someone else's request is reported as not found.
                if (row == null)
                {
                    return DonorRequestResult<DonorOwnRequestResponse>.Failure(
                        DonorRequestErrorType.DonorRequestNotFound, "No donor request exists with the specified id.");
                }

                if (row.BloodRequestStatus != BloodRequestStatus.Open)
                {
                    return DonorRequestResult<DonorOwnRequestResponse>.Failure(
                        DonorRequestErrorType.BloodRequestNotOpen, "This blood request is no longer open.");
                }

                return DonorRequestResult<DonorOwnRequestResponse>.Failure(
                    DonorRequestErrorType.InvalidTransition, "This donor request can no longer be changed that way.");
            }

            // Notify every member of the requesting hospital (admin and staff), one row each.
            var context = await (
                from r in _dbContext.DonorRequests.AsNoTracking()
                where r.Id == donorRequestId
                join b in _dbContext.BloodRequests.AsNoTracking() on r.BloodRequestId equals b.Id
                select new { BloodRequestId = b.Id, b.HospitalId, b.PatientBloodGroup }
            ).FirstAsync();

            var memberUserIds = await _dbContext.HospitalUsers
                .AsNoTracking()
                .Where(hu => hu.HospitalId == context.HospitalId)
                .Select(hu => hu.UserId)
                .ToListAsync();

            var type = newStatus == DonorRequestStatus.Accepted
                ? NotificationType.DonorRequestAccepted
                : NotificationType.DonorRequestDeclined;

            var message = newStatus == DonorRequestStatus.Accepted
                ? NotificationBuilder.DonorRequestAccepted(context.PatientBloodGroup)
                : NotificationBuilder.DonorRequestDeclined(context.PatientBloodGroup);

            foreach (var memberUserId in memberUserIds)
            {
                _dbContext.Notifications.Add(NotificationBuilder.Create(
                    memberUserId, type, message, context.BloodRequestId, donorRequestId));
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return DonorRequestResult<DonorOwnRequestResponse>.Success(ToResponse(row!));
        }

        private IQueryable<OwnRequestRow> BuildOwnQuery(string donorUserId)
        {
            return
                from r in _dbContext.DonorRequests.AsNoTracking()
                join p in _dbContext.DonorProfiles.AsNoTracking() on r.DonorProfileId equals p.Id
                where p.UserId == donorUserId
                join b in _dbContext.BloodRequests.AsNoTracking() on r.BloodRequestId equals b.Id
                join h in _dbContext.Hospitals.AsNoTracking() on b.HospitalId equals h.Id
                select new OwnRequestRow
                {
                    DonorRequestId = r.Id,
                    Status = r.Status,
                    BloodRequestStatus = b.Status,
                    HospitalName = h.Name,
                    HospitalAddress = h.Address,
                    HospitalArea = h.Area,
                    HospitalCity = h.City,
                    BloodGroupNeeded = b.PatientBloodGroup,
                    Component = b.Component,
                    UnitsRequired = b.UnitsRequired,
                    Urgency = b.Urgency,
                    SharedContactNumber = r.ContactNumber,
                    CreatedAt = r.CreatedAt,
                    RespondedAt = r.RespondedAt
                };
        }

        private static DonorOwnRequestResponse ToResponse(OwnRequestRow row)
        {
            return new DonorOwnRequestResponse
            {
                DonorRequestId = row.DonorRequestId,
                Status = row.Status,
                HospitalName = row.HospitalName,
                HospitalAddress = row.HospitalAddress,
                HospitalArea = row.HospitalArea,
                HospitalCity = row.HospitalCity,
                BloodGroupNeeded = row.BloodGroupNeeded,
                Component = row.Component,
                UnitsRequired = row.UnitsRequired,
                Urgency = row.Urgency,
                SharedContactNumber = row.SharedContactNumber,
                CreatedAt = row.CreatedAt,
                RespondedAt = row.RespondedAt
            };
        }

        private sealed class OwnRequestRow
        {
            public Guid DonorRequestId { get; set; }
            public DonorRequestStatus Status { get; set; }
            public BloodRequestStatus BloodRequestStatus { get; set; }
            public string HospitalName { get; set; } = string.Empty;
            public string HospitalAddress { get; set; } = string.Empty;
            public string HospitalArea { get; set; } = string.Empty;
            public string HospitalCity { get; set; } = string.Empty;
            public BloodGroup BloodGroupNeeded { get; set; }
            public BloodComponent Component { get; set; }
            public int UnitsRequired { get; set; }
            public UrgencyLevel Urgency { get; set; }
            public string? SharedContactNumber { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? RespondedAt { get; set; }
        }
    }
}