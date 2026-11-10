Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing

#End Region

Namespace Theming

    ''' <summary>
    ''' The set of colors used to paint the user interface for a given <see cref="AppTheme"/>.
    ''' </summary>
    Friend NotInheritable Class ThemePalette

#Region " Fields "

        ''' <summary>The brand color of MouseLock (the background of the original logo).</summary>
        Friend Shared ReadOnly BrandColor As Color = Color.FromArgb(0, 74, 123)

        ''' <summary>The three gradient stops of the secondary buttons.</summary>
        Private ReadOnly buttonGradientStops As Color()

#End Region

#Region " Properties "

        ''' <summary>Gets the theme this palette belongs to.</summary>
        Friend ReadOnly Property Theme As AppTheme

        ''' <summary>Gets a value indicating whether this is a dark palette.</summary>
        Friend ReadOnly Property IsDark As Boolean
            Get
                Return Me.Theme = AppTheme.Dark
            End Get
        End Property

        ''' <summary>Gets the background color of windows and containers.</summary>
        Friend ReadOnly Property Background As Color

        ''' <summary>Gets the text color of windows and containers.</summary>
        Friend ReadOnly Property Foreground As Color

        ''' <summary>Gets the text color of disabled elements painted by custom controls.</summary>
        Friend ReadOnly Property DisabledForeground As Color

        ''' <summary>Gets the background color of input controls (text boxes, combo boxes, lists).</summary>
        Friend ReadOnly Property InputBackground As Color

        ''' <summary>Gets the text color of input controls.</summary>
        Friend ReadOnly Property InputForeground As Color

        ''' <summary>Gets the background color of disabled input controls painted by custom controls.</summary>
        Friend ReadOnly Property DisabledInputBackground As Color

        ''' <summary>Gets the color of borders and separators.</summary>
        Friend ReadOnly Property Border As Color

        ''' <summary>Gets the background color of selected or highlighted items.</summary>
        Friend ReadOnly Property Highlight As Color

        ''' <summary>Gets the text color of selected or highlighted items.</summary>
        Friend ReadOnly Property HighlightForeground As Color

        ''' <summary>Gets the background color of hovered items (menus).</summary>
        Friend ReadOnly Property Hover As Color

        ''' <summary>Gets the background color of column headers.</summary>
        Friend ReadOnly Property HeaderBackground As Color

        ''' <summary>Gets the text color of the secondary buttons.</summary>
        Friend ReadOnly Property ButtonForeground As Color

        ''' <summary>Gets the accent color (e.g. the "on" track of the theme switch).</summary>
        Friend ReadOnly Property Accent As Color

        ''' <summary>Gets the color used to paint the logo text and shapes.</summary>
        Friend ReadOnly Property LogoForeground As Color

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemePalette"/> class.
        ''' </summary>
        Private Sub New(theme As AppTheme,
                        background As Color, foreground As Color, disabledForeground As Color,
                        inputBackground As Color, inputForeground As Color, disabledInputBackground As Color,
                        border As Color, highlight As Color, highlightForeground As Color, hover As Color,
                        headerBackground As Color, buttonGradient As Color(), buttonForeground As Color,
                        accent As Color, logoForeground As Color)

            Me.Theme = theme
            Me.Background = background
            Me.Foreground = foreground
            Me.DisabledForeground = disabledForeground
            Me.InputBackground = inputBackground
            Me.InputForeground = inputForeground
            Me.DisabledInputBackground = disabledInputBackground
            Me.Border = border
            Me.Highlight = highlight
            Me.HighlightForeground = highlightForeground
            Me.Hover = hover
            Me.HeaderBackground = headerBackground
            Me.buttonGradientStops = DirectCast(buttonGradient.Clone(), Color())
            Me.ButtonForeground = buttonForeground
            Me.Accent = accent
            Me.LogoForeground = logoForeground
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Gets the palette of the specified theme.
        ''' </summary>
        ''' <param name="theme">The theme.</param>
        ''' <returns>A new palette instance. The light palette is built from the current Windows system colors.</returns>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="theme"/> is not defined.</exception>
        Friend Shared Function FromTheme(theme As AppTheme) As ThemePalette

            Select Case theme
                Case AppTheme.Light
                    Return ThemePalette.CreateLight()
                Case AppTheme.Dark
                    Return ThemePalette.CreateDark()
                Case Else
                    Throw New InvalidEnumArgumentException(NameOf(theme), CInt(theme), GetType(AppTheme))
            End Select
        End Function

        ''' <summary>
        ''' Returns the three gradient stops of the secondary buttons as a new array, so callers cannot alter the palette.
        ''' </summary>
        ''' <returns>A copy of the gradient stops.</returns>
        Friend Function GetButtonGradient() As Color()
            Return DirectCast(Me.buttonGradientStops.Clone(), Color())
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Creates the light palette from the Windows system colors.
        ''' </summary>
        ''' <returns>The light palette.</returns>
        Private Shared Function CreateLight() As ThemePalette

            Return New ThemePalette(
                theme:=AppTheme.Light,
                background:=SystemColors.Control,
                foreground:=SystemColors.ControlText,
                disabledForeground:=SystemColors.GrayText,
                inputBackground:=SystemColors.Window,
                inputForeground:=SystemColors.WindowText,
                disabledInputBackground:=SystemColors.Control,
                border:=SystemColors.ControlDark,
                highlight:=SystemColors.Highlight,
                highlightForeground:=SystemColors.HighlightText,
                hover:=SystemColors.MenuHighlight,
                headerBackground:=SystemColors.Control,
                buttonGradient:={SystemColors.ControlLightLight, SystemColors.Control, SystemColors.ControlLight},
                buttonForeground:=SystemColors.ControlText,
                accent:=SystemColors.Highlight,
                logoForeground:=ThemePalette.BrandColor)
        End Function

        ''' <summary>
        ''' Creates the dark palette.
        ''' </summary>
        ''' <returns>The dark palette.</returns>
        Private Shared Function CreateDark() As ThemePalette

            Return New ThemePalette(
                theme:=AppTheme.Dark,
                background:=Color.FromArgb(32, 32, 32),
                foreground:=Color.FromArgb(232, 232, 232),
                disabledForeground:=Color.FromArgb(120, 120, 120),
                inputBackground:=Color.FromArgb(48, 48, 48),
                inputForeground:=Color.FromArgb(240, 240, 240),
                disabledInputBackground:=Color.FromArgb(40, 40, 40),
                border:=Color.FromArgb(90, 90, 90),
                highlight:=Color.FromArgb(0, 95, 184),
                highlightForeground:=Color.White,
                hover:=Color.FromArgb(62, 62, 64),
                headerBackground:=Color.FromArgb(43, 43, 43),
                buttonGradient:={Color.FromArgb(72, 72, 72), Color.FromArgb(58, 58, 58), Color.FromArgb(72, 72, 72)},
                buttonForeground:=Color.FromArgb(240, 240, 240),
                accent:=Color.FromArgb(76, 160, 255),
                logoForeground:=Color.White)
        End Function

#End Region

    End Class

End Namespace
