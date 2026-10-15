using ClassScheduler.Input;
using ClassScheduler.Models;
using ClassScheduler.Output;
using ClassScheduler.Integration;
using ClassScheduler.Scheduling;
using ClassScheduler.Validation;

namespace ClassScheduler.App
{
    /*
    Class Name: ScheduleGenerationService
    Module Name: Orchestration
    Description: Runs the full pipeline from parsing to export.
    Method List:
    ScheduleGenerationService(ILogger log, AppConfig config) : constructor
    Run(CommandLineArgs args) : ValidationReport
    */
    public class ScheduleGenerationService
    {
        private readonly ILogger _log;
        private readonly AppConfig _config;

        /*
        Class Name: ScheduleGenerationService
        Module Name: Orchestration
        Description: Stores dependencies.
        Variable List:
        ILogger log - logger
        AppConfig config - settings
        */
        public ScheduleGenerationService(ILogger log, AppConfig config)
        {
            _log = log;
            _config = config;
        }

        /*
        Class Name: ScheduleGenerationService
        Module Name: Orchestration
        Description: Parse, merge, detect conflicts, allocate office hours, validate, export (CSV, HTML, Excel, Word) and write a review-notes file listing every repaired or flagged section.
        Variable List:
        CommandLineArgs args - validated input paths
        ValidationReport (return) - final validation results
        */
        public ValidationReport Run(CommandLineArgs args)
        {
            // 1. Parse both inputs. The factory picks the parser from each file's extension.
            var syllabus = FileParserFactory.Create<List<CourseSection>>(Path.GetExtension(args.SyllabusPath)).Parse(args.SyllabusPath);
            var faculty = FileParserFactory.Create<List<Professor>>(Path.GetExtension(args.FacultyPath)).Parse(args.FacultyPath);

            // Warnings mean a row was kept (repaired or flagged); errors mean data was lost.
            foreach (var e in syllabus.Errors.Concat(faculty.Errors))
            {
                string msg = $"Parse issue (line {e.Line}, {e.Field}): {e.Message}";
                if (e.Severity == ParseSeverity.Warning) _log.Warn("Repaired/flagged: " + msg);
                else _log.Error(msg);
            }

            // Nothing usable means there is nothing to schedule.
            if (syllabus.Data.Count == 0) throw new InvalidDataException("No class sections could be read from the syllabus file.");
            if (faculty.Data.Count == 0) throw new InvalidDataException("No professors could be read from the faculty file.");

            // 2. Join the two files.
            var integrator = new DataIntegrator();
            var schedules = integrator.Merge(syllabus.Data, faculty.Data);
            foreach (var issue in integrator.CheckConsistency()) _log.Warn(issue.Message);

            // 3. Report teaching conflicts (reported, not fixed: class times come from the input).
            var detector = new ConflictDetector();
            foreach (var s in schedules)
                foreach (var c in detector.FindProfessorConflicts(s)) _log.Warn(c.Description);
            foreach (var c in detector.FindRoomConflicts(schedules.SelectMany(s => s.Teaching))) _log.Warn(c.Description);

            // 4. Generate office hours around the classes.
            new OfficeHoursAllocator().AllocateAll(schedules, new GreedyOfficeHoursStrategy(), _config.DefaultPolicy);

            // 5. Check the finished schedules.
            var report = new ScheduleValidator().Validate(schedules);
            foreach (var w in report.Warnings) _log.Warn(w.Message);
            foreach (var e in report.Errors) _log.Error(e.Message);

            // 6. Write outputs.
            Directory.CreateDirectory(_config.OutputDirectory);
            string Out(string file) => Path.Combine(_config.OutputDirectory, file);

            new CsvScheduleExporter().Export(schedules, Out("schedules.csv"));
            new HtmlScheduleExporter(_config.ScheduleNote).Export(schedules, Out("schedules.html"));
            new ExcelScheduleExporter().Export(schedules, Out("schedules.xlsx"));
            //new DocxScheduleExporter(_config.ScheduleNote).Export(schedules, Out("schedules.docx"));
            var docx = new DocxScheduleExporter(_config.ScheduleNote);
            foreach (var s in schedules)
            {
                string safe = string.Concat(s.Professor.Id.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
                docx.Export(new[] { s }, Out($"{safe}.docx"));
            }

            // Review list: every repaired or flagged section, one line each, plus integration issues.
            var review = schedules.SelectMany(s => s.Teaching)
                .Where(t => t.Notes.Count > 0)
                .SelectMany(t => t.Notes.Select(n => $"{t.Course.Code} {t.SectionId} ({t.LecturerId}): {n}"))
                .Concat(integrator.CheckConsistency().Select(i => $"[{i.Code}] {i.Message}"));
            File.WriteAllLines(Out("review-notes.txt"), review);

            _log.Info($"Done: {report.Errors.Count} errors, {report.Warnings.Count} warnings. Output in {Path.GetFullPath(_config.OutputDirectory)}");
            return report;
        }
    }
}
