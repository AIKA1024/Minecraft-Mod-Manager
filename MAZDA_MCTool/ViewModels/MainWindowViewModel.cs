using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAZDA_MCTool.Enums;
using MAZDA_MCTool.Pages;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Collections;
using MAZDA_MCTool.UI.Models;
using MAZDA_MCTool.UI.Services;

namespace MAZDA_MCTool.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{

  [ObservableProperty] private Control _currentPage = App.Current.Services.GetRequiredService<ViewModPage>();
  public AvaloniaList<InfoMessage> Infos => App.Current.Services.GetRequiredService<InfoBarService>().Messages;

  [RelayCommand]
  private void ChangePage(PageType pageType)
  {
    switch (pageType)
    {
      case PageType.Mod:
        CurrentPage = App.Current.Services.GetRequiredService<ViewModPage>();
        break;
      case PageType.Settings:
        CurrentPage = App.Current.Services.GetRequiredService<SettingPage>();
        break;
      case PageType.DownLoad:
        CurrentPage = App.Current.Services.GetRequiredService<DownloadPage>();
        break;
    }
  }
}