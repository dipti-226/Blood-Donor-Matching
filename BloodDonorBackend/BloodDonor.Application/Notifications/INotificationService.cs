namespace BloodDonor.Application.Notifications
{
    public interface INotificationService
    {
        Task<IReadOnlyList<NotificationResponse>> ListAsync(string userId, bool unreadOnly);

        Task<int> GetUnreadCountAsync(string userId);

        Task<NotificationResult> MarkReadAsync(string userId, Guid notificationId);

        Task<int> MarkAllReadAsync(string userId);
    }
}