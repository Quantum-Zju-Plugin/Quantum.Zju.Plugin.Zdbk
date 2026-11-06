using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Quantum.Plugin.Abstraction;
using Quantum.Plugins;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace Quantum.HostIntegration.Tests;

public sealed class AcademicSessionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HostEnforcesCasDependencyAndDispatchesAcademicReads(bool includeCas)
    {
        var root = Directory.CreateTempSubdirectory("quantum-zdbk-host-");
        var modules = Path.Combine(root.FullName, "Modules");
        var shadow = Path.Combine(root.FullName, "Shadow");
        var catalog = new PluginCatalog([]);
        using var services = new ServiceCollection().AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance).AddQuantumPluginEventBus().BuildServiceProvider();
        try
        {
            foreach (var name in includeCas ? new[] { "Zdbk", "Cas" } : new[] { "Zdbk" })
            {
                var destination = Directory.CreateDirectory(Path.Combine(modules, name == "Cas" ? "quantum.zju.plugin.cas" : "quantum.plugin.zdbk"));
                foreach (var file in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "PluginFixtures", name)))
                {
                    File.Copy(file, Path.Combine(destination.FullName, Path.GetFileName(file)));
                }
            }
            await using var manager = new PluginRuntimeManager(catalog, new PluginRuntimeOptions(modules, shadow, Path.Combine(root.FullName, "test.db")));
            await manager.InitializeAsync(services);
            if (!includeCas)
            {
                Assert.Null(catalog.FindPlugin("quantum.plugin.zdbk"));
                Assert.Contains(catalog.Failures, failure => failure.Reason.Contains("zju.cas.account", StringComparison.Ordinal));
                return;
            }
            Assert.Empty(catalog.Failures);
            using var router = new PluginRpcRouter(catalog);
            var context = new PluginRpcCallContext("test.caller", "test", new Dictionary<string, JsonElement>());
            async Task<QuantumRequestOutcome<JsonElement?>> CallAsync(string name, object payload)
            {
                var result = await router.RequestAsync(name, JsonSerializer.SerializeToElement(payload), context, default);
                Assert.True(result.IsSuccess, result.Message);
                return Assert.Single(result.Value!.Outcomes);
            }
            var discovery = await CallAsync(QuantumCapabilityCatalog.CapabilityName, new { });
            var capabilities = discovery.Value!.Value.GetProperty("capabilities").EnumerateArray().Select(x => x.GetProperty("name").GetString()).ToArray();
            Assert.Contains("zdbk.list-selected", capabilities);
            Assert.Contains("zdbk.list-courses", capabilities);
            Assert.DoesNotContain("zjuam.read-resource", capabilities);
            var anonymous = await CallAsync("zdbk.list-selected", new { });
            Assert.Equal("zdbk_sign_in_required", anonymous.ErrorCode);
            var identity = catalog.FindPlugin("quantum.zju.plugin.cas")!;
            var casType = identity.EntryAssembly!.GetType("Quantum.Zju.Plugin.Cas.ZjuCasSession", true)!;
            var casSession = identity.Services!.GetRequiredService(casType);
            var client = new HttpClient(new CasEndpoint());
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FictionalBrowser/1.0");
            casType.GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(casSession, client);
            casType.GetField("_sessionId", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(casSession, "fictional-connection");
            casType.GetField("_userAgent", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(casSession, "FictionalBrowser/1.0");
            var plugin = catalog.FindPlugin("quantum.plugin.zdbk")!;
            var type = plugin.EntryAssembly!.GetType("Quantum.OfficialPlugins.Zdbk.AcademicSession", true)!;
            type.GetField("_createHandler", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(plugin.Services!.GetRequiredService(type), (Func<CookieContainer, HttpMessageHandler>)(_ => new AcademicEndpoint()));
            var courses = await CallAsync("zdbk.list-courses", new { category = "MyCategory" });
            Assert.Equal(QuantumRequestStatus.Handled, courses.Status);
            Assert.Equal("Course", Assert.Single(courses.Value!.Value.EnumerateArray()).GetProperty("name").GetString());
            var selected = await CallAsync("zdbk.list-selected", new { });
            var section = Assert.Single(selected.Value!.Value.EnumerateArray());
            Assert.Equal("周一第1,2节", section.GetProperty("meetings")[0].GetProperty("time").GetString());
            Assert.Equal("Room", section.GetProperty("meetings")[0].GetProperty("location").GetString());
            Assert.Equal(3.5m, section.GetProperty("course").GetProperty("credits").GetDecimal());
            Assert.Equal(QuantumRequestStatus.Handled, (await CallAsync("zdbk.list-sections", new { course = "ABC" })).Status);
            Assert.Equal(QuantumRequestStatus.Handled, (await CallAsync("zdbk.list-graduation", new { })).Status);
            var introduction = await CallAsync("zdbk.get-introduction", new { course = "42" });
            Assert.Equal("Course description", introduction.Value!.Value.GetString());
            Assert.Equal("zdbk_invalid_pages", (await CallAsync("zdbk.list-courses", new { category = "MyCategory", start = 2, end = 1 })).ErrorCode);
            Assert.Equal(QuantumRequestStatus.Handled, (await CallAsync("zdbk.list-selected", new { })).Status);
            Assert.True((await manager.ReloadAsync("quantum.plugin.zdbk")).Succeeded);
            Assert.NotNull(catalog.FindPlugin("quantum.plugin.zdbk"));
        }
        finally
        {
            if (PluginShadowCopy.TryDelete(shadow)) { root.Delete(true); }
        }
    }

    private sealed class CasEndpoint : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("https", request.RequestUri!.Scheme);
            Assert.Equal("zjuam.zju.edu.cn", request.RequestUri.Host);
            var service = Uri.UnescapeDataString(request.RequestUri.Query["?service=".Length..]);
            var reply = new HttpResponseMessage(HttpStatusCode.Found);
            reply.Headers.Location = new Uri(service + "?ticket=ST-fictional");
            return Task.FromResult(reply);
        }
    }
    private sealed class AcademicEndpoint : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("https", request.RequestUri!.Scheme);
            Assert.Equal("zdbk.zju.edu.cn", request.RequestUri.Host);
            var path = request.RequestUri.AbsolutePath;
            var content = path.Split('/').Last() switch
            {
                "login_ssologin.html" => "Academic session established",
                "zzxkghb_cxZzxkGhbIndex.html" => "<span id='xkxn'>2026</span><input id='xq' value='1'><input id='nj' value='2025'><input id='zydm' value='A'>",
                "zzxkghb_cxZzxkGhbKcList.html" => """[{"kcdm":"42","xkkh":"ABC","kcmc":"Course","kcxx":"ignored~3.5~2"}]""",
                "zzxkghb_cxZzxkGhbJxbList.html" => """[{"xkkh":"ABC","rs":"4/30","sksj":"Monday","skdd":"Room"}]""",
                "zzxkghb_cxZzxkGhbChoosed.html" => """[{"kcdm":"42","xkkh":"ABC","kcmc":"Course","xf":"3.5","sksj":"周一第1,2节","skdd":"Room"}]""",
                "byshck_cxByshzsIndex.html" => """[{"KCDM":"42","JDMC":"Course","KCXF":"3.5","SFTG":"\u901a\u8fc7"}]""",
                "kcjjck_cxXkjjPage.html" => """<input name="xkjjHtml" value="Course description">""",
                _ => throw new InvalidOperationException(path)
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent(content)
            });
        }
    }

}
