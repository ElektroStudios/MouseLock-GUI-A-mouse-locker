Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Imports MouseLock.Interop

#End Region

Namespace CustomControls

    ''' <summary>
    ''' A <see cref="NumericUpDown"/> whose border and spin buttons can be painted with custom colors,
    ''' as required by a dark theme.
    ''' </summary>
    ''' <remarks>
    ''' The spin buttons are an internal child control of <see cref="UpDownBase"/>; they are subclassed
    ''' with a <see cref="NativeWindow"/> so they can be repainted after their own WM_PAINT processing.
    ''' </remarks>
    Friend NotInheritable Class ThemedNumericUpDown : Inherits NumericUpDown

#Region " Fields "

        ''' <summary>Backing field of <see cref="UseThemedRendering"/>.</summary>
        Private useThemedRenderingValue As Boolean

        ''' <summary>Backing field of <see cref="BorderColor"/>.</summary>
        Private borderColorValue As Color = SystemColors.ControlDark

        ''' <summary>Backing field of <see cref="FocusBorderColor"/>.</summary>
        Private focusBorderColorValue As Color = SystemColors.Highlight

        ''' <summary>Backing field of <see cref="ButtonBackColor"/>.</summary>
        Private buttonBackColorValue As Color = SystemColors.Control

        ''' <summary>Backing field of <see cref="ArrowColor"/>.</summary>
        Private arrowColorValue As Color = SystemColors.ControlText

        ''' <summary>Backing field of <see cref="DisabledForeColor"/>.</summary>
        Private disabledForeColorValue As Color = SystemColors.GrayText

        ''' <summary>The subclass of the spin buttons child control.</summary>
        Private ReadOnly buttonsPainter As SpinButtonsPainter

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets a value indicating whether the border and the spin buttons are painted with the custom colors.
        ''' </summary>
        <Category("Appearance")>
        <DefaultValue(False)>
        <Description("Indicates whether the border and the spin buttons are painted with the custom colors.")>
        Public Property UseThemedRendering As Boolean
            Get
                Return Me.useThemedRenderingValue
            End Get
            Set(value As Boolean)
                If Me.useThemedRenderingValue = value Then
                    Return
                End If
                Me.useThemedRenderingValue = value
                Me.InvalidateAll()
            End Set
        End Property

        ''' <summary>Gets or sets the border color used by the themed rendering.</summary>
        <Category("Appearance")>
        Public Property BorderColor As Color
            Get
                Return Me.borderColorValue
            End Get
            Set(value As Color)
                Me.borderColorValue = value
                Me.InvalidateAll()
            End Set
        End Property

        ''' <summary>Gets or sets the border color used by the themed rendering while the control has the focus.</summary>
        <Category("Appearance")>
        Public Property FocusBorderColor As Color
            Get
                Return Me.focusBorderColorValue
            End Get
            Set(value As Color)
                Me.focusBorderColorValue = value
                Me.InvalidateAll()
            End Set
        End Property

        ''' <summary>Gets or sets the background color of the spin buttons used by the themed rendering.</summary>
        <Category("Appearance")>
        Public Property ButtonBackColor As Color
            Get
                Return Me.buttonBackColorValue
            End Get
            Set(value As Color)
                Me.buttonBackColorValue = value
                Me.InvalidateAll()
            End Set
        End Property

        ''' <summary>Gets or sets the color of the spin button arrows used by the themed rendering.</summary>
        <Category("Appearance")>
        Public Property ArrowColor As Color
            Get
                Return Me.arrowColorValue
            End Get
            Set(value As Color)
                Me.arrowColorValue = value
                Me.InvalidateAll()
            End Set
        End Property

        ''' <summary>Gets or sets the color of the spin button arrows when the control is disabled (themed rendering).</summary>
        <Category("Appearance")>
        Public Property DisabledForeColor As Color
            Get
                Return Me.disabledForeColorValue
            End Get
            Set(value As Color)
                Me.disabledForeColorValue = value
                Me.InvalidateAll()
            End Set
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedNumericUpDown"/> class.
        ''' </summary>
        Public Sub New()

            ' The spin buttons are the only child that is not the text editor.
            Dim spinButtons As Control = Me.Controls.Cast(Of Control)().FirstOrDefault(Function(c As Control) Not TypeOf c Is TextBoxBase)
            If spinButtons Is Nothing Then
                Throw New InvalidOperationException("The spin buttons child control of the NumericUpDown was not found.")
            End If
            Me.buttonsPainter = New SpinButtonsPainter(Me, spinButtons)
        End Sub

#End Region

