namespace BloodDonor.Application.DonorRequests
{
    public class DonorRequestResult<T>
    {
        public bool Succeeded { get; private set; }
        public DonorRequestErrorType ErrorType { get; private set; } = DonorRequestErrorType.None;
        public string? ErrorMessage { get; private set; }
        public T? Response { get; private set; }

        public static DonorRequestResult<T> Success(T response)
        {
            return new DonorRequestResult<T>
            {
                Succeeded = true,
                Response = response
            };
        }

        public static DonorRequestResult<T> Failure(DonorRequestErrorType errorType, string errorMessage)
        {
            return new DonorRequestResult<T>
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}