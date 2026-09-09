using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactivity;

namespace MAZDA_MCTool.Behaviors;

public class DropFileBehavior:Behavior<Control>
{
  public static readonly StyledProperty<IEnumerable<IStorageItem>?> FilesProperty =
    AvaloniaProperty.Register<DropFileBehavior, IEnumerable<IStorageItem>?>(
      nameof(Files),
      defaultValue: []);
  public IEnumerable<IStorageItem>? Files
  {
    get => GetValue(FilesProperty);
    set => SetValue(FilesProperty, value);
  }

  protected override void OnAttached()
  {
    base.OnAttached();
    
    AssociatedObject!.SetValue(DragDrop.AllowDropProperty, true);
    AssociatedObject.AddHandler(DragDrop.DropEvent, OnDrop, RoutingStrategies.Bubble);
  }

  protected override void OnDetaching()
  {
    base.OnDetaching();
    
    AssociatedObject!.SetValue(DragDrop.AllowDropProperty, false);
    AssociatedObject.RemoveHandler(DragDrop.DropEvent, OnDrop);
  }

  private void OnDrop(object? sender, DragEventArgs e)
  {
    Files = e.Data.GetFiles();
  }
}