namespace Quantum.OfficialPlugins.Zdbk;

internal sealed record AcademicQuery
{
    public required string Resource
    {
        get; set;
    }
    public string? Category
    {
        get; set;
    }
    public int Start { get; set; } = 1;
    public int End { get; set; } = 1;
    public string? Course
    {
        get; set;
    }
}
