using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MAZDA_MCTool.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using HotAvalonia;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using MAZDA_MCTool.Models;
using MAZDA_MCTool.Pages;
using MAZDA_MCTool.Services;
using MAZDA_MCTool.UI.Services;
using ModLoadCore.Services;
using NetCore.Services;

namespace MAZDA_MCTool;

public class App : Application
{
  public IServiceProvider Services { get; }

  public new static App Current => (App)Application.Current!;
  public string SettingPath { get; } = Path.Join("..", "settings.json");
  public string ModCachePath { get; } = Path.Join("..", "Cache", "ModCache");
  public string IconCachePath { get; } = Path.Join("..", "Cache", "IconCache");


  public string ModPackName
  {
    get
    {
      var gamePath = Current.Services.GetRequiredService<Setting>().GamePath;
      var files = Directory.GetFiles(gamePath, "*.jar");
      return files.Length == 0
        ? new FileInfo(gamePath).Directory!.Name
        : Path.GetFileNameWithoutExtension(files[0]);
    }
  }


  public App()
  {
    if (!File.Exists(SettingPath))
      File.Create(SettingPath).Close();
    Directory.CreateDirectory(ModCachePath);
    Directory.CreateDirectory(IconCachePath);
    Services = ConfigureServices();
  }

  private IServiceProvider ConfigureServices()
  {
    var services = new ServiceCollection();
    services.AddSingleton<MainWindow>(sp => new MainWindow
      { DataContext = sp.GetRequiredService<MainWindowViewModel>() });
    services.AddSingleton<MainWindowViewModel>();
    services.AddSingleton<ViewModPage>(sp => new ViewModPage
      { DataContext = sp.GetRequiredService<ViewModPageViewModel>() });
    services.AddSingleton<DownloadPage>(sp=>new DownloadPage{DataContext = sp.GetRequiredService<DownloadPageViewModel>()});
    services.AddSingleton<DownloadPageViewModel>();
    services.AddSingleton<ViewModPageViewModel>();
    services.AddSingleton<SettingPageViewModels>();
    services.AddLoadServices();
    services.AddNetServices();
    services.AddSingleton<InfoBarService>();
    services.AddTransient<IDialogService>(_=>new DialogService());
    services.AddSingleton<CaCheService>();
    services.AddSingleton<Setting>(sp => sp.GetRequiredService<SettingService>().GetSetting());
    services.AddSingleton<SettingService>();
    services.AddSingleton<SettingPage>(sp => new SettingPage
      { DataContext = sp.GetRequiredService<SettingPageViewModels>() });
    services.AddSingleton(sp=>TopLevel.GetTopLevel(sp.GetRequiredService<MainWindow>())!);
    return services.BuildServiceProvider();
  }

  public override void Initialize()
  {
    this.EnableHotReload();
    AvaloniaXamlLoader.Load(this);
    
  }

  public override void OnFrameworkInitializationCompleted()
  {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = Services.GetRequiredService<MainWindow>();
    }
    Services.GetRequiredService<DownloadPageViewModel>();//需要订阅事件,所以立即实例化
    base.OnFrameworkInitializationCompleted();
  }
}