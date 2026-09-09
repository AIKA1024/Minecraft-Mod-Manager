using MAZDA_MCTool.Abstraction.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NetCore.Services;

public static class NetServicesExtensions
{
  public static void AddNetServices(this IServiceCollection services)
  {
    services.AddSingleton<INetService>(_ => new CurseForgeNetService());
    services.AddSingleton<INetService>(_ => new ModrinthNetService());
  }
}