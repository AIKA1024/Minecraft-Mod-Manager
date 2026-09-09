using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace MAZDA_MCTool.Abstraction.Contracts.Services;

public interface IModHashListGenerator
{
  Task CreateModHashListJsonAsync(ICollection<IModInfo> modeInfos);

  Task ApplyModHashListJsonAsync(string jsonFilePath);
}