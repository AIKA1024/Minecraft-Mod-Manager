using Avalonia;
using Avalonia.Media.Imaging;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Models.Events;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using System.Diagnostics;

namespace NetCore.Models;

public abstract class DownloadBase : INetService
{
  protected abstract HttpClient Client { get; init; }
  public event INetService.DownloadStartedHandler? DownloadProgressStarted;
  public event INetService.DownloadChangedHandler? DownloadProgressChanged;
  public event INetService.DownloadCompletedHandler? DownloadProgressCompleted;
  public abstract Task<IModFileVersionNetInfo?> GetVersionFromHashAsync(IHashInfo hashInfo);

  public abstract Task<Dictionary<string, IModFileVersionNetInfo>> GetVersionDictAsync(
    IReadOnlyCollection<IHashInfo> hashInfoList);

  public abstract Task<IModProjectNetInfo> GetProjectAsync(string id);
  public abstract Task<List<IModProjectNetInfo>> GetMultipleProjectsAsync(IReadOnlyCollection<string> ids);

  public async Task<Bitmap> GetIconAsync(Uri uri)
  {
    var imageBytes = await Client.GetByteArrayAsync(uri);
    using MemoryStream memoryStream = new MemoryStream(imageBytes);
    using var originalBitmapBitmap = new Bitmap(memoryStream);
    var bitmap = originalBitmapBitmap.CreateScaledBitmap(new PixelSize(32, 32));
    return bitmap;
  }

  public async Task<Dictionary<string, IModFileVersionNetInfo>> DownLoadMods(IReadOnlyCollection<IHashInfo> hashInfos,
    string savePath,
    int maxDegreeOfParallelism)
  {
    if (hashInfos.Count == 0)
      return [];
    var sha1AndVersionFileDict = await GetVersionDictAsync(hashInfos);
    using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
    var downloadTasks = new List<Task<IModFileVersionNetInfo?>>();
    foreach (var versionNetInfo in sha1AndVersionFileDict.Values)
      downloadTasks.Add(DownloadSingleModAsync(versionNetInfo, savePath, semaphore));
    var downloadResults = await Task.WhenAll(downloadTasks);
    var hashes = sha1AndVersionFileDict.Keys.ToArray();
    var dict = new Dictionary<string, IModFileVersionNetInfo>();
    for (int i = 0; i < downloadResults.Length; i++)
    {
      if (downloadResults[i] == null)
        continue;
      dict.Add(hashes[i], downloadResults[i]!);
    }

    return dict;
  }

  public async Task<IModFileVersionNetInfo?> DownloadSingleModAsync(IModFileVersionNetInfo? versionNetInfo,
    string savePath,
    SemaphoreSlim semaphore)
  {
    if (versionNetInfo == null)
      return null;

    await semaphore.WaitAsync();
    DownloadProgressStarted?.Invoke(new DownloadProgressStartedEventArgs(versionNetInfo.Title));

    try
    {
      if (string.IsNullOrEmpty(versionNetInfo.DownloadUri))
        return null;

      int maxRetryCount = 3;
      int retryCount = 0;
      Exception? lastException = null;

      while (retryCount < maxRetryCount)
      {
        try
        {
          return await DownloadCoreAsync(versionNetInfo, savePath).ConfigureAwait(false);
        }
        catch (Exception ex) when (retryCount < maxRetryCount - 1)
        {
          lastException = ex;
          retryCount++;
          Debug.WriteLine($"下载失败，正在重试 ({retryCount}/{maxRetryCount}): {versionNetInfo.DownloadUri} - {ex.Message}");

          // 重试前等待（指数退避）
          await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          // 最后一次重试失败
          lastException = ex;
          throw;
        }
      }

      throw lastException ?? new Exception("下载失败");
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"下载失败: {versionNetInfo.DownloadUri} - {ex.Message}");
      return null;
    }
    finally
    {
      semaphore.Release();
    }
  }

  private async Task<IModFileVersionNetInfo> DownloadCoreAsync(IModFileVersionNetInfo versionNetInfo, string savePath)
  {
    var response = await Client.GetAsync(versionNetInfo.DownloadUri, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();

    Uri uri = new Uri(versionNetInfo.DownloadUri);
    string fileName = Path.GetFileName(uri.LocalPath);

    byte[] buffer = new byte[8192];
    long totalRead = 0;
    int bytesRead;
    double lastReportedProgress = 0;
    string saveFilePath = Path.Join(savePath, fileName);

    await using var stream = await response.Content.ReadAsStreamAsync();
    await using var fileStream = File.Create(saveFilePath);
    versionNetInfo.LocalPath = saveFilePath;

    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
    {
      await fileStream.WriteAsync(buffer, 0, bytesRead);
      totalRead += bytesRead;

      if (versionNetInfo.Size == 0) continue;
      var progressPercentage = (double)totalRead / versionNetInfo.Size * 100;
      if (progressPercentage - lastReportedProgress >= 1)
      {
        lastReportedProgress = progressPercentage;
        DownloadProgressChanged?.Invoke(new DownloadProgressChangedEventArgs(versionNetInfo.Title, progressPercentage));
      }
    }

    DownloadProgressCompleted?.Invoke(new DownloadCompletedHandlerEventArgs(versionNetInfo.Title));
    return versionNetInfo;
  }
}