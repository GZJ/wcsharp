# Wcsharp

Suite of Window Control Tools for Windows (for Windows 10).

This suite provides a set of programs to control window behavior on Windows. Each program targets a specific window management function to enhance user productivity and window management experience.

- **wcs-exit.cs**: Exit the window.
- **wcs-focus.cs**: Focus the window.
- **wcs-hide-altab.cs**: Hide a window from the taskbar and Alt-Tab/Win+Tab shortcuts.
- **wcs-show-altab.cs**: Show a window in the taskbar and with Alt-Tab/Win+Tab shortcuts.
- **wcs-hide-vis.cs**: Hide window visibility.
- **wcs-show-vis.cs**: Show window visibility.
- **wcs-max.cs**: Maximize the window.
- **wcs-min.cs**: Minimize the window.
- **wcs-restore.cs**: Restore a minimized or maximized window.
- **wcs-move.cs**: Move the window.
- **wcs-move-resize.cs**: Move and resize the window.
- **wcs-resize.cs**: Resize the window.
- **wcs-tray.cs**: Trigger window show/hide using the system tray icon and hotkeys.

# Build
To build these programs, you will need to use Visual Studio Build Tools. Below are the steps to compile the programs using `csc` (C# compiler):

1. Install Visual Studio Build Tools:
   - Download and install the Visual Studio Build Tools from the [official website](https://visualstudio.microsoft.com/visual-cpp-build-tools/).

2. Open a Developer Command Prompt(Launch-VsDevShell.ps1):
   - Open the Developer Command Prompt for Visual Studio from the Start Menu.

3. Navigate to the directory containing the programs:
```
git clone https://github.com/GZJ/wcsharp.git
cd wcsharp
./build.ps1
```
