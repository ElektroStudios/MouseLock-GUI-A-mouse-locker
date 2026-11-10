Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

#End Region

Namespace CustomControls

    ''' <summary>
    ''' A <see cref="Panel"/> that draws a single-pixel border with a customizable color.
    ''' </summary>
    ''' <remarks>
    ''' A regular panel with <see cref="BorderStyle.FixedSingle"/> always uses a black frame,
    ''' which is almost invisible over a dark background.
    ''' </remarks>
    Friend NotInheritable Class ThemedBorderPanel : Inherits Panel

#Region " Fields "

        ''' <summary>Backing field of <see cref="BorderColor"/>.</summary>
        Private borderColorValue As Color = SystemColors.ControlDark

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the color of the border.
        ''' </summary>
        <Category("Appearance")>
        <Description("The color of the border.")>
        Public Property BorderColor As Color
            Get
                Return Me.borderColorValue
            End Get
            Set(value As Color)
                If Me.borderColorValue = value Then
                    Return
                End If
                Me.borderColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the border style. Always <see cref="System.Windows.Forms.BorderStyle.None"/>: the border is custom drawn.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Shadows Property BorderStyle As BorderStyle
            Get
                Return BorderStyle.None
            End Get
            Set(value As BorderStyle)
                If value <> BorderStyle.None Then
                    Throw New NotSupportedException($"{NameOf(ThemedBorderPanel)} draws its own border; use the {NameOf(ThemedBorderPanel.BorderColor)} property instead.")
                End If
            End Set
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedBorderPanel"/> class.
        ''' </summary>
        Public Sub New()
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
            Me.Padding = New Padding(1)
        End Sub

#End Region

#Region " Protected Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)

            MyBase.OnPaint(e)
            Using pen As New Pen(Me.borderColorValue)
                e.Graphics.DrawRectangle(pen, 0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1)
            End Using
        End Sub

#End Region

    End Class

End Namespace
