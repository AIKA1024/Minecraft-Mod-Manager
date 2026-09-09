using System.Windows.Input;
using Avalonia;
using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace MAZDA_MCTool.Models;

public class DialogRequest:IDialogRequest
{
  public string Header { get; set; } = "";
  public string Subheader { get; set; } = "";
  public object? Content { get; set; } = "";
  public AvaloniaObject? IconSource { get; set; }
  public ICommand? Command{ get; set; }
  public string CommandText { get; set; } = "";
  public string? CommandDescription { get; set; }
  public AvaloniaObject? CommandIconSource { get; set; }
  public bool CloseOnCommandInvoked { get; set; } = true;
  public bool ShowProgressBar { get;set; } = false;
  public bool ProgressBarIsIndeterminate { get;set; } = false;
}