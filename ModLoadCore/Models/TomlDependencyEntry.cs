namespace ModLoadCore.Models;

public class TomlDependencyEntry
{
  public string ModId { get; set; } = string.Empty;
  public bool Mandatory { get; set; }
  public string VersionRange { get; set; } = string.Empty;
  public string Ordering { get; set; } = string.Empty;
  public string Side { get; set; } = string.Empty;
}