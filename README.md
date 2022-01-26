# KMCCC

This is basically a port of KMCCC to the modern .NET. Also it merges Shared and Basic, rename Basic to Core, rename Pro to Extensions, and removed tracking.

## Features

### Core Module

The basic launching features.

- Launching (LauncherCore, LaunchOptions, ...)
- Authentication (Yggdrasil, Offline, ...)
- Java & System Information
- Zipping
- Basic VersionLocator (JVersion)
- A set of useful extensions
- Custom authentication server

### Extensions

Some extensions.

- Mojang API
- Assets, Libraries and Client files download & verify

## Examples

### Creating a Launcher instance

```csharp

LauncherCore core = LauncherCore.Create(
    new LauncherCoreCreationOption(
        javaPath: Config.Instance.JavaPath, // by default it will be the first version finded
        gameRootPath: null, // by defualt it will be ./.minecraft/
        versionLocator: the Version Locator // by default it will be new JVersionLocator()
    ));

```

### Locate versions

```csharp

var versions = core.GetVersions();

var version = core.GetVersion("1.8");

```

Unsupported versions are simply ignored.

### Launching Minecraft

```csharp
var result = core.Launch(new LaunchOptions
{
    Version = App.LauncherCore.GetVersion(server.VersionId)
    Authenticator = new OfflineAuthenticator("Steve"), // offline
    //Authenticator = new YggdrasilLogin("*@*.*", "***", true), // online
    MaxMemory = Config.Instance.MaxMemory, // optional
    MinMemory = Config.Instance.MaxMemory, // optional
    Mode = LaunchMode.MCLauncher, // optional
    Server = new ServerInfo {Address = "mc.hypixel.net"}, //optional
    Size = new WindowSize {Height = 768, Width = 1280} //optional
}, (Action<MinecraftLaunchArguments>) (x => { })); // optional ( modify arguments before launching
```

## License

Licensed under GNU Lesser General Public License, either version 3, or (at your opinion), any later version.
