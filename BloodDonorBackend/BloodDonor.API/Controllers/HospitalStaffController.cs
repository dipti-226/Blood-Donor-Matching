using System.Security.Claims;
using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/hospitals/{hospitalId:guid}/staff")]
    [Authorize(Roles = Roles.HospitalAdmin)]
    public class HospitalStaffController : ControllerBase
    {
        private readonly ICreateHospitalStaffService _createHospitalStaffService;

        public HospitalStaffController(ICreateHospitalStaffService createHospitalStaffService)
        {
            _createHospitalStaffService = createHospitalStaffService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateHospitalStaff(
            Guid hospitalId,
            [FromBody] CreateHospitalStaffRequest request)
        {
            var callerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _createHospitalStaffService.CreateHospitalStaffAsync(
                hospitalId, callerUserId, request);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status201Created, result.Response);
            }

            return result.ErrorType switch
            {
                CreateHospitalStaffErrorType.HospitalNotFound => NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Hospital staff creation failed.",
                    Detail = result.ErrorMessage
                }),
                CreateHospitalStaffErrorType.NotAuthorizedForHospital => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Hospital staff creation failed.",
                        Detail = result.ErrorMessage
                    }),
                CreateHospitalStaffErrorType.EmailAlreadyExists => Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Hospital staff creation failed.",
                    Detail = result.ErrorMessage
                }),
                CreateHospitalStaffErrorType.IdentityCreationFailed => BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Hospital staff creation failed.",
                    Detail = result.ErrorMessage
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Hospital staff creation failed."
                })
            };
        }
    }
}