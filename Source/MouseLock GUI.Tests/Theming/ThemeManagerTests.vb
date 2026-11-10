Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports MouseLock.CustomControls
Imports MouseLock.Theming

#End Region

Namespace Theming

    ''' <summary>
    ''' Unit tests for <see cref="ThemeManager"/> and <see cref="ThemedToolStripColorTable"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class ThemeManagerTests

        <TestMethod>
        Public Sub Apply_Dark_ThemesEveryNestedControl()
            Dim palette As ThemePalette = ThemePalette.FromTheme(AppTheme.Dark)

            Using form As New Form()
                Dim panel As New ThemedBorderPanel()
                Dim innerPanel As New Panel()
                Dim label As New Label()
                Dim radio As New RadioButton()
                Dim textBox As New TextBox()
                Dim combo As New ThemedComboBox()
                Dim numeric As New ThemedNumericUpDown()
                Dim toggle As New ThemeToggleSwitch()
                innerPanel.Controls.Add(label)
                panel.Controls.AddRange({innerPanel, radio, textBox})
                form.Controls.AddRange({panel, combo, numeric, toggle})

                ThemeManager.Apply(form, palette)

                Assert.AreEqual(palette.Background, form.BackColor)
                Assert.AreEqual(palette.Border, panel.BorderColor)
                Assert.AreEqual(palette.Background, innerPanel.BackColor)
                Assert.AreEqual(palette.Foreground, label.ForeColor, "Controls nested at any depth must be themed.")
                Assert.AreEqual(Color.Transparent, label.BackColor)
                Assert.AreEqual(Color.Transparent, radio.BackColor)
                Assert.AreEqual(palette.InputBackground, textBox.BackColor)
                Assert.AreEqual(BorderStyle.FixedSingle, textBox.BorderStyle)
                Assert.IsTrue(combo.UseThemedRendering)
                Assert.AreEqual(palette.InputBackground, combo.BackColor)
                Assert.IsTrue(numeric.UseThemedRendering)
                Assert.AreEqual(palette.Accent, toggle.AccentColor)
            End Using
        End Sub

        <TestMethod>
        Public Sub Apply_Light_RestoresNativeRendering()
            Using form As New Form()
                Dim textBox As New TextBox()
                Dim combo As New ThemedComboBox()
                Dim numeric As New ThemedNumericUpDown()
                Dim list As New ThemedListView()
                form.Controls.AddRange({textBox, combo, numeric, list})

                ThemeManager.Apply(form, ThemePalette.FromTheme(AppTheme.Dark))
                ThemeManager.Apply(form, ThemePalette.FromTheme(AppTheme.Light))

                Assert.AreEqual(SystemColors.Control, form.BackColor)
                Assert.AreEqual(SystemColors.Window, textBox.BackColor)
                Assert.AreEqual(BorderStyle.Fixed3D, textBox.BorderStyle)
                Assert.IsFalse(combo.UseThemedRendering)
                Assert.IsFalse(numeric.UseThemedRendering)
                Assert.IsFalse(list.UseThemedRendering)
            End Using
        End Sub

        <TestMethod>
        Public Sub Apply_KeepsTheTextBoxHeight()
            Using form As New Form()
                Dim textBox As New TextBox()
                form.Controls.Add(textBox)
                Dim height As Integer = textBox.Height

                ThemeManager.Apply(form, ThemePalette.FromTheme(AppTheme.Dark))
                Assert.AreEqual(height, textBox.Height)

                ThemeManager.Apply(form, ThemePalette.FromTheme(AppTheme.Light))
                Assert.AreEqual(height, textBox.Height)
            End Using
        End Sub

        <TestMethod>
        Public Sub Apply_ExcludedControls_KeepTheirColors_ButTheirChildrenAreThemed()
            Dim palette As ThemePalette = ThemePalette.FromTheme(AppTheme.Dark)

            Using form As New Form()
                Dim excluded As New Panel() With {.BackColor = Color.Green, .ForeColor = Color.Yellow}
                Dim child As New Label()
                excluded.Controls.Add(child)
                form.Controls.Add(excluded)

                ThemeManager.Apply(form, palette, excluded)

                Assert.AreEqual(Color.Green, excluded.BackColor)
                Assert.AreEqual(Color.Yellow, excluded.ForeColor)
                Assert.AreEqual(palette.Foreground, child.ForeColor)
            End Using
        End Sub

        <TestMethod>
        Public Sub Apply_CButton_UsesTheButtonGradient()
            Dim palette As ThemePalette = ThemePalette.FromTheme(AppTheme.Dark)

            Using form As New Form()
                Dim button As New CButtonLib.CButton()
                form.Controls.Add(button)

                ThemeManager.Apply(form, palette)

                Dim gradient As Color() = palette.GetButtonGradient()
                For i As Integer = 0 To Math.Min(gradient.Length, button.ColorFillBlend.iColor.Length) - 1
                    Assert.AreEqual(gradient(i), button.ColorFillBlend.iColor(i))
                Next
                Assert.AreEqual(palette.ButtonForeground, button.ForeColor)
            End Using
        End Sub

        <TestMethod>
        Public Sub Apply_DeepHierarchy_DoesNotOverflowTheStack()
            ' The traversal is iterative; a very deep tree must not throw StackOverflowException.
            Using form As New Form()
                Dim parent As Control = form
                For i As Integer = 1 To 300
                    Dim child As New Panel()
                    parent.Controls.Add(child)
                    parent = child
                Next

                ThemeManager.Apply(form, ThemePalette.FromTheme(AppTheme.Dark))

                Assert.AreEqual(ThemePalette.FromTheme(AppTheme.Dark).Background, parent.BackColor)
            End Using
        End Sub

        <TestMethod>
        Public Sub Apply_InvalidArguments_Throw()
            Using form As New Form()
                Assert.ThrowsException(Of ArgumentNullException)(Sub() ThemeManager.Apply(Nothing, ThemePalette.FromTheme(AppTheme.Dark)))
                Assert.ThrowsException(Of ArgumentNullException)(Sub() ThemeManager.Apply(form, Nothing))
            End Using
        End Sub

        <TestMethod>
        Public Sub ApplyToToolStrip_Dark_UsesThemedRenderer_Light_RestoresDefault()
            Using menu As New ContextMenuStrip()
                Dim item As ToolStripItem = menu.Items.Add("Lock")

                Dim dark As ThemePalette = ThemePalette.FromTheme(AppTheme.Dark)
                ThemeManager.ApplyToToolStrip(menu, dark)
                Assert.IsInstanceOfType(menu.Renderer, GetType(ToolStripProfessionalRenderer))
                Assert.IsInstanceOfType(DirectCast(menu.Renderer, ToolStripProfessionalRenderer).ColorTable, GetType(ThemedToolStripColorTable))
                Assert.AreEqual(dark.InputForeground, item.ForeColor)

                ThemeManager.ApplyToToolStrip(menu, ThemePalette.FromTheme(AppTheme.Light))
                Assert.AreEqual(ToolStripRenderMode.ManagerRenderMode, menu.RenderMode)
                Assert.AreEqual(SystemColors.MenuText.ToArgb(), item.ForeColor.ToArgb())
                Assert.AreEqual(SystemColors.Menu.ToArgb(), menu.BackColor.ToArgb())
            End Using
        End Sub

        <TestMethod>
        Public Sub ApplyToToolStrip_InvalidArguments_Throw()
            Using menu As New ContextMenuStrip()
                Assert.ThrowsException(Of ArgumentNullException)(Sub() ThemeManager.ApplyToToolStrip(Nothing, ThemePalette.FromTheme(AppTheme.Dark)))
                Assert.ThrowsException(Of ArgumentNullException)(Sub() ThemeManager.ApplyToToolStrip(menu, Nothing))
            End Using
        End Sub

        <TestMethod>
        Public Sub ThemedToolStripColorTable_UsesThePalette()
            Dim palette As ThemePalette = ThemePalette.FromTheme(AppTheme.Dark)
            Dim table As New ThemedToolStripColorTable(palette)

            Assert.AreEqual(palette.InputBackground, table.ToolStripDropDownBackground)
            Assert.AreEqual(palette.InputBackground, table.ImageMarginGradientBegin)
            Assert.AreEqual(palette.Border, table.MenuBorder)
            Assert.AreEqual(palette.Hover, table.MenuItemSelected)
            Assert.AreEqual(palette.Border, table.SeparatorDark)
            Assert.ThrowsException(Of ArgumentNullException)(Function() New ThemedToolStripColorTable(Nothing))
        End Sub

    End Class

End Namespace
