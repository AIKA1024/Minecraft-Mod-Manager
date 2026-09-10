using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using ModLoadCore.Models;
using Tomlyn;

namespace ModLoadCore.Services;

public class ForgeModParserService : IModParserStrategy
{
  public async Task ParserAsync(IModFile modFile, IModInfo modInfo)
  {
    // 尝试解析 mcmod.info (旧版 Forge)
    try
    {
      var entryStream = modFile.GetEntryStream("mcmod.info");
      if (entryStream != null)
      {
        using var jsonDoc = await JsonDocument.ParseAsync(entryStream).ConfigureAwait(false);
        var root = jsonDoc.RootElement;
        JsonElement infoElement;
        if (root.ValueKind == JsonValueKind.Array)
          infoElement = root[0];
        else
          infoElement = root.GetProperty("modList")[0];

        if (infoElement.TryGetProperty("Description", out var name))
          modInfo.Name = name.GetString() ?? string.Empty;
        if (infoElement.TryGetProperty("Description", out var description))
          modInfo.Description = description.GetString() ?? string.Empty;
        if (infoElement.TryGetProperty("version", out var versionValue))
        {
          var versionStr = versionValue.GetString();
          modInfo.Version =
              versionStr != null && Regex.IsMatch(versionStr, @"^\$\{.*\}$")
                  ? string.Empty
                  : versionStr ?? string.Empty;
        }
        if (infoElement.TryGetProperty("logoPath", out var logoPath))
          modInfo.Logo = await modFile.GetLogoAsync(logoPath.GetString(), new Avalonia.PixelSize(32, 32)).ConfigureAwait(false);
      }
      else
      {
        // 尝试解析 META-INF/mods.toml (高版本Forge或NeoForge)
        entryStream = modFile.GetEntryStream("META-INF/mods.toml")
                      ?? modFile.GetEntryStream("META-INF/neoforge.mods.toml");
        if (entryStream == null)
          return;

        using (var memoryStream = new MemoryStream())
        {
          await entryStream.CopyToAsync(memoryStream).ConfigureAwait(false);
          var tomlStr = Encoding.UTF8.GetString(memoryStream.ToArray());
          if (string.IsNullOrEmpty(tomlStr))
            return;
          var model = Toml.ToModel<TomlRootModel>(tomlStr, options: new TomlModelOptions
          {
            IgnoreMissingProperties = true,
            ConvertPropertyName = name => char.ToLowerInvariant(name[0]) + name.Substring(1)
          });

          modInfo.Name = model.Mods.FirstOrDefault()?.DisplayName ?? string.Empty;
          modInfo.Description = model.Mods.FirstOrDefault()?.Description ?? string.Empty;
          var versionValue = model.Mods.FirstOrDefault()?.Version;
          if (string.IsNullOrEmpty(modInfo.Version))
          {
            modInfo.Version =
              versionValue != null && Regex.IsMatch(versionValue, @"^\$\{.*\}$")
                ? string.Empty
                : versionValue ?? string.Empty;
          }

          // 获取 displayURL 和转换为 Uri 类型
          var displayUrlStr = model.Mods.FirstOrDefault()?.DisplayUrl;
          modInfo.Website ??= !string.IsNullOrEmpty(displayUrlStr) ? new Uri(displayUrlStr) : null;
          modInfo.Logo ??= await modFile.GetLogoAsync(model.Mods.FirstOrDefault()?.LogoFile, new Avalonia.PixelSize(32, 32)).ConfigureAwait(false);
          modInfo.Path = modFile.Path;
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error parsing mcmod.info: {ex.Message}");
      modInfo.IsZipValid = false;
    }
  }
}