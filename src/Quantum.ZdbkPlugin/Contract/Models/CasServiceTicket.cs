namespace Quantum.OfficialPlugins.Zdbk;

internal sealed record CasServiceTicket
{
    public required string SessionId { get; set; }
    public required string ServiceUrl { get; set; }
    public required string CallbackUrl { get; set; }
    public required string UserAgent { get; set; }
    public override string ToString() => "CAS service ticket (redacted)";
}
