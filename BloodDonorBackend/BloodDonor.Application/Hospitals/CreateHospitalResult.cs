namespace BloodDonor.Application.Hospitals
{
    public class CreateHospitalResult
    {
        public bool Succeeded { get; private set; }
        public CreateHospitalErrorType ErrorType { get; private set; } = CreateHospitalErrorType.None;
        public string? ErrorMessage { get; private set; }
        public HospitalResponse? Response { get; private set; }

        public static CreateHospitalResult Success(HospitalResponse response)
        {
            return new CreateHospitalResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static CreateHospitalResult Failure(CreateHospitalErrorType errorType, string errorMessage)
        {
            return new CreateHospitalResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}