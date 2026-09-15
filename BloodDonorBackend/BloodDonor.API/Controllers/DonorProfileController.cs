using System.Security.Claims;
using BloodDonor.Application.Donors;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/donor")]
    [Authorize(Roles = Roles.Donor)]
    public class DonorProfileController : ControllerBase
    {
        private readonly IDonorProfileService _donorProfileService;

        public DonorProfileController(IDonorProfileService donorProfileService)
        {
            _donorProfileService = donorProfileService;
        }

        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateDonorProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _donorProfileService.CreateProfileAsync(userId, request);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Donor profile creation failed.",
                Detail = result.ErrorMessage
            });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _donorProfileService.GetOwnProfileAsync(userId);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Donor profile not found.",
                Detail = result.ErrorMessage
            });
        }
    }
}