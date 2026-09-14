using BloodDonor.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace BloodDonor.Infrastructure.Identity
{
    public class LoginService : ILoginService
    {
        private const string InvalidCredentialsMessage = "Invalid email or password.";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<LoginUserResult> LoginAsync(LoginUserRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return LoginUserResult.Failure(LoginErrorType.InvalidCredentials, InvalidCredentialsMessage);
            }

            var checkResult = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!checkResult.Succeeded)
            {
                return LoginUserResult.Failure(LoginErrorType.InvalidCredentials, InvalidCredentialsMessage);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new LoginUserResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                Roles = roles.ToList()
            };

            return LoginUserResult.Success(response);
        }
    }
}