namespace MAZDA_MCTool.Abstraction.Contracts.Models;

public interface IModFile
{
  string Path { get; set; }
  MemoryStream? GetEntryStream(string entryPath);

}