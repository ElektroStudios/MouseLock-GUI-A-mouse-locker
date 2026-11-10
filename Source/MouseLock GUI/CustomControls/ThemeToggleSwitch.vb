Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

#End Region

Namespace CustomControls

    ''' <summary>
    ''' An owner-drawn toggle switch (similar to the Windows 10/11 settings switches) with a caption,
    ''' intended to switch between the light and the dark theme.
    ''' </summary>
    ''' <remarks>
    ''' The thumb shows a sun glyph when the switch is off (light theme) and a moon glyph when it is on (dark theme).
    ''' The control is focusable and can be toggled with the mouse or with the SPACE key, and it exposes
    ''' itself to accessibility clients as a check button.
    ''' </remarks>
    <DefaultEvent(NameOf(ThemeToggleSwitch.CheckedChanged))>
    <DefaultProperty(NameOf(ThemeToggleSwitch.Checked))>
    Friend NotInheritable Class ThemeToggleSwitch : Inherits Control

#Region " Constants "

        ''' <summary>The width of the switch track, in pixels at 96 DPI.</summary>
        Private Const TrackWidth As Integer = 36

        ''' <summary>The height of the switch track, in pixels at 96 DPI.</summary>
        Private Const TrackHeight As Integer = 18

        ''' <summary>The space between the track and the caption, in pixels at 96 DPI.</summary>
        Private Const CaptionSpacing As Integer = 6

#End Region

#Region " Fields "

        ''' <summary>Backing field of <see cref="Checked"/>.</summary>
        Private checkedValue As Boolean

        ''' <summary>Backing field of <see cref="AccentColor"/>.</summary>
        Private accentColorValue As Color = SystemColors.Highlight

        ''' <summary>Backing field of <see cref="TrackBorderColor"/>.</summary>
        Private trackBorderColorValue As Color = SystemColors.ControlDarkDark

        ''' <summary>Backing field of <see cref="DisabledForeColor"/>.</summary>
        Private disabledForeColorValue As Color = SystemColors.GrayText

        ''' <summary>Indicates whether the mouse pointer is over the control.</summary>
        Private isHovered As Boolean

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets a value indicating whether the switch is on.
        ''' </summary>
        <Category("Behavior")>
        <Description("Indicates whether the switch is on.")>
        <DefaultValue(False)>
        Public Property Checked As Boolean
            Get
                Return Me.checkedValue
            End Get
            Set(value As Boolean)
                If Me.checkedValue = value Then
                    Return
                End If
                Me.checkedValue = value
                Me.Invalidate()
                Me.AccessibilityNotifyClients(AccessibleEvents.StateChange, -1)
                Me.OnCheckedChanged(EventArgs.Empty)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the fill color of the track when the switch is on.
        ''' </summary>
        <Category("Appearance")>
        <Description("The fill color of the track when the switch is on.")>
        Public Property AccentColor As Color
            Get
                Return Me.accentColorValue
            End Get
            Set(value As Color)
                Me.accentColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the border color of the track, and the thumb color, when the switch is off.
        ''' </summary>
        <Category("Appearance")>
        <Description("The border color of the track, and the thumb color, when the switch is off.")>
        Public Property TrackBorderColor As Color
            Get
                Return Me.trackBorderColorValue
            End Get
            Set(value As Color)
                Me.trackBorderColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the color used to paint the control when it is disabled.
        ''' </summary>
        <Category("Appearance")>
        <Description("The color used to paint the control when it is disabled.")>
        Public Property DisabledForeColor As Color
            Get
                Return Me.disabledForeColorValue
            End Get
            Set(value As Color)
                Me.disabledForeColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gets the default size of the control.
        ''' </summary>
        Protected Overrides ReadOnly Property DefaultSize As Size
            Get
                Return New Size(120, 22)
            End Get
        End Property

#End Region

