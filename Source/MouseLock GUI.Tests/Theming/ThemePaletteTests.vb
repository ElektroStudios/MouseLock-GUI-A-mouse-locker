Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel

Imports MouseLock.Theming

#End Region

Namespace Theming

    ''' <summary>
    ''' Unit tests for <see cref="ThemePalette"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class ThemePaletteTests

        <TestMethod>
        Public Sub Light_UsesTheWindowsSystemColors()
            Dim palette As ThemePalette = ThemePalette.FromTheme(AppTheme.Light)

            Assert.IsFalse(palette.IsDark)
            Assert.AreEqual(AppTheme.Light, palette.Theme)
            Assert.AreEqual(SystemColors.Control, palette.Background)
            Assert.AreEqual(SystemColors.ControlText, palette.Foreground)
            Assert.AreEqual(SystemColors.Window, palette.InputBackground)
            Assert.AreEqual(SystemColors.WindowText, palette.InputForeground)
            Assert.AreEqual(SystemColors.Highlight, palette.Highlight)
            Assert.AreEqual(SystemColors.HighlightText, palette.HighlightForeground)
        End Sub

        <TestMethod>
        Public Sub Light_LogoUsesTheBrandColor()
            Assert.AreEqual(ThemePalette.BrandColor, ThemePalette.FromTheme(AppTheme.Light).LogoForeground)
        End Sub

        <TestMethod>
        Public Sub Dark_IsDark_AndHasDarkBackgrounds()
            Dim palette As ThemePalette = ThemePalette.FromTheme(AppTheme.Dark)

            Assert.IsTrue(palette.IsDark)
            Assert.IsTrue(palette.Background.GetBrightness() < 0.2F)
            Assert.IsTrue(palette.InputBackground.GetBrightness() < 0.25F)
            Assert.AreEqual(Color.White.ToArgb(), palette.LogoForeground.ToArgb())
        End Sub

        '<DataTestMethod>
        '<DataRow(0)>
        '<DataRow(1)>
        'Public Sub TextColors_MeetWcagAaContrast(theme As Integer)
        '    Dim palette As ThemePalette = ThemePalette.FromTheme(CType(theme, AppTheme))

        '    ThemePaletteTests.AssertContrast(palette.Foreground, palette.Background, 4.5R, NameOf(palette.Foreground))
        '    ThemePaletteTests.AssertContrast(palette.InputForeground, palette.InputBackground, 4.5R, NameOf(palette.InputForeground))
        '    ThemePaletteTests.AssertContrast(palette.HighlightForeground, palette.Highlight, 4.5R, NameOf(palette.HighlightForeground))
        '    ThemePaletteTests.AssertContrast(palette.ButtonForeground, palette.GetButtonGradient()(1), 4.5R, NameOf(palette.ButtonForeground))
        '    ThemePaletteTests.AssertContrast(palette.LogoForeground, palette.Background, 3.0R, NameOf(palette.LogoForeground))
        'End Sub

        <DataTestMethod>
        <DataRow(0)>
        <DataRow(1)>
        Public Sub GetButtonGradient_ReturnsThreeStops_AsACopy(theme As Integer)
            Dim palette As ThemePalette = ThemePalette.FromTheme(CType(theme, AppTheme))
            Dim first As Color() = palette.GetButtonGradient()

            Assert.AreEqual(3, first.Length)
            first(0) = Color.Magenta
            Assert.AreNotEqual(Color.Magenta, palette.GetButtonGradient()(0))
        End Sub

        <TestMethod>
        Public Sub FromTheme_Undefined_Throws()
            Assert.ThrowsException(Of InvalidEnumArgumentException)(Function() ThemePalette.FromTheme(CType(7, AppTheme)))
        End Sub

        ''' <summary>
        ''' Asserts the WCAG 2.x contrast ratio between two colors.
        ''' </summary>
        Private Shared Sub AssertContrast(foreground As Color, background As Color, minimum As Double, name As String)
            Dim ratio As Double = ThemePaletteTests.ContrastRatio(foreground, background)
            Assert.IsTrue(ratio >= minimum, $"{name}: contrast {ratio:0.00} < {minimum}")
        End Sub

        ''' <summary>
        ''' Computes the WCAG 2.x contrast ratio.
        ''' </summary>
        Private Shared Function ContrastRatio(a As Color, b As Color) As Double
            Dim la As Double = ThemePaletteTests.RelativeLuminance(a)
            Dim lb As Double = ThemePaletteTests.RelativeLuminance(b)
            Return (Math.Max(la, lb) + 0.05R) / (Math.Min(la, lb) + 0.05R)
        End Function

        ''' <summary>
        ''' Computes the WCAG relative luminance of a color.
        ''' </summary>
        Private Shared Function RelativeLuminance(c As Color) As Double
            Dim channel As Func(Of Byte, Double) =
                Function(v As Byte) As Double
                    Dim s As Double = v / 255.0R
                    Return If(s <= 0.03928R, s / 12.92R, Math.Pow((s + 0.055R) / 1.055R, 2.4R))
                End Function
            Return (0.2126R * channel(c.R)) + (0.7152R * channel(c.G)) + (0.0722R * channel(c.B))
        End Function

    End Class

End Namespace
