namespace MAZDA_MCTool.Abstraction.Contracts.Models.Events;

public class DownloadCompletedHandlerEventArgs(string name) : EventArgs
{
  public string Name { get; set; } = name;
}