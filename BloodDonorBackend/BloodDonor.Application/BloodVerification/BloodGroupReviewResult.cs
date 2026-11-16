namespace BloodDonor.Application.BloodVerification
{
    public class BloodGroupReviewResult
    {
        public bool Succeeded { get; private set; }
        public BloodGroupReviewErrorType ErrorType { get; private set; } = BloodGroupReviewErrorType.None;
        public string? ErrorMessage { get; private set; }
        public BloodGroupReviewResponse? Response { get; private set; }

        public static BloodGroupReviewResult Success(BloodGroupReviewResponse response)
        {
            return new BloodGroupReviewResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static BloodGroupReviewResult Failure(BloodGroupReviewErrorType errorType, string errorMessage)
        {
            return new BloodGroupReviewResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}