#Region " Events "

        ''' <summary>
        ''' Occurs when the value of the <see cref="Checked"/> property changes.
        ''' </summary>
        <Category("Behavior")>
        <Description("Occurs when the value of the Checked property changes.")>
        Public Event CheckedChanged As EventHandler

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemeToggleSwitch"/> class.
        ''' </summary>
        Public Sub New()

            Me.SetStyle(ControlStyles.UserPaint Or
                        ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or
                        ControlStyles.ResizeRedraw Or
                        ControlStyles.Selectable Or
                        ControlStyles.StandardClick Or
                        ControlStyles.SupportsTransparentBackColor, True)
            Me.SetStyle(ControlStyles.StandardDoubleClick, False)

            Me.Cursor = Cursors.Hand
            Me.TabStop = True
            Me.AccessibleRole = AccessibleRole.CheckButton
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Toggles the <see cref="Checked"/> state.
        ''' </summary>
        Public Sub Toggle()
            Me.Checked = Not Me.Checked
        End Sub

#End Region

#Region " Protected Methods "

        ''' <summary>
        ''' Raises the <see cref="CheckedChanged"/> event.
        ''' </summary>
        ''' <param name="e">The event data.</param>
        Private Sub OnCheckedChanged(e As EventArgs)
            RaiseEvent CheckedChanged(Me, e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function CreateAccessibilityInstance() As AccessibleObject
            Return New ToggleSwitchAccessibleObject(Me)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnClick(e As EventArgs)
            MyBase.OnClick(e)
            If Me.CanFocus Then
                Me.Focus()
            End If
            Me.Toggle()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
            MyBase.OnKeyUp(e)
            If e.KeyCode = Keys.Space AndAlso e.Modifiers = Keys.None Then
                Me.Toggle()
                e.Handled = True
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseEnter(e As EventArgs)
            MyBase.OnMouseEnter(e)
            Me.isHovered = True
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            MyBase.OnMouseLeave(e)
            Me.isHovered = False
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnGotFocus(e As EventArgs)
            MyBase.OnGotFocus(e)
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLostFocus(e As EventArgs)
            MyBase.OnLostFocus(e)
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            MyBase.OnEnabledChanged(e)
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnTextChanged(e As EventArgs)
            MyBase.OnTextChanged(e)
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)

            MyBase.OnPaint(e)

            Dim g As Graphics = e.Graphics
            Dim scale As Single = g.DpiX / 96.0F
            Dim trackSize As New Size(CInt(ThemeToggleSwitch.TrackWidth * scale), CInt(ThemeToggleSwitch.TrackHeight * scale))
            Dim track As New Rectangle(1, (Me.ClientSize.Height - trackSize.Height) \ 2, trackSize.Width, trackSize.Height)

            Dim borderColor As Color = If(Me.Enabled, Me.trackBorderColorValue, Me.disabledForeColorValue)
            Dim accent As Color = If(Me.Enabled, Me.accentColorValue, Me.disabledForeColorValue)
            If Me.isHovered AndAlso Me.Enabled Then
                accent = ControlPaint.Light(accent, 0.25F)
            End If

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.PixelOffsetMode = PixelOffsetMode.HighQuality

            ' Track.
            Using trackPath As GraphicsPath = ThemeToggleSwitch.CreatePillPath(track)
                If Me.checkedValue Then
                    Using fill As New SolidBrush(accent)
                        g.FillPath(fill, trackPath)
                    End Using
                Else
                    Using pen As New Pen(borderColor, Math.Max(1.0F, scale))
                        g.DrawPath(pen, trackPath)
                    End Using
                End If
            End Using

            ' Thumb.
            Dim thumbMargin As Integer = CInt(3 * scale)
            Dim thumbDiameter As Integer = track.Height - (thumbMargin * 2)
            Dim thumbX As Integer = If(Me.checkedValue, track.Right - thumbMargin - thumbDiameter, track.Left + thumbMargin)
            Dim thumb As New RectangleF(thumbX, track.Top + thumbMargin, thumbDiameter, thumbDiameter)
            Dim thumbColor As Color = If(Me.checkedValue, Color.White, borderColor)
            Using fill As New SolidBrush(thumbColor)
                g.FillEllipse(fill, thumb)
            End Using

            ' Glyph inside the thumb: moon (on) or sun (off).
            Dim glyphColor As Color = If(Me.checkedValue, accent, If(Me.BackColor.A = 0, SystemColors.Control, Me.BackColor))
            If Me.checkedValue Then
                ThemeToggleSwitch.DrawMoonGlyph(g, thumb, glyphColor, thumbColor)
            Else
                ThemeToggleSwitch.DrawSunGlyph(g, thumb, glyphColor)
            End If

            g.SmoothingMode = SmoothingMode.Default
            g.PixelOffsetMode = PixelOffsetMode.Default

            ' Caption.
            Dim captionLeft As Integer = track.Right + CInt(ThemeToggleSwitch.CaptionSpacing * scale)
            Dim captionBounds As New Rectangle(captionLeft, 0, Math.Max(0, Me.ClientSize.Width - captionLeft), Me.ClientSize.Height)
            Dim textColor As Color = If(Me.Enabled, Me.ForeColor, Me.disabledForeColorValue)
            Dim flags As TextFormatFlags = TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                           TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis
            If Not Me.ShowKeyboardCues Then
                flags = flags Or TextFormatFlags.HidePrefix
            End If
            TextRenderer.DrawText(g, Me.Text, Me.Font, captionBounds, textColor, flags)

            ' Focus cue around the caption.
            If Me.Focused AndAlso Me.ShowFocusCues AndAlso Not String.IsNullOrEmpty(Me.Text) Then
                Dim textSize As Size = TextRenderer.MeasureText(g, Me.Text, Me.Font, captionBounds.Size, flags)
                Dim focusBounds As New Rectangle(captionLeft - 1, (Me.ClientSize.Height - textSize.Height) \ 2,
                                                 Math.Min(textSize.Width + 2, captionBounds.Width), textSize.Height)
                ControlPaint.DrawFocusRectangle(g, focusBounds, textColor, Me.BackColor)
            End If
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Creates a pill-shaped (fully rounded rectangle) path.
        ''' </summary>
        ''' <param name="bounds">The bounds of the pill.</param>
        ''' <returns>The path. The caller is responsible for disposing it.</returns>
        Private Shared Function CreatePillPath(bounds As Rectangle) As GraphicsPath

            Dim diameter As Integer = bounds.Height
            Dim path As New GraphicsPath()
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 90, 180)
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 180)
            path.CloseFigure()
            Return path
        End Function

        ''' <summary>
        ''' Draws a sun glyph (a disc surrounded by eight rays) centered in the specified bounds.
        ''' </summary>
        ''' <param name="g">The graphics surface.</param>
        ''' <param name="bounds">The bounds of the glyph.</param>
        ''' <param name="color">The glyph color.</param>
        Private Shared Sub DrawSunGlyph(g As Graphics, bounds As RectangleF, color As Color)

            Dim cx As Single = bounds.Left + (bounds.Width / 2.0F)
            Dim cy As Single = bounds.Top + (bounds.Height / 2.0F)
            Dim discRadius As Single = bounds.Width * 0.2F
            Dim rayInner As Single = bounds.Width * 0.3F
            Dim rayOuter As Single = bounds.Width * 0.42F

            Using fill As New SolidBrush(color)
                g.FillEllipse(fill, cx - discRadius, cy - discRadius, discRadius * 2.0F, discRadius * 2.0F)
            End Using

            Using pen As New Pen(color, Math.Max(1.0F, bounds.Width * 0.08F))
                pen.StartCap = LineCap.Round
                pen.EndCap = LineCap.Round
                For i As Integer = 0 To 7
                    Dim angle As Double = i * Math.PI / 4.0R
                    Dim cos As Single = CSng(Math.Cos(angle))
                    Dim sin As Single = CSng(Math.Sin(angle))
                    g.DrawLine(pen, cx + (cos * rayInner), cy + (sin * rayInner), cx + (cos * rayOuter), cy + (sin * rayOuter))
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Draws a crescent moon glyph centered in the specified bounds.
        ''' </summary>
        ''' <param name="g">The graphics surface.</param>
        ''' <param name="bounds">The bounds of the glyph.</param>
        ''' <param name="color">The glyph color.</param>
        ''' <param name="backColor">The color behind the glyph, used to carve the crescent.</param>
        Private Shared Sub DrawMoonGlyph(g As Graphics, bounds As RectangleF, color As Color, backColor As Color)

            Dim size As Single = bounds.Width * 0.62F
            Dim moon As New RectangleF(bounds.Left + ((bounds.Width - size) / 2.0F),
                                       bounds.Top + ((bounds.Height - size) / 2.0F), size, size)
            Dim carve As New RectangleF(moon.Left + (size * 0.38F), moon.Top - (size * 0.18F), size, size)

            Using fill As New SolidBrush(color)
                g.FillEllipse(fill, moon)
            End Using
            Using fill As New SolidBrush(backColor)
                g.FillEllipse(fill, carve)
            End Using
        End Sub

#End Region

#Region " ToggleSwitchAccessibleObject "

        ''' <summary>
        ''' Exposes the <see cref="ThemeToggleSwitch"/> to accessibility clients as a check button.
        ''' </summary>
        Private NotInheritable Class ToggleSwitchAccessibleObject : Inherits ControlAccessibleObject

            ''' <summary>The owner switch.</summary>
            Private ReadOnly ownerSwitch As ThemeToggleSwitch

            ''' <summary>
            ''' Initializes a new instance of the <see cref="ToggleSwitchAccessibleObject"/> class.
            ''' </summary>
            ''' <param name="owner">The owner switch.</param>
            Friend Sub New(owner As ThemeToggleSwitch)
                MyBase.New(owner)
                Me.ownerSwitch = owner
            End Sub

            ''' <inheritdoc/>
            Public Overrides ReadOnly Property Role As AccessibleRole
                Get
                    Return AccessibleRole.CheckButton
                End Get
            End Property

            ''' <inheritdoc/>
            Public Overrides ReadOnly Property State As AccessibleStates
                Get
                    Dim states As AccessibleStates = MyBase.State
                    If Me.ownerSwitch.Checked Then
                        states = states Or AccessibleStates.Checked
                    End If
                    Return states
                End Get
            End Property

            ''' <inheritdoc/>
            Public Overrides ReadOnly Property DefaultAction As String
                Get
                    Return "Toggle"
                End Get
            End Property

            ''' <inheritdoc/>
            Public Overrides Sub DoDefaultAction()
                Me.ownerSwitch.Toggle()
            End Sub

        End Class

#End Region

    End Class

End Namespace
