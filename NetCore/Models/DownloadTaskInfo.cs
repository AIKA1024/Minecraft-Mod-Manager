using System.ComponentModel;
using System.Runtime.CompilerServices;
using MAZDA_MCTool.Abstraction.Contracts.Models;

namespace NetCore.Models;

public class DownloadTaskInfo:IDownloadTaskInfo,INotifyPropertyChanged
{
  public event PropertyChangedEventHandler? PropertyChanged;
  
  public string Name { get; set; }
  
  private double _progress;

  public double Progress
  {
    get => _progress;
    set
    {
      if (Math.Abs(value - _progress) < 0.1) return;
      _progress = value;
      OnPropertyChanged();
    }
  }

  public DownloadTaskInfo(string name,double progress)
  {
    Name = name;
    Progress = progress;
  }
  
  protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}