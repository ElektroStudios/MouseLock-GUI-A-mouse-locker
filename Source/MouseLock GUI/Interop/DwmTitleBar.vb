Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Windows.Forms

#End Region

Namespace Interop

    ''' <summary>
    ''' Provides helpers to switch the title bar (non-client area) of a top-level window
    ''' between the light and the dark immersive color mode of Windows 10/11.
    ''' </summary>
    ''' <remarks>
    ''' The attribute must be (re)applied from <c>OnHandleCreated</c>,
    ''' because the <see cref="Form.Load"/> event fires too late for the first frame to honor it,
    ''' and because the native window handle can be recreated at any time
    ''' (e.g. when changing <see cref="Form.ShowInTaskbar"/>).
    ''' </remarks>
    Friend NotInheritable Class DwmTitleBar

#Region " Constants "

        ''' <summary>
        ''' DWMWA_USE_IMMERSIVE_DARK_MODE attribute value for Windows 10 20H1 (build 19041) and newer, and Windows 11.
        ''' </summary>
        Friend Const DwmaUseImmersiveDarkMode As Integer = 20

        ''' <summary>
        ''' Undocumented DWMWA_USE_IMMERSIVE_DARK_MODE attribute value for Windows 10 1809 (build 17763) up to 1909 (build 18363).
        ''' </summary>
        Friend Const DwmaUseImmersiveDarkModeBefore20H1 As Integer = 19

        ''' <summary>
        ''' The HRESULT value S_OK.
        ''' </summary>
        Private Const S_OK As Integer = 0

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="DwmTitleBar"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Enables or disables the dark immersive title bar of the specified form,
        ''' and forces the non-client area to be repainted so the change is visible immediately.
        ''' </summary>
        ''' <param name="form">The target form. Its handle must have been created.</param>
        ''' <param name="enabled"><see langword="True"/> to use a dark title bar; <see langword="False"/> to use the default (light) one.</param>
        ''' <returns><see langword="True"/> if the operating system accepted the attribute; otherwise, <see langword="False"/>
        ''' (e.g. on Windows versions that do not support dark title bars).</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="form"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidOperationException">Thrown when the handle of <paramref name="form"/> has not been created yet.</exception>
        Friend Shared Function SetDarkMode(form As Form, enabled As Boolean) As Boolean

            If form Is Nothing Then
                Throw New ArgumentNullException(NameOf(form))
            End If
            If Not form.IsHandleCreated Then
                Throw New InvalidOperationException($"The handle of form '{form.Name}' has not been created yet.")
            End If

            Dim hWnd As IntPtr = form.Handle
            Dim value As Integer = If(enabled, 1, 0)
            Dim succeeded As Boolean = DwmTitleBar.TrySetAttribute(hWnd, DwmTitleBar.DwmaUseImmersiveDarkMode, value) OrElse
                                       DwmTitleBar.TrySetAttribute(hWnd, DwmTitleBar.DwmaUseImmersiveDarkModeBefore20H1, value)

            If succeeded AndAlso form.Visible Then
                DwmTitleBar.RefreshNonClientArea(form)
            End If

            Return succeeded
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Tries to set an integer DWM window attribute.
        ''' </summary>
        ''' <param name="hWnd">The window handle.</param>
        ''' <param name="attribute">The attribute identifier.</param>
        ''' <param name="value">The attribute value.</param>
        ''' <returns><see langword="True"/> if the call succeeded; otherwise, <see langword="False"/>.</returns>
        Private Shared Function TrySetAttribute(hWnd As IntPtr, attribute As Integer, value As Integer) As Boolean

            Dim hResult As Integer
            Try
                hResult = NativeMethods.DwmSetWindowAttribute(hWnd, attribute, value, Runtime.InteropServices.Marshal.SizeOf(GetType(Integer)))
            Catch ex As DllNotFoundException
                ' DWM is not available (pre-Vista or non-Windows host): dark title bars are simply unsupported.
                Return False
            Catch ex As EntryPointNotFoundException
                Return False
            End Try
            Return hResult = DwmTitleBar.S_OK
        End Function

        ''' <summary>
        ''' Forces the title bar of the specified form to be repainted.
        ''' </summary>
        ''' <remarks>
        ''' On Windows 10 the caption is not repainted after changing the attribute until the window
        ''' is re-activated, so a WM_NCACTIVATE round trip is sent in addition to SWP_FRAMECHANGED.
        ''' </remarks>
        ''' <param name="form">The form to refresh.</param>
        Private Shared Sub RefreshNonClientArea(form As Form)

            Dim hWnd As IntPtr = form.Handle
            Dim flags As SetWindowPosFlags = SetWindowPosFlags.NoMove Or SetWindowPosFlags.NoSize Or
                                             SetWindowPosFlags.NoZOrder Or SetWindowPosFlags.NoActivate Or
                                             SetWindowPosFlags.NoOwnerZOrder Or SetWindowPosFlags.FrameChanged

            If Not NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, flags) Then
                Throw New Win32Exception()
            End If

            Dim isActive As Boolean = Form.ActiveForm Is form
            NativeMethods.SendMessage(hWnd, NativeMethods.WM_NCACTIVATE, New IntPtr(If(isActive, 0, 1)), IntPtr.Zero)
            NativeMethods.SendMessage(hWnd, NativeMethods.WM_NCACTIVATE, New IntPtr(If(isActive, 1, 0)), IntPtr.Zero)
        End Sub

#End Region

    End Class

End Namespace
