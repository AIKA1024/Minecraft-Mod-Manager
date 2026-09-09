using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MAZDA_MCTool.Abstraction.Contracts.Models;

public interface IModFileVersionNetInfo
{
  public string ProjectId { get; set; }
  public string Slug { get; set; }
  public string Title { get; set; }
  public string Description { get; set; }
  public string VersionNumber { get; set; }
  public string DownloadUri { get; set; }
  public string IconUrl { get; set; }
  public int Size { get; set; }
  public Type ServiceProvider { get; set; }
  //todo 破坏了单一职责原则
  public string LocalPath { get; set; }
}