namespace Quantum.OfficialPlugins.Zdbk;

internal sealed record CasAccount
{
    public string Provider { get; set; } = "";
    public string Protocol { get; set; } = "";
    public bool IsSignedIn { get; set; }
    public string? SessionId { get; set; }
}
