Imports System.Linq

Namespace Electropad
    Public Enum RoutingMode
        Direct
        Orthogonal
        Manhattan
        Grid
    End Enum

    Public Class RouteSegment
        Public Property StartPoint As ModelPoint
        Public Property EndPoint As ModelPoint
        Public ReadOnly Property Length As Double
            Get
                Return StartPoint.DistanceTo(EndPoint)
            End Get
        End Property
    End Class

    Public Class WireRoute
        Public Property WireReference As String
        Public Property Mode As RoutingMode
        Public Property Segments As New List(Of RouteSegment)
        Public ReadOnly Property Length As Double
            Get
                Return Segments.Sum(Function(item) item.Length)
            End Get
        End Property
        Public Function Points() As IEnumerable(Of ModelPoint)
            If Segments.Count = 0 Then Return Enumerable.Empty(Of ModelPoint)()
            Return Segments.Select(Function(item) item.StartPoint).Concat({Segments.Last().EndPoint})
        End Function
    End Class

    Public Class CircuitRouter
        Public Function Route(project As CircuitProject, wire As CircuitWire, mode As RoutingMode, gridSize As Double) As WireRoute
            Dim result As New WireRoute With {.WireReference = wire.Reference, .Mode = mode}
            Dim first = project.Components.FirstOrDefault(Function(item) item.Reference = wire.StartReference)
            Dim second = project.Components.FirstOrDefault(Function(item) item.Reference = wire.EndReference)
            If first Is Nothing OrElse second Is Nothing Then Return result
            Dim startPoint = CircuitGeometry.WireStart(first)
            Dim endPoint = CircuitGeometry.WireEnd(second)
            If mode = RoutingMode.Direct Then
                result.Segments.Add(New RouteSegment With {.StartPoint = startPoint, .EndPoint = endPoint})
            Else
                Dim bendX = If(mode = RoutingMode.Grid, Math.Round((startPoint.X + endPoint.X) / 2 / gridSize) * gridSize, (startPoint.X + endPoint.X) / 2)
                Dim bend = New ModelPoint(bendX, startPoint.Y)
                Dim bendTwo = New ModelPoint(bendX, endPoint.Y)
                If mode = RoutingMode.Orthogonal OrElse mode = RoutingMode.Manhattan OrElse mode = RoutingMode.Grid Then
                    result.Segments.Add(New RouteSegment With {.StartPoint = startPoint, .EndPoint = bend})
                    result.Segments.Add(New RouteSegment With {.StartPoint = bend, .EndPoint = bendTwo})
                    result.Segments.Add(New RouteSegment With {.StartPoint = bendTwo, .EndPoint = endPoint})
                End If
            End If
            Return result
        End Function
        Public Function RouteAll(project As CircuitProject, mode As RoutingMode, gridSize As Double) As List(Of WireRoute)
            Return project.Wires.Where(Function(item) Not item.Hidden).Select(Function(item) Route(project, item, mode, gridSize)).ToList()
        End Function
        Public Function TotalLength(routes As IEnumerable(Of WireRoute)) As Double
            Return routes.Sum(Function(item) item.Length)
        End Function
        Public Function HasCrossing(first As WireRoute, second As WireRoute) As Boolean
            For Each a In first.Segments
                For Each b In second.Segments
                    If SegmentsIntersect(a.StartPoint, a.EndPoint, b.StartPoint, b.EndPoint) Then Return True
                Next
            Next
            Return False
        End Function
        Private Function Orientation(a As ModelPoint, b As ModelPoint, c As ModelPoint) As Double
            Return (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y)
        End Function
        Private Function SegmentsIntersect(a As ModelPoint, b As ModelPoint, c As ModelPoint, d As ModelPoint) As Boolean
            Dim first = Orientation(a, b, c)
            Dim second = Orientation(a, b, d)
            Dim third = Orientation(c, d, a)
            Dim fourth = Orientation(c, d, b)
            Return ((first > 0 AndAlso second < 0) OrElse (first < 0 AndAlso second > 0)) AndAlso ((third > 0 AndAlso fourth < 0) OrElse (third < 0 AndAlso fourth > 0))
        End Function
    End Class
End Namespace
