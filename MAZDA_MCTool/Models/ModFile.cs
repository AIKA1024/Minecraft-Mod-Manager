using System.IO;
using System.IO.Compression;
using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace MAZDA_MCTool.Models;

public class ModFile : IModFile
{
  public string Path { get; set; } = "";

  public MemoryStream? GetEntryStream(string entryPath)
  {
    using ZipArchive archive = ZipFile.OpenRead(Path);
    var entry = archive.GetEntry(entryPath);
    if (entry == null) return null;
    
    // 将 entry 内容复制到内存流中，然后返回内存流
    MemoryStream memoryStream = new MemoryStream();
    using (var entryStream = entry.Open())
    {
      entryStream.CopyTo(memoryStream);
    }
    memoryStream.Position = 0;
    return memoryStream;
  }
}