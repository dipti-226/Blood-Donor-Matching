using BloodDonor.Application.Hospitals;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/hospitals")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class HospitalsController : ControllerBase
    {
        private readonly ICreateHospitalService _createHospitalService;
        private readonly IHospitalQueryService _hospitalQueryService;

        public HospitalsController(
            ICreateHospitalService createHospitalService,
            IHospitalQueryService hospitalQueryService)
        {
            _createHospitalService = createHospitalService;
            _hospitalQueryService = hospitalQueryService;
        }

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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _hospitalQueryService.GetByIdAsync(id);

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var hospitals = await _hospitalQueryService.GetAllAsync();

            return Ok(hospitals);
        }
    }
}