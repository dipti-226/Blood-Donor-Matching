using System.Security.Claims;
using BloodDonor.Application.Donors;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/donor/availability")]
    [Authorize(Roles = Roles.Donor)]
    public class DonorAvailabilityController : ControllerBase
    {
        private readonly IDonorAvailabilityService _donorAvailabilityService;

        public DonorAvailabilityController(IDonorAvailabilityService donorAvailabilityService)
        {
            _donorAvailabilityService = donorAvailabilityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailability()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _donorAvailabilityService.GetAsync(userId);

            return ToActionResult(result);
        }

        [HttpPut]
        public async Task<IActionResult> SetAvailability([FromBody] SetDonorAvailabilityRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _donorAvailabilityService.SetAsync(userId, request);

            return ToActionResult(result);
        }

        private IActionResult ToActionResult(DonorAvailabilityResult result)
        {
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