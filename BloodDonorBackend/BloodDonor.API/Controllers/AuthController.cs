using BloodDonor.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly ILoginService _loginService;

        public AuthController(
            IUserRegistrationService userRegistrationService,
            ILoginService loginService)
        {
            _userRegistrationService = userRegistrationService;
            _loginService = loginService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var result = await _userRegistrationService.RegisterAsync(request);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return result.ErrorType switch
            {
                RegistrationErrorType.RoleNotAllowed => BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Registration failed.",
                    Detail = string.Join(" ", result.Errors)
                }),
                RegistrationErrorType.EmailAlreadyExists => Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Registration failed.",
                    Detail = string.Join(" ", result.Errors)
                }),
                RegistrationErrorType.IdentityCreationFailed => BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Registration failed.",
                    Detail = string.Join(" ", result.Errors)
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Registration failed."
                })
            };
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
        {
            var result = await _loginService.LoginAsync(request);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Login failed.",
                Detail = result.ErrorMessage
            });
        }
    }
}