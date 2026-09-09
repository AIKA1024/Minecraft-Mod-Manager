using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using NetCore.Extensions;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NetCore.Services;

internal readonly struct ModInfoEntry(int wikiId, string? chineseName)
{
  public int WikiId { get; } = wikiId;
  public string? ChineseName { get; } = chineseName;
}

public partial class ModInfoUpdateService(IEnumerable<INetService> netServices) : IModInfoUpdateService
{
  private readonly Dictionary<string, ModInfoEntry> _modrinthSlugToWikiIdDict = new();
  private readonly Dictionary<string, ModInfoEntry> _curseForgeSlugToWikiIdDict = new();
  private static readonly SemaphoreSlim Semaphore = new(5);

  /// <summary>
  /// 给已经获取网络信息的modInfos填充属性
  /// </summary>
  public async Task UpdateModInfoAsync(IReadOnlyList<IModInfo> modInfos,
    Dictionary<string, IModFileVersionNetInfo> netModVersionInfoDict)
  {
    if (_modrinthSlugToWikiIdDict.Keys.Count ==0)
      await LoadModDataAsync();
      
    List<Func<Task>> downloadLogoTasks = [];
    int i = 0;
    foreach (var key in netModVersionInfoDict.Keys)
    {
      var modInfo = modInfos[i];
      var modFileVersionNetInfo = netModVersionInfoDict[key];
      modInfo.Sha1Hash = key;
      if (!string.IsNullOrEmpty(modFileVersionNetInfo.Title))
        modInfo.Name = modFileVersionNetInfo.Title;
      if (!string.IsNullOrEmpty(modFileVersionNetInfo.VersionNumber))
        modInfo.Version = modFileVersionNetInfo.VersionNumber;
      if (!string.IsNullOrEmpty(modFileVersionNetInfo.Description))
        modInfo.Description = modFileVersionNetInfo.Description;

      foreach (var netService in netServices)
      {
        if (netService.GetType() != modFileVersionNetInfo.ServiceProvider)
          continue;
        if (!string.IsNullOrEmpty(modFileVersionNetInfo.IconUrl))
        {
          downloadLogoTasks.Add(async () =>
          {
            await Semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
              await DownloadAndAssignIconAsync(modInfo,
                new Uri(modFileVersionNetInfo.IconUrl), netService);
            }
            finally
            {
              Semaphore.Release();
            }
          });
        }
        if (_modrinthSlugToWikiIdDict.TryGetValue(modFileVersionNetInfo.Slug, out var modInfoEntry) ||
            _curseForgeSlugToWikiIdDict.TryGetValue(modFileVersionNetInfo.Slug, out modInfoEntry))
        {
          modInfo.Website = new Uri($"https://www.mcmod.cn/class/{modInfoEntry.WikiId}.html");
          if (!string.IsNullOrEmpty(modInfoEntry.ChineseName))
            modInfo.Name = modInfoEntry.ChineseName;
        }
        i++;
      }
    }

