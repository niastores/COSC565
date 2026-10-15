namespace ClassScheduler.Models
{
    /*
    Class Name: Professor
    Module Name: Domain Model
    Description: Represents a lecturer, including courses taught, availability, and office-hour requirements.
    Method List:
    None (data container only)
    */
    public class Professor
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Department { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> CourseCodes { get; set; } = new();
        public List<TimeBlock> AvailabilityTable { get; set; } = new();
        public int RequiredOfficeHoursPerWeek { get; set; }
    }
}