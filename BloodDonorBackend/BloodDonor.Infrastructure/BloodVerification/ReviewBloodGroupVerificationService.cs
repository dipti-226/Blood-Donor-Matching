using BloodDonor.Application.BloodVerification;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.BloodVerification
{
    public class ReviewBloodGroupVerificationService : IReviewBloodGroupVerificationService
    {
        private readonly ApplicationDbContext _dbContext;

        public ReviewBloodGroupVerificationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<BloodGroupReviewResult> ApproveAsync(Guid verificationId, string reviewerUserId, ApproveBloodGroupRequest request)
        {
            return DecideAsync(
                verificationId,
                reviewerUserId,
                VerificationStatus.Verified,
                request.VerifiedBloodGroup,
                null);
        }

        public Task<BloodGroupReviewResult> RejectAsync(Guid verificationId, string reviewerUserId, RejectBloodGroupRequest request)
        {
            return DecideAsync(
                verificationId,
                reviewerUserId,
                VerificationStatus.Rejected,
                null,
                request.Reason.Trim());
        }

        private async Task<BloodGroupReviewResult> DecideAsync(Guid verificationId,string reviewerUserId,VerificationStatus newStatus,BloodGroup? verifiedBloodGroup,string? rejectionReason)
        {
            var canReview = await ReviewerAccessChecker.CanReviewAsync(_dbContext, reviewerUserId);

            if (!canReview)
            {
                return BloodGroupReviewResult.Failure(
                    BloodGroupReviewErrorType.NotAuthorizedToReview,
                    "You are not authorized to review blood group verifications.");
            }

            var now = DateTime.UtcNow;
            DateTime? decidedAt = now;
            string? decidedBy = reviewerUserId;

            // Atomic conditional update: only a record that is still Pending can be decided,
            // so two reviewers acting at the same time cannot both succeed.
            var affectedRows = await _dbContext.DonorBloodGroupVerifications
                .Where(v => v.Id == verificationId && v.Status == VerificationStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(v => v.Status, newStatus)
                    .SetProperty(v => v.VerifiedBloodGroup, verifiedBloodGroup)
                    .SetProperty(v => v.RejectionReason, rejectionReason)
                    .SetProperty(v => v.VerifiedByUserId, decidedBy)
                    .SetProperty(v => v.VerifiedAt, decidedAt)
                    .SetProperty(v => v.UpdatedAt, now));

            if (affectedRows == 0)
            {
                var exists = await _dbContext.DonorBloodGroupVerifications
                    .AnyAsync(v => v.Id == verificationId);

                if (!exists)
                {
                    return BloodGroupReviewResult.Failure(
                        BloodGroupReviewErrorType.VerificationNotFound,
                        "No blood group verification exists with the specified id.");
                }

                return BloodGroupReviewResult.Failure(
                    BloodGroupReviewErrorType.VerificationNotPending,
                    "This blood group verification has already been decided.");
            }

            var record = await _dbContext.DonorBloodGroupVerifications
                .AsNoTracking()
                .FirstAsync(v => v.Id == verificationId);

            var response = new BloodGroupReviewResponse
            {
                Id = record.Id,
                DeclaredBloodGroup = record.DeclaredBloodGroup,
                VerifiedBloodGroup = record.VerifiedBloodGroup,
                Status = record.Status,
                VerifiedAt = record.VerifiedAt,
                RejectionReason = record.RejectionReason
            };

            return BloodGroupReviewResult.Success(response);
        }
    }
}