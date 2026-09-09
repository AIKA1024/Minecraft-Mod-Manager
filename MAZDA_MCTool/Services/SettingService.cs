using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using MAZDA_MCTool.Models;
using MAZDA_MCTool.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace MAZDA_MCTool.Services;

public class SettingService
{
    public Setting GetSetting()
    {
        var settingStr = File.ReadAllText(App.Current.SettingPath,Encoding.Unicode);
        if (string.IsNullOrEmpty(settingStr))
            return new Setting();

        return JsonSerializer.Deserialize(settingStr,
                   JsonContext.Default.Setting) ??
               new Setting();
    }

    public void SaveSetting()
    {
        File.WriteAllText(App.Current.SettingPath,
            JsonSerializer.Serialize(App.Current.Services.GetRequiredService<Setting>(),
                JsonContext.Default.Setting), System.Text.Encoding.Unicode);


        // var setting = App.Current.Services.GetRequiredService<Setting>();
        // if (setting != null)
        // {
        //     Console.WriteLine($"GamePath: {setting.GamePath}, Width: {setting.Width}, Height: {setting.Height}");
        //     File.WriteAllText(App.SettingPath,
        //         JsonSerializer.Serialize(setting, SettingJsonContext.Default.Setting));
        // }
        // else
        // {
        //     Console.WriteLine("Setting object is null");
        // }
    }
}