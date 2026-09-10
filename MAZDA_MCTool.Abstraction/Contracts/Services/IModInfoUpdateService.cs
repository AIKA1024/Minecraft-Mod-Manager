using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace MAZDA_MCTool.Abstraction.Contracts.Services;

public interface IModInfoUpdateService
{
  Task UpdateModInfoAsync(IReadOnlyCollection<IModInfo> modInfos);
}