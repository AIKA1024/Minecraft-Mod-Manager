using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace MAZDA_MCTool.Abstraction.Contracts.Services;

public interface IDialogService
{
  Task<object> ShowDialogAsync(IDialogRequest request);
}