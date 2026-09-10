using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using System.IO;

namespace MAZDA_MCTool.Models;

public partial class Setting : ObservableObject
{
  //使用ObservableProperty生成的属性不能被aot后json序列化
  public string GamePath
  {
    get;
    set => SetProperty(ref field, value);
  } = string.Empty;

  public double Width
  {
    get;
    set => SetProperty(ref field, value);
  }

  public double Height
  {
    get;
    set => SetProperty(ref field, value);
  }

  public PixelPoint Position
  {
    get;
    set => SetProperty(ref field, value);
  }

  [JsonIgnore]
  public string ModPath => Path.Join(GamePath, "mods");
}