Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports MouseLock.CustomControls

#End Region

Namespace CustomControls

    ''' <summary>
    ''' Unit tests for the custom controls of the <c>MouseLock.CustomControls</c> namespace.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class CustomControlsTests

#Region " ThemeToggleSwitch "

        <TestMethod>
        Public Sub ToggleSwitch_DefaultsToOff_AndIsFocusable()
            Using toggle As New ThemeToggleSwitch()
                Assert.IsFalse(toggle.Checked)
                Assert.IsTrue(toggle.TabStop)
                Assert.AreEqual(AccessibleRole.CheckButton, toggle.AccessibleRole)
            End Using
        End Sub

        <TestMethod>
        Public Sub ToggleSwitch_CheckedChanged_IsRaisedOnlyOnActualChanges()
            Using toggle As New ThemeToggleSwitch()
                Dim count As Integer = 0
                AddHandler toggle.CheckedChanged, Sub(sender As Object, e As EventArgs) count += 1

                toggle.Checked = False ' Same value: no event.
                toggle.Checked = True
                toggle.Checked = True  ' Same value: no event.
                toggle.Toggle()
                toggle.Toggle()

                Assert.AreEqual(3, count)
                Assert.IsTrue(toggle.Checked)
            End Using
        End Sub

        <TestMethod>
        Public Sub ToggleSwitch_AccessibilityState_ReflectsChecked()
            Using toggle As New ThemeToggleSwitch() With {.Text = "Dark theme"}
                Dim accessible As AccessibleObject = toggle.AccessibilityObject

                Assert.AreEqual(AccessibleRole.CheckButton, accessible.Role)
                Assert.AreEqual(AccessibleStates.None, accessible.State And AccessibleStates.Checked)

                accessible.DoDefaultAction()

                Assert.IsTrue(toggle.Checked)
                Assert.AreEqual(AccessibleStates.Checked, accessible.State And AccessibleStates.Checked)
            End Using
        End Sub

        <TestMethod>
        Public Sub ToggleSwitch_PaintsInBothStates_WithoutErrors()
            Using toggle As New ThemeToggleSwitch() With {.Text = "Dark theme", .Size = New Size(125, 20)}
                For Each state As Boolean In {False, True}
                    toggle.Checked = state
                    Using bmp As New Bitmap(toggle.Width, toggle.Height)
                        toggle.DrawToBitmap(bmp, New Rectangle(Point.Empty, toggle.Size))
                    End Using
                Next
            End Using
        End Sub

#End Region

#Region " ThemedBorderPanel "

        <TestMethod>
        Public Sub BorderPanel_BorderStyle_IsAlwaysNone()
            Using panel As New ThemedBorderPanel()
                Assert.AreEqual(BorderStyle.None, panel.BorderStyle)
                panel.BorderStyle = BorderStyle.None
                Assert.ThrowsException(Of NotSupportedException)(Sub() panel.BorderStyle = BorderStyle.FixedSingle)
            End Using
        End Sub

        <TestMethod>
        Public Sub BorderPanel_BorderColor_IsStored()
            Using panel As New ThemedBorderPanel() With {.BorderColor = Color.Red}
                Assert.AreEqual(Color.Red, panel.BorderColor)
            End Using
        End Sub

#End Region

#Region " ThemedComboBox "

        <TestMethod>
        Public Sub ComboBox_IsAnOwnerDrawnDropDownList()
            Using combo As New ThemedComboBox()
                Assert.AreEqual(DrawMode.OwnerDrawFixed, combo.DrawMode)
                Assert.AreEqual(ComboBoxStyle.DropDownList, combo.DropDownStyle)
                Assert.IsFalse(combo.UseThemedRendering)
            End Using
        End Sub

        <TestMethod>
        Public Sub ComboBox_UnsupportedModes_Throw()
            Using combo As New ThemedComboBox()
                combo.DropDownStyle = ComboBoxStyle.DropDownList
                combo.DrawMode = DrawMode.OwnerDrawFixed
                Assert.ThrowsException(Of NotSupportedException)(Sub() combo.DropDownStyle = ComboBoxStyle.DropDown)
                Assert.ThrowsException(Of NotSupportedException)(Sub() combo.DrawMode = DrawMode.Normal)
            End Using
        End Sub

        <TestMethod>
        Public Sub ComboBox_ThemedRendering_CanBeToggledWithAHandle()
            Using form As New Form()
                Using combo As New ThemedComboBox()
                    combo.Items.AddRange({"NONE", "ALT"})
                    combo.SelectedIndex = 1
                    form.Controls.Add(combo)
                    Dim handle As IntPtr = combo.Handle

                    combo.UseThemedRendering = True
                    combo.UseThemedRendering = False
                    combo.UseThemedRendering = True

                    Assert.AreEqual(1, combo.SelectedIndex)
                    Assert.AreEqual(handle, combo.Handle, "Switching the theme must not recreate the handle.")
                End Using
            End Using
        End Sub

