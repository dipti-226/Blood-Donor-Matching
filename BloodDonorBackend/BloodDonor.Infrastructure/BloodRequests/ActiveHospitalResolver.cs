using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.BloodRequests
{
    internal static class ActiveHospitalResolver
    {
        // Returns the caller's hospital id only if they belong to a hospital AND it is Active.
        // The hospital is always derived from the membership, never from client input.
        public static Task<Guid?> GetActiveHospitalIdAsync(ApplicationDbContext dbContext, string userId)
        {
            return (
                from hu in dbContext.HospitalUsers.AsNoTracking()
                join h in dbContext.Hospitals.AsNoTracking() on hu.HospitalId equals h.Id
                where hu.UserId == userId && h.Status == HospitalStatus.Active
                select (Guid?)hu.HospitalId
            ).FirstOrDefaultAsync();
        }
    }
}