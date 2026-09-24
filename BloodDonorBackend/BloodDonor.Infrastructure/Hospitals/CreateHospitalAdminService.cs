using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Constants;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Hospitals
{
    public class CreateHospitalAdminService : ICreateHospitalAdminService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateHospitalAdminService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<CreateHospitalAdminResult> CreateHospitalAdminAsync(
            Guid hospitalId,
            CreateHospitalAdminRequest request)
        {
            var hospitalExists = await _dbContext.Hospitals
                .AnyAsync(h => h.Id == hospitalId);

            if (!hospitalExists)
            {
                return CreateHospitalAdminResult.Failure(
                    CreateHospitalAdminErrorType.HospitalNotFound,
                    "No hospital exists with the specified id.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                var isDuplicateEmail = createResult.Errors.Any(e =>
                    e.Code == nameof(IdentityErrorDescriber.DuplicateEmail) ||
                    e.Code == nameof(IdentityErrorDescriber.DuplicateUserName));

                if (isDuplicateEmail)
                {
                    return CreateHospitalAdminResult.Failure(
                        CreateHospitalAdminErrorType.EmailAlreadyExists,
                        "A user with this email already exists.");
                }

                // Unexpected Identity failure (not a duplicate-email case) — do not
                // silently misclassify it. Let the project's GlobalExceptionHandler
                // translate this into the standard error response.
                var combinedErrors = string.Join(" ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Hospital admin user creation failed: {combinedErrors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.HospitalAdmin);

            if (!roleResult.Succeeded)
            {
                var combinedErrors = string.Join(" ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Hospital admin role assignment failed: {combinedErrors}");
            }

            var now = DateTime.UtcNow;

            var hospitalUser = new HospitalUser
            {
                Id = Guid.NewGuid(),
                HospitalId = hospitalId,
                UserId = user.Id,
                CreatedAt = now,
                UpdatedAt = now
            };

            _dbContext.HospitalUsers.Add(hospitalUser);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return CreateHospitalAdminResult.Success(new HospitalAdminResponse
            {
                HospitalUserId = hospitalUser.Id,
                HospitalId = hospitalUser.HospitalId,
                UserId = user.Id,
                Email = user.Email!,
                CreatedAt = hospitalUser.CreatedAt
            });
        }
    }
}