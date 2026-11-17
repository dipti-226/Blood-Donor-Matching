namespace BloodDonor.Application.BloodRequests
{
    public class BloodRequestResult
    {
        public bool Succeeded { get; private set; }
        public BloodRequestErrorType ErrorType { get; private set; } = BloodRequestErrorType.None;
        public string? ErrorMessage { get; private set; }
        public BloodRequestResponse? Response { get; private set; }

        public static BloodRequestResult Success(BloodRequestResponse response)
        {
            return new BloodRequestResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static BloodRequestResult Failure(BloodRequestErrorType errorType, string errorMessage)
        {
            return new BloodRequestResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}