namespace BloodDonor.Application.BloodRequests
{
    public class BloodRequestListResult
    {
        public bool Succeeded { get; private set; }
        public BloodRequestErrorType ErrorType { get; private set; } = BloodRequestErrorType.None;
        public string? ErrorMessage { get; private set; }
        public IReadOnlyList<BloodRequestResponse> Response { get; private set; } = Array.Empty<BloodRequestResponse>();

        public static BloodRequestListResult Success(IReadOnlyList<BloodRequestResponse> response)
        {
            return new BloodRequestListResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static BloodRequestListResult Failure(BloodRequestErrorType errorType, string errorMessage)
        {
            return new BloodRequestListResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}