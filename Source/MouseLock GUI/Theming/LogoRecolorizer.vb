Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

#End Region

Namespace Theming

    ''' <summary>
    ''' Recolors a two-tone image (such as the MouseLock logo: white shapes over a solid blue background)
    ''' into another pair of colors, preserving the anti-aliased edges.
    ''' </summary>
    ''' <remarks>
    ''' Every source pixel is interpreted as a linear blend between the source background and foreground
    ''' colors. The blend factor is obtained by projecting the pixel onto the background-foreground segment
    ''' in RGB space, and is then used to blend the target colors.
    ''' </remarks>
    Friend NotInheritable Class LogoRecolorizer

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="LogoRecolorizer"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Computes the foreground coverage (0 = pure background, 1 = pure foreground) of a source pixel.
        ''' </summary>
        ''' <param name="pixel">The source pixel.</param>
        ''' <param name="sourceBackground">The source background color.</param>
        ''' <param name="sourceForeground">The source foreground color.</param>
        ''' <returns>The coverage, clamped to the range 0 - 1.</returns>
        ''' <exception cref="ArgumentException">Thrown when both source colors are equal.</exception>
        Friend Shared Function GetCoverage(pixel As Color, sourceBackground As Color, sourceForeground As Color) As Double

            Dim dr As Integer = CInt(sourceForeground.R) - CInt(sourceBackground.R)
            Dim dg As Integer = CInt(sourceForeground.G) - CInt(sourceBackground.G)
            Dim db As Integer = CInt(sourceForeground.B) - CInt(sourceBackground.B)
            Dim lengthSquared As Integer = (dr * dr) + (dg * dg) + (db * db)
            If lengthSquared = 0 Then
                Throw New ArgumentException("The source background and foreground colors must be different.", NameOf(sourceForeground))
            End If

            Dim pr As Integer = CInt(pixel.R) - CInt(sourceBackground.R)
            Dim pg As Integer = CInt(pixel.G) - CInt(sourceBackground.G)
            Dim pb As Integer = CInt(pixel.B) - CInt(sourceBackground.B)
            Dim coverage As Double = ((pr * dr) + (pg * dg) + (pb * db)) / CDbl(lengthSquared)
            Return Math.Max(0.0R, Math.Min(1.0R, coverage))
        End Function

        ''' <summary>
        ''' Linearly blends two colors.
        ''' </summary>
        ''' <param name="background">The color for an amount of 0.</param>
        ''' <param name="foreground">The color for an amount of 1.</param>
        ''' <param name="amount">The blend amount, in the range 0 - 1.</param>
        ''' <returns>The blended, fully opaque color.</returns>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is out of range.</exception>
        Friend Shared Function Blend(background As Color, foreground As Color, amount As Double) As Color

            If Double.IsNaN(amount) OrElse amount < 0.0R OrElse amount > 1.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(amount), amount, "The amount must be in the range 0 - 1.")
            End If

            Dim r As Integer = CInt(Math.Round(background.R + ((CInt(foreground.R) - CInt(background.R)) * amount)))
            Dim g As Integer = CInt(Math.Round(background.G + ((CInt(foreground.G) - CInt(background.G)) * amount)))
            Dim b As Integer = CInt(Math.Round(background.B + ((CInt(foreground.B) - CInt(background.B)) * amount)))
            Return Color.FromArgb(255, r, g, b)
        End Function

        ''' <summary>
        ''' Creates a recolored copy of a two-tone image.
        ''' </summary>
        ''' <param name="source">The source image. It is not modified.</param>
        ''' <param name="sourceBackground">The background color of the source image.</param>
        ''' <param name="sourceForeground">The foreground color of the source image.</param>
        ''' <param name="targetBackground">The background color of the new image.</param>
        ''' <param name="targetForeground">The foreground color of the new image.</param>
        ''' <returns>A new 32-bpp ARGB bitmap. The caller is responsible for disposing it.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentException">Thrown when both source colors are equal.</exception>
        Friend Shared Function Recolor(source As Image,
                                       sourceBackground As Color, sourceForeground As Color,
                                       targetBackground As Color, targetForeground As Color) As Bitmap

            If source Is Nothing Then
                Throw New ArgumentNullException(NameOf(source))
            End If
            If sourceBackground.ToArgb() = sourceForeground.ToArgb() Then
                Throw New ArgumentException("The source background and foreground colors must be different.", NameOf(sourceForeground))
            End If

            ' Opaque target colors: the logo is painted as an opaque block.
            Dim opaqueBackground As Color = Color.FromArgb(255, targetBackground)
            Dim opaqueForeground As Color = Color.FromArgb(255, targetForeground)

            Dim result As New Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb)
            Try
                Using g As Graphics = Graphics.FromImage(result)
                    g.DrawImage(source, New Rectangle(0, 0, source.Width, source.Height))
                End Using

                Dim bounds As New Rectangle(0, 0, result.Width, result.Height)
                Dim data As BitmapData = result.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
                Try
                    Dim pixelCount As Integer = result.Width * result.Height
                    Dim pixels(pixelCount - 1) As Integer
                    Dim rowLength As Integer = result.Width
                    For y As Integer = 0 To result.Height - 1
                        Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), pixels, y * rowLength, rowLength)
                    Next

                    ' Cache: the logo contains a few thousand distinct colors at most.
                    Dim cache As New Dictionary(Of Integer, Integer)
                    For i As Integer = 0 To pixelCount - 1
                        Dim argb As Integer = pixels(i)
                        Dim mapped As Integer
                        If Not cache.TryGetValue(argb, mapped) Then
                            Dim coverage As Double = LogoRecolorizer.GetCoverage(Color.FromArgb(argb), sourceBackground, sourceForeground)
                            mapped = LogoRecolorizer.Blend(opaqueBackground, opaqueForeground, coverage).ToArgb()
                            cache.Add(argb, mapped)
                        End If
                        pixels(i) = mapped
                    Next

                    For y As Integer = 0 To result.Height - 1
                        Marshal.Copy(pixels, y * rowLength, IntPtr.Add(data.Scan0, y * data.Stride), rowLength)
                    Next
                Finally
                    result.UnlockBits(data)
                End Try

                Return result
            Catch
                result.Dispose()
                Throw
            End Try
        End Function

#End Region

    End Class

End Namespace
