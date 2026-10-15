using ClassScheduler.Models;

namespace ClassScheduler.Scheduling
{
    /*
    Class Name: TeachingScheduleBuilder
    Module Name: Teaching Schedule Generation
    Description: Assembles each professor's teaching timetable from assigned sections.
    Method List:
    BuildFor(Professor p, IEnumerable<CourseSection> sections) : ProfessorSchedule
    GetWeeklyTimeline(Professor p) : IEnumerable<MeetingTime>
    */
    public class TeachingScheduleBuilder
    {
        /*
        Class Name: TeachingScheduleBuilder
        Module Name: Teaching Schedule Generation
        Description: Builds a schedule from the sections assigned to the professor.
        Variable List:
        Professor p - target professor
        IEnumerable<CourseSection> sections - candidate sections
        ProfessorSchedule (return) - schedule with teaching populated
        */
        public ProfessorSchedule BuildFor(Professor p, IEnumerable<CourseSection> sections)
            => new() { Professor = p, Teaching = sections.Where(s => s.LecturerId == p.Id).ToList() };

        /*
        Class Name: TeachingScheduleBuilder
        Module Name: Teaching Schedule Generation
        Description: Returns all meetings ordered by day then start time.
        Variable List:
        ProfessorSchedule schedule - schedule whose meetings are collected (takes the schedule rather than Professor, since the professor object does not hold sections)
        IEnumerable<MeetingTime> (return) - sorted meetings
        */
        public IEnumerable<MeetingTime> GetWeeklyTimeline(ProfessorSchedule schedule)
            => schedule.Teaching.SelectMany(s => s.Meetings).OrderBy(m => m.Day).ThenBy(m => m.Start);
    }
}
