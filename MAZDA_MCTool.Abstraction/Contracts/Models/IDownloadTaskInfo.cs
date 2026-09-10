namespace MAZDA_MCTool.Abstraction.Contracts.Models;

public interface IDownloadTaskInfo
{
  string Name { get; set; }
  double Progress { get; set; }
}