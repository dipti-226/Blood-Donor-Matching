using BloodDonor.Application.Auth;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace BloodDonor.Infrastructure.Identity
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRegistrationService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<RegisterUserResult> RegisterAsync(RegisterUserRequest request)
        {
            if (!PubliclyRegisterableRoles.Contains(request.RequestedRole))
            {
                return RegisterUserResult.Failure(
                    RegistrationErrorType.RoleNotAllowed,
                    new[] { "The requested role is not available for self-registration." });
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return RegisterUserResult.Failure(
                    RegistrationErrorType.EmailAlreadyExists,
                    new[] { "An account with this email already exists." });
            }

            var newUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var createResult = await _userManager.CreateAsync(newUser, request.Password);
            if (!createResult.Succeeded)
            {
                return RegisterUserResult.Failure(
                    RegistrationErrorType.IdentityCreationFailed,
                    createResult.Errors.Select(e => e.Description));
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, request.RequestedRole);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser);

                return RegisterUserResult.Failure(
                    RegistrationErrorType.IdentityCreationFailed,
                    roleResult.Errors.Select(e => e.Description));
            }

            var response = new RegisterUserResponse
            {
                UserId = newUser.Id,
                Email = newUser.Email!,
                Role = request.RequestedRole
            };

            return RegisterUserResult.Success(response);
        }
    }
}