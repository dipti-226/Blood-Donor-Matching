using System.Security.Claims;
using BloodDonor.Application.Notifications;
using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize(Roles = Roles.Donor + "," + Roles.HospitalAdmin + "," + Roles.HospitalStaff)]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] bool unreadOnly = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var items = await _notificationService.ListAsync(userId, unreadOnly);

            return Ok(items);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new { unreadCount = count });
        }

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _notificationService.MarkReadAsync(userId, id);

            if (result.Succeeded)
            {
                return Ok(result.Response);
            }

            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Notification not found.",
                Detail = result.ErrorMessage
            });
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var markedCount = await _notificationService.MarkAllReadAsync(userId);

            return Ok(new { markedCount });
        }
    }
}