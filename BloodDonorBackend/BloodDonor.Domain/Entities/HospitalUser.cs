namespace BloodDonor.Domain.Entities
{
    public class HospitalUser
    {
        public Guid Id { get; set; }
        public Guid HospitalId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}