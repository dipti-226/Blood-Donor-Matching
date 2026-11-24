using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/hospitals/{hospitalId:guid}/admins")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class HospitalAdminsController : ControllerBase
    {
        private readonly ICreateHospitalAdminService _createHospitalAdminService;
        private readonly IHospitalAdminQueryService _hospitalAdminQueryService;

        public HospitalAdminsController(
            ICreateHospitalAdminService createHospitalAdminService,
            IHospitalAdminQueryService hospitalAdminQueryService)
        {
            _createHospitalAdminService = createHospitalAdminService;
            _hospitalAdminQueryService = hospitalAdminQueryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHospitalAdmins(Guid hospitalId)
        {
            var result = await _hospitalAdminQueryService.GetAdminsAsync(hospitalId);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Hospital admins not found.",
                Detail = result.ErrorMessage
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateHospitalAdmin(Guid hospitalId, [FromBody] CreateHospitalAdminRequest request)
        {
            var result = await _createHospitalAdminService.CreateHospitalAdminAsync(hospitalId, request);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status201Created, result.Response);
            }

            return result.ErrorType switch
            {
                CreateHospitalAdminErrorType.HospitalNotFound => NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Hospital admin creation failed.",
                    Detail = result.ErrorMessage
                }),
                CreateHospitalAdminErrorType.EmailAlreadyExists => Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Hospital admin creation failed.",
                    Detail = result.ErrorMessage
                }),
                CreateHospitalAdminErrorType.IdentityCreationFailed => BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Hospital admin creation failed.",
                    Detail = result.ErrorMessage
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Hospital admin creation failed."
                })
            };
        }
    }
}