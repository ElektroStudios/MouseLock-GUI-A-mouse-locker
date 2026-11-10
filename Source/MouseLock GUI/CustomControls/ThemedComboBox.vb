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
    ''' A drop-down list <see cref="ComboBox"/> that can be fully painted with custom colors
    ''' (closed box, arrow button, border and drop-down items), as required by a dark theme.
    ''' </summary>
    ''' <remarks>
    ''' When <see cref="UseThemedRendering"/> is <see langword="False"/> the closed box keeps the native
    ''' Windows look; the drop-down items are always owner-drawn using the configured colors.
    ''' </remarks>
    Friend NotInheritable Class ThemedComboBox : Inherits ComboBox

#Region " Fields "

        ''' <summary>Backing field of <see cref="UseThemedRendering"/>.</summary>
        Private useThemedRenderingValue As Boolean

        ''' <summary>Backing field of <see cref="BorderColor"/>.</summary>
        Private borderColorValue As Color = SystemColors.ControlDark

        ''' <summary>Backing field of <see cref="ButtonBackColor"/>.</summary>
        Private buttonBackColorValue As Color = SystemColors.Control

        ''' <summary>Backing field of <see cref="ArrowColor"/>.</summary>
        Private arrowColorValue As Color = SystemColors.ControlText

        ''' <summary>Backing field of <see cref="DisabledBackColor"/>.</summary>
        Private disabledBackColorValue As Color = SystemColors.Control

        ''' <summary>Backing field of <see cref="DisabledForeColor"/>.</summary>
        Private disabledForeColorValue As Color = SystemColors.GrayText

        ''' <summary>Backing field of <see cref="HighlightBackColor"/>.</summary>
        Private highlightBackColorValue As Color = SystemColors.Highlight

        ''' <summary>Backing field of <see cref="HighlightForeColor"/>.</summary>
        Private highlightForeColorValue As Color = SystemColors.HighlightText

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets a value indicating whether the closed box is painted with the custom colors
        ''' instead of the native visual style, and whether the drop-down list uses the dark native style.
        ''' </summary>
        <Category("Appearance")>
        <DefaultValue(False)>
        <Description("Indicates whether the control is fully painted with the custom colors.")>
        Public Property UseThemedRendering As Boolean
            Get
                Return Me.useThemedRenderingValue
            End Get
            Set(value As Boolean)
                If Me.useThemedRenderingValue = value Then
                    Return
                End If
                Me.useThemedRenderingValue = value
                If Me.IsHandleCreated Then
                    Me.ApplyNativeTheme()
                End If
                Me.Invalidate()
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
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>Gets or sets the background color of the arrow button used by the themed rendering.</summary>
        <Category("Appearance")>
        Public Property ButtonBackColor As Color
            Get
                Return Me.buttonBackColorValue
            End Get
            Set(value As Color)
                Me.buttonBackColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>Gets or sets the color of the arrow glyph used by the themed rendering.</summary>
        <Category("Appearance")>
        Public Property ArrowColor As Color
            Get
                Return Me.arrowColorValue
            End Get
            Set(value As Color)
                Me.arrowColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>Gets or sets the background color of the closed box when the control is disabled (themed rendering).</summary>
        <Category("Appearance")>
        Public Property DisabledBackColor As Color
            Get
                Return Me.disabledBackColorValue
            End Get
            Set(value As Color)
                Me.disabledBackColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>Gets or sets the text color of the closed box when the control is disabled (themed rendering).</summary>
        <Category("Appearance")>
        Public Property DisabledForeColor As Color
            Get
                Return Me.disabledForeColorValue
            End Get
            Set(value As Color)
                Me.disabledForeColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>Gets or sets the background color of the selected drop-down item.</summary>
        <Category("Appearance")>
        Public Property HighlightBackColor As Color
            Get
                Return Me.highlightBackColorValue
            End Get
            Set(value As Color)
                Me.highlightBackColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>Gets or sets the text color of the selected drop-down item.</summary>
        <Category("Appearance")>
        Public Property HighlightForeColor As Color
            Get
                Return Me.highlightForeColorValue
            End Get
            Set(value As Color)
                Me.highlightForeColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the drawing mode. Always <see cref="System.Windows.Forms.DrawMode.OwnerDrawFixed"/>.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Shadows Property DrawMode As DrawMode
            Get
                Return MyBase.DrawMode
            End Get
            Set(value As DrawMode)
                If value <> DrawMode.OwnerDrawFixed Then
                    Throw New NotSupportedException($"{NameOf(ThemedComboBox)} only supports {NameOf(DrawMode.OwnerDrawFixed)}.")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the drop-down style. Always <see cref="ComboBoxStyle.DropDownList"/>.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Shadows Property DropDownStyle As ComboBoxStyle
            Get
                Return MyBase.DropDownStyle
            End Get
            Set(value As ComboBoxStyle)
                If value <> ComboBoxStyle.DropDownList Then
                    Throw New NotSupportedException($"{NameOf(ThemedComboBox)} only supports {NameOf(ComboBoxStyle.DropDownList)}.")
                End If
            End Set
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedComboBox"/> class.
        ''' </summary>
        Public Sub New()
            MyBase.DrawMode = DrawMode.OwnerDrawFixed
            MyBase.DropDownStyle = ComboBoxStyle.DropDownList
        End Sub

