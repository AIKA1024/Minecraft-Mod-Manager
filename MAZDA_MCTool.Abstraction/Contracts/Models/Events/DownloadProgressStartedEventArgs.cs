using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MAZDA_MCTool.Abstraction.Contracts.Models.Events;

public class DownloadProgressStartedEventArgs(string name) : EventArgs
{
  public string Name { get; set; } = name;

  // public IDownloadTaskInfo DownloadTaskInfo { get; } = downloadTaskInfo;
}