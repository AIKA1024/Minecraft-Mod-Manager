using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MAZDA_MCTool.Services;

public class DialogService : IDialogService
{
  public async Task<object> ShowDialogAsync(IDialogRequest request)
  {
    var td = new TaskDialog
    {
      // Title property only applies on Windowed dialogs

      Header = request.Header,
      SubHeader = request.Subheader,
      Content = request.Content,
      IconSource = (IconSource)request.IconSource,
      Buttons =
      {
        new TaskDialogButton("取消",false),
        new TaskDialogButton("确定",true)
      },
      XamlRoot = App.Current.Services.GetRequiredService<TopLevel>()
    };
    if (request.Command is not null)
    {
      td.Commands =[new TaskDialogCommand
              {
                Text = request.CommandText,
                Description = request.CommandDescription,
                Command = request.Command,
                DialogResult = "CommandResult",
                // ClosesOnInvoked property lets you choose if invoking this command closes the dialog
                // automatically (default true)
                ClosesOnInvoked = request.CloseOnCommandInvoked,
                IconSource = (IconSource)request.CommandIconSource
                // Can also set IsEnabled
              }];
    }
    return await td.ShowAsync(true);
  }
}