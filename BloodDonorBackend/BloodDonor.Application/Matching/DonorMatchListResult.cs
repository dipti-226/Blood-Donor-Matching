namespace BloodDonor.Application.Matching
{
    public class DonorMatchListResult
    {
        public bool Succeeded { get; private set; }
        public DonorMatchErrorType ErrorType { get; private set; } = DonorMatchErrorType.None;
        public string? ErrorMessage { get; private set; }
        public DonorMatchListResponse? Response { get; private set; }

        public static DonorMatchListResult Success(DonorMatchListResponse response)
        {
            return new DonorMatchListResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static DonorMatchListResult Failure(DonorMatchErrorType errorType, string errorMessage)
        {
            return new DonorMatchListResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}