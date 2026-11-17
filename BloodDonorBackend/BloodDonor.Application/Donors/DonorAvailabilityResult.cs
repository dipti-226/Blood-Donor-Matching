namespace BloodDonor.Application.Donors
{
    public class DonorAvailabilityResult
    {
        public bool Succeeded { get; private set; }
        public DonorAvailabilityErrorType ErrorType { get; private set; } = DonorAvailabilityErrorType.None;
        public string? ErrorMessage { get; private set; }
        public DonorAvailabilityResponse? Response { get; private set; }

        public static DonorAvailabilityResult Success(DonorAvailabilityResponse response)
        {
            return new DonorAvailabilityResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static DonorAvailabilityResult Failure(DonorAvailabilityErrorType errorType, string errorMessage)
        {
            return new DonorAvailabilityResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}