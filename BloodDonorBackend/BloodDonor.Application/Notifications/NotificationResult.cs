namespace BloodDonor.Application.Notifications
{
    public class NotificationResult
    {
        public bool Succeeded { get; private set; }
        public NotificationErrorType ErrorType { get; private set; } = NotificationErrorType.None;
        public string? ErrorMessage { get; private set; }
        public NotificationResponse? Response { get; private set; }

        public static NotificationResult Success(NotificationResponse response)
        {
            return new NotificationResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static NotificationResult Failure(NotificationErrorType errorType, string errorMessage)
        {
            return new NotificationResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}