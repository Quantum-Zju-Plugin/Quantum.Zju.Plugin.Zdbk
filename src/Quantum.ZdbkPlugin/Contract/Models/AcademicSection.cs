using System.ComponentModel;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed record AcademicSection
{
    [Description("Section ID.")]
    public required string Id { get; set; }

    [Description("Course details supplied for selected sections.")]
    public required AcademicCourse? Course { get; set; }

    [Description("Instructor names.")]
    public required string[] Instructors { get; set; }

    [Description("Class times and locations.")]
    public required Meeting[] Meetings { get; set; }

    [Description("Exam schedule as reported by the upstream system.")]
    public required string Exam { get; set; }

    [Description("Term label.")]
    public required string Semesters { get; set; }

    [Description("Whether the section is international.")]
    public required bool IsInternational { get; set; }

    [Description("Teaching form.")]
    public required string TeachingForm { get; set; }

    [Description("Teaching method.")]
    public required string TeachingMethod { get; set; }

    [Description("Available seat count.")]
    public required int AvailableSeats { get; set; }

    [Description("Total seat count.")]
    public required int Capacity { get; set; }

    [Description("Waiting count in the current major.")]
    public required int MajorWaitingCount { get; set; }

    [Description("Total waiting count.")]
    public required int TotalWaitingCount { get; set; }
}
