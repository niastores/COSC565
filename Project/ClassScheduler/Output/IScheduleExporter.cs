using ClassScheduler.Models;

namespace ClassScheduler.Output
{
    /*
    Class Name: IScheduleExporter
    Module Name: Output
    Description: Interface for all schedule export formats.
    Method List:
    Export(IEnumerable<ProfessorSchedule> s, string path) : void
    */
    public interface IScheduleExporter
    {
        /*
        Class Name: IScheduleExporter
        Module Name: Output
        Description: Contract for writing schedules to a file in a specific format.
        Variable List:
        IEnumerable<ProfessorSchedule> s - schedules to export
        string path - output file location
        */
        public void Export(IEnumerable<ProfessorSchedule> s, string path)
            => throw new NotImplementedException();
    }
}