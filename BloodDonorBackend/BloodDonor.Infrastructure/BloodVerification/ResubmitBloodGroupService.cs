using BloodDonor.Application.BloodVerification;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.BloodVerification
{
    public class ResubmitBloodGroupService : IResubmitBloodGroupService
    {
        private readonly ApplicationDbContext _dbContext;

        public ResubmitBloodGroupService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResubmitBloodGroupResult> ResubmitAsync(string donorUserId, ResubmitBloodGroupRequest request)
        {
            var now = DateTime.UtcNow;
            var newDeclaredBloodGroup = request.BloodGroup;
            BloodGroup? clearedVerifiedBloodGroup = null;
            string? clearedText = null;
            DateTime? clearedDate = null;

            // Atomic conditional update: only this donor's Rejected record can go back to Pending,
            // so a duplicate or concurrent submit cannot succeed twice.
            var affectedRows = await _dbContext.DonorBloodGroupVerifications
                .Where(v => v.UserId == donorUserId && v.Status == VerificationStatus.Rejected)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(v => v.DeclaredBloodGroup, newDeclaredBloodGroup)
                    .SetProperty(v => v.Status, VerificationStatus.Pending)
                    .SetProperty(v => v.VerifiedBloodGroup, clearedVerifiedBloodGroup)
                    .SetProperty(v => v.RejectionReason, clearedText)
                    .SetProperty(v => v.VerifiedByUserId, clearedText)
                    .SetProperty(v => v.VerifiedAt, clearedDate)
                    .SetProperty(v => v.UpdatedAt, now));

            if (affectedRows == 0)
            {
                var exists = await _dbContext.DonorBloodGroupVerifications
                    .AnyAsync(v => v.UserId == donorUserId);

                if (!exists)
                {
                    return ResubmitBloodGroupResult.Failure(
                        ResubmitBloodGroupErrorType.VerificationNotFound,
                        "No blood group has been declared for this account.");
                }

                return ResubmitBloodGroupResult.Failure(
                    ResubmitBloodGroupErrorType.VerificationNotRejected,
                    "Only a rejected blood group declaration can be resubmitted.");
            }

            var response = await _dbContext.DonorBloodGroupVerifications
                .AsNoTracking()
                .Where(v => v.UserId == donorUserId)
                .Select(v => new DonorBloodGroupStatusResponse
                {
                    DeclaredBloodGroup = v.DeclaredBloodGroup,
                    VerifiedBloodGroup = v.VerifiedBloodGroup,
                    Status = v.Status,
                    RejectionReason = v.RejectionReason,
                    CreatedAt = v.CreatedAt,
                    VerifiedAt = v.VerifiedAt
                })
                .FirstAsync();

            return ResubmitBloodGroupResult.Success(response);
        }
    }
}