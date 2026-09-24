using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Hospitals
{
    public class CreateHospitalService : ICreateHospitalService
    {
        private readonly ApplicationDbContext _dbContext;

        public CreateHospitalService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateHospitalResult> CreateHospitalAsync(CreateHospitalRequest request)
        {
            var existingHospital = await _dbContext.Hospitals
                .FirstOrDefaultAsync(h => h.RegistrationNumber == request.RegistrationNumber);

            if (existingHospital != null)
            {
                return CreateHospitalResult.Failure(
                    CreateHospitalErrorType.DuplicateRegistrationNumber,
                    "A hospital with this registration number already exists.");
            }

            var now = DateTime.UtcNow;

            var hospital = new Hospital
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                RegistrationNumber = request.RegistrationNumber,
                Address = request.Address,
                City = request.City,
                Area = request.Area,
                Pincode = request.Pincode,
                ContactNumber = request.ContactNumber,
                Email = request.Email,
                Status = HospitalStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            };

            try
            {
                _dbContext.Hospitals.Add(hospital);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return CreateHospitalResult.Failure(
                    CreateHospitalErrorType.DuplicateRegistrationNumber,
                    "A hospital with this registration number already exists.");
            }

            return CreateHospitalResult.Success(MapToResponse(hospital));
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