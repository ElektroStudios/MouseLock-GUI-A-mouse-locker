Option Strict On
Option Explicit On
Option Infer Off

Namespace Theming

    ''' <summary>
    ''' Holds the current application theme, persists it in <see cref="My.MySettings.VisualTheme"/>,
    ''' and notifies the open windows when it changes.
    ''' </summary>
    Friend NotInheritable Class AppThemeService

#Region " Fields "

        ''' <summary>
        ''' The current theme, or <see langword="Nothing"/> until it is first resolved.
        ''' </summary>
        Private Shared currentTheme As AppTheme?

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the current theme. On first access it is resolved from the user settings,
        ''' falling back to the Windows app mode when the user never chose a theme.
        ''' </summary>
        Friend Shared ReadOnly Property Current As AppTheme
            Get
                If Not AppThemeService.currentTheme.HasValue Then
                    AppThemeService.currentTheme = ThemePreference.Resolve(My.Settings.VisualTheme, ThemePreference.ReadSystemUsesDarkTheme())
                End If
                Return AppThemeService.currentTheme.Value
            End Get
        End Property

        ''' <summary>
        ''' Gets the palette of the current theme.
        ''' </summary>
        Friend Shared ReadOnly Property CurrentPalette As ThemePalette
            Get
                Return ThemePalette.FromTheme(AppThemeService.Current)
            End Get
        End Property

#End Region

#Region " Events "

        ''' <summary>
        ''' Occurs when the current theme changes. Subscribers must unsubscribe when they are disposed.
        ''' </summary>
        Friend Shared Event ThemeChanged As EventHandler

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="AppThemeService"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Changes the current theme, saves it in the user settings and raises <see cref="ThemeChanged"/>.
        ''' </summary>
        ''' <param name="theme">The new theme.</param>
        ''' <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when <paramref name="theme"/> is not defined.</exception>
        Friend Shared Sub SetTheme(theme As AppTheme)

            Dim storedValue As String = ThemePreference.ToStoredValue(theme)
            If AppThemeService.currentTheme.HasValue AndAlso AppThemeService.currentTheme.Value = theme Then
                Return
            End If

            AppThemeService.currentTheme = theme
            My.Settings.VisualTheme = storedValue
            My.Settings.Save()
            RaiseEvent ThemeChanged(Nothing, EventArgs.Empty)
        End Sub

#End Region

    End Class

End Namespace
