namespace BloodDonor.Application.Hospitals
{
    public interface ICreateHospitalStaffService
    {
        Task<CreateHospitalStaffResult> CreateHospitalStaffAsync(
            Guid hospitalId,
            string callerUserId,
            CreateHospitalStaffRequest request);
    }
}