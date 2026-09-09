using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace MAZDA_MCTool.Models;

public partial class ModInfo : ObservableObject, IModInfo
{
  public string Name { get; set=>SetProperty(ref field,value); } = string.Empty;
  public string Description { get; set=>SetProperty(ref field,value); } = string.Empty;
  public string Version { get; set=>SetProperty(ref field,value); } = string.Empty;
  public string Path { get; set=>SetProperty(ref field,value); }
  public string Sha1Hash { get; set=>SetProperty(ref field,value); } = string.Empty;
  /// <summary>
  /// CurseForgeHash
  /// </summary>
  public uint Fingerprint { get; set=>SetProperty(ref field,value); }
  public Uri? Website { get; set=>SetProperty(ref field,value); }
  [JsonIgnore]
  public Bitmap? Logo { get; set=>SetProperty(ref field,value); }

  public string LogoPath
  {
    get;
    set
    {
      SetProperty(ref field,value);
      if (File.Exists(LogoPath) && Logo == null)
        Logo = new Bitmap(LogoPath);
    }
  } = string.Empty;

  public bool IsZipValid { get; set=>SetProperty(ref field,value); }

  public ModInfo(string path)
  {
    Path = path;
  }

  public void Dispose()
  {
    Logo?.Dispose();
    Logo = null;
  }
}