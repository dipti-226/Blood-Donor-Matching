using System.Security.Claims;
using BloodDonor.Application.BloodVerification;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/donor/blood-group")]
    [Authorize(Roles = Roles.Donor)]
    public class DonorBloodGroupController : ControllerBase
    {
        private readonly IDonorBloodGroupVerificationService _donorBloodGroupVerificationService;

        public DonorBloodGroupController(IDonorBloodGroupVerificationService donorBloodGroupVerificationService)
        {
            _donorBloodGroupVerificationService = donorBloodGroupVerificationService;
        }

        [HttpPost]
        public async Task<IActionResult> DeclareBloodGroup([FromBody] DeclareBloodGroupRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _donorBloodGroupVerificationService.DeclareBloodGroupAsync(userId, request);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Blood group declaration failed.",
                Detail = result.ErrorMessage
            });
        }
    }
}