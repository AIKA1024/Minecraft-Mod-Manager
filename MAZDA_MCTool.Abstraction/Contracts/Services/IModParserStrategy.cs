using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace MAZDA_MCTool.Abstraction.Contracts.Services;

public interface IModParserStrategy
{
    Task ParserAsync(IModFile modFile, IModInfo modInfo);//todo 可以标记一下可能是损坏的文件，并且现在解析损坏的文件没做处理
}