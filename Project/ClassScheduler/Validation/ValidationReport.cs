using ClassScheduler.Models;

namespace ClassScheduler.Validation
{
    /*
    Class Name: ValidationReport
    Module Name: Validation
    Description: Errors and warnings produced by validation.
    Method List:
    None (data container only)
    */
    public class ValidationReport
    {
        public List<ValidationIssue> Errors { get; set; } = new();
        public List<ValidationIssue> Warnings { get; set; } = new();
    }
}