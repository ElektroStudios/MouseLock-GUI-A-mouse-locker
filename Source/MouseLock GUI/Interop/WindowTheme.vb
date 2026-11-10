Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Runtime.InteropServices

#End Region

Namespace Interop

    ''' <summary>
    ''' Applies the native (uxtheme) dark or default visual style to individual common controls,
    ''' which affects parts that WinForms does not paint itself, such as scroll bars and drop-down lists.
    ''' </summary>
    Friend NotInheritable Class WindowTheme

#Region " Constants "

        ''' <summary>The visual style class name for dark list views, tree views and scroll bars.</summary>
        Friend Const DarkExplorer As String = "DarkMode_Explorer"

        ''' <summary>The visual style class name for dark combo boxes (including their drop-down list).</summary>
        Friend Const DarkComboBox As String = "DarkMode_CFD"

        ''' <summary>The visual style class name for dark list view column headers.</summary>
        Friend Const DarkItemsView As String = "DarkMode_ItemsView"

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="WindowTheme"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Sets the visual style class of a window.
        ''' </summary>
        ''' <param name="hWnd">The window handle.</param>
        ''' <param name="subAppName">The visual style class name to use,
        ''' or <see langword="Nothing"/> to restore the default visual style of the window class.</param>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="hWnd"/> is <see cref="IntPtr.Zero"/>.</exception>
        ''' <exception cref="COMException">Thrown when the operating system rejects the request.</exception>
        Friend Shared Sub Apply(hWnd As IntPtr, subAppName As String)

            If hWnd = IntPtr.Zero Then
                Throw New ArgumentException("The window handle cannot be zero.", NameOf(hWnd))
            End If

            Dim hResult As Integer = NativeMethods.SetWindowTheme(hWnd, subAppName, Nothing)
            If hResult <> 0 Then
                Marshal.ThrowExceptionForHR(hResult)
            End If
        End Sub

#End Region

    End Class

End Namespace
