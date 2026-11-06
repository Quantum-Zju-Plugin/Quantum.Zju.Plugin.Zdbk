using System.ComponentModel;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed record AcademicCourseRequest
{
    [Description("Course code for sections, or course ID for the introduction.")]
    public required string Course { get; set; }
}
