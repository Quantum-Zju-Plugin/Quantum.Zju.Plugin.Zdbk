using NOF.Contract;
using Quantum.Plugin.Abstraction;
using System.Text.Json;

namespace Quantum.ZdbkPlugin.Tests.TestDoubles;

internal sealed class FakeCas : IQuantumRequester
{
    public string? SessionId { get; set; } = "connection-a";
    public bool InvalidCallback
    {
        get; set;
    }
    public bool WaitForTicket
    {
        get; set;
    }
    public int TicketRequests
    {
        get; private set;
    }
    public TaskCompletionSource TicketStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public List<string> Capabilities { get; } = [];

    public async Task<Result<QuantumRequestReport<T>>> RequestAsync<T>(string rpcName, object payload, Context context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Capabilities.Add(rpcName);
        object response;
        if (rpcName == "zju.cas.account")
        {
            response = new
            {
                provider = "zju",
                protocol = "cas",
                isSignedIn = SessionId is not null,
                sessionId = SessionId
            };
        }
        else
        {
            Assert.Equal("zju.cas.service-ticket", rpcName);
            TicketRequests++;
            TicketStarted.TrySetResult();
            if (WaitForTicket)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            var request = JsonSerializer.SerializeToElement(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.Equal(SessionId, request.GetProperty("sessionId").GetString());
            var service = request.GetProperty("serviceUrl").GetString()!;
            response = new
            {
                sessionId = SessionId,
                serviceUrl = service,
                callbackUrl = InvalidCallback ? "https://attacker.invalid/?ticket=ST-fictional" : service + (service.Contains('?') ? "&" : "?") + "ticket=ST-fictional-" + TicketRequests,
                userAgent = "FictionalBrowser/1.0"
            };
        }
        var value = JsonSerializer.SerializeToElement(response).Deserialize<T>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return Result.Success(new QuantumRequestReport<T>([new("quantum.zju.plugin.cas", QuantumRequestStatus.Handled, value, null, null) { HasValue = true }], false));
    }
}
