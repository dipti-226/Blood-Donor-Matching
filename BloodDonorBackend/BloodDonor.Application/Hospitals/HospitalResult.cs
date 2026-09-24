namespace BloodDonor.Application.Hospitals
{
    public class HospitalResult
    {
        public bool Succeeded { get; private set; }
        public HospitalErrorType ErrorType { get; private set; } = HospitalErrorType.None;
        public string? ErrorMessage { get; private set; }
        public HospitalResponse? Response { get; private set; }

        public static HospitalResult Success(HospitalResponse response)
        {
            return new HospitalResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static HospitalResult Failure(HospitalErrorType errorType, string errorMessage)
        {
            return new HospitalResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}