using ClassSchedulerCommon.Common;
using ClassScheduler.Models;

namespace ClassScheduler.Validation
{
   
    /*
    Class Name: ScheduleValidator
    Module Name: Validation
    Description: Verifies final schedules against the rules.
    Method List:
    Validate(IEnumerable<ProfessorSchedule> schedules) : ValidationReport
    MeetsMinimumHours(ProfessorSchedule s) : bool (private)
    OfficeHoursOverlapClasses(ProfessorSchedule s) : bool (private)
    */
    public class ScheduleValidator
    {
        /*
        Class Name: ScheduleValidator
        Module Name: Validation
        Description: Checks every schedule and collects errors and warnings.
        Variable List:
        IEnumerable<ProfessorSchedule> schedules - schedules to check
        ValidationReport (return) - findings
        */
        public ValidationReport Validate(IEnumerable<ProfessorSchedule> schedules)
        {
            var report = new ValidationReport();
            foreach (var s in schedules)
            {
                if (s.Professor.Id.StartsWith("UNLISTED:", StringComparison.Ordinal)) continue;
                if (!MeetsMinimumHours(s))
                    report.Warnings.Add(new ValidationIssue
                    {
                        Code = "OH_BELOW_MIN",
                        ProfessorId = s.Professor.Id,
                        Message = $"{s.Professor.Name} has fewer office hours than required."
                    });
                if (OfficeHoursOverlapClasses(s))
                    report.Errors.Add(new ValidationIssue
                    {
                        Code = "OH_OVERLAP_CLASS",
                        ProfessorId = s.Professor.Id,
                        Message = $"{s.Professor.Name} has office hours overlapping a class."
                    });
            }
            return report;
        }

        /*
        Class Name: ScheduleValidator
        Module Name: Validation
        Description: Checks assigned hours against the weekly requirement.
        Variable List:
        ProfessorSchedule s - schedule to check
        bool (return) - true if minimum met
        */
        private bool MeetsMinimumHours(ProfessorSchedule s)
        {
            double hours = s.OfficeHours.Sum(o => (o.Block.End - o.Block.Start).TotalHours);
            return hours >= s.Professor.RequiredOfficeHoursPerWeek;
        }

        /*
        Class Name: ScheduleValidator
        Module Name: Validation
        Description: Checks whether any office-hour slot overlaps a class meeting.
        Variable List:
        ProfessorSchedule s - schedule to check
        bool (return) - true if an overlap exists
        */
        private bool OfficeHoursOverlapClasses(ProfessorSchedule s)
            => s.OfficeHours.Any(o => s.Teaching.SelectMany(t => t.Meetings).Any(m =>
                   m.Day == o.Block.Day && TimeMath.Overlaps(m.Start, m.End, o.Block.Start, o.Block.End)));
    }
}
