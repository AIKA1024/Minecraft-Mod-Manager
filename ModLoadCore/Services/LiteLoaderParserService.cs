using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Services;

namespace ModLoadCore.Services;

public class LiteLoaderParserService : IModParserStrategy
{

  public Task ParserAsync(IModFile modFile, IModInfo modInfo)
  {
    throw new NotImplementedException();
  }
}