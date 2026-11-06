using NOF.Contract;
using NOF.Domain;
using Quantum.Plugin.Abstraction;
using System.Net;
using System.Text.Json;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed class AcademicSession : IDisposable
{
    private const string ServiceUrl = "https://zdbk.zju.edu.cn/jwglxt/xtgl/login_ssologin.html";
    private readonly IQuantumRequester _rpc;
    private readonly Func<CookieContainer, HttpMessageHandler> _createHandler;
    private readonly SemaphoreSlim _gate = new(1);
    private readonly CancellationTokenSource _lifetime = new();
    private HttpClient? _client;
    private string? _sessionId;

    public AcademicSession(IQuantumRequester rpc) : this(rpc, cookies =>
        new HttpClientHandler { CookieContainer = cookies, AllowAutoRedirect = false })
    {
    }

    internal AcademicSession(IQuantumRequester rpc, Func<CookieContainer, HttpMessageHandler> createHandler)
    {
        _rpc = rpc;
        _createHandler = createHandler;
    }

    internal async Task<JsonElement> ReadAsync(AcademicQuery request, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
        linked.Token.ThrowIfCancellationRequested();
        AcademicReader.Validate(request);
        await _gate.WaitAsync(linked.Token);
        try
        {
            var account = await _rpc.RequestSingleAsync<CasAccount>("zju.cas.account", new
            {
            }, Context.Empty, linked.Token);
            if (!account.IsSuccess || account.Value is not { Provider: "zju", Protocol: "cas", IsSignedIn: true } state
                || string.IsNullOrWhiteSpace(state.SessionId))
            {
                Reset();
                throw new DomainException("zdbk_sign_in_required", "Sign in to the ZJU CAS account first.");
            }
            if (_sessionId != state.SessionId)
            {
                Reset();
            }
            for (var attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    if (_client is null)
                    {
                        _client = await OpenAsync(state.SessionId, linked.Token);
                        _sessionId = state.SessionId;
                    }
                    var payload = await AcademicReader.ReadAsync(_client, request, linked.Token);
                    var current = await _rpc.RequestSingleAsync<CasAccount>("zju.cas.account", new
                    {
                    }, Context.Empty, linked.Token);
                    if (!current.IsSuccess || current.Value is not { Provider: "zju", Protocol: "cas", IsSignedIn: true } active
                        || active.SessionId != state.SessionId)
                    {
                        Reset();
                        throw new DomainException("zdbk_account_changed", "The ZJU account changed during the request. Retry with the current account.");
                    }
                    return payload;
                }
                catch (SiteSessionExpiredException) when (attempt == 0)
                {
                    Reset();
                }
            }
            throw new InvalidOperationException("The academic session retry did not complete.");
        }
        catch (SiteSessionExpiredException)
        {
            Reset();
            throw new DomainException("zdbk_session_unavailable", "The academic session is unavailable. Check the account and network.");
        }
        catch (HttpRequestException)
        {
            throw new DomainException("zdbk_unavailable", "The academic system could not be reached.");
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<HttpClient> OpenAsync(string sessionId, CancellationToken cancellationToken)
    {
        var result = await _rpc.RequestSingleAsync<CasServiceTicket>("zju.cas.service-ticket", new
        {
            SessionId = sessionId,
            ServiceUrl
        }, Context.Empty, cancellationToken);
        if (!result.IsSuccess || result.Value is not { } ticket)
        {
            throw new DomainException(result.ErrorCode ?? "zdbk_authentication_failed", "CAS could not authenticate the academic service.");
        }
        if (ticket.SessionId != sessionId || ticket.ServiceUrl != ServiceUrl || !IsAllowed(ticket.CallbackUrl)
            || new Uri(ticket.CallbackUrl).GetLeftPart(UriPartial.Path) != ServiceUrl || string.IsNullOrWhiteSpace(ticket.UserAgent))
        {
            throw new DomainException("zdbk_invalid_callback", "The academic authentication callback is invalid.");
        }
        var client = new HttpClient(_createHandler(new CookieContainer()))
        {
            Timeout = TimeSpan.FromSeconds(30),
            MaxResponseContentBufferSize = 8 * 1024 * 1024
        };
        try
        {
            if (!client.DefaultRequestHeaders.UserAgent.TryParseAdd(ticket.UserAgent))
            {
                throw new DomainException("zdbk_invalid_callback", "The authentication browser identity is invalid.");
            }
            var url = new Uri(ticket.CallbackUrl);
            for (var hop = 0; hop < 10; hop++)
            {
                if (!IsAllowed(url.AbsoluteUri))
                {
                    throw new DomainException("zdbk_invalid_callback", "The academic service redirected outside its approved destination.");
                }
                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if ((int)response.StatusCode is >= 300 and < 400)
                {
                    url = response.Headers.Location is { } location ? new Uri(url, location) : throw new SiteSessionExpiredException();
                    continue;
                }
                response.EnsureSuccessStatusCode();
                return client;
            }
            throw new DomainException("zdbk_authentication_failed", "The academic authentication redirect limit was exceeded.");
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    private static bool IsAllowed(string? url) => Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme == "https" && uri.IsDefaultPort && uri.Host == "zdbk.zju.edu.cn"
        && uri.UserInfo.Length == 0 && uri.Fragment.Length == 0;

    private void Reset()
    {
        _client?.Dispose();
        _client = null;
        _sessionId = null;
    }

    public void Dispose()
    {
        _lifetime.Cancel();
        Reset();
        _lifetime.Dispose();
        _gate.Dispose();
    }
}
