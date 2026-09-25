Imports System.Drawing
Imports System.Linq

Namespace Electropad
    Public Structure ModelPoint
        Public Property X As Double
        Public Property Y As Double
        Public Sub New(xValue As Double, yValue As Double)
            X = xValue
            Y = yValue
        End Sub
        Public Function DistanceTo(other As ModelPoint) As Double
            Return Math.Sqrt((X - other.X) ^ 2 + (Y - other.Y) ^ 2)
        End Function
        Public Function Offset(dx As Double, dy As Double) As ModelPoint
            Return New ModelPoint(X + dx, Y + dy)
        End Function
        Public Function ToScreen() As Point
            Return New Point(CInt(Math.Round(X)), CInt(Math.Round(Y)))
        End Function
    End Structure

    Public Structure ModelBounds
        Public Property Left As Double
        Public Property Top As Double
        Public Property Width As Double
        Public Property Height As Double
        Public ReadOnly Property Right As Double
            Get
                Return Left + Width
            End Get
        End Property
        Public ReadOnly Property Bottom As Double
            Get
                Return Top + Height
            End Get
        End Property
        Public Function Contains(point As ModelPoint) As Boolean
            Return point.X >= Left AndAlso point.X <= Right AndAlso point.Y >= Top AndAlso point.Y <= Bottom
        End Function
        Public Function Inflate(amount As Double) As ModelBounds
            Return New ModelBounds With {.Left = Left - amount, .Top = Top - amount, .Width = Width + amount * 2, .Height = Height + amount * 2}
        End Function
        Public Function Center() As ModelPoint
            Return New ModelPoint(Left + Width / 2, Top + Height / 2)
        End Function
    End Structure

    Public Module CircuitGeometry
        Public Function ComponentBounds(component As CircuitComponent) As ModelBounds
            Return New ModelBounds With {.Left = component.X, .Top = component.Y, .Width = 155, .Height = 72}
        End Function
        Public Function ComponentCenter(component As CircuitComponent) As ModelPoint
            Return ComponentBounds(component).Center()
        End Function
        Public Function WireStart(component As CircuitComponent) As ModelPoint
            Return New ModelPoint(component.X + 155, component.Y + 36)
        End Function
        Public Function WireEnd(component As CircuitComponent) As ModelPoint
            Return New ModelPoint(component.X, component.Y + 36)
        End Function
        Public Function WireScreenLength(first As CircuitComponent, second As CircuitComponent) As Double
            Return WireStart(first).DistanceTo(WireEnd(second))
        End Function
        Public Function DistanceToSegment(point As ModelPoint, startPoint As ModelPoint, endPoint As ModelPoint) As Double
            Dim dx = endPoint.X - startPoint.X
            Dim dy = endPoint.Y - startPoint.Y
            If dx = 0 AndAlso dy = 0 Then Return point.DistanceTo(startPoint)
            Dim t = Math.Max(0, Math.Min(1, ((point.X - startPoint.X) * dx + (point.Y - startPoint.Y) * dy) / (dx * dx + dy * dy)))
            Return point.DistanceTo(New ModelPoint(startPoint.X + t * dx, startPoint.Y + t * dy))
        End Function
        Public Function FindComponent(project As CircuitProject, point As ModelPoint) As CircuitComponent
            Return project.Components.FirstOrDefault(Function(item) Not item.Hidden AndAlso ComponentBounds(item).Contains(point))
        End Function
        Public Function FindWire(project As CircuitProject, point As ModelPoint, tolerance As Double) As CircuitWire
            For Each wire In project.Wires.Where(Function(item) Not item.Hidden)
                Dim first = project.Components.FirstOrDefault(Function(item) item.Reference = wire.StartReference)
                Dim second = project.Components.FirstOrDefault(Function(item) item.Reference = wire.EndReference)
                If first Is Nothing OrElse second Is Nothing OrElse first.Hidden OrElse second.Hidden Then Continue For
                If DistanceToSegment(point, WireStart(first), WireEnd(second)) <= tolerance Then Return wire
            Next
            Return Nothing
        End Function
        Public Function Snap(point As ModelPoint, spacing As Double) As ModelPoint
            If spacing <= 0 Then Return point
            Return New ModelPoint(Math.Round(point.X / spacing) * spacing, Math.Round(point.Y / spacing) * spacing)
        End Function
        Public Function AlignLeft(components As IEnumerable(Of CircuitComponent)) As Integer
            Dim visible = components.Where(Function(item) Not item.Hidden).ToList()
            If visible.Count = 0 Then Return 0
            Dim left = visible.Min(Function(item) item.X)
            For Each component In visible
                component.X = left
            Next
            Return visible.Count
        End Function
        Public Function AlignTop(components As IEnumerable(Of CircuitComponent)) As Integer
            Dim visible = components.Where(Function(item) Not item.Hidden).ToList()
            If visible.Count = 0 Then Return 0
            Dim top = visible.Min(Function(item) item.Y)
            For Each component In visible
                component.Y = top
            Next
            Return visible.Count
        End Function
        Public Function DistributeHorizontal(components As IEnumerable(Of CircuitComponent), gap As Integer) As Integer
            Dim ordered = components.Where(Function(item) Not item.Hidden).OrderBy(Function(item) item.X).ToList()
            For index = 1 To ordered.Count - 1
                ordered(index).X = ordered(index - 1).X + 155 + gap
            Next
            Return ordered.Count
        End Function
        Public Function DistributeVertical(components As IEnumerable(Of CircuitComponent), gap As Integer) As Integer
            Dim ordered = components.Where(Function(item) Not item.Hidden).OrderBy(Function(item) item.Y).ToList()
            For index = 1 To ordered.Count - 1
                ordered(index).Y = ordered(index - 1).Y + 72 + gap
            Next
            Return ordered.Count
        End Function
        Public Function BoundingBox(components As IEnumerable(Of CircuitComponent)) As ModelBounds
            Dim visible = components.Where(Function(item) Not item.Hidden).ToList()
            If visible.Count = 0 Then Return New ModelBounds()
            Dim left = visible.Min(Function(item) item.X)
            Dim top = visible.Min(Function(item) item.Y)
            Dim right = visible.Max(Function(item) item.X + 155)
            Dim bottom = visible.Max(Function(item) item.Y + 72)
            Return New ModelBounds With {.Left = left, .Top = top, .Width = right - left, .Height = bottom - top}
        End Function
    End Module
End Namespace
