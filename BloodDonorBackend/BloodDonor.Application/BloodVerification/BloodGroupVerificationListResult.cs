namespace BloodDonor.Application.BloodVerification
{
    public class BloodGroupVerificationListResult
    {
        public bool Succeeded { get; private set; }
        public BloodGroupVerificationQueryErrorType ErrorType { get; private set; } = BloodGroupVerificationQueryErrorType.None;
        public string? ErrorMessage { get; private set; }
        public IReadOnlyList<BloodGroupVerificationListItemResponse> Response { get; private set; } = Array.Empty<BloodGroupVerificationListItemResponse>();

        public static BloodGroupVerificationListResult Success(IReadOnlyList<BloodGroupVerificationListItemResponse> response)
        {
            return new BloodGroupVerificationListResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static BloodGroupVerificationListResult Failure(BloodGroupVerificationQueryErrorType errorType, string errorMessage)
        {
            return new BloodGroupVerificationListResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}