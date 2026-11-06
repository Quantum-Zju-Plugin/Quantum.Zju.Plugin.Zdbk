using System.ComponentModel;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed record ListCoursesRequest
{
    [Description("Academic course category, such as MyCategory, ElectiveAll, or MyMajor.")]
    public required string Category { get; set; }

    [Description("First page, starting at 1.")]
    public int Start { get; set; } = 1;

    [Description("Last page, inclusive.")]
    public int End { get; set; } = 1;
}
