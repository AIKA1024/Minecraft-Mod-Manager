using Avalonia.Media.Imaging;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Models.Events;
namespace MAZDA_MCTool.Abstraction.Contracts.Services;

public interface INetService
{
  delegate void DownloadStartedHandler(DownloadProgressStartedEventArgs e);
  delegate void DownloadChangedHandler(DownloadProgressChangedEventArgs e);
  delegate void DownloadCompletedHandler(DownloadCompletedHandlerEventArgs e);
  event DownloadStartedHandler DownloadProgressStarted;
  event DownloadChangedHandler DownloadProgressChanged;
  event DownloadCompletedHandler DownloadProgressCompleted;
  Task<IModFileVersionNetInfo?> GetVersionFromHashAsync(IHashInfo hashInfo);
  /// <summary>
  /// 
  /// </summary>
  /// <param name="hashInfoList"></param>
  /// <returns>字典的key统一为sha1Hash</returns>
  Task<Dictionary<string, IModFileVersionNetInfo>> GetVersionDictAsync(IReadOnlyCollection<IHashInfo> hashInfoList);
  Task<IModProjectNetInfo> GetProjectAsync(string id);
  Task<List<IModProjectNetInfo>> GetMultipleProjectsAsync(IReadOnlyCollection<string> ids);
  
  Task<Bitmap> GetIconAsync(Uri uri);

  Task<Dictionary<string,IModFileVersionNetInfo>> DownLoadMods(IReadOnlyCollection<IHashInfo> hashInfos,string savePath,int maxDegreeOfParallelism);

  Task<IModFileVersionNetInfo?> DownloadSingleModAsync(IModFileVersionNetInfo? versionNetInfo, string savePath,
    SemaphoreSlim semaphore);
}