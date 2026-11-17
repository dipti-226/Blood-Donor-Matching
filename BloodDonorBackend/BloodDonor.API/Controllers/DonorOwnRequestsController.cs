using System.Security.Claims;
using BloodDonor.Application.DonorRequests;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/donor/requests")]
    [Authorize(Roles = Roles.Donor)]
    public class DonorOwnRequestsController : ControllerBase
    {
        private readonly IDonorOwnRequestService _service;

        public DonorOwnRequestsController(IDonorOwnRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.ListAsync(userId);

            return Ok(result.Response);
        }

        [HttpPost("{donorRequestId:guid}/accept")]
        public async Task<IActionResult> Accept(Guid donorRequestId, [FromBody] AcceptDonorRequestRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.AcceptAsync(userId, donorRequestId, request);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return ToProblem(result, "Accepting the donor request failed.");
        }

        [HttpPost("{donorRequestId:guid}/decline")]
        public async Task<IActionResult> Decline(Guid donorRequestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.DeclineAsync(userId, donorRequestId);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return ToProblem(result, "Declining the donor request failed.");
        }

        private IActionResult ToProblem<T>(DonorRequestResult<T> result, string title)
        {
            var statusCode = result.ErrorType == DonorRequestErrorType.DonorRequestNotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status409Conflict;

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = result.ErrorMessage
            });
        }
    }
}