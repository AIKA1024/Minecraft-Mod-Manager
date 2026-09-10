using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using MAZDA_MCTool.Messages;
using MAZDA_MCTool.Models;
using MAZDA_MCTool.Serialization;
using MAZDA_MCTool.Services;
using MAZDA_MCTool.UI.Models;
using MAZDA_MCTool.UI.Services;
using MAZDA_MCTool.Utils;
using Microsoft.Extensions.DependencyInjection;
using ModLoadCore;
using ModLoadCore.Services;
using NetCore.Extensions;
using NetCore.Services;

namespace MAZDA_MCTool.ViewModels;

public partial class ViewModPageViewModel : ObservableObject
{
  private readonly Setting _appSetting = App.Current.Services.GetRequiredService<Setting>();
  public IEnumerable<IStorageItem>? DropFiles { get; set; }
  private readonly InfoBarService _infoBarService = App.Current.Services.GetRequiredService<InfoBarService>();

  private readonly LoadModEngine _loadModEngine = new(
    App.Current.Services.GetServices<IModParserStrategy>().ToArray());

  [ObservableProperty] private AvaloniaList<IModInfo> _displayModInfos = [];
  [ObservableProperty] private AvaloniaList<IModInfo> _allModInfos = [];
  [ObservableProperty] private AvaloniaList<IModInfo> _searchModInfos = [];
  public AvaloniaList<string> ModNames { get; set; } = [];

  [ObservableProperty] private string _message = string.Empty;

  public ViewModPageViewModel()
  {
    WeakReferenceMessenger.Default.Register<GamePathChangedMessage>(this, (_, message) =>
    {
      var modFiles = Directory.EnumerateFiles($"{message.Value}/mods")
        .Where(file =>
          file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
          file.EndsWith(".jar", StringComparison.OrdinalIgnoreCase))
        .Select(path => new ModFile { Path = path });
      LoadModAsync(modFiles.ToArray()).SafeFireAndForget();
    });
    DisplayModInfos = AllModInfos;
  }

  private static void AddSorted<T>(AvaloniaList<T> collection, T item, Comparison<T> comparison)
  {
    int low = 0;
    int high = collection.Count;

    while (low < high)
    {
      int mid = (low + high) / 2;
      if (comparison(collection[mid], item) < 0)
        low = mid + 1;
      else
        high = mid;
    }

    collection.Insert(low, item);
  }

