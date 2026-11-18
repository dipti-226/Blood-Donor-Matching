using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Hospitals
{
    public class HospitalQueryService : IHospitalQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public HospitalQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HospitalResult> GetByIdAsync(Guid id, string callerUserId, bool isSuperAdmin)
        {
            var hospital = await _dbContext.Hospitals
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hospital == null)
            {
                return HospitalResult.Failure(
                    HospitalErrorType.HospitalNotFound,
                    "No hospital exists with the specified id.");
            }

            if (!isSuperAdmin)
            {
                var membership = await _dbContext.HospitalUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(hu => hu.UserId == callerUserId);

                if (membership == null || membership.HospitalId != id)
                {
                    return HospitalResult.Failure(
                        HospitalErrorType.NotAuthorizedForHospital,
                        "You are not authorized to view this hospital.");
                }
            }

            return HospitalResult.Success(MapToResponse(hospital));
        }

        public async Task<IReadOnlyList<HospitalResponse>> GetAllAsync()
        {
            var hospitals = await _dbContext.Hospitals
                .AsNoTracking()
                .OrderBy(h => h.Name)
                .ToListAsync();

            return hospitals.Select(MapToResponse).ToList();
        }
        public async Task<HospitalResult> GetMineAsync(string callerUserId)
        {
            // The hospital comes from the caller's membership. A Pending or Suspended hospital is still
            // returned, so the app can tell the user what state their hospital is in.
            var hospital = await (
                from hu in _dbContext.HospitalUsers.AsNoTracking()
                where hu.UserId == callerUserId
                join h in _dbContext.Hospitals.AsNoTracking() on hu.HospitalId equals h.Id
                select h
            ).FirstOrDefaultAsync();

            if (hospital == null)
            {
                return HospitalResult.Failure(
                    HospitalErrorType.HospitalNotFound,
                    "Your account is not linked to a hospital.");
            }

            return HospitalResult.Success(MapToResponse(hospital));
        }
        private static HospitalResponse MapToResponse(Hospital hospital)
        {
            return new HospitalResponse
            {
                Id = hospital.Id,
                Name = hospital.Name,
                RegistrationNumber = hospital.RegistrationNumber,
                Address = hospital.Address,
                City = hospital.City,
                Area = hospital.Area,
                Pincode = hospital.Pincode,
                ContactNumber = hospital.ContactNumber,
                Email = hospital.Email,
                Status = hospital.Status,
                CreatedAt = hospital.CreatedAt,
                UpdatedAt = hospital.UpdatedAt
            };
        }
    }
}