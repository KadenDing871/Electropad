Imports System.Linq

Namespace Electropad
    Public Enum ConstraintKind
        Horizontal
        Vertical
        Coincident
        Distance
        EqualLength
        Fixed
        AlignedLeft
        AlignedTop
    End Enum

    Public Class CircuitConstraint
        Public Property Name As String
        Public Property Kind As ConstraintKind
        Public Property FirstReference As String
        Public Property SecondReference As String
        Public Property TargetValue As Double
        Public Property IsSuppressed As Boolean
        Public ReadOnly Property Description As String
            Get
                Return Kind.ToString() & " " & FirstReference & If(String.IsNullOrWhiteSpace(SecondReference), "", " / " & SecondReference)
            End Get
        End Property
    End Class

    Public Class ConstraintResult
        Public Property Constraint As CircuitConstraint
        Public Property Satisfied As Boolean
        Public Property ErrorValue As Double
        Public Property Message As String
    End Class

    Public Class ConstraintEngine
        Public Function Check(project As CircuitProject, constraint As CircuitConstraint) As ConstraintResult
            Dim result As New ConstraintResult With {.Constraint = constraint, .Satisfied = True}
            If constraint Is Nothing OrElse constraint.IsSuppressed Then Return result
            Dim first = project.Components.FirstOrDefault(Function(item) item.Reference = constraint.FirstReference)
            Dim second = project.Components.FirstOrDefault(Function(item) item.Reference = constraint.SecondReference)
            If first Is Nothing Then Return Failure(result, "First reference is missing.")
            Select Case constraint.Kind
                Case ConstraintKind.Fixed
                    result.ErrorValue = Math.Abs(first.X - constraint.TargetValue)
                    result.Satisfied = result.ErrorValue < 0.5
                Case ConstraintKind.Horizontal
                    If second Is Nothing Then Return Failure(result, "Second reference is missing.")
                    result.ErrorValue = Math.Abs(first.Y - second.Y)
                    result.Satisfied = result.ErrorValue < 0.5
                Case ConstraintKind.Vertical
                    If second Is Nothing Then Return Failure(result, "Second reference is missing.")
                    result.ErrorValue = Math.Abs(first.X - second.X)
                    result.Satisfied = result.ErrorValue < 0.5
                Case ConstraintKind.Distance
                    If second Is Nothing Then Return Failure(result, "Second reference is missing.")
                    result.ErrorValue = Math.Abs(CircuitGeometry.ComponentCenter(first).DistanceTo(CircuitGeometry.ComponentCenter(second)) - constraint.TargetValue)
                    result.Satisfied = result.ErrorValue < 0.5
                Case ConstraintKind.AlignedLeft
                    If second Is Nothing Then Return Failure(result, "Second reference is missing.")
                    result.ErrorValue = Math.Abs(first.X - second.X)
                    result.Satisfied = result.ErrorValue < 0.5
                Case ConstraintKind.AlignedTop
                    If second Is Nothing Then Return Failure(result, "Second reference is missing.")
                    result.ErrorValue = Math.Abs(first.Y - second.Y)
                    result.Satisfied = result.ErrorValue < 0.5
                Case Else
                    result.Satisfied = True
            End Select
            result.Message = If(result.Satisfied, "Satisfied", "Off by " & result.ErrorValue.ToString("0.###") & " mm")
            Return result
        End Function
        Private Function Failure(result As ConstraintResult, message As String) As ConstraintResult
            result.Satisfied = False
            result.Message = message
            Return result
        End Function
        Public Function CheckAll(project As CircuitProject, constraints As IEnumerable(Of CircuitConstraint)) As List(Of ConstraintResult)
            Return constraints.Where(Function(item) item IsNot Nothing).Select(Function(item) Check(project, item)).ToList()
        End Function
        Public Function Solve(project As CircuitProject, constraint As CircuitConstraint) As Boolean
            If constraint Is Nothing OrElse constraint.IsSuppressed Then Return False
            Dim first = project.Components.FirstOrDefault(Function(item) item.Reference = constraint.FirstReference)
            Dim second = project.Components.FirstOrDefault(Function(item) item.Reference = constraint.SecondReference)
            If first Is Nothing Then Return False
            Select Case constraint.Kind
                Case ConstraintKind.Fixed
                    first.X = CInt(Math.Round(constraint.TargetValue))
                Case ConstraintKind.Horizontal
                    If second Is Nothing Then Return False
                    first.Y = second.Y
                Case ConstraintKind.Vertical
                    If second Is Nothing Then Return False
                    first.X = second.X
                Case ConstraintKind.AlignedLeft
                    If second Is Nothing Then Return False
                    first.X = second.X
                Case ConstraintKind.AlignedTop
                    If second Is Nothing Then Return False
                    first.Y = second.Y
                Case Else
                    Return False
            End Select
            Return True
        End Function
        Public Function SolveAll(project As CircuitProject, constraints As IEnumerable(Of CircuitConstraint), passes As Integer) As Integer
            Dim solved = 0
            For pass = 1 To Math.Max(1, passes)
                For Each constraint In constraints
                    If Solve(project, constraint) Then solved += 1
                Next
            Next
            Return solved
        End Function
    End Class
End Namespace
