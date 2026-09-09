namespace ModLoadCore.Models;

// 暂时只需要一个mods的信息 属性名和toml必须一致
public class TomlRootModel
{
  public string ModLoader { get; set; } = string.Empty;
  public string LoaderVersion { get; set; } = string.Empty;
  public string License { get; set; } = string.Empty;
  public string IssueTrackerUrl { get; set; } = string.Empty;

  // 数组表（注意是 List）
  public List<ForgeTomlEntry> Mods { get; set; } = [];

  // 依赖项（嵌套数组表）
  //public List<TomlDependencyEntry> Dependencies { get; set; } = new();
  //他们的toml格式不通一，无法统一解析，反正应该用不上，先不理
}