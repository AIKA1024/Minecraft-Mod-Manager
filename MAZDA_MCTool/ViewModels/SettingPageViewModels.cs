using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAZDA_MCTool.Models;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using MAZDA_MCTool.Messages;
using MAZDA_MCTool.UI.Services;
using Velopack;
using Velopack.Sources;

namespace MAZDA_MCTool.ViewModels;

public partial class SettingPageViewModels : ObservableObject
{
  public Setting AppSetting { get; } = App.Current.Services.GetRequiredService<Setting>();

  public string Version { get; } = typeof(Program)
    .Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion ?? "";
  
  private UpdateInfo? _updateInfo;

  [RelayCommand(AllowConcurrentExecutions = true)]
  private async Task SelectGamePath()
  {
    var topLevel = App.Current.Services.GetRequiredService<TopLevel>();
    var storageProvider = topLevel.StorageProvider;
    var resultFolders =
      await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });
    if (resultFolders.Count == 0)
      return;
    var gamePath = Uri.UnescapeDataString(resultFolders[0].Path.LocalPath).Replace('/', Path.DirectorySeparatorChar);
    if (resultFolders.Any() && Directory.Exists(Path.Combine(gamePath, "mods")))
    {
      if (gamePath != AppSetting.GamePath)
      {
        AppSetting.GamePath = gamePath;
        WeakReferenceMessenger.Default.Send(new GamePathChangedMessage(gamePath));
      }
    }
    else
    {
      await App.Current.Services.GetRequiredService<IDialogService>().ShowDialogAsync(new DialogRequest
      {
        Header = "错误",
        Content = "不合法的路径"
      });
    }
  }

  [RelayCommand]
  private async Task Update()
  {
    var dialogService = App.Current.Services.GetRequiredService<IDialogService>();
    var mgr = new UpdateManager(new GithubSource("https://github.com/AIKA1024/MAZDA_MCToolPublish", null, false));
#if DEBUG
    if (!mgr.IsInstalled)
    {
      await dialogService.ShowDialogAsync(new DialogRequest
      {
        Header = "提示",
        Content = "开发环境就不要点更新了"
      });
      return;
    }
#endif
    _updateInfo ??= await mgr.CheckForUpdatesAsync();
    if (_updateInfo == null)
    {
      await dialogService.ShowDialogAsync(new DialogRequest
      {
        Header = "提示",
        Content = "无可用更新"
      });
      return;
    }

    long size = 0;
    if (_updateInfo.DeltasToTarget.Length > 0)
      size += _updateInfo.DeltasToTarget.Sum(delta => delta.Size);
    else
      size =  _updateInfo.TargetFullRelease.Size;

    
    await dialogService.ShowDialogAsync(new DialogRequest
    {
      Header = "提示",
      Subheader = "有新版本可下载",
      Content = $"需要下载{size/1048576}MB",
      CommandText = "下载",
      CommandDescription = "点击开始下载",
      CommandIconSource = new SymbolIconSource { Symbol = Symbol.Download },
      Command = new AsyncRelayCommand(async () =>
      {
        App.Current.Services.GetRequiredService<InfoBarService>().ShowMessage("提示","开始下载新版本",delayToClose:2000);
        var updateTask = mgr.DownloadUpdatesAsync(_updateInfo);
        await updateTask;
        if (updateTask.IsCompletedSuccessfully)
          mgr.ApplyUpdatesAndRestart(_updateInfo);
      })
    });
  }
}