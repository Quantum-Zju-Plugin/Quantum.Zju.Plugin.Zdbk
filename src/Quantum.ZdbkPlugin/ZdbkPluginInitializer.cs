using Microsoft.Extensions.DependencyInjection;
using NOF.Abstraction;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed class ZdbkPluginInitializer : IAssemblyInitializer
{
    public static void Initialize(IServiceCollection services)
    {
        services.AddSingleton<AcademicSession>();
        services.AddRpcServer<ZdbkRpcService>();
        services.AddScoped<IZdbkRpcServiceClient, LocalZdbkRpcServiceClient>();
    }
}
