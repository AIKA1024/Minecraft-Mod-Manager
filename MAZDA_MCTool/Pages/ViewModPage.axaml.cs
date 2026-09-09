using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MAZDA_MCTool.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MAZDA_MCTool.Pages;

public partial class ViewModPage : UserControl
{
  public ViewModPage()
  {
    InitializeComponent();
  }

  private void StyledElement_OnInitialized(object? sender, EventArgs e)
  {
    App.Current.Services.GetRequiredService<ViewModPageViewModel>().InitializationCommand.Execute(null);
  }
}