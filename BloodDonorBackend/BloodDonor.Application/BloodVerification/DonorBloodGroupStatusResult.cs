namespace BloodDonor.Application.BloodVerification
{
    public class DonorBloodGroupStatusResult
    {
        public bool Succeeded { get; private set; }
        public BloodGroupVerificationQueryErrorType ErrorType { get; private set; } = BloodGroupVerificationQueryErrorType.None;
        public string? ErrorMessage { get; private set; }
        public DonorBloodGroupStatusResponse? Response { get; private set; }

        public static DonorBloodGroupStatusResult Success(DonorBloodGroupStatusResponse response)
        {
            return new DonorBloodGroupStatusResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static DonorBloodGroupStatusResult Failure(BloodGroupVerificationQueryErrorType errorType, string errorMessage)
        {
            return new DonorBloodGroupStatusResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}