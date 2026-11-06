using System.ComponentModel;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed record Meeting
{
    [Description("Time or week range.")]
    public required string Time { get; set; }

    [Description("Classroom or campus.")]
    public required string Location { get; set; }
}
