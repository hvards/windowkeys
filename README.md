# WindowKeys

Windows utility for switching between visible windows using keyboard shortcuts. Overlay is displayed on all visible windows with key combination for selection.

```
+---------------+--------------+-------------------------------+
|               |              |                               |
|               |              |                               |
|     [A]       |     [R]      |    [S]   +-----------+        |
|               |              |          |    [T]    |        |
|               |              |          +-----------+        |
|               |              |                               |
+---------------+--------------+-------------------------------+
```

## Requirements

- .NET 10 Runtime
- Need to run as admin to register keyboard hook

## Installation

Download the latest `.msi` installer from the [Releases](https://github.com/hvards/windowkeys/releases) page.

## Usage

1. Press **LWin + H** to activate
2. Type the displayed letter(s) to switch to that window
3. Press **Escape** to cancel

## Configuration

Edit `appsettings.json` to customize:

- `ActivationSettings.HotKey` - Virtual key codes for activation
- `ActivationSettings.SelectionKeys` - Letters used for window selection
- `OverlaySettings` - Font, colors, and opacity

## License

[MIT](LICENSE)
