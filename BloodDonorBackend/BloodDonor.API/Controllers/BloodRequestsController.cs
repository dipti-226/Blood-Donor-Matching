using System.Security.Claims;
using BloodDonor.Application.BloodRequests;
using BloodDonor.Domain.Constants;
using BloodDonor.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/blood-requests")]
    [Authorize(Roles = Roles.HospitalAdmin + "," + Roles.HospitalStaff)]
    public class BloodRequestsController : ControllerBase
    {
        private readonly IBloodRequestService _bloodRequestService;
        private readonly IBloodRequestQueryService _bloodRequestQueryService;

        public BloodRequestsController(IBloodRequestService bloodRequestService,IBloodRequestQueryService bloodRequestQueryService)
        {
            _bloodRequestService = bloodRequestService;
            _bloodRequestQueryService = bloodRequestQueryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBloodRequestRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _bloodRequestService.CreateAsync(userId, request);

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Response!.Id }, result.Response);
            }

            return ToProblem(result, "Blood request creation failed.");
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] BloodRequestStatus? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _bloodRequestQueryService.GetListAsync(userId, status ?? BloodRequestStatus.Open);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Blood request access denied.",
                Detail = result.ErrorMessage
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _bloodRequestQueryService.GetByIdAsync(userId, id);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return ToProblem(result, "Blood request lookup failed.");
        }

        [HttpPost("{id:guid}/fulfill")]
        public async Task<IActionResult> Fulfill(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _bloodRequestService.FulfillAsync(userId, id);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return ToProblem(result, "Blood request fulfillment failed.");
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBloodRequestRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _bloodRequestService.CancelAsync(userId, id, request);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return ToProblem(result, "Blood request cancellation failed.");
        }

        private IActionResult ToProblem(BloodRequestResult result, string title)
        {
            var statusCode = result.ErrorType switch
            {
                BloodRequestErrorType.NotAuthorized => StatusCodes.Status403Forbidden,
                BloodRequestErrorType.RequestNotFound => StatusCodes.Status404NotFound,
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