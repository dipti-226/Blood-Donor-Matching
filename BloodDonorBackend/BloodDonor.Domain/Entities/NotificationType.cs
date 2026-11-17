namespace BloodDonor.Domain.Entities
{
    public enum NotificationType
    {
        DonorRequestReceived,
        DonorRequestAccepted,
        DonorRequestDeclined,
        DonorRequestCancelled,
        BloodRequestClosed
    }
}