#End Region

#Region " ThemedNumericUpDown "

        <TestMethod>
        Public Sub NumericUpDown_ThemedProperties_AreStored()
            Using numeric As New ThemedNumericUpDown()
                numeric.UseThemedRendering = True
                numeric.BorderColor = Color.Red
                numeric.FocusBorderColor = Color.Lime
                numeric.ButtonBackColor = Color.Blue
                numeric.ArrowColor = Color.Yellow
                numeric.DisabledForeColor = Color.Gray

                Assert.IsTrue(numeric.UseThemedRendering)
                Assert.AreEqual(Color.Red, numeric.BorderColor)
                Assert.AreEqual(Color.Lime, numeric.FocusBorderColor)
                Assert.AreEqual(Color.Blue, numeric.ButtonBackColor)
                Assert.AreEqual(Color.Yellow, numeric.ArrowColor)
                Assert.AreEqual(Color.Gray, numeric.DisabledForeColor)
            End Using
        End Sub

        <TestMethod>
        Public Sub NumericUpDown_PaintsThemed_WithoutErrors()
            Using numeric As New ThemedNumericUpDown() With {.UseThemedRendering = True, .Size = New Size(56, 20)}
                Using bmp As New Bitmap(numeric.Width, numeric.Height)
                    numeric.DrawToBitmap(bmp, New Rectangle(Point.Empty, numeric.Size))
                End Using
            End Using
        End Sub

#End Region

#Region " ThemedListView "

        <TestMethod>
        Public Sub ListView_IsOwnerDrawn()
            Using list As New ThemedListView()
                Assert.IsTrue(list.OwnerDraw)
                Assert.IsFalse(list.UseThemedRendering)
            End Using
        End Sub

        <TestMethod>
        Public Sub ListView_ThemedRendering_CanBeToggledWithAHandle()
            Using form As New Form()
                Using list As New ThemedListView() With {.View = View.Details, .CheckBoxes = True}
                    list.Columns.Add("Running applications", 160)
                    list.Items.Add("notepad.exe").Checked = True
                    form.Controls.Add(list)
                    Dim handle As IntPtr = list.Handle

                    list.UseThemedRendering = True
                    list.UseThemedRendering = False

                    Assert.AreEqual(1, list.CheckedItems.Count)
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                End Using
            End Using
        End Sub

#End Region

#Region " GlyphPainter "

        <TestMethod>
        Public Sub GlyphPainter_DrawChevron_DrawsInsideTheBounds()
            Using bmp As New Bitmap(20, 20)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.Clear(Color.Black)
                    GlyphPainter.DrawChevron(g, New Rectangle(5, 5, 10, 10), Color.White, pointsUp:=True)
                End Using

                Dim lit As Integer = 0
                For x As Integer = 0 To 19
                    For y As Integer = 0 To 19
                        If bmp.GetPixel(x, y).R > 0 Then
                            lit += 1
                            Assert.IsTrue(x >= 3 AndAlso x <= 16 AndAlso y >= 3 AndAlso y <= 16, $"Pixel outside the bounds: {x},{y}")
                        End If
                    Next
                Next
                Assert.IsTrue(lit > 0)
            End Using
        End Sub

        <TestMethod>
        Public Sub GlyphPainter_DrawChevron_NullGraphics_Throws()
            Assert.ThrowsException(Of ArgumentNullException)(Sub() GlyphPainter.DrawChevron(Nothing, Rectangle.Empty, Color.Black, pointsUp:=False))
        End Sub

#End Region

    End Class

End Namespace
