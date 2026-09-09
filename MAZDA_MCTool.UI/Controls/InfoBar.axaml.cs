using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using MAZDA_MCTool.UI.Models;

namespace MAZDA_MCTool.UI.Controls;

public class InfoBar : ItemsControl
{
  public static readonly StyledProperty<double> SpacingProperty =
    AvaloniaProperty.Register<InfoBar, double>(
      nameof(Spacing),
      defaultValue: 0);

  public double Spacing
  {
    get => GetValue(SpacingProperty);
    set => SetValue(SpacingProperty, value);
  }

  public InfoBar()
  {
    AddHandler(Button.ClickEvent, OnButtonClick, RoutingStrategies.Bubble);
  }
  private void OnButtonClick(object? sender, RoutedEventArgs e)
  {
    if (e.Source is Button { Tag: InfoMessage msg } && ItemsSource is IList list)
    {
      list.Remove(msg);
      e.Handled=true;
    }
  }
}