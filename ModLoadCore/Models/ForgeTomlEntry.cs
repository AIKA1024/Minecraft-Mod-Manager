namespace ModLoadCore.Models;
public class ForgeTomlEntry
{
  public string ModId { get; set; } = string.Empty;
  public string Version { get; set; } = string.Empty;
  public string DisplayName { get; set; } = string.Empty;
  public string DisplayUrl { get; set; } = string.Empty;
  public List<string> Authors { get; set; } = [];
  public string Description { get; set; } = string.Empty;

  public string LogoFile { get; set; } = string.Empty;
}