#Region " Protected Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)

            MyBase.OnPaint(e)
            If Not Me.useThemedRenderingValue Then
                Return
            End If

            ' Replace the native (light) visual style border with a flat one.
            Dim bounds As Rectangle = Me.ClientRectangle
            Using innerPen As New Pen(Me.BackColor)
                e.Graphics.DrawRectangle(innerPen, bounds.Left + 1, bounds.Top + 1, bounds.Width - 3, bounds.Height - 3)
            End Using
            Using borderPen As New Pen(If(Me.Focused OrElse Me.ContainsFocus, Me.focusBorderColorValue, Me.borderColorValue))
                e.Graphics.DrawRectangle(borderPen, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1)
            End Using
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnter(e As EventArgs)
            MyBase.OnEnter(e)
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLeave(e As EventArgs)
            MyBase.OnLeave(e)
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            MyBase.OnEnabledChanged(e)
            Me.InvalidateAll()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                Me.buttonsPainter.ReleaseHandle()
            End If
            MyBase.Dispose(disposing)
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Invalidates the control and its children.
        ''' </summary>
        Private Sub InvalidateAll()
            Me.Invalidate(invalidateChildren:=True)
        End Sub

        ''' <summary>
        ''' Paints the spin buttons with the custom colors.
        ''' </summary>
        ''' <param name="g">The graphics surface of the spin buttons control.</param>
        ''' <param name="bounds">The client bounds of the spin buttons control.</param>
        ''' <param name="pressedButton">-1 for none, 0 for the up button, 1 for the down button.</param>
        Private Sub PaintSpinButtons(g As Graphics, bounds As Rectangle, pressedButton As Integer)

            Dim halfHeight As Integer = bounds.Height \ 2
            Dim upBounds As New Rectangle(bounds.Left, bounds.Top, bounds.Width, halfHeight)
            Dim downBounds As New Rectangle(bounds.Left, bounds.Top + halfHeight, bounds.Width, bounds.Height - halfHeight)
            Dim arrow As Color = If(Me.Enabled, Me.arrowColorValue, Me.disabledForeColorValue)

            Using normal As New SolidBrush(Me.buttonBackColorValue)
                Using pressed As New SolidBrush(ControlPaint.Light(Me.buttonBackColorValue, 0.5F))
                    g.FillRectangle(If(pressedButton = 0, pressed, normal), upBounds)
                    g.FillRectangle(If(pressedButton = 1, pressed, normal), downBounds)
                End Using
            End Using
            Using pen As New Pen(Me.borderColorValue)
                g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom)
                g.DrawLine(pen, bounds.Left, upBounds.Bottom, bounds.Right, upBounds.Bottom)
            End Using

            GlyphPainter.DrawChevron(g, upBounds, arrow, pointsUp:=True)
            GlyphPainter.DrawChevron(g, downBounds, arrow, pointsUp:=False)
        End Sub

#End Region

#Region " SpinButtonsPainter "

        ''' <summary>
        ''' Subclasses the spin buttons child control to repaint it with the custom colors.
        ''' </summary>
        Private NotInheritable Class SpinButtonsPainter : Inherits NativeWindow

            ''' <summary>The owner control.</summary>
            Private ReadOnly owner As ThemedNumericUpDown

            ''' <summary>The spin buttons child control.</summary>
            Private ReadOnly buttons As Control

            ''' <summary>
            ''' Initializes a new instance of the <see cref="SpinButtonsPainter"/> class.
            ''' </summary>
            ''' <param name="owner">The owner control.</param>
            ''' <param name="buttons">The spin buttons child control.</param>
            Friend Sub New(owner As ThemedNumericUpDown, buttons As Control)

                Me.owner = owner
                Me.buttons = buttons
                AddHandler buttons.HandleCreated, AddressOf Me.Buttons_HandleCreated
                AddHandler buttons.HandleDestroyed, AddressOf Me.Buttons_HandleDestroyed
                AddHandler buttons.MouseDown, AddressOf Me.Buttons_MouseChanged
                AddHandler buttons.MouseUp, AddressOf Me.Buttons_MouseChanged
                If buttons.IsHandleCreated Then
                    Me.AssignHandle(buttons.Handle)
                End If
            End Sub

            ''' <inheritdoc/>
            Protected Overrides Sub WndProc(ByRef m As Message)

                MyBase.WndProc(m)
                If m.Msg = NativeMethods.WM_PAINT AndAlso Me.owner.UseThemedRendering Then
                    Using g As Graphics = Graphics.FromHwnd(m.HWnd)
                        Me.owner.PaintSpinButtons(g, Me.buttons.ClientRectangle, Me.GetPressedButton())
                    End Using
                End If
            End Sub

            ''' <summary>
            ''' Determines which spin button is being pressed with the left mouse button.
            ''' </summary>
            ''' <returns>-1 for none, 0 for the up button, 1 for the down button.</returns>
            Private Function GetPressedButton() As Integer

                If (Control.MouseButtons And MouseButtons.Left) <> MouseButtons.Left Then
                    Return -1
                End If
                Dim location As Point = Me.buttons.PointToClient(Control.MousePosition)
                If Not Me.buttons.ClientRectangle.Contains(location) Then
                    Return -1
                End If
                Return If(location.Y < Me.buttons.ClientSize.Height \ 2, 0, 1)
            End Function

            ''' <summary>Handles the <see cref="Control.HandleCreated"/> event of the spin buttons.</summary>
            Private Sub Buttons_HandleCreated(sender As Object, e As EventArgs)
                Me.AssignHandle(Me.buttons.Handle)
            End Sub

            ''' <summary>Handles the <see cref="Control.HandleDestroyed"/> event of the spin buttons.</summary>
            Private Sub Buttons_HandleDestroyed(sender As Object, e As EventArgs)
                Me.ReleaseHandle()
            End Sub

            ''' <summary>Handles the mouse button events of the spin buttons, to repaint the pressed state.</summary>
            Private Sub Buttons_MouseChanged(sender As Object, e As MouseEventArgs)
                Me.buttons.Invalidate()
            End Sub

        End Class

#End Region

    End Class

End Namespace
