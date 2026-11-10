Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports MouseLock.Theming

#End Region

Namespace Theming

    ''' <summary>
    ''' Unit tests for <see cref="LogoRecolorizer"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class LogoRecolorizerTests

        Private Shared ReadOnly SourceBack As Color = ThemePalette.BrandColor
        Private Shared ReadOnly SourceFore As Color = Color.White

        <TestMethod>
        Public Sub GetCoverage_PureColors()
            Assert.AreEqual(0.0R, LogoRecolorizer.GetCoverage(SourceBack, SourceBack, SourceFore), 0.000001R)
            Assert.AreEqual(1.0R, LogoRecolorizer.GetCoverage(SourceFore, SourceBack, SourceFore), 0.000001R)
        End Sub

        <TestMethod>
        Public Sub GetCoverage_AntiAliasedEdge_IsProportional()
            Dim halfway As Color = Color.FromArgb(128, 165, 189) ' ≈ 50% between (0,74,123) and white.
            Assert.AreEqual(0.5R, LogoRecolorizer.GetCoverage(halfway, SourceBack, SourceFore), 0.01R)
        End Sub

        <TestMethod>
        Public Sub GetCoverage_IsClamped()
            Assert.AreEqual(0.0R, LogoRecolorizer.GetCoverage(Color.Black, SourceBack, SourceFore))
            Assert.AreEqual(1.0R, LogoRecolorizer.GetCoverage(Color.FromArgb(255, 255, 255), SourceBack, SourceFore))
        End Sub

        <TestMethod>
        Public Sub GetCoverage_EqualSourceColors_Throws()
            Assert.ThrowsException(Of ArgumentException)(Function() LogoRecolorizer.GetCoverage(Color.Red, Color.Blue, Color.Blue))
        End Sub

        <TestMethod>
        Public Sub Blend_Endpoints_AndMidpoint()
            Assert.AreEqual(Color.Black.ToArgb(), LogoRecolorizer.Blend(Color.Black, Color.White, 0.0R).ToArgb())
            Assert.AreEqual(Color.White.ToArgb(), LogoRecolorizer.Blend(Color.Black, Color.White, 1.0R).ToArgb())
            Assert.AreEqual(Color.FromArgb(128, 128, 128).ToArgb(), LogoRecolorizer.Blend(Color.Black, Color.White, 0.5R).ToArgb())
        End Sub

        <TestMethod>
        Public Sub Blend_AlwaysOpaque()
            Assert.AreEqual(255, LogoRecolorizer.Blend(Color.FromArgb(0, 10, 20, 30), Color.FromArgb(10, 40, 50, 60), 0.3R).A)
        End Sub

        <DataTestMethod>
        <DataRow(-0.01R)>
        <DataRow(1.01R)>
        <DataRow(Double.NaN)>
        Public Sub Blend_AmountOutOfRange_Throws(amount As Double)
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() LogoRecolorizer.Blend(Color.Black, Color.White, amount))
        End Sub

        <TestMethod>
        Public Sub Recolor_MapsBackgroundForegroundAndEdges()
            Using source As New Bitmap(3, 1)
                source.SetPixel(0, 0, SourceBack)
                source.SetPixel(1, 0, SourceFore)
                source.SetPixel(2, 0, LogoRecolorizer.Blend(SourceBack, SourceFore, 0.5R))

                Using result As Bitmap = LogoRecolorizer.Recolor(source, SourceBack, SourceFore, Color.White, Color.Black)
                    Assert.AreEqual(Color.White.ToArgb(), result.GetPixel(0, 0).ToArgb())
                    Assert.AreEqual(Color.Black.ToArgb(), result.GetPixel(1, 0).ToArgb())
                    Dim edge As Color = result.GetPixel(2, 0)
                    Assert.IsTrue(edge.R > 100 AndAlso edge.R < 156, $"Edge pixel: {edge}")
                End Using

                ' The source is not modified.
                Assert.AreEqual(SourceBack.ToArgb(), source.GetPixel(0, 0).ToArgb())
            End Using
        End Sub

        <TestMethod>
        Public Sub Recolor_KeepsTheImageSize()
            Using source As New Bitmap(17, 9)
                Using result As Bitmap = LogoRecolorizer.Recolor(source, SourceBack, SourceFore, Color.Black, Color.White)
                    Assert.AreEqual(source.Size, result.Size)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Recolor_TheRealLogo_ForEveryTheme()
            For Each theme As AppTheme In [Enum].GetValues(GetType(AppTheme))
                Dim palette As ThemePalette = ThemePalette.FromTheme(theme)
                Using logo As Bitmap = Global.MouseLock.My.Resources.Logo
                    Using result As Bitmap = LogoRecolorizer.Recolor(logo, SourceBack, SourceFore, palette.Background, palette.LogoForeground)
                        ' The top-left corner of the logo is background; it must blend with the window.
                        Assert.AreEqual(palette.Background.ToArgb(), result.GetPixel(0, 0).ToArgb(), theme.ToString())

                        ' The logo text must still be there.
                        Dim foregroundPixels As Integer = 0
                        For x As Integer = 0 To result.Width - 1 Step 2
                            For y As Integer = 0 To result.Height - 1 Step 2
                                If LogoRecolorizerTests.IsClose(result.GetPixel(x, y), palette.LogoForeground) Then
                                    foregroundPixels += 1
                                End If
                            Next
                        Next
                        Assert.IsTrue(foregroundPixels > 100, $"{theme}: {foregroundPixels} foreground pixels")
                    End Using
                End Using
            Next
        End Sub

        ''' <summary>
        ''' Determines whether two colors are equal within a small tolerance (GDI+ rounding).
        ''' </summary>
        Private Shared Function IsClose(a As Color, b As Color) As Boolean
            Const Tolerance As Integer = 8
            Return Math.Abs(CInt(a.R) - CInt(b.R)) <= Tolerance AndAlso
                   Math.Abs(CInt(a.G) - CInt(b.G)) <= Tolerance AndAlso
                   Math.Abs(CInt(a.B) - CInt(b.B)) <= Tolerance
        End Function

        <TestMethod>
        Public Sub Recolor_InvalidArguments_Throw()
            Assert.ThrowsException(Of ArgumentNullException)(Function() LogoRecolorizer.Recolor(Nothing, SourceBack, SourceFore, Color.Black, Color.White))
            Using source As New Bitmap(1, 1)
                Assert.ThrowsException(Of ArgumentException)(Function() LogoRecolorizer.Recolor(source, Color.Red, Color.Red, Color.Black, Color.White))
            End Using
        End Sub

    End Class

End Namespace
