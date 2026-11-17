using System.Linq.Expressions;
using BloodDonor.Application.Notifications;
using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Notifications
{
    public class NotificationService : INotificationService
    {
        private const int MaxListed = 50;

        private static readonly Expression<Func<Notification, NotificationResponse>> ToResponse =
            n => new NotificationResponse
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                BloodRequestId = n.BloodRequestId,
                DonorRequestId = n.DonorRequestId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            };

        private readonly ApplicationDbContext _dbContext;

        public NotificationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<NotificationResponse>> ListAsync(string userId, bool unreadOnly)
        {
            var query = _dbContext.Notifications
                .AsNoTracking()
                .Where(n => n.RecipientUserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(MaxListed)
                .Select(ToResponse)
                .ToListAsync();
        }

        public Task<int> GetUnreadCountAsync(string userId)
        {
            return _dbContext.Notifications
                .AsNoTracking()
                .CountAsync(n => n.RecipientUserId == userId && !n.IsRead);
        }

        public async Task<NotificationResult> MarkReadAsync(string userId, Guid notificationId)
        {
            var now = DateTime.UtcNow;
            DateTime? readAt = now;

            // Only the owner's unread notification is changed. Marking an already-read one is a harmless no-op.
            await _dbContext.Notifications
                .Where(n => n.Id == notificationId && n.RecipientUserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, readAt));

            // Someone else's notification is reported as not found.
            var response = await _dbContext.Notifications
                .AsNoTracking()
                .Where(n => n.Id == notificationId && n.RecipientUserId == userId)
                .Select(ToResponse)
                .FirstOrDefaultAsync();

            if (response == null)
            {
                return NotificationResult.Failure(
                    NotificationErrorType.NotificationNotFound,
                    "No notification exists with the specified id.");
            }

            return NotificationResult.Success(response);
        }

        public Task<int> MarkAllReadAsync(string userId)
        {
            var now = DateTime.UtcNow;
            DateTime? readAt = now;

            return _dbContext.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, readAt));
        }
    }
}