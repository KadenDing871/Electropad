Imports System.Text.Json

Namespace Electropad
    Public Interface IProjectCommand
        ReadOnly Property Description As String
        Sub Execute(project As CircuitProject)
        Sub Undo(project As CircuitProject)
    End Interface

    Public Class ProjectHistory
        Private ReadOnly undoStack As New Stack(Of IProjectCommand)
        Private ReadOnly redoStack As New Stack(Of IProjectCommand)
        Public ReadOnly Property CanUndo As Boolean
            Get
                Return undoStack.Count > 0
            End Get
        End Property
        Public ReadOnly Property CanRedo As Boolean
            Get
                Return redoStack.Count > 0
            End Get
        End Property
        Public Sub Execute(project As CircuitProject, command As IProjectCommand)
            command.Execute(project)
            undoStack.Push(command)
            redoStack.Clear()
        End Sub
        Public Sub Undo(project As CircuitProject)
            If Not CanUndo Then Return
            Dim command = undoStack.Pop()
            command.Undo(project)
            redoStack.Push(command)
        End Sub
        Public Sub Redo(project As CircuitProject)
            If Not CanRedo Then Return
            Dim command = redoStack.Pop()
            command.Execute(project)
            undoStack.Push(command)
        End Sub
        Public Sub Clear()
            undoStack.Clear()
            redoStack.Clear()
        End Sub
        Public Function UndoDescriptions() As IEnumerable(Of String)
            Return undoStack.Select(Function(item) item.Description)
        End Function
    End Class

    Public Class ProjectSnapshot
        Public Property Material As String
        Public Property WireLength As Double
        Public Property WireRadius As Double
        Public Property Components As List(Of CircuitComponent)
        Public Property Wires As List(Of CircuitWire)
        Public Shared Function Capture(project As CircuitProject) As ProjectSnapshot
            Dim options = New JsonSerializerOptions()
            Dim copy = JsonSerializer.Deserialize(Of CircuitProject)(JsonSerializer.Serialize(project, options), options)
            Return New ProjectSnapshot With {.Material = copy.Material, .WireLength = copy.WireLength, .WireRadius = copy.WireRadius, .Components = copy.Components, .Wires = copy.Wires}
        End Function
        Public Sub Restore(project As CircuitProject)
            project.Material = Material
            project.WireLength = WireLength
            project.WireRadius = WireRadius
            project.Components.Clear()
            project.Components.AddRange(Components.Select(Function(item) New CircuitComponent With {.Kind = item.Kind, .Reference = item.Reference, .Value = item.Value, .Color = item.Color, .X = item.X, .Y = item.Y, .Z = item.Z, .RotationX = item.RotationX, .RotationY = item.RotationY, .RotationZ = item.RotationZ, .LengthMm = item.LengthMm, .BodyDiameterMm = item.BodyDiameterMm, .HeightMm = item.HeightMm, .Hidden = item.Hidden, .Symbol = item.Symbol}))
            project.Wires.Clear()
            project.Wires.AddRange(Wires.Select(Function(item) New CircuitWire With {.Reference = item.Reference, .StartReference = item.StartReference, .EndReference = item.EndReference, .LengthMm = item.LengthMm, .RadiusMm = item.RadiusMm, .Material = item.Material, .Hidden = item.Hidden}))
        End Sub
    End Class

    Public Class SnapshotCommand
        Implements IProjectCommand
        Private ReadOnly beforeState As ProjectSnapshot
        Private ReadOnly afterState As ProjectSnapshot
        Private ReadOnly title As String
        Public Sub New(project As CircuitProject, description As String, action As Action(Of CircuitProject))
            title = description
            beforeState = ProjectSnapshot.Capture(project)
            Dim copy = ProjectSnapshot.Capture(project)
            Dim temporary As New CircuitProject()
            copy.Restore(temporary)
            action(temporary)
            afterState = ProjectSnapshot.Capture(temporary)
        End Sub
        Public ReadOnly Property Description As String Implements IProjectCommand.Description
            Get
                Return title
            End Get
        End Property
        Public Sub Execute(project As CircuitProject) Implements IProjectCommand.Execute
            afterState.Restore(project)
        End Sub
        Public Sub Undo(project As CircuitProject) Implements IProjectCommand.Undo
            beforeState.Restore(project)
        End Sub
    End Class
End Namespace
