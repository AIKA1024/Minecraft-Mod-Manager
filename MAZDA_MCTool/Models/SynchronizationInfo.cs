using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace MAZDA_MCTool.Models;

public class SynchronizationInfo:IHashInfo
{
  public string Name { get; set; } =  string.Empty;
  public string Sha1Hash { get; set; } =  string.Empty;
  public uint Fingerprint { get; set; }
}