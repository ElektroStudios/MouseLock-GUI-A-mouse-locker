# MouseLock GUI Change Log 📋

## v1.2 (2026) *(current)* 🆕
#### 🌟 Improvements:
    • Added light and dark visual themes, switchable at any time with a new "Dark theme" toggle switch.
      The light theme uses the default Windows colors; the dark theme also darkens the title bar
      (DWM immersive dark mode), menus, lists, combo boxes and numeric boxes.
      The chosen theme is remembered; on first run it follows the Windows "app mode" setting.
    • The logo is recolored to match the active theme.
    • The toggle hot key no longer repeats while the key is held down.
    • The scheduled lock ("Lock the mouse at certain time") now fires every day at the configured time,
      instead of only once per session.
    • Application names can be typed with or without the ".exe" extension.
    • When the settings file cannot be read, saved or deleted, a warning is shown instead of failing silently.
    • Added a unit test project (MSTest).

#### 🐛 Bug Fixes:
    • The scheduled lock never fired in cultures whose short time format differs from "HH:mm" (e.g. "9:05" or "9:05 AM").
    • The scheduled lock time loaded from the settings file was ignored until the time was picked again.
    • Re-selecting a lock type accumulated timer event handlers, which could lock and immediately unlock the mouse.
    • Pressing "Unlock" while monitoring an application with the "Kill MouseLock" option closed MouseLock.
    • After unlocking manually while monitoring an application, the mouse was locked again when that application ended.
    • The scheduled time or the monitored application could unlock an already active lock instead of leaving it locked.
    • Pressing the toggle hot key while the mouse was not locked broke the lock state (the "Lock now" button then unlocked).
    • Resuming with the hot key left the lock flagged as paused.
    • Changing the hot key did not unregister the previous one, and the hot key stayed active even when it was disabled.
    • Digit hot keys combined with ALT, CTRL or SHIFT registered a wrong key (a mouse button value).
    • A hot key already in use by another application failed silently; a notification is shown now.
    • Checking or unchecking an application in the running applications list matched partial names
      (e.g. "pad.exe" inside "notepad.exe") and lowercased the whole list.
    • The tray menu "Lock"/"Unlock" items did not reflect locks started or stopped from the main window.
    • The hot key combo boxes were enabled after unlocking even when the hot key was disabled.
    • A lock delay of 0 ms crashed the application, and an initial delay above 2147483 seconds overflowed.
    • An invalid or out-of-range value in the settings file crashed the application on startup.
    • Unchecking "Remember settings" did not take effect: the previous settings were still loaded on the next start.
    • The settings file depended on the current working directory, and could not be saved when the application
      was installed in a read-only folder such as "Program Files" (in that case it is now stored in "%APPDATA%\ElektroStudios\MouseLock").
    • "Minimize at startup" left a minimized window title bar on the desktop instead of hiding the window in the tray.
    • The mouse was centered on the screen resolution captured at startup, ignoring later resolution changes.
    • Process handles were not released while monitoring applications.

## v1.1 (2024) 🔄
#### 🌟 Improvements:
    • Updated to .NET Framework 4.8
    • Added a "Refresh" button in the process list floating window.

## v1.0 (2013) 🔄
Initial Release.