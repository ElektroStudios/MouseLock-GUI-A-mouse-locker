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
    ''' A <see cref="ListView"/> whose column headers and native parts (scroll bars, header background)
    ''' can be painted with custom colors, as required by a dark theme.
    ''' </summary>
    ''' <remarks>
    ''' Items are drawn by the system (<see cref="DrawListViewItemEventArgs.DrawDefault"/>), so check boxes,
    ''' selection and hot tracking keep their native behavior and use <see cref="Control.BackColor"/> and <see cref="Control.ForeColor"/>.
    ''' </remarks>
    Friend NotInheritable Class ThemedListView : Inherits ListView

#Region " Fields "

        ''' <summary>Backing field of <see cref="UseThemedRendering"/>.</summary>
        Private useThemedRenderingValue As Boolean

        ''' <summary>Backing field of <see cref="HeaderBackColor"/>.</summary>
        Private headerBackColorValue As Color = SystemColors.Control

        ''' <summary>Backing field of <see cref="HeaderForeColor"/>.</summary>
        Private headerForeColorValue As Color = SystemColors.ControlText

        ''' <summary>Backing field of <see cref="HeaderBorderColor"/>.</summary>
        Private headerBorderColorValue As Color = SystemColors.ControlDark

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets a value indicating whether the column headers are painted with the custom colors
        ''' and the native dark visual style is used for the scroll bars.
        ''' </summary>
        <Category("Appearance")>
        <DefaultValue(False)>
        <Description("Indicates whether the column headers are painted with the custom colors and the native dark style is used.")>
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

        ''' <summary>Gets or sets the background color of the column headers (themed rendering).</summary>
        <Category("Appearance")>
        Public Property HeaderBackColor As Color
            Get
                Return Me.headerBackColorValue
            End Get
            Set(value As Color)
                Me.headerBackColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>Gets or sets the text color of the column headers (themed rendering).</summary>
        <Category("Appearance")>
        Public Property HeaderForeColor As Color
            Get
                Return Me.headerForeColorValue
            End Get
            Set(value As Color)
                Me.headerForeColorValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>Gets or sets the separator color of the column headers (themed rendering).</summary>
        <Category("Appearance")>
        Public Property HeaderBorderColor As Color
            Get
                Return Me.headerBorderColorValue
            End Get
            Set(value As Color)
                Me.headerBorderColorValue = value
                Me.Invalidate()
            End Set
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedListView"/> class.
        ''' </summary>
        Public Sub New()
            Me.OwnerDraw = True
            Me.DoubleBuffered = True
        End Sub

#End Region

#Region " Protected Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnHandleCreated(e As EventArgs)
            MyBase.OnHandleCreated(e)
            Me.ApplyNativeTheme()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnDrawColumnHeader(e As DrawListViewColumnHeaderEventArgs)

            If Not Me.useThemedRenderingValue Then
                e.DrawDefault = True
                MyBase.OnDrawColumnHeader(e)
                Return
            End If

            Using fill As New SolidBrush(Me.headerBackColorValue)
                e.Graphics.FillRectangle(fill, e.Bounds)
            End Using

            Dim textBounds As New Rectangle(e.Bounds.Left + 6, e.Bounds.Top, Math.Max(0, e.Bounds.Width - 10), e.Bounds.Height)
            TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, textBounds, Me.headerForeColorValue,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or
                                  TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)

            Using pen As New Pen(Me.headerBorderColorValue)
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top + 3, e.Bounds.Right - 1, e.Bounds.Bottom - 4)
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
            End Using
            MyBase.OnDrawColumnHeader(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnDrawItem(e As DrawListViewItemEventArgs)
            e.DrawDefault = True
            MyBase.OnDrawItem(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnDrawSubItem(e As DrawListViewSubItemEventArgs)
            e.DrawDefault = True
            MyBase.OnDrawSubItem(e)
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Applies the native visual style matching <see cref="UseThemedRendering"/> to the list and its header.
        ''' </summary>
        Private Sub ApplyNativeTheme()

            WindowTheme.Apply(Me.Handle, If(Me.useThemedRenderingValue, WindowTheme.DarkExplorer, Nothing))

            Dim header As IntPtr = NativeMethods.SendMessage(Me.Handle, NativeMethods.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero)
            If header <> IntPtr.Zero Then
                WindowTheme.Apply(header, If(Me.useThemedRenderingValue, WindowTheme.DarkItemsView, Nothing))
            End If
        End Sub

#End Region

    End Class

End Namespace
