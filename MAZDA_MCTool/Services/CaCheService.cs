using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Models;
using MAZDA_MCTool.Serialization;
using NetCore.Models;

namespace MAZDA_MCTool.Services;

public class CaCheService
{
  public async Task SaveCachesAsync(IEnumerable<IModInfo> modInfos)
  {
    List<Task> tasks = [];
    foreach (var modInfo in modInfos)
    {
      if (modInfo.Logo is not null)
      {
        var logoPath = Path.Join(App.Current.IconCachePath, $"{modInfo.Sha1Hash}.png");
        modInfo.Logo.Save(logoPath);
        modInfo.LogoPath = logoPath;
      }
      var jsonStr = JsonSerializer.Serialize(modInfo, JsonContext.Default.ModInfo);
      tasks.Add(File.WriteAllTextAsync(Path.Join(App.Current.ModCachePath, $"{modInfo.Sha1Hash}.json"),
        jsonStr));
    }
    await Task.WhenAll(tasks);
  }
/// <summary>
/// 获取缓存
/// </summary>
/// <returns>string :FileNameWithoutExtension</returns>
  public async Task<Dictionary<string,ModInfo>?> GetCachesAsync()
  {
    var files = Directory.GetFiles(App.Current.ModCachePath, "*.json");
    if (files.Length == 0)
      return null;
    
    var lastWriteTime = File.GetLastWriteTime(files[0]);
    if ((DateTime.Now - lastWriteTime).TotalDays > 10)
    {
      foreach (var file in files)
        File.Delete(file);
      return null;
    }
    var tasks = files.Select(async file =>
    {
      var str = await File.ReadAllTextAsync(file);

      var info = JsonSerializer.Deserialize(str, JsonContext.Default.ModInfo);
      var fileName = Path.GetFileNameWithoutExtension(file);
      return (Key: fileName, Value: info);
    });
    
    var results = await Task.WhenAll(tasks);

    return results
      .ToDictionary(x => x.Key, x => x.Value!);
  }
}