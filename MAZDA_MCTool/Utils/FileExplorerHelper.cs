using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MAZDA_MCTool.Utils;
public static class FileManagerHelper
{
    /// <summary>
    /// 打开系统资源管理器并高亮选中多个文件
    /// </summary>
    /// <param name="filePaths">需要选中的文件绝对路径集合</param>
    public static void RevealMultipleInExplorer(IEnumerable<string> filePaths)
    {
        // 过滤掉不存在的文件
        var validPaths = filePaths.Where(p => File.Exists(p) || Directory.Exists(p)).ToList();
        if (validPaths.Count == 0) return;

        // 必须按目录分组，因为一个窗口只能展示一个目录的内容
        var groupedPaths = validPaths.GroupBy(p => Path.GetDirectoryName(p));

        foreach (var group in groupedPaths)
        {
            string directory = group.Key ?? string.Empty;
            List<string> itemsInDir = group.ToList();

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    OpenInWindows(directory, itemsInDir);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    OpenInMacOS(itemsInDir);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    OpenInLinux(itemsInDir);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"无法在 {directory} 选中文件: {ex.Message}");
            }
        }
    }

    #region Windows Implementation

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr ILCreateFromPathW(string pszPath);

    [DllImport("shell32.dll")]
    private static extern void ILFree(IntPtr pidl);

    [DllImport("shell32.dll")]
    private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr[] apidl, uint dwFlags);

    private static void OpenInWindows(string directory, List<string> items)
    {
        IntPtr dirPidl = IntPtr.Zero;
        IntPtr[] itemPidls = new IntPtr[items.Count];

        try
        {
            dirPidl = ILCreateFromPathW(directory);
            for (int i = 0; i < items.Count; i++)
            {
                itemPidls[i] = ILCreateFromPathW(items[i]);
            }

            // 调用 Windows 原生 API 选中多个项目
            SHOpenFolderAndSelectItems(dirPidl, (uint)itemPidls.Length, itemPidls, 0);
        }
        finally
        {
            // 释放非托管内存，防止内存泄漏
            if (dirPidl != IntPtr.Zero) ILFree(dirPidl);
            foreach (var pidl in itemPidls)
            {
                if (pidl != IntPtr.Zero) ILFree(pidl);
            }
        }
    }

    #endregion

    #region macOS Implementation

    private static void OpenInMacOS(List<string> items)
    {
        // 构建 AppleScript，将路径转为 POSIX file 格式
        var posixFiles = items.Select(p => $"POSIX file \"{p.Replace("\"", "\\\"")}\"");
        string fileArray = string.Join(", ", posixFiles);
        
        string script = $@"
            tell application ""Finder""
                activate
                reveal {{{fileArray}}}
            end tell";

        Process.Start(new ProcessStartInfo
        {
            FileName = "osascript",
            Arguments = $"-e '{script}'",
            UseShellExecute = true
        });
    }

    #endregion

    #region Linux Implementation

    private static void OpenInLinux(List<string> items)
    {
        // 将本地路径转换为 file:// URI 格式
        var uris = items.Select(p => $"\"file://{p.Replace("\"", "\\\"")}\"");
        string uriArray = string.Join(",", uris);

        // 使用 dbus-send 调用 Freedesktop 的标准文件管理器接口
        // 兼容 Nautilus (Ubuntu), Dolphin (KDE), Nemo 等绝大多数现代 Linux 桌面
        string arguments = $"--session --dest=org.freedesktop.FileManager1 --type=method_call " +
                           $"/org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems " +
                           $"array:string:{uriArray} string:\"\"";

        Process.Start(new ProcessStartInfo
        {
            FileName = "dbus-send",
            Arguments = arguments,
            UseShellExecute = true
        });
    }

    #endregion
}