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

        public HospitalsController(ICreateHospitalService createHospitalService)
        {
            _createHospitalService = createHospitalService;
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
    }
}