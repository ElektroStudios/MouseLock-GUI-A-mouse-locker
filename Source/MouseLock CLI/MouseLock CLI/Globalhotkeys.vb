Option Strict On
Option Explicit On
Option Infer Off

Imports System
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

#Region " 01. GlobalHotkeys Class "

''' <summary>
''' Represents a global system-wide hotkey registered via the Windows API.
''' </summary>
Public Class Shortcut
    Inherits NativeWindow
    Implements IDisposable

#Region " 02. Native Methods & Constants "

    Private Const WM_HOTKEY As Integer = &H312

    ''' <summary>
    ''' Registers a system-wide hotkey.
    ''' </summary>
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function RegisterHotKey(ByVal hWnd As IntPtr, ByVal id As Integer, ByVal fsModifiers As UInteger, ByVal vk As UInteger) As Boolean
    End Function

    ''' <summary>
    ''' Frees a system-wide hotkey previously registered by the calling thread.
    ''' </summary>
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function UnregisterHotKey(ByVal hWnd As IntPtr, ByVal id As Integer) As Boolean
    End Function

#End Region

#Region " 03. Types & Events "

    ''' <summary>
    ''' Modifiers that can be applied to a global hotkey.
    ''' </summary>
    <Flags>
    Public Enum Modifier As UInteger
        None = 0
        Alt = 1 << 0
        Ctrl = 1 << 1
        Shift = 1 << 2
        Win = 1 << 3
    End Enum

    ''' <summary>
    ''' Event arguments for the hotkey press event.
    ''' </summary>
    Public Class HotKeyEventArgs
        Inherits EventArgs

        Public ReadOnly Property KeyModifier As Modifier
        Public ReadOnly Property KeyCode As Keys

        Public Sub New(ByVal modValue As Modifier, ByVal keyValue As Keys)
            Me.KeyModifier = modValue
            Me.KeyCode = keyValue
        End Sub
    End Class

    ''' <summary>
    ''' Exception thrown when a hotkey fails to register (e.g., already in use).
    ''' </summary>
    Public Class HotkeyRegistrationException
        Inherits Exception

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub
    End Class

    ''' <summary>
    ''' Occurs when the registered global hotkey is pressed.
    ''' </summary>
    Public Event Press As EventHandler(Of HotKeyEventArgs)

#End Region

#Region " 04. Fields "

    Private _disposed As Boolean = False
    Private ReadOnly _id As Integer
    Private ReadOnly _eventArgs As HotKeyEventArgs

#End Region

#Region " 05. Constructors "

    ''' <summary>
    ''' Initializes a new instance of the Shortcut class and registers the global hotkey.
    ''' </summary>
    ''' <param name="modifierArg">The modifier key(s) to use.</param>
    ''' <param name="keyArg">The main key to use.</param>
    ''' <exception cref="HotkeyRegistrationException">Thrown if the hotkey cannot be registered.</exception>
    Public Sub New(ByVal modifierArg As Modifier, ByVal keyArg As Keys)
        MyBase.New()

        Me.CreateHandle(New CreateParams())
        _id = Me.GetHashCode()
        _eventArgs = New HotKeyEventArgs(modifierArg, keyArg)

        ' Fail-Fast: We check the return value of RegisterHotKey and throw immediately with exact Win32 error code.
        If Not RegisterHotKey(Me.Handle, _id, CUInt(modifierArg), CUInt(keyArg)) Then
            Dim errorCode As Integer = Marshal.GetLastWin32Error()
            Throw New HotkeyRegistrationException($"Failed to register global hotkey combination '{modifierArg} + {keyArg}'. The combination might already be in use. Win32 Error Code: {errorCode}")
        End If
    End Sub

    ''' <summary>
    ''' Factory method to create and register a new global hotkey.
    ''' </summary>
    ''' <param name="modifierArg">The modifier key(s) to use.</param>
    ''' <param name="keyArg">The main key to use.</param>
    ''' <returns>A new registered Shortcut instance.</returns>
    Public Shared Function Create(ByVal modifierArg As Modifier, ByVal keyArg As Keys) As Shortcut
        Return New Shortcut(modifierArg, keyArg)
    End Function

#End Region

#Region " 06. Windows Message Processing "

    ''' <summary>
    ''' Overrides the default window procedure to intercept hotkey messages.
    ''' </summary>
    Protected Overrides Sub WndProc(ByRef m As Message)
        If (m.Msg = WM_HOTKEY) Then
            RaiseEvent Press(Me, _eventArgs)
        End If

        MyBase.WndProc(m)
    End Sub

#End Region

#Region " 07. IDisposable Implementation "

    ''' <summary>
    ''' Releases the unmanaged resources used by the Shortcut and optionally releases the managed resources.
    ''' </summary>
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not _disposed Then
            If (Me.Handle <> IntPtr.Zero) Then
                UnregisterHotKey(Me.Handle, _id)
                Me.DestroyHandle()
            End If

            _disposed = True
        End If
    End Sub

    ''' <summary>
    ''' Finalizer to ensure resources are released if Dispose is not called.
    ''' </summary>
    Protected Overrides Sub Finalize()
        Me.Dispose(False)
        MyBase.Finalize()
    End Sub

    ''' <summary>
    ''' Disposes the Shortcut instance and unregisters the global hotkey.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Me.Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class

#End Region