namespace MAZDA_MCTool.Abstraction.Contracts.Models;

public interface IHashInfo
{
  string Sha1Hash { get; set; }
  uint Fingerprint{ get; set; }
}