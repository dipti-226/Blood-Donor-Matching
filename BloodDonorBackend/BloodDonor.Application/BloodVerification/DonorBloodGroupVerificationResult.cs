namespace BloodDonor.Application.BloodVerification
{
    public class DonorBloodGroupVerificationResult
    {
        public bool Succeeded { get; private set; }
        public DonorBloodGroupVerificationErrorType ErrorType { get; private set; } = DonorBloodGroupVerificationErrorType.None;
        public string? ErrorMessage { get; private set; }
        public DonorBloodGroupVerificationResponse? Response { get; private set; }

        public static DonorBloodGroupVerificationResult Success(DonorBloodGroupVerificationResponse response)
        {
            return new DonorBloodGroupVerificationResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static DonorBloodGroupVerificationResult Failure(DonorBloodGroupVerificationErrorType errorType, string errorMessage)
        {
            return new DonorBloodGroupVerificationResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}