using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace NetCore.Models
{
  public class ModFileVersionNetInfo : IModFileVersionNetInfo
  {
    public string ProjectId { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VersionNumber { get; set; } = string.Empty;
    public string DownloadUri { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public int Size { get; set; }
    public required Type ServiceProvider { get; set; }
    public string LocalPath { get; set; } = string.Empty;
  }
}
