using System.Security.Claims;
using BloodDonor.Application.DonorRequests;
using BloodDonor.Domain.Constants;
using BloodDonor.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/blood-requests/{bloodRequestId:guid}/donor-requests")]
    [Authorize(Roles = Roles.HospitalAdmin + "," + Roles.HospitalStaff)]
    public class DonorRequestsController : ControllerBase
    {
        private readonly IHospitalDonorRequestService _service;

        public DonorRequestsController(IHospitalDonorRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Send(Guid bloodRequestId, [FromBody] SendDonorRequestsRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.SendAsync(userId, bloodRequestId, request);

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetList), new { bloodRequestId }, result.Response);
            }

            return ToProblem(result, "Sending donor requests failed.");
        }

        [HttpGet]
        public async Task<IActionResult> GetList(Guid bloodRequestId, [FromQuery] DonorRequestStatus? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.ListAsync(userId, bloodRequestId, status);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return ToProblem(result, "Listing donor requests failed.");
        }

        [HttpPost("{donorRequestId:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid bloodRequestId, Guid donorRequestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.CancelAsync(userId, bloodRequestId, donorRequestId);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return ToProblem(result, "Cancelling the donor request failed.");
        }

        [HttpGet("{donorRequestId:guid}/contact")]
        public async Task<IActionResult> GetContact(Guid bloodRequestId, Guid donorRequestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.GetContactAsync(userId, bloodRequestId, donorRequestId);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return ToProblem(result, "Donor contact is not available.");
        }

        private IActionResult ToProblem<T>(DonorRequestResult<T> result, string title)
        {
            var statusCode = result.ErrorType switch
            {
                DonorRequestErrorType.NotAuthorized => StatusCodes.Status403Forbidden,
                DonorRequestErrorType.BloodRequestNotFound => StatusCodes.Status404NotFound,
                DonorRequestErrorType.DonorRequestNotFound => StatusCodes.Status404NotFound,
                DonorRequestErrorType.DonorsNotEligible => StatusCodes.Status400BadRequest,
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