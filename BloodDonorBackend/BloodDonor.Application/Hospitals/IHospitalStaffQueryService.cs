namespace BloodDonor.Application.Hospitals
{
    public interface IHospitalStaffQueryService
    {
        Task<HospitalStaffListResult> GetStaffAsync(Guid hospitalId, string callerUserId);
    }
}