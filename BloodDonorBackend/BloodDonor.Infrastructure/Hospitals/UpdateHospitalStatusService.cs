using BloodDonor.Application.Hospitals;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Hospitals
{
    public class UpdateHospitalStatusService : IUpdateHospitalStatusService
    {
        private readonly ApplicationDbContext _dbContext;

        public UpdateHospitalStatusService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HospitalResult> UpdateStatusAsync(Guid id, UpdateHospitalStatusRequest request)
        {
            var hospital = await _dbContext.Hospitals
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hospital == null)
            {
                return HospitalResult.Failure(
                    HospitalErrorType.HospitalNotFound,
                    "No hospital exists with the specified id.");
            }

            hospital.Status = request.Status;
            hospital.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return HospitalResult.Success(MapToResponse(hospital));
        }

        private static HospitalResponse MapToResponse(Domain.Entities.Hospital hospital)
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