namespace BloodDonor.Application.Hospitals
{
    public class HospitalAdminListResult
    {
        public bool Succeeded { get; private set; }

        public HospitalAdminQueryErrorType ErrorType { get; private set; } = HospitalAdminQueryErrorType.None;

        public string? ErrorMessage { get; private set; }

        public IReadOnlyList<HospitalAdminResponse> Response { get; private set; } = Array.Empty<HospitalAdminResponse>();

        public static HospitalAdminListResult Success(IReadOnlyList<HospitalAdminResponse> response)
        {
            return new HospitalAdminListResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static HospitalAdminListResult Failure(HospitalAdminQueryErrorType errorType, string errorMessage)
        {
            return new HospitalAdminListResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}