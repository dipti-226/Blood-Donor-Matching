namespace BloodDonor.Application.BloodVerification
{
    public class ResubmitBloodGroupResult
    {
        public bool Succeeded { get; private set; }
        public ResubmitBloodGroupErrorType ErrorType { get; private set; } = ResubmitBloodGroupErrorType.None;
        public string? ErrorMessage { get; private set; }
        public DonorBloodGroupStatusResponse? Response { get; private set; }

        public static ResubmitBloodGroupResult Success(DonorBloodGroupStatusResponse response)
        {
            return new ResubmitBloodGroupResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static ResubmitBloodGroupResult Failure(ResubmitBloodGroupErrorType errorType, string errorMessage)
        {
            return new ResubmitBloodGroupResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}