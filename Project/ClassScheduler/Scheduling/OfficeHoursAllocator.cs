using ClassSchedulerCommon.Common;
using ClassScheduler.Models;

namespace ClassScheduler.Scheduling
{
    /*
    Class Name: OfficeHoursAllocator
    Module Name: Office-Hours Scheduling
    Description: Applies a strategy to all professors and assigns locations. A slot tagged with a course is held in that course's classroom when the room is a real room and is free at that time.
    Method List:
    AllocateAll(List<ProfessorSchedule> schedules, IOfficeHoursStrategy strategy, OfficeHoursPolicy policy) : void
    TryAssignRoom(OfficeHourSlot slot, ProfessorSchedule schedule) : bool
    */
    public class OfficeHoursAllocator
    {
        // Every known room booking (classes and already-placed office hours).
        private readonly List<(string Room, DayOfWeek Day, TimeSpan Start, TimeSpan End)> _booked = new();

        /*
        Class Name: OfficeHoursAllocator
        Module Name: Office-Hours Scheduling
        Description: Registers all class bookings first, then generates and places office hours professor by professor.
        Variable List:
        List<ProfessorSchedule> schedules - schedules to populate
        IOfficeHoursStrategy strategy - slot-generation algorithm
        OfficeHoursPolicy policy - rules passed to the strategy
        */
        public void AllocateAll(List<ProfessorSchedule> schedules, IOfficeHoursStrategy strategy, OfficeHoursPolicy policy)
        {
            _booked.Clear();

            // Record every class booking in a real room so office hours never take a room that is in use.
            foreach (var sec in schedules.SelectMany(s => s.Teaching).Where(s => s.Room is not null && !s.Room.IsConflictExempt))
                foreach (var m in sec.Meetings)
                    _booked.Add((sec.Room!.Key, m.Day, m.Start, m.End));

            foreach (var schedule in schedules)
            {
                // Instructors found only in the schedule Excel have no professor record:
                // list their classes but do not generate office hours.
                if (schedule.Professor.Id.StartsWith("UNLISTED:", StringComparison.Ordinal))
                {
                    schedule.OfficeHours = new List<OfficeHourSlot>();
                    continue;
                }
                schedule.OfficeHours = strategy.Generate(schedule.Professor, schedule, policy);
                foreach (var slot in schedule.OfficeHours)
                    TryAssignRoom(slot, schedule);
            }
        }

        /*
        Class Name: OfficeHoursAllocator
        Module Name: Office-Hours Scheduling
        Description: Uses the room of the course the slot is tagged with, if that room is free for the whole slot. Untagged slots, online courses and pseudo-rooms stay without a location.
        Variable List:
        OfficeHourSlot slot - slot needing a location
        ProfessorSchedule schedule - the professor's schedule, used to find the tagged course's room
        bool (return) - true if a location was assigned
        */
        public bool TryAssignRoom(OfficeHourSlot slot, ProfessorSchedule schedule)
        {
            if (string.IsNullOrEmpty(slot.CourseCode)) return false;

            // Find the room of the tagged course.
            string wanted = CourseCodeNormalizer.Normalize(slot.CourseCode);
            //var room = schedule.Teaching
            //    .FirstOrDefault(t => CourseCodeNormalizer.Normalize(t.Course.Code) == wanted && t.Room is not null)?.Room;
            var room = schedule.Teaching
                .Where(t => CourseCodeNormalizer.Normalize(t.Course.Code) == wanted && t.Room is not null)
                .OrderBy(t => t.Room!.IsConflictExempt)   // real rooms first
                .FirstOrDefault()?.Room;
            if (room is null || room.IsConflictExempt) return false;   // don't hold office hours in "Online"/"Dept"

            // Reject if anything else holds that room at the same time.
            bool clash = _booked.Any(b => b.Room == room.Key && b.Day == slot.Block.Day &&
                                          TimeMath.Overlaps(b.Start, b.End, slot.Block.Start, slot.Block.End));
            if (clash) return false;

            slot.Location = room;
            _booked.Add((room.Key, slot.Block.Day, slot.Block.Start, slot.Block.End));
            return true;
        }
    }
}