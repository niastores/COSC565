using ClassSchedulerCommon.Common;
using ClassScheduler.Models;

namespace ClassScheduler.Integration
{
    /*
    Class Name: DataIntegrator
    Module Name: Data Integration
    Description: Joins syllabus and faculty data into per-professor schedules and records inconsistencies. Lecturers missing from the faculty file get a placeholder professor so their courses still appear.
    Method List:
    Merge(List<CourseSection> sections, List<Professor> professors) : List<ProfessorSchedule>
    ResolveLecturer(CourseSection s, IDictionary<string, Professor> index) : Professor? (private)
    CheckConsistency() : List<ValidationIssue>
    */
    public class DataIntegrator
    {
        private readonly List<ValidationIssue> _issues = new();
        private List<Professor> _professors = new();   // working copy used for fuzzy name matching

        /*
        Class Name: DataIntegrator
        Module Name: Data Integration
        Description: Attaches each section to its lecturer and records issues for placeholder lecturers, fuzzy matches, courses missing from the professor's list, and professors with no sections.
        Variable List:
        List<CourseSection> sections - parsed syllabus sections
        List<Professor> professors - parsed faculty records (not modified)
        List<ProfessorSchedule> (return) - one schedule per professor, including placeholders
        */
        public List<ProfessorSchedule> Merge(List<CourseSection> sections, List<Professor> professors)
        {
            _issues.Clear();
            _professors = professors.ToList();

            // One schedule per professor, keyed by ID.
            var schedules = professors.ToDictionary(p => p.Id, p => new ProfessorSchedule { Professor = p });

            // Lookup used to match the lecturer text in the syllabus (by ID and by normalized name).
            var index = new Dictionary<string, Professor>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in professors)
            {
                index[p.Id] = p;
                index[NameNormalizer.Normalize(p.Name)] = p;
            }

            foreach (var section in sections)
            {
                var prof = ResolveLecturer(section, index);

                if (prof is null)
                {
                    // Keep the course: create (or reuse) a placeholder professor from the syllabus name.
                    string name = string.IsNullOrWhiteSpace(section.LecturerId) ? "Unassigned" : section.LecturerId.Trim();
                    string id = "UNLISTED:" + NameNormalizer.Normalize(name);

                    if (!schedules.TryGetValue(id, out var placeholder))
                    {
                        var newProf = new Professor { Id = id, Name = name };
                        placeholder = new ProfessorSchedule { Professor = newProf };
                        schedules[id] = placeholder;
                        _professors.Add(newProf);                      // later fuzzy matches can reuse it
                        index[NameNormalizer.Normalize(name)] = newProf;
                    }
                    _issues.Add(new ValidationIssue
                    {
                        Code = "LECTURER_NOT_FOUND",
                        Message = $"'{name}' is not in the faculty file; created a placeholder (section {section.Course.Code} {section.SectionId}).",
                    });
                    placeholder.Teaching.Add(section);
                    ApplyLecturerInfo(placeholder.Professor, section);
                    continue;
                }

                schedules[prof.Id].Teaching.Add(section);
                ApplyLecturerInfo(prof, section);

                // The faculty file lists courses taught; flag sections that disagree with it.
                // Only checked when the professor has a course list at all.
                string wanted = CourseCodeNormalizer.Normalize(section.Course.Code);
                if (prof.CourseCodes.Count > 0 &&
                    !prof.CourseCodes.Any(c => CourseCodeNormalizer.Normalize(c) == wanted))
                {
                    _issues.Add(new ValidationIssue
                    {
                        Code = "COURSE_NOT_LISTED",
                        ProfessorId = prof.Id,
                        Message = $"{prof.Name} is assigned {section.Course.Code} in the syllabus but it is not in the faculty file's course list.",
                    });
                }
            }

            foreach (var s in schedules.Values.Where(s => s.Teaching.Count == 0))
                _issues.Add(new ValidationIssue
                {
                    Code = "NO_SECTIONS",
                    ProfessorId = s.Professor.Id,
                    Message = $"{s.Professor.Name} has no sections in the syllabus.",
                });

            return schedules.Values.ToList();
        }

        /*
        Class Name: DataIntegrator
        Module Name: Data Integration
        Description: Matches a section's lecturer by ID, then normalized name, then surname plus first initial. A fuzzy match is accepted only if exactly one professor fits, and is recorded as a warning so it can be verified.
        Variable List:
        CourseSection s - section being resolved
        IDictionary<string, Professor> index - lookup by ID and normalized name
        Professor? (return) - match or null
        */
        private Professor? ResolveLecturer(CourseSection s, IDictionary<string, Professor> index)
        {
            string raw = s.LecturerId.Trim();
            if (index.TryGetValue(raw, out var byId)) return byId;
            if (index.TryGetValue(NameNormalizer.Normalize(raw), out var byName)) return byName;

            // Fuzzy fallback: "Bemley, J" vs "Jesse Bemley".
            var candidates = _professors.Where(p => NameMatcher.IsLikelyMatch(raw, p.Name)).ToList();
            if (candidates.Count == 1)
            {
                _issues.Add(new ValidationIssue
                {
                    Code = "LECTURER_FUZZY_MATCH",
                    ProfessorId = candidates[0].Id,
                    Message = $"'{raw}' matched to '{candidates[0].Name}' by surname/initial. Please verify.",
                });
                return candidates[0];
            }
            if (candidates.Count > 1)
                _issues.Add(new ValidationIssue
                {
                    Code = "LECTURER_AMBIGUOUS",
                    Message = $"'{raw}' could be any of: {string.Join(", ", candidates.Select(c => c.Name))}.",
                });
            return null;
        }

        /*
        Class Name: DataIntegrator
        Module Name: Data Integration
        Description: Returns the issues collected by the last Merge call.
        Variable List:
        List<ValidationIssue> (return) - collected issues
        */
        public List<ValidationIssue> CheckConsistency() => _issues;

        // Copies contact and office-hour information stated in the syllabus onto the professor.
        private static void ApplyLecturerInfo(Professor prof, CourseSection section)
        {
            if (prof.Email.Length == 0) prof.Email = section.LecturerEmail;

            foreach (var b in section.LecturerOfficeHours)
                if (!prof.AvailabilityTable.Any(a => a.Day == b.Day && a.Start == b.Start && a.End == b.End))
                    prof.AvailabilityTable.Add(b);
        }
    }

}