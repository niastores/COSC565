using System;
using System.Collections.Generic;
using System.Text;

namespace ClassScheduler.Models
{
    /*
    Class Name: Conflict
    Module Name: Domain Model
    Description: Describes two items that overlap in time or space.
    Method List:
    None (data container only)
    */
    public class Conflict
    {
        public string Description { get; set; } = "";
        public string ProfessorId { get; set; } = "";
        public MeetingTime? First { get; set; }
        public MeetingTime? Second { get; set; }
    }
}
