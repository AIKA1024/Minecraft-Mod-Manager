using Avalonia.Controls;
using MAZDA_MCTool.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;

namespace MAZDA_MCTool;

public partial class MainWindow : AppWindow
{
  public MainWindow()
  {
    InitializeComponent();
    
    if (App.Current.TryGetResource("LayerOnMicaBaseAltFillColorSecondaryBrush", out var buttonHoverBackgroundColor) 
        && buttonHoverBackgroundColor is SolidColorBrush brush1)
      TitleBar.ButtonHoverBackgroundColor = brush1.Color;
    if (App.Current.TryGetResource("SolidBackgroundFillColorBaseAltBrush", out var buttonPressedBackgroundColor) 
        && buttonPressedBackgroundColor is SolidColorBrush brush2)
      TitleBar.ButtonPressedBackgroundColor = brush2.Color;
    TitleBar.Height = 48;
  }

  private void TopLevel_OnClosed(object? sender, EventArgs e)
  {
    App.Current.Services.GetRequiredService<SettingService>().SaveSetting();
  }
}