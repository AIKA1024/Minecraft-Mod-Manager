using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using ModLoadCore.Services;

namespace ModLoadCore;


public class LoadModEngine()
{
  private readonly IModParserStrategy[] _parserStrategies;
  private readonly HashService _sha1HashService = new();
  public LoadModEngine(params IModParserStrategy[] modParserStrategies) : this()
  {
    if (modParserStrategies.Length == 0)
      throw new InvalidOperationException("Provide at least one strategy");
    _parserStrategies = modParserStrategies;
  }

  public async Task<IModInfo> ParserAsync(IModFile modFile, IModInfo modInfo,bool calculateSha1Hash = true)
  {
    if (calculateSha1Hash)
    {
      modInfo.Sha1Hash = await _sha1HashService.ComputeSha1HashAsync(modFile.Path).ConfigureAwait(false);
      modInfo.Fingerprint = await _sha1HashService.ComputeCurseForgeHashAsync(modFile.Path).ConfigureAwait(false);
    }
    foreach (var parserStrategy in _parserStrategies)
    {
      await parserStrategy.ParserAsync(modFile, modInfo);
      if (!string.IsNullOrEmpty(modInfo.Name))
        return modInfo;
    }
    return modInfo;
  }
}