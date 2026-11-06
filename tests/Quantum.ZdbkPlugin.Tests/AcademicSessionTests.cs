using NOF.Domain;
using Quantum.OfficialPlugins.Zdbk;
using Quantum.ZdbkPlugin.Tests.TestDoubles;
using System.Net;

namespace Quantum.ZdbkPlugin.Tests;

public sealed class AcademicSessionTests
{
    [Fact]
    public async Task OwnsTheAcademicSessionAndDropsItOnReconnectOrDisconnect()
    {
        var cas = new FakeCas();
        var site = new AcademicSite();
        using var session = new AcademicSession(cas, site.Create);
        Assert.Single((await session.ReadAsync(new()
        {
            Resource = "zdbk-selected"
        }, default)).GetProperty("rows").EnumerateArray());
        await session.ReadAsync(new()
        {
            Resource = "zdbk-selected"
        }, default);
        Assert.Equal(1, cas.TicketRequests);
        site.ExpireNext = true;
        await session.ReadAsync(new()
        {
            Resource = "zdbk-selected"
        }, default);
        Assert.Equal(2, cas.TicketRequests);
        cas.SessionId = "connection-b";
        await session.ReadAsync(new()
        {
            Resource = "zdbk-selected"
        }, default);
        Assert.Equal(3, cas.TicketRequests);
        Assert.Equal(2, site.DisposedHandlers);
        cas.SessionId = null;
        await Assert.ThrowsAsync<DomainException>(() => session.ReadAsync(new() { Resource = "zdbk-selected" }, default));
        Assert.Equal(3, site.DisposedHandlers);
        Assert.All(cas.Capabilities, name => Assert.StartsWith("zju.cas.", name));
    }

    [Fact]
    public async Task ValidatesBusinessInputBeforeRequestingAuthentication()
    {
        var cas = new FakeCas();
        var site = new AcademicSite();
        using var session = new AcademicSession(cas, site.Create);
        await Assert.ThrowsAsync<DomainException>(() => session.ReadAsync(new() { Resource = "zdbk-courses", Category = "MyCategory", Start = 2, End = 1 }, default));
        Assert.Empty(cas.Capabilities);
        Assert.Empty(site.Paths);
    }

    [Fact]
    public async Task RejectsUnapprovedTicketDestinationsBeforeOpeningAnAcademicClient()
    {
        var cas = new FakeCas { InvalidCallback = true };
        var site = new AcademicSite();
        using var session = new AcademicSession(cas, site.Create);
        await Assert.ThrowsAsync<DomainException>(() => session.ReadAsync(new() { Resource = "zdbk-selected" }, default));
        Assert.Empty(site.Paths);
    }

    [Fact]
    public async Task CancellationReleasesTheSessionGate()
    {
        var cas = new FakeCas { WaitForTicket = true };
        var site = new AcademicSite();
        using var session = new AcademicSession(cas, site.Create);
        using var token = new CancellationTokenSource();
        var read = session.ReadAsync(new()
        {
            Resource = "zdbk-selected"
        }, token.Token);
        await cas.TicketStarted.Task.WaitAsync(TimeSpan.FromSeconds(3));
        token.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read);
        cas.WaitForTicket = false;
        await session.ReadAsync(new()
        {
            Resource = "zdbk-selected"
        }, default).WaitAsync(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task AccountSwitchDuringTheReadDoesNotReturnPreviousAccountData()
    {
        var cas = new FakeCas();
        var site = new AcademicSite { OnRows = () => cas.SessionId = "connection-b" };
        using var session = new AcademicSession(cas, site.Create);
        await Assert.ThrowsAsync<DomainException>(() => session.ReadAsync(new() { Resource = "zdbk-selected" }, default));
        Assert.Equal(1, site.DisposedHandlers);
        site.OnRows = null;
        await session.ReadAsync(new()
        {
            Resource = "zdbk-selected"
        }, default);
        Assert.Equal(2, cas.TicketRequests);
    }

    private sealed class AcademicSite
    {
        public bool ExpireNext
        {
            get; set;
        }
        public Action? OnRows
        {
            get; set;
        }
        public int DisposedHandlers
        {
            get; set;
        }
        public List<string> Paths { get; } = [];
        public HttpMessageHandler Create(CookieContainer cookies)
        {
            Assert.Equal(0, cookies.Count);
            return new Endpoint(this);
        }
        private sealed class Endpoint(AcademicSite site) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Assert.Equal("https", request.RequestUri!.Scheme);
                Assert.Equal("zdbk.zju.edu.cn", request.RequestUri.Host);
                Assert.Equal("FictionalBrowser/1.0", request.Headers.UserAgent.ToString());
                site.Paths.Add(request.RequestUri.AbsolutePath);
                if (site.ExpireNext)
                {
                    site.ExpireNext = false;
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
                }
                if (request.RequestUri.AbsolutePath.EndsWith("Choosed.html", StringComparison.Ordinal))
                {
                    site.OnRows?.Invoke();
                }
                var body = request.RequestUri.AbsolutePath.Split('/').Last() switch
                {
                    "login_ssologin.html" => "signed in",
                    "zzxkghb_cxZzxkGhbIndex.html" => "<span id='xkxn'>2026</span><input id='xq' value='1'>",
                    "zzxkghb_cxZzxkGhbChoosed.html" => "[{\"kcdm\":\"fixture\"}]",
                    _ => throw new InvalidOperationException("Unexpected academic endpoint.")
                };
                var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
                return Task.FromResult(response);
            }
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    site.DisposedHandlers++;
                }
                base.Dispose(disposing);
            }
        }
    }
}