  private async Task LoadModAsync(ModFile[] modFiles, bool removeItem = true)
  {
    if (removeItem)
    {
      foreach (var modInfo in AllModInfos)
        modInfo.Dispose();
      AllModInfos.Clear();
    }

    if (modFiles.Length == 0)
      return;
    var caches = await App.Current.Services.GetRequiredService<CaCheService>().GetCachesAsync() ?? [];
    var sha1HashService = App.Current.Services.GetRequiredService<HashService>();
    var tasks = new List<Task<IModInfo>>();
    var unSortModInfos = new List<IModInfo>();
    Message = "计算哈希值";
    foreach (var modFile in modFiles)
    {
      if (caches.TryGetValue(await sha1HashService.ComputeSha1HashAsync(modFile.Path).ConfigureAwait(false),
            out var modInfo))
        unSortModInfos.Add(modInfo);
      else
        tasks.Add(_loadModEngine.ParserAsync(modFile, new ModInfo(modFile.Path)));
    }

    if (tasks.Count > 0)
    {
      var taskResults = await Task.WhenAll(tasks).ConfigureAwait(false);
      unSortModInfos.AddRange(taskResults);
      Message = "联网查询信息";
      await GetModNetInfo(taskResults).ConfigureAwait(false);
    }

    if (AllModInfos.Count == 0)
      AllModInfos.AddRange(unSortModInfos.OrderBy(item => item.Name));
    else
      foreach (var modInfo in unSortModInfos)
        AddSorted(AllModInfos, modInfo, (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
    Message = string.Empty;

    await Dispatcher.UIThread.InvokeAsync(() =>
    {
      ModNames.Clear();
      ModNames.AddRange(AllModInfos.Select(m=>m.Name));
    });
  }

  [RelayCommand]
  private async Task Initialization()
  {
    if (string.IsNullOrEmpty(_appSetting.GamePath) || !Directory.Exists(_appSetting.GamePath))
    {
      _infoBarService.ShowMessage("提示", "游戏目录无效", severity: InfoSeverity.Warning, delayToClose: 5000);
      return;
    }


    var modFiles = Directory.EnumerateFiles(_appSetting.ModPath)
      .Where(file =>
        file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
        file.EndsWith(".jar", StringComparison.OrdinalIgnoreCase))
      .Select(path => new ModFile { Path = path });
    await LoadModAsync(modFiles.ToArray());
  }

  /// <summary>
  /// 从网络获取信息
  /// </summary>
  /// <param name="modInfos">无缓存的modInfo列表</param>
  private async Task GetModNetInfo(IModInfo[] modInfos)
  {
    if (modInfos.Length == 0)
      return;
    var cacheService = App.Current.Services.GetRequiredService<CaCheService>();
    await new ModInfoUpdateService(App.Current.Services.GetServices<INetService>())
      .UpdateModInfoAsync(modInfos).ConfigureAwait(false); //todo 这里可以加个try
    await cacheService.SaveCachesAsync(modInfos).ConfigureAwait(false);
  }

  [RelayCommand]
  private async Task GenerateSynchronizationFileAsync()
  {
    if (AllModInfos.Count == 0)
      return;
    var modPackName = App.Current.ModPackName;
    var path = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
      "Downloads",
      $"{modPackName} - {DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"
    );

    var jsonStr = JsonSerializer.Serialize(AllModInfos.Select(m =>
      new SynchronizationInfo
      {
        Name = m.Name,
        Sha1Hash = m.Sha1Hash,
        Fingerprint = m.Fingerprint
      }
    ).ToList(), JsonContext.Default.ListSynchronizationInfo);
    await File.WriteAllTextAsync(path, jsonStr);
    _infoBarService.ShowMessage("成功", $"文件已生成于：{path}", "打开文件夹", delayToClose: 5000,
      action: () => Process.Start("explorer", Path.GetDirectoryName(path)));
  }

  private async Task UseSynchronizationFileAsync(string path)
  {
    await using var stream = File.OpenRead(path);
    try
    {
      var synchronizationInfos = await JsonSerializer
        .DeserializeAsync(stream, JsonContext.Default.ListSynchronizationInfo) ?? [];
      var lackModInfos = synchronizationInfos
        .Where(synchronizationInfo => AllModInfos.All(m => m.Sha1Hash != synchronizationInfo.Sha1Hash)).ToList();
      var extraModInfos = AllModInfos
        .Where(modInfo => synchronizationInfos.All(s => s.Sha1Hash != modInfo.Sha1Hash)).ToList();
      if (extraModInfos.Count > 0)
      {
        var dialogResult = await App.Current.Services.GetRequiredService<IDialogService>().ShowDialogAsync(
          new DialogRequest
          {
            Header = "警告",
            Subheader = "本地有同步文件之外的mod，是否删除？",
            Content = string.Join(Environment.NewLine, extraModInfos.Select(m=>m.Name))
          });
        if (dialogResult is true)
        {
          foreach (var modInfo in extraModInfos)
          {
            File.Delete(modInfo.Path);
            AllModInfos.Remove(modInfo);
          }
          _infoBarService.ShowMessage("提示", "删除成功", severity: InfoSeverity.Success, delayToClose: 3000);
        }
      }

      if (lackModInfos.Count == 0)
      {
        _infoBarService.ShowMessage("提示", "Mod齐全", severity: InfoSeverity.Informational, delayToClose: 5000);
        return;
      }

      var netServices = App.Current.Services.GetServices<INetService>();
      _infoBarService.ShowMessage("提示", $"下载Mod文件中，共有{lackModInfos.Count}个文件需要下载", severity: InfoSeverity.Informational,
        delayToClose: 5000);
      foreach (var netService in netServices)
      {
        var results = await netService.DownLoadMods(lackModInfos, _appSetting.ModPath, 8)
          .ConfigureAwait(false);
        for (int i = lackModInfos.Count - 1; i >= 0; i--)
          if (results.ContainsKey(lackModInfos[i].Sha1Hash))
            lackModInfos.RemoveAt(i);
        // 由于DownLoadMods已经获取过ModFileVersionNetInfo了，所以逻辑和获取本地mod网络信息的逻辑不太一样 屎山初见端倪，看看怎么改下代码
        var modInfos = results.Values.Select(v => new ModInfo(v.LocalPath)).ToArray();
        await new ModInfoUpdateService(App.Current.Services.GetServices<INetService>()).UpdateModInfoAsync(modInfos, results).ConfigureAwait(false);
        await App.Current.Services.GetRequiredService<CaCheService>().SaveCachesAsync(modInfos).ConfigureAwait(false);
        if (AllModInfos.Count == 0)
          foreach (var modInfo in modInfos.OrderBy(item => item.Name))
            AllModInfos.Add(modInfo);
        else
          foreach (var modInfo in modInfos)
            AddSorted(AllModInfos, modInfo, (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
      }

      Message = string.Empty;
      if (lackModInfos.Count != 0)
      {
        _infoBarService.ShowMessage("警告",
          $"{string.Join(Environment.NewLine, lackModInfos.Select(m => m.Name))} {Environment.NewLine}共{lackModInfos.Count}个mod在互联网查询未搜索到信息{Environment.NewLine}",
          severity: InfoSeverity.Warning);
      }
      else
        _infoBarService.ShowMessage("提示", $"下载完成", severity: InfoSeverity.Success, delayToClose: 5000);
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex);
      _infoBarService.ShowMessage("错误", ex.Message, severity: InfoSeverity.Error);
    }
  }

  [RelayCommand]
  private async Task DragSynchronizationFileAsync(DragEventArgs e)
  {
    var files = e.Data.GetFiles()?.ToArray();
    if (files?.Length > 1)
    {
      _infoBarService.ShowMessage("提示", "只能拖入一个同步文件", severity: InfoSeverity.Informational);
      return;
    }

    await UseSynchronizationFileAsync(files![0].Path.LocalPath);
  }

  [RelayCommand]
  private async Task SelectSynchronizationFileAsync()
  {
    var topLevel = App.Current.Services.GetRequiredService<TopLevel>();
    var storageProvider = topLevel.StorageProvider;
    var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
    {
      Title = "选择文件",
      AllowMultiple = false,
      FileTypeFilter =
      [
        new FilePickerFileType("文本文件") { Patterns = ["*.txt"] },
        new FilePickerFileType("所有文件") { Patterns = ["*"] }
      ]
    });

    if (files.Count > 0)
    {
      var file = files[0];
      // 获取本地路径（可选）
      var path = file.Path.LocalPath;
      await UseSynchronizationFileAsync(path).ConfigureAwait(false);
    }
  }

  [RelayCommand]
  private void OpenModWebsite(Uri uri)
  {
    ProcessStartInfo psi = new()
    {
      FileName = uri.ToString(),
      UseShellExecute = true
    };
    Process.Start(psi);
  }

  [RelayCommand]
  private void SearchLocalMod(string text)
  {
    if (text == string.Empty)
    {
      DisplayModInfos = AllModInfos;
      return;
    }

    SearchModInfos = new AvaloniaList<IModInfo>(AllModInfos.Where(m => m.Name.Contains(text)));
    DisplayModInfos = SearchModInfos;
  }

  [RelayCommand]
  private void RevealInExplorer(IReadOnlyCollection<object> items)
  {
    FileManagerHelper.RevealMultipleInExplorer(items.OfType<IModInfo>().Select(m=>m.Path));
  }

  [RelayCommand]
  private async Task DeleteMods(IReadOnlyCollection<object> items)
  {
    if (items.Count == 0)
      return;

    var mods = items.OfType<IModInfo>().ToArray();
    var dialogService = App.Current.Services.GetRequiredService<IDialogService>();
    var result = await dialogService.ShowDialogAsync(new DialogRequest
    {
      Header = "警告",
      Content = "删除所选mod?"
    });
    if (result is true)
    {
      foreach (var mod in mods)
      {
        File.Delete(mod.Path);
        AllModInfos.Remove(mod);
      }
    }
  }
}