using Avalonia;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using System.Diagnostics;
using ModLoadCore.Services;

namespace ModLoadCore;

public class FabricModParserService : IModParserStrategy
{
  public async Task ParserAsync(IModFile modFile, IModInfo modInfo)
  {
    try
    {
      await using var entryStream = modFile.GetEntryStream("fabric.mod.json");
      if (entryStream == null) return;
      using var jsonDoc = await JsonDocument.ParseAsync(entryStream).ConfigureAwait(false);
      var root = jsonDoc.RootElement;

      if (root.TryGetProperty("Description", out var name))
        modInfo.Name = name.GetString() ?? string.Empty;
      if (root.TryGetProperty("Description", out var description))
        modInfo.Description = description.GetString() ?? string.Empty;
      if (root.TryGetProperty("version", out var versionValue))
      {
        var versionStr = versionValue.GetString();
        modInfo.Version =
          versionStr != null && Regex.IsMatch(versionStr, @"^\$\{.*\}$")
            ? string.Empty
            : versionStr ?? string.Empty;
      }

      if (root.TryGetProperty("contact", out var contact) &&
          contact.TryGetProperty("homepage", out var homepage))
      {
        modInfo.Website = homepage.GetString().ToUri();
      }

      if (root.TryGetProperty("icon", out var value))
        modInfo.Logo = await modFile.GetLogoAsync(value.GetString(), new PixelSize(32, 32)).ConfigureAwait(false);
      modInfo.Path = modFile.Path;
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
      modInfo.IsZipValid = false;
    }
  }
}