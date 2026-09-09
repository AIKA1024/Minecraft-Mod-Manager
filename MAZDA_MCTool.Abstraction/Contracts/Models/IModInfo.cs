using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace MAZDA_MCTool.Abstraction.Contracts.Models;

public interface IModInfo:IHashInfo,IDisposable//todo 💩 具体业务逻辑模型感觉不应该这样抽象，其他项目也不应该依赖这个
{
  string Name { get; set; }
  
  string Description { get; set; }
  string Version { get; set; }
  string Path { get; set; }
  Uri? Website { get; set; }
  Bitmap? Logo { get; set; }
  string LogoPath{ get; set; }
  bool IsZipValid{ get; set; }
}