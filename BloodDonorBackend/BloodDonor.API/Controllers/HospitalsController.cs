using System.Security.Claims;
using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/hospitals")]
    public class HospitalsController : ControllerBase
    {
        private readonly ICreateHospitalService _createHospitalService;
        private readonly IHospitalQueryService _hospitalQueryService;
        private readonly IUpdateHospitalStatusService _updateHospitalStatusService;

        public HospitalsController(
            ICreateHospitalService createHospitalService,
            IHospitalQueryService hospitalQueryService,
            IUpdateHospitalStatusService updateHospitalStatusService)
        {
            _createHospitalService = createHospitalService;
            _hospitalQueryService = hospitalQueryService;
            _updateHospitalStatusService = updateHospitalStatusService;
        }

        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> CreateHospital([FromBody] CreateHospitalRequest request)
        {
            var result = await _createHospitalService.CreateHospitalAsync(request);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status201Created, result.Response);
            }

            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Hospital creation failed.",
                Detail = result.ErrorMessage
            });
        }

        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.HospitalAdmin)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var callerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isSuperAdmin = User.IsInRole(Roles.SuperAdmin);

            var result = await _hospitalQueryService.GetByIdAsync(id, callerUserId, isSuperAdmin);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            if (result.ErrorType == HospitalErrorType.NotAuthorizedForHospital)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Hospital access denied.",
                    Detail = result.ErrorMessage
                });
            }

            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Hospital not found.",
                Detail = result.ErrorMessage
            });
        }

        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var hospitals = await _hospitalQueryService.GetAllAsync();

            return Ok(hospitals);
        }

        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateHospitalStatusRequest request)
        {
            var result = await _updateHospitalStatusService.UpdateStatusAsync(id, request);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Hospital not found.",
                Detail = result.ErrorMessage
            });
        }
    }
}