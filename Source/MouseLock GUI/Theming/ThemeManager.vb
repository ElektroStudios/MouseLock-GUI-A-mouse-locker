Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Windows.Forms

Imports MouseLock.CustomControls

#End Region

Namespace Theming

    ''' <summary>
    ''' Applies a <see cref="ThemePalette"/> to a tree of controls and to tool strips.
    ''' </summary>
    Friend NotInheritable Class ThemeManager

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ThemeManager"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Applies the palette to the specified control and all its descendants.
        ''' </summary>
        ''' <remarks>
        ''' The control tree is traversed iteratively (no recursion).
        ''' The descendants of an excluded control are still themed.
        ''' </remarks>
        ''' <param name="root">The root control, typically a form.</param>
        ''' <param name="palette">The palette to apply.</param>
        ''' <param name="excludedControls">Controls whose own colors must be preserved (e.g. the colored lock button).</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="root"/> or <paramref name="palette"/> is <see langword="Nothing"/>.</exception>
        Friend Shared Sub Apply(root As Control, palette As ThemePalette, ParamArray excludedControls As Control())

            If root Is Nothing Then
                Throw New ArgumentNullException(NameOf(root))
            End If
            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            Dim excluded As New HashSet(Of Control)
            If excludedControls IsNot Nothing Then
                excluded.UnionWith(excludedControls)
            End If

            root.SuspendLayout()
            Try
                Dim pending As New Stack(Of Control)
                pending.Push(root)
                While pending.Count <> 0
                    Dim current As Control = pending.Pop()
                    If Not excluded.Contains(current) Then
                        ThemeManager.ApplyToControl(current, palette)
                    End If
                    For Each child As Control In current.Controls
                        pending.Push(child)
                    Next
                End While
            Finally
                root.ResumeLayout(performLayout:=False)
            End Try

            root.Invalidate(invalidateChildren:=True)
        End Sub

        ''' <summary>
        ''' Applies the palette to a tool strip (e.g. a context menu) and its items.
        ''' </summary>
        ''' <param name="toolStrip">The tool strip.</param>
        ''' <param name="palette">The palette to apply.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="toolStrip"/> or <paramref name="palette"/> is <see langword="Nothing"/>.</exception>
        Friend Shared Sub ApplyToToolStrip(toolStrip As ToolStrip, palette As ThemePalette)

            If toolStrip Is Nothing Then
                Throw New ArgumentNullException(NameOf(toolStrip))
            End If
            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            If palette.IsDark Then
                toolStrip.Renderer = New ToolStripProfessionalRenderer(New ThemedToolStripColorTable(palette)) With {
                    .RoundedEdges = False
                }
                toolStrip.BackColor = palette.InputBackground
                toolStrip.ForeColor = palette.InputForeground
            Else
                ' The default Windows look: the manager renderer and the system menu colors.
                toolStrip.RenderMode = ToolStripRenderMode.ManagerRenderMode
                toolStrip.BackColor = SystemColors.Menu
                toolStrip.ForeColor = SystemColors.MenuText
            End If

            For Each item As ToolStripItem In toolStrip.Items
                item.ForeColor = toolStrip.ForeColor
            Next
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Applies the palette to a single control, according to its type.
        ''' </summary>
        ''' <param name="control">The control.</param>
        ''' <param name="palette">The palette.</param>
        Private Shared Sub ApplyToControl(control As Control, palette As ThemePalette)

            Dim isDark As Boolean = palette.IsDark

            If TypeOf control Is Form Then
                control.BackColor = palette.Background
                control.ForeColor = palette.Foreground

            ElseIf TypeOf control Is ThemedBorderPanel Then
                Dim panel As ThemedBorderPanel = DirectCast(control, ThemedBorderPanel)
                panel.BackColor = palette.Background
                panel.ForeColor = palette.Foreground
                panel.BorderColor = palette.Border

            ElseIf TypeOf control Is ThemeToggleSwitch Then
                Dim toggle As ThemeToggleSwitch = DirectCast(control, ThemeToggleSwitch)
                toggle.BackColor = palette.Background
                toggle.ForeColor = palette.Foreground
                toggle.AccentColor = palette.Accent
                toggle.TrackBorderColor = If(isDark, palette.Foreground, SystemColors.ControlDarkDark)
                toggle.DisabledForeColor = palette.DisabledForeground

            ElseIf TypeOf control Is ThemedComboBox Then
                Dim combo As ThemedComboBox = DirectCast(control, ThemedComboBox)
                combo.UseThemedRendering = isDark
                combo.BackColor = palette.InputBackground
                combo.ForeColor = palette.InputForeground
                combo.BorderColor = palette.Border
                combo.ButtonBackColor = palette.InputBackground
                combo.ArrowColor = palette.InputForeground
                combo.DisabledBackColor = palette.DisabledInputBackground
                combo.DisabledForeColor = palette.DisabledForeground
                combo.HighlightBackColor = palette.Highlight
                combo.HighlightForeColor = palette.HighlightForeground

            ElseIf TypeOf control Is ThemedNumericUpDown Then
                Dim numeric As ThemedNumericUpDown = DirectCast(control, ThemedNumericUpDown)
                numeric.UseThemedRendering = isDark
                numeric.BackColor = palette.InputBackground
                numeric.ForeColor = palette.InputForeground
                numeric.BorderColor = palette.Border
                numeric.FocusBorderColor = palette.Highlight
                numeric.ButtonBackColor = palette.HeaderBackground
                numeric.ArrowColor = palette.InputForeground
                numeric.DisabledForeColor = palette.DisabledForeground

            ElseIf TypeOf control Is ThemedListView Then
                Dim list As ThemedListView = DirectCast(control, ThemedListView)
                list.UseThemedRendering = isDark
                list.BackColor = palette.InputBackground
                list.ForeColor = palette.InputForeground
                list.HeaderBackColor = palette.HeaderBackground
                list.HeaderForeColor = palette.Foreground
                list.HeaderBorderColor = palette.Border

            ElseIf TypeOf control Is TextBoxBase Then
                Dim textBox As TextBoxBase = DirectCast(control, TextBoxBase)
                textBox.BackColor = palette.InputBackground
                textBox.ForeColor = palette.InputForeground
                Dim originalHeight As Integer = textBox.Height
                textBox.BorderStyle = If(isDark, BorderStyle.FixedSingle, BorderStyle.Fixed3D)
                If Not textBox.Multiline AndAlso textBox.Height <> originalHeight Then
                    ' Keep the designed height: a flat border would make the box 2 pixels shorter.
                    textBox.AutoSize = False
                    textBox.Height = originalHeight
                End If

            ElseIf TypeOf control Is gTimePickerControl.gTimePicker Then
                Dim picker As gTimePickerControl.gTimePicker = DirectCast(control, gTimePickerControl.gTimePicker)
                picker.BackColor = palette.Background
                picker.TextBackColor = palette.InputBackground
                picker.TextForeColor = palette.InputForeground
                picker.ButtonForeColor = palette.InputForeground

            ElseIf TypeOf control Is CButtonLib.CButton Then
                Dim button As CButtonLib.CButton = DirectCast(control, CButtonLib.CButton)
                Dim gradient As Color() = palette.GetButtonGradient()
                For i As Integer = 0 To Math.Min(gradient.Length, button.ColorFillBlend.iColor.Length) - 1
                    button.ColorFillBlend.iColor(i) = gradient(i)
                Next
                button.BorderColor = palette.Border
                button.ForeColor = palette.ButtonForeground
                button.UpdateDimBlends()

            ElseIf TypeOf control Is Label OrElse TypeOf control Is CheckBox OrElse TypeOf control Is RadioButton Then
                ' Labels, check boxes and radio buttons stay transparent: Windows computes a readable
                ' disabled text color from the (transparent) background color.
                control.BackColor = Color.Transparent
                control.ForeColor = palette.Foreground

            ElseIf TypeOf control Is PictureBox Then
                control.BackColor = palette.Background

            Else
                control.BackColor = palette.Background
                control.ForeColor = palette.Foreground
            End If
        End Sub

#End Region

    End Class

End Namespace
