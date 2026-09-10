namespace MAZDA_MCTool.Abstraction.Contracts.Models.Events;

public class DownloadProgressChangedEventArgs(string name,double process) : EventArgs
{
  public string Name { get; set; } = name;
  public double Progress { get; set; } = process;
}