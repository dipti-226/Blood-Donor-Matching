using BloodDonor.Application.Compatibility;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/blood-compatibility")]
    [Authorize(Roles = Roles.HospitalAdmin + "," + Roles.HospitalStaff)]
    public class BloodCompatibilityController : ControllerBase
    {
        private readonly IBloodCompatibilityService _compatibilityService;

        public BloodCompatibilityController(IBloodCompatibilityService compatibilityService)
        {
            _compatibilityService = compatibilityService;
        }

        [HttpGet]
        public IActionResult GetCompatibleDonorGroups([FromQuery] BloodCompatibilityRequest request)
        {
            var recipient = request.RecipientBloodGroup!.Value;
            var component = request.Component!.Value;

            var donorGroups = _compatibilityService.GetCompatibleDonorBloodGroups(recipient, component);

            return Ok(new BloodCompatibilityResponse
            {
                RecipientBloodGroup = recipient,
                Component = component,
                CompatibleDonorBloodGroups = donorGroups
            });
        }
    }
}