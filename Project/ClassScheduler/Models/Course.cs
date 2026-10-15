namespace ClassScheduler.Models
{
    /*
    Class Name: Course
    Module Name: Domain Model
    Description: Represents a course catalog entry.
    Method List:
    None (data container only)
    */
    public class Course
    {
        public string Code { get; set; } = "";
        public string Title { get; set; } = "";
        public int Credits { get; set; }
        public string Department { get; set; } = "";
    }

}
