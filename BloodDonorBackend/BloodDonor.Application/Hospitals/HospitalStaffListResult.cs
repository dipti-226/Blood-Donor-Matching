namespace BloodDonor.Application.Hospitals
{
    public class HospitalStaffListResult
    {
        public bool Succeeded { get; private set; }
        public HospitalStaffQueryErrorType ErrorType { get; private set; } = HospitalStaffQueryErrorType.None;
        public string? ErrorMessage { get; private set; }
        public IReadOnlyList<HospitalStaffResponse> Response { get; private set; } = Array.Empty<HospitalStaffResponse>();

        public static HospitalStaffListResult Success(IReadOnlyList<HospitalStaffResponse> response)
        {
            return new HospitalStaffListResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static HospitalStaffListResult Failure(HospitalStaffQueryErrorType errorType, string errorMessage)
        {
            return new HospitalStaffListResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}