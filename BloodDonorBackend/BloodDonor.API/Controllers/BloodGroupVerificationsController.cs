using System.Security.Claims;
using BloodDonor.Application.BloodVerification;
using BloodDonor.Domain.Constants;
using BloodDonor.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/blood-group-verifications")]
    [Authorize(Roles = Roles.HospitalAdmin + "," + Roles.HospitalStaff)]
    public class BloodGroupVerificationsController : ControllerBase
    {
        private readonly IReviewBloodGroupVerificationService _reviewService;
        private readonly IBloodGroupVerificationQueryService _queryService;

        public BloodGroupVerificationsController(IReviewBloodGroupVerificationService reviewService,IBloodGroupVerificationQueryService queryService)
        {
            _reviewService = reviewService;
            _queryService = queryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetForReview([FromQuery] VerificationStatus? status)
        {
            var reviewerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _queryService.GetForReviewAsync(
                reviewerUserId,
                status ?? VerificationStatus.Pending);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Blood group verification access denied.",
                Detail = result.ErrorMessage
            });
        }

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveBloodGroupRequest request)
        {
            var reviewerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _reviewService.ApproveAsync(id, reviewerUserId, request);

            return ToActionResult(result, "Blood group approval failed.");
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectBloodGroupRequest request)
        {
            var reviewerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _reviewService.RejectAsync(id, reviewerUserId, request);

            return ToActionResult(result, "Blood group rejection failed.");
        }

        private IActionResult ToActionResult(BloodGroupReviewResult result, string title)
        {
            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            var statusCode = result.ErrorType switch
            {
                BloodGroupReviewErrorType.NotAuthorizedToReview => StatusCodes.Status403Forbidden,
                BloodGroupReviewErrorType.VerificationNotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status409Conflict
            };

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = result.ErrorMessage
            });
        }
    }
}