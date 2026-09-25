Imports System.Text.Json

Namespace Electropad
    Public Class ProjectSettings
        Public Property GridSpacing As Double = 28
        Public Property SnapSpacing As Double = 14
        Public Property WireTolerance As Double = 8
        Public Property DefaultWireLength As Double = 42
        Public Property DefaultWireRadius As Double = 2
        Public Property RenderQuality As Integer = 2
        Public Property ShowLabels As Boolean = True
        Public Property ShowDimensions As Boolean = True
        Public Property ShowGrid As Boolean = True
        Public Property AutoValidate As Boolean = True
        Public Property CameraZoom As Double = 1
        Public Property CameraYaw As Double = -0.45
        Public Property CameraPitch As Double = 0.55
        Public Function Clone() As ProjectSettings
            Return JsonSerializer.Deserialize(Of ProjectSettings)(JsonSerializer.Serialize(Me))
        End Function
        Public Sub Reset()
            GridSpacing = 28
            SnapSpacing = 14
            WireTolerance = 8
            DefaultWireLength = 42
            DefaultWireRadius = 2
            RenderQuality = 2
            ShowLabels = True
            ShowDimensions = True
            ShowGrid = True
            AutoValidate = True
            CameraZoom = 1
            CameraYaw = -0.45
            CameraPitch = 0.55
        End Sub
    End Class

    Public Class SettingsStore
        Private ReadOnly settingsPath As String
        Public Sub New(folder As String)
            settingsPath = IO.Path.Combine(folder, "electropad.settings.json")
        End Sub
        Public Function Load() As ProjectSettings
            Try
                If IO.File.Exists(settingsPath) Then Return JsonSerializer.Deserialize(Of ProjectSettings)(IO.File.ReadAllText(settingsPath))
            Catch
            End Try
            Return New ProjectSettings()
        End Function
        Public Function Save(settings As ProjectSettings) As Boolean
            Try
                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(settingsPath))
                IO.File.WriteAllText(settingsPath, JsonSerializer.Serialize(settings, New JsonSerializerOptions With {.WriteIndented = True}))
                Return True
            Catch
                Return False
            End Try
        End Function
        Public ReadOnly Property Path As String
            Get
                Return settingsPath
            End Get
        End Property
    End Class
End Namespace
