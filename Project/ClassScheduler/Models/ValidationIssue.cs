using System;
using System.Collections.Generic;
using System.Text;

namespace ClassScheduler.Models
{
    /*
    Class Name: ValidationIssue
    Module Name: Domain Model
    Description: One problem found during integration or validation.
    Method List:
    None (data container only)
    */
    public class ValidationIssue
    {
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
        public string? ProfessorId { get; set; }
    }
}
