namespace ClassScheduler.Models
{
    /*
    Class Name: ProfessorSchedule
    Module Name: Domain Model
    Description: Combines a professor's teaching sections and office-hour slots.
    Method List:
    None (data container only)
    */
    public class ProfessorSchedule
    {
        public Professor Professor { get; set; } = new();
        public List<CourseSection> Teaching { get; set; } = new();
        public List<OfficeHourSlot> OfficeHours { get; set; } = new();
    }
}