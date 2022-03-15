# KMCCC

由原 KMCCC Fork 而来，主要移植到 .NET 6、合并 Basic 和 Shared、、重命名 Basic 到 Core、重命名 Pro 到 Extensions，并移除启动报告功能。

## 功能

### Core

基本启动功能

- 基本的启动功能
- 离线与在线模式严重
- Java与系统信息的查询
- ZIP压缩格式支持
- 基本的版本定位器（JVersion）
- 一些有用的扩展
- 自定义验证服务器

### Extensions

对 Core 的扩展（必须与 Core 配合使用）

- Mojang API
- 游戏文件的下载和校验 (包括 Version, Library, Native&Assets)

## 实例

### 如何初始化 LauncherCore

```csharp

LauncherCore core = LauncherCore.Create(
    new LauncherCoreCreationOption(
        javaPath: Config.Instance.JavaPath, // 默认为找到的第一个版本
        gameRootPath: null, // 默认为 ./.minecraft/
        versionLocator: the Version Locator // 默认情况下将会 new JVersionLocator()
    ));

```

### 如何找到 Versions（指定游戏版本）

```csharp

var versions = core.GetVersions();

var version = core.GetVersion("1.8");

```

无效的版本将会被忽略。

### 如何启动 Minecraft

```csharp
var result = core.Launch(new LaunchOptions
{
    Version = App.LauncherCore.GetVersion(server.VersionId)
    Authenticator = new OfflineAuthenticator("Steve"), // 离线模式启动
    //Authenticator = new YggdrasilLogin("*@*.*", "***", true), // 在线模式
    MaxMemory = Config.Instance.MaxMemory, // 可选
    MinMemory = Config.Instance.MaxMemory, // 可选
    Mode = LaunchMode.MCLauncher, // 可选
    Server = new ServerInfo {Address = "mc.hypixel.net"}, //可选
    Size = new WindowSize {Height = 768, Width = 1280} //可选
}, (Action<MinecraftLaunchArguments>) (x => { })); // 可选 ( 启动前修改参数
```

## 基本改动

- [ ] 所有的文档全部改写为英文并且变的严肃，并遵循 Microsoft Docs 文档格式
- [ ] 将不必要的内容删除
- [ ] 升级到 .NET 6
- [ ] 移除启动报告

## 许可证

本项目依照 GNU LGPL v3（及以后版本）许可。
