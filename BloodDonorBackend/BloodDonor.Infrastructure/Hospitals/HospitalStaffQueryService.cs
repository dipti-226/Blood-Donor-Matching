using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Constants;
using BloodDonor.Infrastructure.Identity;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Hospitals
{
    public class HospitalStaffQueryService : IHospitalStaffQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public HospitalStaffQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HospitalStaffListResult> GetStaffAsync(Guid hospitalId, string callerUserId)
        {
            var hospitalExists = await _dbContext.Hospitals
                .AnyAsync(h => h.Id == hospitalId);

            if (!hospitalExists)
            {
                return HospitalStaffListResult.Failure(
                    HospitalStaffQueryErrorType.HospitalNotFound,
                    "No hospital exists with the specified id.");
            }

            var callerMembership = await _dbContext.HospitalUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(hu => hu.UserId == callerUserId);

            if (callerMembership == null || callerMembership.HospitalId != hospitalId)
            {
                return HospitalStaffListResult.Failure(
                    HospitalStaffQueryErrorType.NotAuthorizedForHospital,
                    "You are not authorized to view staff for this hospital.");
            }

            var staff = await (
                from hu in _dbContext.HospitalUsers.AsNoTracking()
                join u in _dbContext.Set<ApplicationUser>().AsNoTracking() on hu.UserId equals u.Id
                join ur in _dbContext.Set<IdentityUserRole<string>>().AsNoTracking() on u.Id equals ur.UserId
                join r in _dbContext.Set<IdentityRole>().AsNoTracking() on ur.RoleId equals r.Id
                where hu.HospitalId == hospitalId && r.Name == Roles.HospitalStaff
                orderby u.Email
                select new HospitalStaffResponse
                {
                    HospitalUserId = hu.Id,
                    HospitalId = hu.HospitalId,
                    UserId = u.Id,
                    Email = u.Email!,
                    CreatedAt = hu.CreatedAt
                }
            ).ToListAsync();

            return HospitalStaffListResult.Success(staff);
        }
    }
}