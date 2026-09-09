using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace MAZDA_MCTool.UI.Models;

public partial class InfoMessage
{
  public string Title { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public InfoSeverity Severity { get; set; }
  
  public string ButtonStr { get; set; } = string.Empty;
  public Action? ButtonAction { get; set; }
  
  [RelayCommand]
  private void StartAction()
  {
    ButtonAction?.Invoke();
  }
}

public enum InfoSeverity
{
  Informational,
  Success,
  Warning,
  Error
}
