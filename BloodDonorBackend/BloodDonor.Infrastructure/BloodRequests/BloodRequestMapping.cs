using System.Linq.Expressions;
using BloodDonor.Application.BloodRequests;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;

namespace BloodDonor.Infrastructure.BloodRequests
{
    internal static class BloodRequestMapping
    {
        public static Expression<Func<BloodRequest, BloodRequestResponse>> ToResponse(ApplicationDbContext dbContext)
        {
            return r => new BloodRequestResponse
            {
                Id = r.Id,
                HospitalId = r.HospitalId,
                PatientBloodGroup = r.PatientBloodGroup,
                Component = r.Component,
                UnitsRequired = r.UnitsRequired,
                Urgency = r.Urgency,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ClosedAt = r.ClosedAt,
                CancellationReason = r.CancellationReason,
                AskedCount = dbContext.DonorRequests.Count(d =>
                    d.BloodRequestId == r.Id && d.Status != DonorRequestStatus.Cancelled),
                AcceptedCount = dbContext.DonorRequests.Count(d =>
                    d.BloodRequestId == r.Id && d.Status == DonorRequestStatus.Accepted)
            };
        }

        // Used right after creation, when no donor request can exist yet.
        public static BloodRequestResponse Map(BloodRequest request)
        {
            return new BloodRequestResponse
            {
                Id = request.Id,
                HospitalId = request.HospitalId,
                PatientBloodGroup = request.PatientBloodGroup,
                Component = request.Component,
                UnitsRequired = request.UnitsRequired,
                Urgency = request.Urgency,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                ClosedAt = request.ClosedAt,
                CancellationReason = request.CancellationReason,
                AskedCount = 0,
                AcceptedCount = 0
            };
        }
    }
}