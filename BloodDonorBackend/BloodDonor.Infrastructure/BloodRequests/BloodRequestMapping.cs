using System.Linq.Expressions;
using BloodDonor.Application.BloodRequests;
using BloodDonor.Domain.Entities;

namespace BloodDonor.Infrastructure.BloodRequests
{
    internal static class BloodRequestMapping
    {
        public static readonly Expression<Func<BloodRequest, BloodRequestResponse>> ToResponse =
            r => new BloodRequestResponse
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
                CancellationReason = r.CancellationReason
            };

        private static readonly Func<BloodRequest, BloodRequestResponse> ToResponseCompiled = ToResponse.Compile();

        public static BloodRequestResponse Map(BloodRequest request)
        {
            return ToResponseCompiled(request);
        }
    }
}