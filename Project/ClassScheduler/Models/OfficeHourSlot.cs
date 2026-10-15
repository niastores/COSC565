namespace ClassScheduler.Models
{
    /*
    Class Name: OfficeHourSlot
    Module Name: Domain Model
    Description: Represents one scheduled office-hour period for a professor.
    Method List:
    None (data container only)
    */
    public class OfficeHourSlot
    {
        public Professor Professor { get; set; } = new();
        public TimeBlock Block { get; set; } = new();
        public Room? Location { get; set; }
        public string? CourseCode { get; set; }   // course the block is tagged with, e.g. "COSC 330"
    }
}