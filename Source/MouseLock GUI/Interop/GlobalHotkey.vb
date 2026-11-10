Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Forms

#End Region

Namespace Interop

    ''' <summary>
    ''' Specifies the modifier keys of a system-wide hot key.
    ''' </summary>
    <Flags>
    Friend Enum HotkeyModifiers As Integer

        ''' <summary>No modifier key.</summary>
        None = 0

        ''' <summary>The ALT key (MOD_ALT).</summary>
        Alt = 1 << 0

        ''' <summary>The CTRL key (MOD_CONTROL).</summary>
        Control = 1 << 1

        ''' <summary>The SHIFT key (MOD_SHIFT).</summary>
        Shift = 1 << 2

        ''' <summary>The WINDOWS key (MOD_WIN).</summary>
        Win = 1 << 3

        ''' <summary>
        ''' Keyboard auto-repeat does not yield multiple hot key notifications (MOD_NOREPEAT).
        ''' </summary>
        NoRepeat = 1 << 14

    End Enum

    ''' <summary>
    ''' The exception that is thrown when a system-wide hot key cannot be registered,
    ''' typically because another application already registered the same key combination.
    ''' </summary>
    <Serializable>
    Friend NotInheritable Class HotkeyRegistrationException : Inherits Win32Exception

        ''' <summary>
        ''' Initializes a new instance of the <see cref="HotkeyRegistrationException"/> class.
        ''' </summary>
        ''' <param name="modifiers">The modifier keys of the hot key.</param>
        ''' <param name="key">The key of the hot key.</param>
        ''' <param name="win32Error">The Win32 error code returned by RegisterHotKey.</param>
        Friend Sub New(modifiers As HotkeyModifiers, key As Keys, win32Error As Integer)
            MyBase.New(win32Error, $"The hot key '{GlobalHotkey.FormatHotkey(modifiers, key)}' could not be registered. It may already be in use by another application.")
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="HotkeyRegistrationException"/> class with serialized data.
        ''' </summary>
        ''' <param name="info">The serialization info.</param>
        ''' <param name="context">The streaming context.</param>
        Private Sub New(info As Runtime.Serialization.SerializationInfo, context As Runtime.Serialization.StreamingContext)
            MyBase.New(info, context)
        End Sub

    End Class

    ''' <summary>
    ''' Registers a system-wide hot key for the lifetime of the instance, and raises
    ''' the <see cref="GlobalHotkey.Pressed"/> event every time the user presses it.
    ''' </summary>
    Friend NotInheritable Class GlobalHotkey : Inherits NativeWindow : Implements IDisposable

#Region " Fields "

        ''' <summary>
        ''' Seed for the hot key identifiers. Application-defined identifiers must be in the range 0x0000 - 0xBFFF.
        ''' </summary>
        Private Shared lastId As Integer

        ''' <summary>
        ''' Flag that indicates whether this instance has been disposed.
        ''' </summary>
        Private isDisposed As Boolean

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the identifier used to register the hot key.
        ''' </summary>
        Friend ReadOnly Property Id As Integer

        ''' <summary>
        ''' Gets the modifier keys of the hot key (without <see cref="HotkeyModifiers.NoRepeat"/>).
        ''' </summary>
        Friend ReadOnly Property Modifiers As HotkeyModifiers

        ''' <summary>
        ''' Gets the key of the hot key.
        ''' </summary>
        Friend ReadOnly Property Key As Keys

#End Region

#Region " Events "

        ''' <summary>
        ''' Occurs when the hot key is pressed.
        ''' </summary>
        Friend Event Pressed As EventHandler

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GlobalHotkey"/> class and registers the hot key.
        ''' </summary>
        ''' <param name="modifiers">The modifier keys. <see cref="HotkeyModifiers.NoRepeat"/> is always added,
        ''' so holding the key down does not toggle the lock repeatedly.</param>
        ''' <param name="key">The key. Must not contain modifier flags.</param>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <see cref="Keys.None"/> or contains modifier flags.</exception>
        ''' <exception cref="HotkeyRegistrationException">Thrown when the hot key cannot be registered.</exception>
        Friend Sub New(modifiers As HotkeyModifiers, key As Keys)

            If key = Keys.None OrElse (key And Keys.Modifiers) <> Keys.None Then
                Throw New ArgumentException($"Invalid hot key: '{key}'.", NameOf(key))
            End If

            Me.Modifiers = modifiers And Not HotkeyModifiers.NoRepeat
            Me.Key = key
            Me.Id = Interlocked.Increment(GlobalHotkey.lastId) Mod &HBFFF

            Me.CreateHandle(New CreateParams() With {.Caption = NameOf(GlobalHotkey)})

            If Not NativeMethods.RegisterHotKey(Me.Handle, Me.Id, CInt(Me.Modifiers Or HotkeyModifiers.NoRepeat), CInt(key)) Then
                Dim win32Error As Integer = Marshal.GetLastWin32Error()
                Me.DestroyHandle()
                Throw New HotkeyRegistrationException(Me.Modifiers, key, win32Error)
            End If
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Returns a human-readable representation of a hot key, e.g. "CTRL + F1".
        ''' </summary>
        ''' <param name="modifiers">The modifier keys.</param>
        ''' <param name="key">The key.</param>
        ''' <returns>The formatted hot key.</returns>
        Friend Shared Function FormatHotkey(modifiers As HotkeyModifiers, key As Keys) As String

            Dim parts As New List(Of String)
            If (modifiers And HotkeyModifiers.Control) = HotkeyModifiers.Control Then parts.Add("CTRL")
            If (modifiers And HotkeyModifiers.Alt) = HotkeyModifiers.Alt Then parts.Add("ALT")
            If (modifiers And HotkeyModifiers.Shift) = HotkeyModifiers.Shift Then parts.Add("SHIFT")
            If (modifiers And HotkeyModifiers.Win) = HotkeyModifiers.Win Then parts.Add("WIN")
            parts.Add(key.ToString())
            Return String.Join(" + ", parts)
        End Function

        ''' <summary>
        ''' Unregisters the hot key and releases the window handle.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            If Me.isDisposed Then
                Return
            End If

            If Me.Handle <> IntPtr.Zero Then
                NativeMethods.UnregisterHotKey(Me.Handle, Me.Id)
                Me.DestroyHandle()
            End If
            Me.isDisposed = True
            GC.SuppressFinalize(Me)
        End Sub

#End Region

#Region " Protected Methods "

        ''' <summary>
        ''' Processes window messages, raising <see cref="GlobalHotkey.Pressed"/> on WM_HOTKEY.
        ''' </summary>
        ''' <param name="m">The window message.</param>
        Protected Overrides Sub WndProc(ByRef m As Message)

            If m.Msg = NativeMethods.WM_HOTKEY AndAlso m.WParam.ToInt32() = Me.Id Then
                RaiseEvent Pressed(Me, EventArgs.Empty)
                Return
            End If
            MyBase.WndProc(m)
        End Sub

#End Region

    End Class

End Namespace
