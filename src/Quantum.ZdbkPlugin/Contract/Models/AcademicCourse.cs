using System.ComponentModel;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed record AcademicCourse
{
    [Description("Course ID.")]
    public required string Id { get; set; }

    [Description("Course selection code.")]
    public required string Code { get; set; }

    [Description("Course name.")]
    public required string Name { get; set; }

    [Description("Number of credits.")]
    public required decimal Credits { get; set; }

    [Description("Weekly hours as reported by the upstream system.")]
    public required string WeekTime { get; set; }

    [Description("Department name.")]
    public required string Department { get; set; }

    [Description("Upstream course classification.")]
    public required string Property { get; set; }

    [Description("NotSelected, Selected, Passed, Failed, or Unknown.")]
    public required string Status { get; set; }
}
