using Avalonia;
using Avalonia.Media.Imaging;
using MAZDA_MCTool.Abstraction.Contracts.Models;
using MAZDA_MCTool.Abstraction.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ModLoadCore.Services;

public static class LoadServicesExtensions
{
  public static void AddLoadServices(this IServiceCollection services)
  {
    services.AddTransient<IModParserStrategy>(_ =>new FabricModParserService());
    services.AddTransient<IModParserStrategy>(_=>new ForgeModParserService());
    // services.AddTransient<IModParserStrategy>(_=>new LiteLoaderParserService());
    services.AddTransient<HashService>();
  }

  public static Uri? ToUri(this string? uriStr)
  {
    if (string.IsNullOrEmpty(uriStr))
      return null;
    if (!uriStr.StartsWith("http", StringComparison.OrdinalIgnoreCase))
      uriStr = "https://" + uriStr; // 默认使用 https 协议
    return Uri.TryCreate(uriStr, UriKind.Absolute, out var uri) ? uri : null;
  }

  private static async Task<Bitmap?> ToIImageBrushSource(this Stream? stream, PixelSize pixelSize)
  {
    if (stream is { CanRead: true })
    {
      try
      {
        using var logoMemoryStream = new MemoryStream();
        await stream.CopyToAsync(logoMemoryStream);  // 复制到内存流
        if (logoMemoryStream.Length == 0)
          return null;
        logoMemoryStream.Position = 0;  // 重置位置
        using var bitmap = new Bitmap(logoMemoryStream);
        return bitmap.CreateScaledBitmap(pixelSize);  // 从内存流加载图片
      }
      catch (ArgumentException ex)
      {
        Console.WriteLine($"{ex.Message}");
      }
    }
    return null;
  }

  public static async Task<Bitmap?> GetLogoAsync(this IModFile modFile, string? logoPath,PixelSize pixelSize)
  {
    if (string.IsNullOrEmpty(logoPath))
      return null;

    var logoStream = modFile.GetEntryStream(logoPath);
    if (logoStream == null) return null;

    return await logoStream.ToIImageBrushSource(pixelSize);
  }
}