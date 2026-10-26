using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Hospitals
{
    public class UpdateHospitalService : IUpdateHospitalService
    {
        private readonly ApplicationDbContext _dbContext;

        public UpdateHospitalService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HospitalResult> UpdateHospitalAsync(
            Guid hospitalId,
            string callerUserId,
            UpdateHospitalRequest request)
        {
            var hospital = await _dbContext.Hospitals
                .FirstOrDefaultAsync(h => h.Id == hospitalId);

            if (hospital == null)
            {
                return HospitalResult.Failure(
                    HospitalErrorType.HospitalNotFound,
                    "No hospital exists with the specified id.");
            }

            var callerMembership = await _dbContext.HospitalUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(hu => hu.UserId == callerUserId);

            if (callerMembership == null || callerMembership.HospitalId != hospitalId)
            {
                return HospitalResult.Failure(
                    HospitalErrorType.NotAuthorizedForHospital,
                    "You are not authorized to update this hospital.");
            }

            hospital.Name = request.Name;
            hospital.Address = request.Address;
            hospital.City = request.City;
            hospital.Area = request.Area;
            hospital.Pincode = request.Pincode;
            hospital.ContactNumber = request.ContactNumber;
            hospital.Email = request.Email;
            hospital.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

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