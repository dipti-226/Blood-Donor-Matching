namespace BloodDonor.Application.Hospitals
{
    public class CreateHospitalStaffResult
    {
        public bool Succeeded { get; private set; }
        public CreateHospitalStaffErrorType ErrorType { get; private set; } = CreateHospitalStaffErrorType.None;
        public string? ErrorMessage { get; private set; }
        public HospitalStaffResponse? Response { get; private set; }

        public static CreateHospitalStaffResult Success(HospitalStaffResponse response)
        {
            return new CreateHospitalStaffResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static CreateHospitalStaffResult Failure(CreateHospitalStaffErrorType errorType, string errorMessage)
        {
            return new CreateHospitalStaffResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}