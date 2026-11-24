using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Constants;
using BloodDonor.Infrastructure.Identity;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Hospitals
{
    public class HospitalAdminQueryService : IHospitalAdminQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public HospitalAdminQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HospitalAdminListResult> GetAdminsAsync(Guid hospitalId)
        {
            var hospitalExists = await _dbContext.Hospitals
                .AnyAsync(h => h.Id == hospitalId);

            if (!hospitalExists)
            {
                return HospitalAdminListResult.Failure(
                    HospitalAdminQueryErrorType.HospitalNotFound,
                    "No hospital exists with the specified id.");
            }

            var admins = await (
                from hu in _dbContext.HospitalUsers.AsNoTracking()
                join u in _dbContext.Set<ApplicationUser>().AsNoTracking() on hu.UserId equals u.Id
                join ur in _dbContext.Set<IdentityUserRole<string>>().AsNoTracking() on u.Id equals ur.UserId
                join r in _dbContext.Set<IdentityRole>().AsNoTracking() on ur.RoleId equals r.Id
                where hu.HospitalId == hospitalId && r.Name == Roles.HospitalAdmin
                orderby u.Email
                select new HospitalAdminResponse
                {
                    HospitalUserId = hu.Id,
                    HospitalId = hu.HospitalId,
                    UserId = u.Id,
                    Email = u.Email!,
                    CreatedAt = hu.CreatedAt
                }
            ).ToListAsync();

            return HospitalAdminListResult.Success(admins);
        }
    }
}