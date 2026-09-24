namespace BloodDonor.Application.Hospitals
{
    public interface ICreateHospitalAdminService
    {
        Task<CreateHospitalAdminResult> CreateHospitalAdminAsync(
            Guid hospitalId,
            CreateHospitalAdminRequest request);
    }
}