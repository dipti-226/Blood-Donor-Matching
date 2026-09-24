namespace BloodDonor.Application.Hospitals
{
    public class CreateHospitalAdminResult
    {
        public bool Succeeded { get; private set; }
        public CreateHospitalAdminErrorType ErrorType { get; private set; } = CreateHospitalAdminErrorType.None;
        public string? ErrorMessage { get; private set; }
        public HospitalAdminResponse? Response { get; private set; }

        public static CreateHospitalAdminResult Success(HospitalAdminResponse response)
        {
            return new CreateHospitalAdminResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static CreateHospitalAdminResult Failure(CreateHospitalAdminErrorType errorType, string errorMessage)
        {
            return new CreateHospitalAdminResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}