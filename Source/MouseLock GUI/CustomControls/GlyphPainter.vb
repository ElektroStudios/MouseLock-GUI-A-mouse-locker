Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Drawing.Drawing2D

#End Region

Namespace CustomControls

    ''' <summary>
    ''' Draws small vector glyphs shared by the custom controls.
    ''' </summary>
    Friend NotInheritable Class GlyphPainter

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="GlyphPainter"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Draws an upward or downward chevron centered in the specified bounds.
        ''' </summary>
        ''' <param name="g">The graphics surface.</param>
        ''' <param name="bounds">The bounds.</param>
        ''' <param name="color">The chevron color.</param>
        ''' <param name="pointsUp"><see langword="True"/> for an upward chevron; <see langword="False"/> for a downward one.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="g"/> is <see langword="Nothing"/>.</exception>
        Friend Shared Sub DrawChevron(g As Graphics, bounds As Rectangle, color As Color, pointsUp As Boolean)

            If g Is Nothing Then
                Throw New ArgumentNullException(NameOf(g))
            End If

            Dim cx As Single = bounds.Left + (bounds.Width / 2.0F)
            Dim cy As Single = bounds.Top + (bounds.Height / 2.0F)
            Dim halfWidth As Single = Math.Max(2.5F, Math.Min(bounds.Width, bounds.Height * 2) / 5.0F)
            Dim halfHeight As Single = halfWidth / 2.0F
            Dim direction As Single = If(pointsUp, -1.0F, 1.0F)

            Dim previousMode As SmoothingMode = g.SmoothingMode
            g.SmoothingMode = SmoothingMode.AntiAlias
            Using pen As New Pen(color, 1.5F)
                pen.StartCap = LineCap.Round
                pen.EndCap = LineCap.Round
                pen.LineJoin = LineJoin.Round
                g.DrawLines(pen, {New PointF(cx - halfWidth, cy - (halfHeight * direction)),
                                  New PointF(cx, cy + (halfHeight * direction)),
                                  New PointF(cx + halfWidth, cy - (halfHeight * direction))})
            End Using
            g.SmoothingMode = previousMode
        End Sub

#End Region

    End Class

End Namespace