#End Region

#Region " Protected Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnHandleCreated(e As EventArgs)
            MyBase.OnHandleCreated(e)
            Me.ApplyNativeTheme()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)

            MyBase.OnDrawItem(e)
            If e.Index < 0 OrElse e.Index >= Me.Items.Count Then
                Return
            End If

            Dim isSelected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
            Dim isEditPortion As Boolean = (e.State And DrawItemState.ComboBoxEdit) = DrawItemState.ComboBoxEdit
            Dim backColor As Color
            Dim foreColor As Color
            If Not Me.Enabled Then
                backColor = Me.disabledBackColorValue
                foreColor = Me.disabledForeColorValue
            ElseIf isSelected AndAlso (Not isEditPortion OrElse Me.Focused) Then
                backColor = Me.highlightBackColorValue
                foreColor = Me.highlightForeColorValue
            Else
                backColor = Me.BackColor
                foreColor = Me.ForeColor
            End If

            Using fill As New SolidBrush(backColor)
                e.Graphics.FillRectangle(fill, e.Bounds)
            End Using
            TextRenderer.DrawText(e.Graphics, Me.GetItemText(Me.Items(e.Index)), e.Font, e.Bounds, foreColor, backColor,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or
                                  TextFormatFlags.NoPrefix Or TextFormatFlags.EndEllipsis)

            If (e.State And DrawItemState.Focus) = DrawItemState.Focus AndAlso
               (e.State And DrawItemState.NoFocusRect) <> DrawItemState.NoFocusRect Then
                e.DrawFocusRectangle()
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub WndProc(ByRef m As Message)

            MyBase.WndProc(m)

            ' Paint over the native rendering (closed box only; the drop-down list is a separate window).
            If m.Msg = NativeMethods.WM_PAINT AndAlso Me.useThemedRenderingValue Then
                Using g As Graphics = Me.CreateGraphics()
                    Using buffer As BufferedGraphics = BufferedGraphicsManager.Current.Allocate(g, Me.ClientRectangle)
                        Me.PaintThemed(buffer.Graphics)
                        buffer.Render(g)
                    End Using
                End Using
            End If
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
        Protected Overrides Sub OnDropDown(e As EventArgs)
            MyBase.OnDropDown(e)
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnDropDownClosed(e As EventArgs)
            MyBase.OnDropDownClosed(e)
            Me.Invalidate()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnSelectedIndexChanged(e As EventArgs)
            MyBase.OnSelectedIndexChanged(e)
            Me.Invalidate()
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Applies the native visual style matching <see cref="UseThemedRendering"/> (affects the drop-down list and its scroll bar).
        ''' </summary>
        Private Sub ApplyNativeTheme()
            WindowTheme.Apply(Me.Handle, If(Me.useThemedRenderingValue, WindowTheme.DarkComboBox, Nothing))
        End Sub

        ''' <summary>
        ''' Paints the closed box with the custom colors.
        ''' </summary>
        ''' <param name="g">The graphics surface.</param>
        Private Sub PaintThemed(g As Graphics)

            Dim bounds As Rectangle = Me.ClientRectangle
            Dim enabled As Boolean = Me.Enabled
            Dim backColor As Color = If(enabled, Me.BackColor, Me.disabledBackColorValue)
            Dim foreColor As Color = If(enabled, Me.ForeColor, Me.disabledForeColorValue)
            Dim borderColor As Color = If(enabled AndAlso (Me.Focused OrElse Me.DroppedDown), Me.highlightBackColorValue, Me.borderColorValue)

            Using fill As New SolidBrush(backColor)
                g.FillRectangle(fill, bounds)
            End Using

            ' Arrow button.
            Dim buttonWidth As Integer = SystemInformation.VerticalScrollBarWidth
            Dim button As New Rectangle(bounds.Right - buttonWidth - 1, bounds.Top + 1, buttonWidth, bounds.Height - 2)
            Using fill As New SolidBrush(If(enabled, Me.buttonBackColorValue, Me.disabledBackColorValue))
                g.FillRectangle(fill, button)
            End Using
            GlyphPainter.DrawChevron(g, button, If(enabled, Me.arrowColorValue, Me.disabledForeColorValue), pointsUp:=False)

            ' Selected item text.
            If Me.SelectedIndex >= 0 Then
                Dim textBounds As New Rectangle(bounds.Left + 3, bounds.Top, button.Left - bounds.Left - 4, bounds.Height)
                TextRenderer.DrawText(g, Me.GetItemText(Me.SelectedItem), Me.Font, textBounds, foreColor, backColor,
                                      TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or
                                      TextFormatFlags.NoPrefix Or TextFormatFlags.EndEllipsis)
                If Me.Focused AndAlso Me.ShowFocusCues AndAlso Not Me.DroppedDown Then
                    Dim focusBounds As New Rectangle(bounds.Left + 2, bounds.Top + 2, button.Left - bounds.Left - 3, bounds.Height - 4)
                    ControlPaint.DrawFocusRectangle(g, focusBounds, foreColor, backColor)
                End If
            End If

            ' Border.
            Using pen As New Pen(borderColor)
                g.DrawRectangle(pen, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1)
            End Using
        End Sub

#End Region

    End Class

End Namespace