    var runningTasks = downloadLogoTasks.Select(taskFunc => taskFunc());
    await Task.WhenAll(runningTasks).ConfigureAwait(false); // 限流下载图片
    foreach (var item in modInfos)
      FinalizeModInfo(item, TextCleanerRegex());
  }

  public async Task UpdateModInfoAsync(IReadOnlyCollection<IModInfo> modInfos)
  {
    if (_modrinthSlugToWikiIdDict.Keys.Count ==0)
      await LoadModDataAsync();
    var needGetNetModInfoDic = modInfos.ToDictionary(m => m.Sha1Hash, m => m);
    foreach (var netService in netServices)
    {
      Dictionary<string, IModFileVersionNetInfo> netModVersionInfoDict;
      try
      {
        netModVersionInfoDict =
          await netService.GetVersionDictAsync(
            needGetNetModInfoDic.Values).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
        continue;
      }

      if (netModVersionInfoDict.Count == 0)
        continue;

      List<Task> downloadLogoTasks = [];
      foreach (var modInfo in needGetNetModInfoDic.Values.ToArray()) // 要在遍历中删除一些项目，所以创建一个副本
      {
        if (!netModVersionInfoDict.TryGetValue(modInfo.Sha1Hash, out var modFileVersionNetInfo))
          continue;

        if (!string.IsNullOrEmpty(modFileVersionNetInfo.Title))
          modInfo.Name = modFileVersionNetInfo.Title;
        if (!string.IsNullOrEmpty(modFileVersionNetInfo.VersionNumber))
          modInfo.Version = modFileVersionNetInfo.VersionNumber;
        if (!string.IsNullOrEmpty(modFileVersionNetInfo.Description))
          modInfo.Description = modFileVersionNetInfo.Description;
        if (!string.IsNullOrEmpty(modFileVersionNetInfo.IconUrl))
          downloadLogoTasks.Add(DownloadAndAssignIconAsync(modInfo,
            new Uri(modFileVersionNetInfo.IconUrl), netService));

        if (string.IsNullOrEmpty(modInfo.Website?.ToString()) &&
            (_modrinthSlugToWikiIdDict.TryGetValue(modFileVersionNetInfo.Slug, out var modInfoEntry) ||
             _curseForgeSlugToWikiIdDict.TryGetValue(modFileVersionNetInfo.Slug, out modInfoEntry)))
        {
          modInfo.Website = new Uri($"https://www.mcmod.cn/class/{modInfoEntry.WikiId}.html");
          if (!string.IsNullOrEmpty(modInfoEntry.ChineseName))
            modInfo.Name = modInfoEntry.ChineseName;
        }

        needGetNetModInfoDic.Remove(modInfo.Sha1Hash);
      }

      await Task.WhenAll(downloadLogoTasks).ConfigureAwait(false); // 同时下载图片
    }

    foreach (var modInfo in modInfos)
      FinalizeModInfo(modInfo, TextCleanerRegex());
  }

  /// <summary>
  /// 剔除空格、换行，添加默认文本
  /// </summary>
  void FinalizeModInfo(IModInfo modInfo, Regex regex)
  {
    modInfo.Description = regex.Replace(modInfo.Description ?? "", " ").Trim();

    if (string.IsNullOrEmpty(modInfo.Name))
      modInfo.Name = Path.GetFileNameWithoutExtension(modInfo.Path);

    if (string.IsNullOrEmpty(modInfo.Description))
      modInfo.Description = "该mod没有提供描述";
  }

  async Task DownloadAndAssignIconAsync(IModInfo modInfo, Uri iconUrl, INetService netService)
  {
    try
    {
      modInfo.Logo = await netService.GetIconAsync(iconUrl);
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Error downloading icon for {modInfo.Name}: {ex.Message}");
    }
  }

  [GeneratedRegex(@"[\r\n\u2028\u2029]+")]
  private static partial Regex TextCleanerRegex();

  private async Task LoadModDataAsync()
  {
    _modrinthSlugToWikiIdDict.Clear();
    _curseForgeSlugToWikiIdDict.Clear();

    using var reader = new StreamReader("ModData.txt");
    var lineNumber = 0;

    while (await reader.ReadLineAsync() is { } line)
    {
      lineNumber++;
      var trimmed = line.Trim();
      if (string.IsNullOrEmpty(trimmed)) continue;

      var entries = trimmed.Split('¨');
      foreach (var entryData in entries)
      {
        var parts = entryData.Split('|');
        if (parts.Length == 0) continue;

        var slugPart = parts[0].Trim();
        string? modrinthSlug = null;
        string? curseForgeSlug = null;
        string? chineseName = null;

        // 判断 slug 类型
        if (slugPart.StartsWith("@"))
        {
          modrinthSlug = slugPart[1..];
        }
        else if (slugPart.EndsWith("@"))
        {
          curseForgeSlug = slugPart[..^1];
          modrinthSlug = curseForgeSlug;
        }
        else if (slugPart.Contains("@"))
        {
          var slugs = slugPart.Split('@');
          if (slugs.Length >= 2)
          {
            curseForgeSlug = slugs[0];
            modrinthSlug = slugs[1];
          }
        }
        else
        {
          curseForgeSlug = slugPart;
        }

        // 提取中文名
        if (parts.Length >= 2)
        {
          chineseName = parts[1].Trim();

          if (chineseName.Contains('*'))
          {
            var baseSlug = curseForgeSlug ?? modrinthSlug ?? "";
            var words = baseSlug.Split('-')
              .Where(w => w.Length > 0)
              .Select(w => char.ToUpper(w[0]) + w[1..])
              .ToArray();
            chineseName = chineseName.Replace("*", $" ({string.Join(" ", words)})");
          }
        }

        var entry = new ModInfoEntry(lineNumber, chineseName);

        if (!string.IsNullOrEmpty(modrinthSlug))
          _modrinthSlugToWikiIdDict.TryAdd(modrinthSlug, entry);

        if (!string.IsNullOrEmpty(curseForgeSlug))
          _curseForgeSlugToWikiIdDict.TryAdd(curseForgeSlug, entry);
      }
    }
  }
}