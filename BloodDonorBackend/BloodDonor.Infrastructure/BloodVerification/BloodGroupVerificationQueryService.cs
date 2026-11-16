using BloodDonor.Application.BloodVerification;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.BloodVerification
{
    public class BloodGroupVerificationQueryService : IBloodGroupVerificationQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public BloodGroupVerificationQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BloodGroupVerificationListResult> GetForReviewAsync(string reviewerUserId, VerificationStatus status)
        {
            var canReview = await ReviewerAccessChecker.CanReviewAsync(_dbContext, reviewerUserId);

            if (!canReview)
            {
                return BloodGroupVerificationListResult.Failure(
                    BloodGroupVerificationQueryErrorType.NotAuthorizedToReview,
                    "You are not authorized to review blood group verifications.");
            }

            // Left join: a donor may have declared a blood group without creating a profile yet.
            // Only the donor's full name is exposed; no address, date of birth, or contact details.
            var items = await (
                from v in _dbContext.DonorBloodGroupVerifications.AsNoTracking()
                join p in _dbContext.DonorProfiles.AsNoTracking() on v.UserId equals p.UserId into profiles
                from p in profiles.DefaultIfEmpty()
                where v.Status == status
                orderby v.CreatedAt
                select new BloodGroupVerificationListItemResponse
                {
                    Id = v.Id,
                    DonorFullName = p != null ? p.FullName : null,
                    DeclaredBloodGroup = v.DeclaredBloodGroup,
                    VerifiedBloodGroup = v.VerifiedBloodGroup,
                    Status = v.Status,
                    RejectionReason = v.RejectionReason,
                    CreatedAt = v.CreatedAt,
                    VerifiedAt = v.VerifiedAt
                }
            ).ToListAsync();

            return BloodGroupVerificationListResult.Success(items);
        }

        public async Task<DonorBloodGroupStatusResult> GetOwnAsync(string donorUserId)
        {
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
                .FirstOrDefaultAsync();

            if (response == null)
            {
                return DonorBloodGroupStatusResult.Failure(
                    BloodGroupVerificationQueryErrorType.VerificationNotFound,
                    "No blood group has been declared for this account.");
            }

            return DonorBloodGroupStatusResult.Success(response);
        }
    }
}