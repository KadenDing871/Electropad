Imports System.Linq

Namespace Electropad
    Public Class CommandResult
        Public Property Success As Boolean
        Public Property Message As String
        Public Property Changed As Boolean
        Public Property Data As New Dictionary(Of String, String)
        Public Shared Function Ok(message As String) As CommandResult
            Return New CommandResult With {.Success = True, .Message = message}
        End Function
        Public Shared Function Fail(message As String) As CommandResult
            Return New CommandResult With {.Success = False, .Message = message}
        End Function
    End Class

    Public Class CircuitCommandRouter
        Private ReadOnly handlers As New Dictionary(Of String, Func(Of CircuitProject, String(), CommandResult))(StringComparer.OrdinalIgnoreCase)
        Public Sub New()
            Register("count", Function(project, args) CommandResult.Ok(project.Components.Count & " components, " & project.Wires.Count & " wires"))
            Register("material", AddressOf MaterialCommand)
            Register("wire", AddressOf WireCommand)
            Register("hide", AddressOf HideCommand)
            Register("show", AddressOf ShowCommand)
            Register("select", AddressOf SelectCommand)
            Register("stats", AddressOf StatsCommand)
        End Sub
        Public Sub Register(name As String, handler As Func(Of CircuitProject, String(), CommandResult))
            If String.IsNullOrWhiteSpace(name) OrElse handler Is Nothing Then Return
            handlers(name.Trim()) = handler
        End Sub
        Public Function Execute(project As CircuitProject, commandLine As String) As CommandResult
            If project Is Nothing Then Return CommandResult.Fail("No project is loaded.")
            Dim tokens = Tokenize(commandLine)
            If tokens.Count = 0 Then Return CommandResult.Fail("Empty command.")
            Dim handler As Func(Of CircuitProject, String(), CommandResult) = Nothing
            If Not handlers.TryGetValue(tokens(0), handler) Then Return CommandResult.Fail("Unknown command: " & tokens(0))
            Return handler(project, tokens.Skip(1).ToArray())
        End Function
        Private Function Tokenize(commandLine As String) As List(Of String)
            Dim result As New List(Of String)()
            If String.IsNullOrWhiteSpace(commandLine) Then Return result
            Dim current As String = ""
            Dim quoted = False
            For Each character In commandLine.Trim()
                If character = ChrW(34) Then
                    quoted = Not quoted
                ElseIf Char.IsWhiteSpace(character) AndAlso Not quoted Then
                    If current.Length > 0 Then result.Add(current) : current = ""
                Else
                    current &= character
                End If
            Next
            If current.Length > 0 Then result.Add(current)
            Return result
        End Function
        Private Function MaterialCommand(project As CircuitProject, args As String()) As CommandResult
            If args.Length = 0 Then Return CommandResult.Ok(project.Material)
            Dim material = MaterialCatalog.Find(String.Join(" ", args))
            project.Material = material.Name
            Return New CommandResult With {.Success = True, .Changed = True, .Message = "Material set to " & material.Name}
        End Function
        Private Function WireCommand(project As CircuitProject, args As String()) As CommandResult
            If args.Length < 1 Then Return CommandResult.Fail("Usage: wire <reference>")
            Dim wire = project.Wires.FirstOrDefault(Function(item) item.Reference.Equals(args(0), StringComparison.OrdinalIgnoreCase))
            If wire Is Nothing Then Return CommandResult.Fail("Wire not found: " & args(0))
            Return CommandResult.Ok(wire.Reference & " " & wire.LengthMm & " mm " & wire.RadiusMm & " mm radius " & wire.Material)
        End Function
        Private Function HideCommand(project As CircuitProject, args As String()) As CommandResult
            If args.Length < 1 Then Return CommandResult.Fail("Usage: hide <reference>")
            Dim component = project.Components.FirstOrDefault(Function(item) item.Reference.Equals(args(0), StringComparison.OrdinalIgnoreCase))
            If component Is Nothing Then Return CommandResult.Fail("Component not found: " & args(0))
            component.Hidden = True
            Return New CommandResult With {.Success = True, .Changed = True, .Message = component.Reference & " hidden"}
        End Function
        Private Function ShowCommand(project As CircuitProject, args As String()) As CommandResult
            If args.Length < 1 Then Return CommandResult.Fail("Usage: show <reference>")
            Dim component = project.Components.FirstOrDefault(Function(item) item.Reference.Equals(args(0), StringComparison.OrdinalIgnoreCase))
            If component Is Nothing Then Return CommandResult.Fail("Component not found: " & args(0))
            component.Hidden = False
            Return New CommandResult With {.Success = True, .Changed = True, .Message = component.Reference & " restored"}
        End Function
        Private Function SelectCommand(project As CircuitProject, args As String()) As CommandResult
            If args.Length < 1 Then Return CommandResult.Fail("Usage: select <reference>")
            If project.Components.Any(Function(item) item.Reference.Equals(args(0), StringComparison.OrdinalIgnoreCase)) OrElse project.Wires.Any(Function(item) item.Reference.Equals(args(0), StringComparison.OrdinalIgnoreCase)) Then Return CommandResult.Ok("Selected " & args(0))
            Return CommandResult.Fail("Object not found: " & args(0))
        End Function
        Private Function StatsCommand(project As CircuitProject, args As String()) As CommandResult
            Dim visibleComponents = project.Components.Where(Function(item) Not item.Hidden).Count()
            Dim visibleWires = project.Wires.Where(Function(item) Not item.Hidden).Count()
            Return CommandResult.Ok("Visible components: " & visibleComponents & ", visible wires: " & visibleWires & ", material: " & project.Material)
        End Function
    End Class
End Namespace
