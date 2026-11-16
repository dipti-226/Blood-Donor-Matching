using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.BloodVerification
{
    internal static class ReviewerAccessChecker
    {
        // A reviewer must belong to a hospital, and that hospital must be Active.
        public static Task<bool> CanReviewAsync(ApplicationDbContext dbContext, string userId)
        {
            return (
                from hu in dbContext.HospitalUsers.AsNoTracking()
                join h in dbContext.Hospitals.AsNoTracking() on hu.HospitalId equals h.Id
                where hu.UserId == userId && h.Status == HospitalStatus.Active
                select hu.Id
            ).AnyAsync();
        }
    }
}