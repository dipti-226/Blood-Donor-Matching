using System.Security.Claims;
using BloodDonor.Application.BloodVerification;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/donor/blood-group")]
    [Authorize(Roles = Roles.Donor)]
    public class DonorBloodGroupController : ControllerBase
    {
        private readonly IDonorBloodGroupVerificationService _donorBloodGroupVerificationService;
        private readonly IBloodGroupVerificationQueryService _bloodGroupVerificationQueryService;
        private readonly IResubmitBloodGroupService _resubmitBloodGroupService;

        public DonorBloodGroupController(
            IDonorBloodGroupVerificationService donorBloodGroupVerificationService,
            IBloodGroupVerificationQueryService bloodGroupVerificationQueryService,
            IResubmitBloodGroupService resubmitBloodGroupService)
        {
            _donorBloodGroupVerificationService = donorBloodGroupVerificationService;
            _bloodGroupVerificationQueryService = bloodGroupVerificationQueryService;
            _resubmitBloodGroupService = resubmitBloodGroupService;
        }

        [HttpPost]
        public async Task<IActionResult> DeclareBloodGroup([FromBody] DeclareBloodGroupRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _donorBloodGroupVerificationService.DeclareBloodGroupAsync(userId, request);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Blood group declaration failed.",
                Detail = result.ErrorMessage
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetOwnBloodGroup()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _bloodGroupVerificationQueryService.GetOwnAsync(userId);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Blood group not found.",
                Detail = result.ErrorMessage
            });
        }

        [HttpPost("resubmit")]
        public async Task<IActionResult> ResubmitBloodGroup([FromBody] ResubmitBloodGroupRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _resubmitBloodGroupService.ResubmitAsync(userId, request);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            var statusCode = result.ErrorType == ResubmitBloodGroupErrorType.VerificationNotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status409Conflict;

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = "Blood group resubmission failed.",
                Detail = result.ErrorMessage
            });
        }
    }
}