namespace BloodDonor.Application.Donors
{
    public class DonorProfileResult
    {
        public bool Succeeded { get; private set; }
        public DonorProfileErrorType ErrorType { get; private set; } = DonorProfileErrorType.None;
        public string? ErrorMessage { get; private set; }
        public DonorProfileResponse? Response { get; private set; }

        public static DonorProfileResult Success(DonorProfileResponse response)
        {
            return new DonorProfileResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static DonorProfileResult Failure(DonorProfileErrorType errorType, string errorMessage)
        {
            return new DonorProfileResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}