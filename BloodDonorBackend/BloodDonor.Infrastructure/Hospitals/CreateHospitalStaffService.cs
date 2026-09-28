using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Constants;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Hospitals
{
    public class CreateHospitalStaffService : ICreateHospitalStaffService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateHospitalStaffService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<CreateHospitalStaffResult> CreateHospitalStaffAsync(
            Guid hospitalId,
            string callerUserId,
            CreateHospitalStaffRequest request)
        {
            var hospitalExists = await _dbContext.Hospitals
                .AnyAsync(h => h.Id == hospitalId);

            if (!hospitalExists)
            {
                return CreateHospitalStaffResult.Failure(
                    CreateHospitalStaffErrorType.HospitalNotFound,
                    "No hospital exists with the specified id.");
            }

            var callerMembership = await _dbContext.HospitalUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(hu => hu.UserId == callerUserId);

            if (callerMembership == null || callerMembership.HospitalId != hospitalId)
            {
                return CreateHospitalStaffResult.Failure(
                    CreateHospitalStaffErrorType.NotAuthorizedForHospital,
                    "You are not authorized to manage staff for this hospital.");
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
                    return CreateHospitalStaffResult.Failure(
                        CreateHospitalStaffErrorType.EmailAlreadyExists,
                        "A user with this email already exists.");
                }

                // Identity rejected the user (for example a password-policy failure).
                // This is a client-correctable error, not a server fault. The transaction
                // is not committed, so disposing it rolls everything back.
                var identityErrors = string.Join(" ", createResult.Errors.Select(e => e.Description));

                return CreateHospitalStaffResult.Failure(
                    CreateHospitalStaffErrorType.IdentityCreationFailed,
                    identityErrors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.HospitalStaff);

            if (!roleResult.Succeeded)
            {
                var combinedErrors = string.Join(" ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Hospital staff role assignment failed: {combinedErrors}");
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

            return CreateHospitalStaffResult.Success(new HospitalStaffResponse
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