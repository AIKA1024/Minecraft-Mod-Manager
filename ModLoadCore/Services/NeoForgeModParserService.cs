using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Services;

namespace ModLoadCore.Services;

public class NeoForgeModParserService:IModParserStrategy//NeoForge和Forge的文件格式大差不差，只有文件名不一样，先不考虑创建新的ParserService
{
  public Task ParserAsync(IModFile modFile, IModInfo modInfo)
  {
    throw new NotImplementedException();
  }
}