using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAZDA_MCTool.Abstraction.Contracts.Models.Events;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using MAZDA_MCTool.UI.Models;
using MAZDA_MCTool.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MAZDA_MCTool.ViewModels;

public partial class DownloadPageViewModel : ObservableObject
{
  public AvaloniaList<TempDownloadModelClass> TaskList { get; } = [];

  private ConcurrentDictionary<string, TempDownloadModelClass> TaskDictionary { get; } = [];

  private List<TempDownloadModelClass> _completedTasks = [];

  [ObservableProperty]
  private double _overAllProgress;

  public DownloadPageViewModel()
  {
    var netServices = App.Current.Services.GetServices<INetService>();
    foreach (var netService in netServices)
    {
      netService.DownloadProgressStarted += OnDownloadProgressStarted;
      netService.DownloadProgressChanged += OnDownloadProgressChanged;
      netService.DownloadProgressCompleted += OnDownloadProgressCompleted;
    }
  }

  [RelayCommand]
  private void CancelDownload(TempDownloadModelClass info)
  {
    App.Current.Services.GetRequiredService<InfoBarService>().ShowMessage("提示","尚未实现",severity:InfoSeverity.Informational,delayToClose:3000);
  }
  
  private void OnDownloadProgressCompleted(DownloadCompletedHandlerEventArgs e)
  {
    if (TaskDictionary.TryGetValue(e.Name, out var value))
    {
      value.Progress = 100;
      value.IsSucceed = true;
      _completedTasks.Add(value);
      OverAllProgress = (double)_completedTasks.Count / TaskList.Count;
    }
  }

  private void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
  {
    if (TaskDictionary.TryGetValue(e.Name, out var value))
    {
      value.Progress = e.Progress;
    }
  }

  private void OnDownloadProgressStarted(DownloadProgressStartedEventArgs e)
  {
    var info = new TempDownloadModelClass { Name = e.Name, Progress = 0 };
    if (TaskDictionary.TryRemove(e.Name, out var value))
      TaskList.Remove(value);
    if (TaskDictionary.TryAdd(e.Name, info))
      TaskList.Add(info);
  }
}

public partial class TempDownloadModelClass : ObservableObject
{
  [ObservableProperty] private string _name = string.Empty;
  [ObservableProperty] private double _progress;
  [ObservableProperty] private bool? _isSucceed;
}