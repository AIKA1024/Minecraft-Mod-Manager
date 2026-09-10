# MAZDA_MCTool — Minecraft 模组管理器

基于 **Avalonia UI + .NET** 开发的 Windows 桌面 Minecraft 模组管理工具，支持模组识别、下载、更新与管理。

## 功能特性

- **模组信息解析**：扫描本地 mods 目录，自动计算文件 SHA-1 与 CurseForge 指纹，并通过策略模式解析模组内嵌元数据，支持 Forge / NeoForge（`mods.toml`）、Fabric（`fabric.mod.json`）、LiteLoader 等加载器
- **指纹匹配**：对没有内嵌元数据的模组，通过 CurseForge Fingerprint API 按指纹识别项目信息
- **模组清单同步**：一键生成模组清单文件（模组名 + SHA-1 + 指纹）；将清单分享给他人后，对方导入即可自动比对出缺少的模组并批量下载补齐，多余的模组可选择删除——适合整合包分享与多人环境同步
- **在线更新检测**：对接 CurseForge / Modrinth API，批量获取模组最新版本与下载链接
- **下载管理**：多任务下载，实时进度上报（进度条 / InfoBar 通知）
- **本地缓存**：模组信息 SHA-1 键值缓存，减少重复网络请求
- **自更新**：集成 Velopack 实现应用内自动更新（`VelopackPublish.ps1` 为打包发布脚本）

## 技术栈

| 项目 | 说明 |
|---|---|
| `MAZDA_MCTool` | Avalonia UI 桌面主程序（MVVM，CommunityToolkit.Mvvm 消息通信） |
| `MAZDA_MCTool.UI` | 通用 UI 组件库（自定义控件、主题、动画 Behavior） |
| `ModLoadCore` | 模组文件解析引擎（多解析器策略模式） |
| `NetCore` | 网络层（CurseForge / Modrinth API 客户端、下载引擎） |
| `MAZDA_MCTool.Abstraction` | 接口与契约层（依赖倒置） |

## 构建与运行

```bash
git clone https://github.com/AIKA1024/Minecraft-Mod-Manager.git
cd Minecraft-Mod-Manager
dotnet run --project MAZDA_MCTool
```

环境要求：.NET 10 SDK、Windows 10+（x64）。

## 配置 CurseForge API Key

本项目使用 [CurseForge for Studio](https://console.curseforge.com/) 提供的免费 API。请前往
[CurseForge 开发者控制台](https://console.curseforge.com/#/account/api-tokens)
申请你自己的 Api Key，然后替换
`NetCore/Services/CurseForgeNetService.cs` 中的占位符：

```csharp
private const string ApiKey = "输入你自己的curseforge密钥";
```

> ⚠️ 请勿将你自己的 API Key 提交到公开仓库。

## 许可证

仅供学习交流使用。
