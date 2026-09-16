using BloodDonor.Application.BloodVerification;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.BloodVerification
{
    public class DonorBloodGroupVerificationService : IDonorBloodGroupVerificationService
    {
        private readonly ApplicationDbContext _dbContext;

        public DonorBloodGroupVerificationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DonorBloodGroupVerificationResult> DeclareBloodGroupAsync(string userId, DeclareBloodGroupRequest request)
        {
            var existing = await _dbContext.DonorBloodGroupVerifications
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (existing != null)
            {
                return DonorBloodGroupVerificationResult.Failure(
                    DonorBloodGroupVerificationErrorType.VerificationAlreadyExists,
                    "A blood group verification record already exists for this account.");
            }

            var now = DateTime.UtcNow;

            var verification = new DonorBloodGroupVerification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeclaredBloodGroup = request.BloodGroup,
                VerifiedBloodGroup = null,
                Status = VerificationStatus.Pending,
                VerifiedByUserId = null,
                VerifiedAt = null,
                RejectionReason = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            try
            {
                _dbContext.DonorBloodGroupVerifications.Add(verification);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return DonorBloodGroupVerificationResult.Failure(
                    DonorBloodGroupVerificationErrorType.VerificationAlreadyExists,
                    "A blood group verification record already exists for this account.");
            }

            var response = new DonorBloodGroupVerificationResponse
            {
                DeclaredBloodGroup = verification.DeclaredBloodGroup,
                Status = verification.Status
            };

            return DonorBloodGroupVerificationResult.Success(response);
        }
    }
}