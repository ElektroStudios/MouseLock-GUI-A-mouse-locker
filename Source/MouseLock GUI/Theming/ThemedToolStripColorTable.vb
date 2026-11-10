Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Windows.Forms

#End Region

Namespace Theming

    ''' <summary>
    ''' A <see cref="ProfessionalColorTable"/> that paints menus (such as the tray icon context menu)
    ''' with the colors of a <see cref="ThemePalette"/>.
    ''' </summary>
    Friend NotInheritable Class ThemedToolStripColorTable : Inherits ProfessionalColorTable

#Region " Fields "

        ''' <summary>
        ''' The palette that provides the colors.
        ''' </summary>
        Private ReadOnly palette As ThemePalette

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedToolStripColorTable"/> class.
        ''' </summary>
        ''' <param name="palette">The palette that provides the colors.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> is <see langword="Nothing"/>.</exception>
        Friend Sub New(palette As ThemePalette)

            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If
            Me.palette = palette
            Me.UseSystemColors = False
        End Sub

#End Region

#Region " Properties "

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
            Get
                Return Me.palette.InputBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
            Get
                Return Me.palette.InputBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
            Get
                Return Me.palette.InputBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
            Get
                Return Me.palette.InputBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuBorder As Color
            Get
                Return Me.palette.Border
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemBorder As Color
            Get
                Return Me.palette.Hover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemSelected As Color
            Get
                Return Me.palette.Hover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemSelectedGradientBegin As Color
            Get
                Return Me.palette.Hover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemSelectedGradientEnd As Color
            Get
                Return Me.palette.Hover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemPressedGradientBegin As Color
            Get
                Return Me.palette.Hover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemPressedGradientEnd As Color
            Get
                Return Me.palette.Hover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property SeparatorDark As Color
            Get
                Return Me.palette.Border
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property SeparatorLight As Color
            Get
                Return Me.palette.Border
            End Get
        End Property

#End Region

    End Class

End Namespace
