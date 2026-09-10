using System.Text;
using System.Text.Json;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using NetCore.Models;
using NetCore.Serialization;

namespace NetCore.Services;

public class ModrinthNetService : DownloadBase
{
  protected sealed override HttpClient Client { get; init; } = new(
      new SocketsHttpHandler
      {
        ConnectTimeout = TimeSpan.FromSeconds(10),
        // SslOptions = new SslClientAuthenticationOptions // 只有自己使用内网穿透的服务器才需要开这个设置
        // {
        //   RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        // }
      }
    )
    { BaseAddress = new Uri("https://api.modrinth.com/v2/") };

  public override async Task<List<IModProjectNetInfo>> GetMultipleProjectsAsync(IReadOnlyCollection<string> ids)
  {
    if (ids.Count == 0)
      return [];
    var response =
      await Client.GetAsync($"projects?ids={JsonSerializer.Serialize(ids, JsonContext.Default.ListString)}");
    if (!response.IsSuccessStatusCode) return [];
    await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    using var doc = await JsonDocument.ParseAsync(responseStream);
    var root = doc.RootElement;
    List<IModProjectNetInfo> modProjectNetInfos = [];
    foreach (var item in root.EnumerateArray())
    {
      modProjectNetInfos.Add
      (
        new ModProjectNetInfo
        {
          Id = item.GetProperty("id").GetString() ?? string.Empty,
          Title = item.GetProperty("title").GetString() ?? string.Empty,
          Description = item.GetProperty("description").GetString() ?? string.Empty,
          Slug = item.GetProperty("slug").GetString() ?? string.Empty,
          IconUrl = item.GetProperty("icon_url").GetString() ?? string.Empty
        }
      );
    }
    return modProjectNetInfos;
  }

  public override async Task<IModFileVersionNetInfo?> GetVersionFromHashAsync(IHashInfo hashInfo)
  {
    // var response = await Client.GetAsync($"version_file/{hashInfo.Sha1Hash}").ConfigureAwait(false);
    // if (!response.IsSuccessStatusCode) return null;
    // var responseStream = await response.Content.ReadAsStreamAsync();
    // return JsonSerializer.Deserialize(responseStream, JsonContext.Default.ModFileVersionNetInfo);// 使用jsonDocument，不用这个原生成器
    throw new NotImplementedException();
  }

  public override async Task<Dictionary<string, IModFileVersionNetInfo>> GetVersionDictAsync(
    IReadOnlyCollection<IHashInfo> hashInfos)
  {
    if (hashInfos.Count == 0)
      return [];

    var requestBody = new HashsRequest
    {
      Hashes = hashInfos.Select(m => m.Sha1Hash),
      Algorithm = "sha1"
    };
    var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody, JsonContext.Default.HashsRequest),
      Encoding.UTF8, "application/json");
    var response = await Client.PostAsync("version_files", jsonContent);
    if (!response.IsSuccessStatusCode)
      return [];
    var responseContent = await response.Content.ReadAsStringAsync();
    if (string.IsNullOrEmpty(responseContent) || responseContent == "{}")
      return [];

    await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    using var doc = await JsonDocument.ParseAsync(responseStream);
    var root = doc.RootElement;
    Dictionary<string, IModFileVersionNetInfo> result = [];
    foreach (var property in root.EnumerateObject())
    {
      var item = root.GetProperty(property.Name);
      result.TryAdd(property.Name, new ModFileVersionNetInfo
      {
        ProjectId = item.GetProperty("project_id").GetString() ?? "",
        VersionNumber = item.GetProperty("version_number").GetString() ?? "",
        DownloadUri = item.GetProperty("files")[0].GetProperty("url").GetString() ?? "",
        Size = item.GetProperty("files")[0].GetProperty("size").GetInt32(),
        ServiceProvider = typeof(ModrinthNetService)
      });
    }

    var netModInfoList =
      await GetMultipleProjectsAsync(result.Values.Select(versionInfo => versionInfo.ProjectId).ToList())
        .ConfigureAwait(false);

    for (int i = netModInfoList.Count - 1; i >= 0; i--)
    {
      foreach (var netModVersionInfo in result.Values)
      {
        if (netModInfoList[i].Id != netModVersionInfo.ProjectId) continue;
        netModVersionInfo.Slug = netModInfoList[i].Slug;
        netModVersionInfo.Title = netModInfoList[i].Title;
        netModVersionInfo.Description = netModInfoList[i].Description;
        netModVersionInfo.IconUrl = netModInfoList[i].IconUrl;
        netModInfoList.RemoveAt(i);
        break;
      }
    }

    return result;
  }

  public override Task<IModProjectNetInfo> GetProjectAsync(string id)
  {
    throw new NotImplementedException();
  }
}