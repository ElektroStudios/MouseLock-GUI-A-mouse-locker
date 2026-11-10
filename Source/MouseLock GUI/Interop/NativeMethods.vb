Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security

#End Region

Namespace Interop

    ''' <summary>
    ''' Platform invocation declarations used by the application.
    ''' </summary>
    <SuppressUnmanagedCodeSecurity>
    Friend NotInheritable Class NativeMethods

#Region " Constants "

        ''' <summary>
        ''' The WM_HOTKEY window message, posted when the user presses a hot key registered by <see cref="RegisterHotKey"/>.
        ''' </summary>
        Friend Const WM_HOTKEY As Integer = &H312

        ''' <summary>
        ''' The WM_NCACTIVATE window message, sent to a window when its non-client area needs to be changed
        ''' to indicate an active or inactive state.
        ''' </summary>
        Friend Const WM_NCACTIVATE As Integer = &H86

        ''' <summary>
        ''' The LVM_GETHEADER list-view message, which gets the handle to the header control used by a list-view control.
        ''' </summary>
        Friend Const LVM_GETHEADER As Integer = &H1000 + 31

        ''' <summary>
        ''' The WM_PAINT window message.
        ''' </summary>
        Friend Const WM_PAINT As Integer = &HF

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="NativeMethods"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " user32.dll "

        ''' <summary>
        ''' Defines a system-wide hot key.
        ''' </summary>
        <DllImport("user32.dll", SetLastError:=True)>
        Friend Shared Function RegisterHotKey(hWnd As IntPtr, id As Integer, fsModifiers As Integer, vk As Integer) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Frees a hot key previously registered by the calling thread.
        ''' </summary>
        <DllImport("user32.dll", SetLastError:=True)>
        Friend Shared Function UnregisterHotKey(hWnd As IntPtr, id As Integer) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Enables you to produce special effects when showing or hiding windows.
        ''' </summary>
        <DllImport("user32.dll", SetLastError:=True)>
        Friend Shared Function AnimateWindow(hWnd As IntPtr, dwTime As Integer, dwFlags As AnimateWindowFlags) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Sends the specified message to a window.
        ''' </summary>
        <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
        Friend Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
        End Function

        ''' <summary>
        ''' Changes the size, position, and Z order of a window.
        ''' </summary>
        <DllImport("user32.dll", SetLastError:=True)>
        Friend Shared Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr,
                                            x As Integer, y As Integer, cx As Integer, cy As Integer,
                                            uFlags As SetWindowPosFlags) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

#End Region

#Region " dwmapi.dll "

        ''' <summary>
        ''' Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window.
        ''' </summary>
        <DllImport("dwmapi.dll")>
        Friend Shared Function DwmSetWindowAttribute(hWnd As IntPtr, dwAttribute As Integer, ByRef pvAttribute As Integer, cbAttribute As Integer) As Integer
        End Function

#End Region

#Region " uxtheme.dll "

        ''' <summary>
        ''' Causes a window to use a different set of visual style information than its class normally uses.
        ''' </summary>
        <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
        Friend Shared Function SetWindowTheme(hWnd As IntPtr, pszSubAppName As String, pszSubIdList As String) As Integer
        End Function

#End Region

    End Class

    ''' <summary>
    ''' Flags for the <see cref="NativeMethods.AnimateWindow"/> function.
    ''' </summary>
    <Flags>
    Friend Enum AnimateWindowFlags As Integer

        ''' <summary>No flags.</summary>
        None = 0

        ''' <summary>Animates the window from left to right (AW_HOR_POSITIVE).</summary>
        HorizontalPositive = 1 << 0

        ''' <summary>Animates the window from right to left (AW_HOR_NEGATIVE).</summary>
        HorizontalNegative = 1 << 1

        ''' <summary>Animates the window from top to bottom (AW_VER_POSITIVE).</summary>
        VerticalPositive = 1 << 2

        ''' <summary>Animates the window from bottom to top (AW_VER_NEGATIVE).</summary>
        VerticalNegative = 1 << 3

        ''' <summary>Makes the window appear to collapse inward or expand outward (AW_CENTER).</summary>
        Center = 1 << 4

        ''' <summary>Hides the window (AW_HIDE).</summary>
        Hide = 1 << 16

        ''' <summary>Activates the window (AW_ACTIVATE).</summary>
        Activate = 1 << 17

        ''' <summary>Uses slide animation (AW_SLIDE).</summary>
        Slide = 1 << 18

        ''' <summary>Uses a fade effect (AW_BLEND).</summary>
        Blend = 1 << 19

    End Enum

    ''' <summary>
    ''' Flags for the <see cref="NativeMethods.SetWindowPos"/> function.
    ''' </summary>
    <Flags>
    Friend Enum SetWindowPosFlags As UInteger

        ''' <summary>No flags.</summary>
        None = 0UI

        ''' <summary>Retains the current size (SWP_NOSIZE).</summary>
        NoSize = 1UI << 0

        ''' <summary>Retains the current position (SWP_NOMOVE).</summary>
        NoMove = 1UI << 1

        ''' <summary>Retains the current Z order (SWP_NOZORDER).</summary>
        NoZOrder = 1UI << 2

        ''' <summary>Does not activate the window (SWP_NOACTIVATE).</summary>
        NoActivate = 1UI << 4

        ''' <summary>Sends a WM_NCCALCSIZE message to the window, even if its size is not being changed (SWP_FRAMECHANGED).</summary>
        FrameChanged = 1UI << 5

        ''' <summary>Does not change the owner window's position in the Z order (SWP_NOOWNERZORDER).</summary>
        NoOwnerZOrder = 1UI << 9

    End Enum

End Namespace
