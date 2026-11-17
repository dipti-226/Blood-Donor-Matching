using System.Security.Claims;
using BloodDonor.Application.Matching;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/blood-requests/{id:guid}/matches")]
    [Authorize(Roles = Roles.HospitalAdmin + "," + Roles.HospitalStaff)]
    public class BloodRequestMatchesController : ControllerBase
    {
        private readonly IDonorMatchingService _donorMatchingService;

        public BloodRequestMatchesController(IDonorMatchingService donorMatchingService)
        {
            _donorMatchingService = donorMatchingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _donorMatchingService.GetMatchesAsync(userId, id);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            var statusCode = result.ErrorType switch
            {
                DonorMatchErrorType.NotAuthorized => StatusCodes.Status403Forbidden,
                DonorMatchErrorType.RequestNotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status409Conflict
            };

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = "Donor matching failed.",
                Detail = result.ErrorMessage
            });
        }
    }
}