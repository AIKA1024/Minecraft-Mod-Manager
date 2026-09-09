using MAZDA_MCTool.Abstraction.Contracts.Models;
using NetCore.Models;
using NetCore.Serialization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NetCore.Services;

public class CurseForgeNetService : DownloadBase
{
  private const int GameId = 432;
  private const string ApiKey = "输入你自己的curseforge密钥"; // 前往 https://console.curseforge.com 免费申请 Api Key
  protected sealed override HttpClient Client { get; init; }
  public CurseForgeNetService()
  {
    Client =  new HttpClient(
        new SocketsHttpHandler
        {
          ConnectTimeout = TimeSpan.FromSeconds(10),
          // SslOptions = new SslClientAuthenticationOptions // 只有自己使用内网穿透的服务器才需要开这个设置
          // {
          //   RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
          // }
        }
      )
      { BaseAddress = new Uri("https://api.curseforge.com/") };
    Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    Client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
  }
  public override async Task<List<IModProjectNetInfo>> GetMultipleProjectsAsync(IReadOnlyCollection<string> ids)
  {
    if (ids.Count == 0)
      return [];
    var requestBody = new CurseForgeProjectRequest
    {
      modIds = ids
    };
    var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody,JsonContext.Default.CurseForgeProjectRequest),
     Encoding.UTF8, "application/json");

    var response = await Client.PostAsync($"/v1/mods", jsonContent);
    if (!response.IsSuccessStatusCode) return [];
    await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    using var doc = await JsonDocument.ParseAsync(responseStream);
    var root = doc.RootElement;
    var modProjectNetInfoList = new List<IModProjectNetInfo>();
    foreach (var item in root.GetProperty("data").EnumerateArray())
    {
      modProjectNetInfoList.Add(new ModProjectNetInfo
      {
        Id = item.GetProperty("id").ToString(),
        Slug = item.GetProperty("slug").ToString(),
        Description = item.GetProperty("summary").ToString(),
        Title = item.GetProperty("name").ToString(),
        IconUrl = item.GetProperty("logo").GetProperty("url").ToString()
      });
    }
    return modProjectNetInfoList;
  }

  public override Task<IModFileVersionNetInfo?> GetVersionFromHashAsync(IHashInfo hashInfo)
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// </summary>
  /// <returns>字典的key是哈希而不是指纹</returns>
  public override async Task<Dictionary<string, IModFileVersionNetInfo>> GetVersionDictAsync(IReadOnlyCollection<IHashInfo> hashInfos)
  {
    if (hashInfos.Count == 0)
      return [];
    
    var requestBody = new FingerprintRequest
    { fingerprints = hashInfos.Select(m=>m.Fingerprint) };
    var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody, JsonContext.Default.FingerprintRequest),
      Encoding.UTF8, "application/json");
    var response = await Client.PostAsync($"/v1/fingerprints/{GameId}", jsonContent);
    if (!response.IsSuccessStatusCode)
      return [];

    await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    using var doc = await JsonDocument.ParseAsync(responseStream);
    var root = doc.RootElement;
    var jsonArray = root.GetProperty("data").GetProperty("exactMatches").EnumerateArray();
    var netModInfoList = await GetMultipleProjectsAsync(jsonArray.Select(j => j.GetProperty("id").ToString()).ToList());
    var dict = new Dictionary<string, IModFileVersionNetInfo>();
    for (int i = netModInfoList.Count - 1; i >= 0; i--)
    {
      foreach (var item in jsonArray)
      {
        if (netModInfoList[i].Id != item.GetProperty("id").ToString()) continue;
        
        string fileName = item.GetProperty("file").GetProperty("fileName").ToString();
        string modVersion = ExtractModVersion(fileName);
        
        var versionNetInfo = new ModFileVersionNetInfo
        {
          Title = netModInfoList[i].Title,
          DownloadUri = item.GetProperty("file").GetProperty("downloadUrl").ToString(),
          Size = item.GetProperty("file").GetProperty("fileLength").GetInt32(),
          ProjectId = netModInfoList[i].Id,
          Slug = netModInfoList[i].Slug,
          VersionNumber = modVersion,
          Description = netModInfoList[i].Description,
          IconUrl = netModInfoList[i].IconUrl,
          ServiceProvider = typeof(CurseForgeNetService)
        };
        netModInfoList.RemoveAt(i);
        dict.TryAdd(item.GetProperty("file").GetProperty("hashes")[0].GetProperty("value").ToString(), versionNetInfo);
        break;
      }
    }
    return dict;
  }
  
  private string ExtractModVersion(string fileName)
  {
    if (string.IsNullOrWhiteSpace(fileName))
      return "";

    // 正则：专门匹配 CurseForge 文件名格式 模组名-游戏版本-模组版本.jar
    var regex = new Regex(@"(?<=-)(\d+\.\d+(?:\.\d+)*)(?=\.jar|$)", RegexOptions.IgnoreCase);
    var match = regex.Match(fileName);

    return match.Success ? match.Value : "";
  }
  
  public override Task<IModProjectNetInfo> GetProjectAsync(string id)
  {
    throw new NotImplementedException();
  }
}
