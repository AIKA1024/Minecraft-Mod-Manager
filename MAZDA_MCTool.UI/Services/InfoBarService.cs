using Avalonia.Collections;
using MAZDA_MCTool.UI.Models;

namespace MAZDA_MCTool.UI.Services;

public class InfoBarService
{
  public AvaloniaList<InfoMessage> Messages { get; } = [];

  public void ShowMessage(string title, string message,string buttonStr = "", InfoSeverity severity = InfoSeverity.Informational,int delayToClose = 0,Action? action = null)
  {
    var infoMessage = new InfoMessage
    {
      Title = title,
      Message = message,
      Severity = severity,
      ButtonStr = buttonStr,
      ButtonAction = action
    };
    Messages.Insert(0,infoMessage);

    if (delayToClose>0)
    {
      Task.Delay(delayToClose).ContinueWith(_ =>
      {
        Messages.Remove(infoMessage);
        
        // App.Current?.Dispatcher.UIThread.Post(() =>
        // {
        //   
        // });
      });
    }
  }
}