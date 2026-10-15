using ClassSchedulerCommon.Common;
using ClassScheduler.Models;

namespace ClassScheduler.Scheduling
{
    /*
    Class Name: ConflictDetector
    Module Name: Teaching Schedule Generation
    Description: Finds professor and room scheduling conflicts. The in-person (7R1) and online (7R2) sections of a course meet together, so identical meetings of the same course by the same lecturer are one class and never conflict. Pseudo-rooms (Online, Dept, TBA) are skipped. Sections without readable times have no meetings and are not checked.
    Method List:
    FindProfessorConflicts(ProfessorSchedule s) : List<Conflict>
    FindRoomConflicts(IEnumerable<CourseSection> sections) : List<Conflict>
    IsSameClassMeeting(CourseSection a, MeetingTime ma, CourseSection b, MeetingTime mb) : bool (private)
    */
    public class ConflictDetector
    {
        /*
        Class Name: ConflictDetector
        Module Name: Teaching Schedule Generation
        Description: Compares every pair of a professor's meetings and reports overlaps, ignoring pairs that are the same class delivered in person and online.
        Variable List:
        ProfessorSchedule s - schedule to check
        List<Conflict> (return) - overlapping pairs
        */
        public List<Conflict> FindProfessorConflicts(ProfessorSchedule s)
        {
            var meetings = s.Teaching
                .SelectMany(sec => sec.Meetings.Select(m => (Section: sec, Meeting: m)))
                .ToList();
            var result = new List<Conflict>();

            for (int i = 0; i < meetings.Count; i++)
                for (int j = i + 1; j < meetings.Count; j++)
                {
                    var a = meetings[i];
                    var b = meetings[j];
                    if (!a.Meeting.OverlapsWith(b.Meeting)) continue;
                    if (IsSameClassMeeting(a.Section, a.Meeting, b.Section, b.Meeting)) continue;   // 7R1 + 7R2 pair

                    result.Add(new Conflict
                    {
                        ProfessorId = s.Professor.Id,
                        Description = $"{s.Professor.Name}: {a.Section.Course.Code} overlaps {b.Section.Course.Code} on {a.Meeting.Day}.",
                        First = a.Meeting,
                        Second = b.Meeting,
                    });
                }
            return result;
        }

        /*
        Class Name: ConflictDetector
        Module Name: Teaching Schedule Generation
        Description: Groups meetings by real room and reports overlapping pairs, ignoring the in-person/online pair of one class. Sections without a room, and pseudo-rooms, are skipped.
        Variable List:
        IEnumerable<CourseSection> sections - all sections across all professors
        List<Conflict> (return) - room double-bookings
        */
        public List<Conflict> FindRoomConflicts(IEnumerable<CourseSection> sections)
        {
            var result = new List<Conflict>();
            var byRoom = sections
                .Where(s => s.Room is not null && !s.Room.IsConflictExempt)   // skip Online / Dept / TBA
                .SelectMany(s => s.Meetings.Select(m => (Section: s, Meeting: m)))
                .GroupBy(x => x.Section.Room!.Key);

            foreach (var room in byRoom)
            {
                var list = room.ToList();
                for (int i = 0; i < list.Count; i++)
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        var a = list[i];
                        var b = list[j];
                        if (!a.Meeting.OverlapsWith(b.Meeting)) continue;
                        if (IsSameClassMeeting(a.Section, a.Meeting, b.Section, b.Meeting)) continue;

                        result.Add(new Conflict
                        {
                            Description = $"Room {a.Section.Room!.Building} {a.Section.Room!.Number}".Trim() +
                                          $" double-booked on {a.Meeting.Day}: {a.Section.Course.Code} and {b.Section.Course.Code}.",
                            First = a.Meeting,
                            Second = b.Meeting,
                        });
                    }
            }
            return result;
        }

        // Two meetings are the same class when the course code, lecturer, day and times all match.
        // That is how the in-person (7R1) and online (7R2) sections of a course appear in the syllabus.
        private static bool IsSameClassMeeting(CourseSection a, MeetingTime ma, CourseSection b, MeetingTime mb)
            => CourseCodeNormalizer.Normalize(a.Course.Code) == CourseCodeNormalizer.Normalize(b.Course.Code)
               && NameMatcher.IsLikelyMatch(a.LecturerId, b.LecturerId)
               && ma.Day == mb.Day && ma.Start == mb.Start && ma.End == mb.End;
    }
}