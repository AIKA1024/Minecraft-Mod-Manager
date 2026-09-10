using System.Windows.Input;
using Avalonia;

namespace MAZDA_MCTool.Abstraction.Contracts.Models;

public interface IDialogRequest
{
  public string Header { get; set; }
  public string Subheader { get; set; }
  public object? Content { get; set; }
  public AvaloniaObject? IconSource { get; set; }
  public ICommand? Command { get; set; }
  public string CommandText { get; set; }
  public string? CommandDescription { get; set; }
  public AvaloniaObject? CommandIconSource { get; set; }
  public bool CloseOnCommandInvoked { get; set; }
  public bool ShowProgressBar { get;set; }
  public bool ProgressBarIsIndeterminate { get;set; }
}