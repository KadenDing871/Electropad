Imports System.Drawing

Namespace Electropad
    Public Class ThemePalette
        Public Property CanvasBackground As Color
        Public Property PanelBackground As Color
        Public Property HeaderBackground As Color
        Public Property PrimaryText As Color
        Public Property MutedText As Color
        Public Property Accent As Color
        Public Property Warning As Color
        Public Property Wire As Color
        Public Property Selection As Color
        Public Shared Function Studio() As ThemePalette
            Return New ThemePalette With {.CanvasBackground = Color.FromArgb(15, 25, 30), .PanelBackground = Color.FromArgb(20, 33, 39), .HeaderBackground = Color.FromArgb(21, 33, 39), .PrimaryText = Color.FromArgb(242, 245, 247), .MutedText = Color.FromArgb(145, 160, 170), .Accent = Color.FromArgb(85, 224, 208), .Warning = Color.FromArgb(255, 184, 74), .Wire = Color.FromArgb(85, 224, 208), .Selection = Color.FromArgb(255, 184, 74)}
        End Function
        Public Shared Function Graphite() As ThemePalette
            Return New ThemePalette With {.CanvasBackground = Color.FromArgb(28, 30, 34), .PanelBackground = Color.FromArgb(38, 41, 47), .HeaderBackground = Color.FromArgb(45, 48, 55), .PrimaryText = Color.FromArgb(240, 242, 244), .MutedText = Color.FromArgb(160, 166, 174), .Accent = Color.FromArgb(112, 180, 255), .Warning = Color.FromArgb(255, 170, 80), .Wire = Color.FromArgb(112, 180, 255), .Selection = Color.FromArgb(255, 190, 80)}
        End Function
        Public Shared Function HighContrast() As ThemePalette
            Return New ThemePalette With {.CanvasBackground = Color.Black, .PanelBackground = Color.FromArgb(20, 20, 20), .HeaderBackground = Color.FromArgb(35, 35, 35), .PrimaryText = Color.White, .MutedText = Color.LightGray, .Accent = Color.Lime, .Warning = Color.Yellow, .Wire = Color.Cyan, .Selection = Color.Yellow}
        End Function
        Public Function ComponentColor(kind As String) As Color
            Select Case kind
                Case "Resistor" : Return Color.FromArgb(217, 124, 99)
                Case "LED" : Return Color.FromArgb(255, 184, 74)
                Case "Battery" : Return Color.FromArgb(108, 181, 213)
                Case "Ground" : Return Color.FromArgb(127, 204, 157)
                Case Else : Return Accent
            End Select
        End Function
    End Class

    Public Class ThemeManager
        Private ReadOnly themes As New Dictionary(Of String, ThemePalette)(StringComparer.OrdinalIgnoreCase)
        Public Property CurrentName As String = "Studio"
        Public Sub New()
            themes("Studio") = ThemePalette.Studio()
            themes("Graphite") = ThemePalette.Graphite()
            themes("High Contrast") = ThemePalette.HighContrast()
        End Sub
        Public ReadOnly Property Current As ThemePalette
            Get
                Return themes(CurrentName)
            End Get
        End Property
        Public Function Names() As IEnumerable(Of String)
            Return themes.Keys.OrderBy(Function(item) item)
        End Function
        Public Function SetTheme(name As String) As Boolean
            If Not themes.ContainsKey(name) Then Return False
            CurrentName = name
            Return True
        End Function
        Public Function AddTheme(name As String, palette As ThemePalette) As Boolean
            If String.IsNullOrWhiteSpace(name) OrElse palette Is Nothing OrElse themes.ContainsKey(name) Then Return False
            themes(name) = palette
            Return True
        End Function
    End Class
End Namespace
