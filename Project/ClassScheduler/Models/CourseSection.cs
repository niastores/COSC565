namespace ClassScheduler.Models
{
    /*
    Class Name: CourseSection
    Module Name: Domain Model
    Description: One scheduled section with lecturer, meeting times, and room. Session tags the delivery mode: 7R1 = in person, 7R2 = online. Both sections of a course meet at the same time.
    Method List:
    None (data container only; IsOnlineSection is a computed property)
    */
    public class CourseSection
    {
        public Course Course { get; set; } = new();
        public string SectionId { get; set; } = "";
        public string LecturerId { get; set; } = "";
        public List<MeetingTime> Meetings { get; set; } = new();   // empty for asynchronous courses or unreadable times
        public Room? Room { get; set; }
        public string Session { get; set; } = "";                  // "7R1" = in person, "7R2" = online
        public List<string> Notes { get; set; } = new();           // repairs or problems found while reading this section

        // True for the online section of a course (session code ending in R2).
        public bool IsOnlineSection => Session.EndsWith("R2", StringComparison.OrdinalIgnoreCase);
        public string LecturerEmail { get; set; } = "";                         // from the syllabus header, if present
        public List<TimeBlock> LecturerOfficeHours { get; set; } = new();       // office hours stated in the syllabus
    }
}