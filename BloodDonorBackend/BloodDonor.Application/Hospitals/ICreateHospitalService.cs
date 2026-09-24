namespace BloodDonor.Application.Hospitals
{
    public interface ICreateHospitalService
    {
        Task<CreateHospitalResult> CreateHospitalAsync(CreateHospitalRequest request);
    }
}