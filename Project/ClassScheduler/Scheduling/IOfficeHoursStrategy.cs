using ClassScheduler.Models;

namespace ClassScheduler.Scheduling
{
    /*
    Class Name: IOfficeHoursStrategy
    Module Name: Office-Hours Scheduling
    Description: Interface allowing different office-hour generation algorithms to be swapped in.
    Method List:
    Generate(Professor p, ProfessorSchedule teaching, OfficeHoursPolicy policy) : List<OfficeHourSlot>
    */
    public interface IOfficeHoursStrategy
    {
        /*
        Class Name: IOfficeHoursStrategy
        Module Name: Office-Hours Scheduling
        Description: Produces office-hour slots for one professor.
        Variable List:
        Professor p - professor receiving office hours
        ProfessorSchedule teaching - used to avoid class conflicts
        OfficeHoursPolicy policy - placement rules
        List<OfficeHourSlot> (return) - generated slots
        */
        List<OfficeHourSlot> Generate(Professor p, ProfessorSchedule teaching, OfficeHoursPolicy policy);
    }
}