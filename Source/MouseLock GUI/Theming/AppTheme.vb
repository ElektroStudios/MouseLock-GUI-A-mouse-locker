Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports Microsoft.Win32

#End Region

Namespace Theming

    ''' <summary>
    ''' Specifies the visual theme of the application.
    ''' </summary>
    Friend Enum AppTheme As Integer

        ''' <summary>Light theme, based on the default Windows system colors.</summary>
        Light = 0

        ''' <summary>Dark theme.</summary>
        Dark = 1

    End Enum

    ''' <summary>
    ''' Converts the theme stored in the user settings into an <see cref="AppTheme"/>,
    ''' falling back to the Windows "app mode" preference when nothing was stored yet.
    ''' </summary>
    Friend NotInheritable Class ThemePreference

#Region " Constants "

        ''' <summary>
        ''' The registry key that holds the Windows personalization settings of the current user.
        ''' </summary>
        Friend Const PersonalizeRegistryKey As String = "Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"

        ''' <summary>
        ''' The registry value that is 0 when the user chose the dark "app mode" in Windows settings.
        ''' </summary>
        Friend Const AppsUseLightThemeValueName As String = "AppsUseLightTheme"

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ThemePreference"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Tries to parse a stored theme name ("Light" or "Dark", case-insensitive).
        ''' </summary>
        ''' <param name="value">The stored value.</param>
        ''' <param name="theme">When this method returns <see langword="True"/>, contains the parsed theme.</param>
        ''' <returns><see langword="True"/> if <paramref name="value"/> is a valid theme name; otherwise, <see langword="False"/>.</returns>
        Friend Shared Function TryParse(value As String, ByRef theme As AppTheme) As Boolean

            theme = AppTheme.Light
            If String.IsNullOrWhiteSpace(value) Then
                Return False
            End If

            Select Case value.Trim().ToUpperInvariant()
                Case "LIGHT"
                    theme = AppTheme.Light
                    Return True
                Case "DARK"
                    theme = AppTheme.Dark
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' Resolves the theme to use.
        ''' </summary>
        ''' <param name="storedValue">The theme stored in the user settings; empty when the user never chose one.</param>
        ''' <param name="systemUsesDarkTheme"><see langword="True"/> if Windows is configured to use the dark app mode.</param>
        ''' <returns>The stored theme when valid; otherwise, the theme that matches Windows.</returns>
        Friend Shared Function Resolve(storedValue As String, systemUsesDarkTheme As Boolean) As AppTheme

            Dim theme As AppTheme
            If ThemePreference.TryParse(storedValue, theme) Then
                Return theme
            End If
            Return If(systemUsesDarkTheme, AppTheme.Dark, AppTheme.Light)
        End Function

        ''' <summary>
        ''' Converts a theme into the value stored in the user settings.
        ''' </summary>
        ''' <param name="theme">The theme.</param>
        ''' <returns>The value to store.</returns>
        ''' <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when <paramref name="theme"/> is not defined.</exception>
        Friend Shared Function ToStoredValue(theme As AppTheme) As String

            If Not [Enum].IsDefined(GetType(AppTheme), theme) Then
                Throw New System.ComponentModel.InvalidEnumArgumentException(NameOf(theme), CInt(theme), GetType(AppTheme))
            End If
            Return theme.ToString()
        End Function

        ''' <summary>
        ''' Interprets the raw "AppsUseLightTheme" registry value.
        ''' </summary>
        ''' <param name="appsUseLightTheme">The raw registry value, or <see langword="Nothing"/> when it does not exist.</param>
        ''' <returns><see langword="True"/> if the value is a DWORD equal to 0 (dark app mode); otherwise, <see langword="False"/>.</returns>
        Friend Shared Function IsDarkAppsModeValue(appsUseLightTheme As Object) As Boolean

            If TypeOf appsUseLightTheme Is Integer Then
                Return DirectCast(appsUseLightTheme, Integer) = 0
            End If
            Return False
        End Function

        ''' <summary>
        ''' Reads whether Windows is configured to use the dark app mode for the current user.
        ''' </summary>
        ''' <returns><see langword="True"/> if the dark app mode is enabled; otherwise, <see langword="False"/>
        ''' (including Windows versions that have no app mode setting).</returns>
        Friend Shared Function ReadSystemUsesDarkTheme() As Boolean

            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(ThemePreference.PersonalizeRegistryKey, writable:=False)
                If key Is Nothing Then
                    Return False
                End If
                Return ThemePreference.IsDarkAppsModeValue(key.GetValue(ThemePreference.AppsUseLightThemeValueName))
            End Using
        End Function

#End Region

    End Class

End Namespace
