Imports System.Linq

Namespace Electropad
    Public Class MeasurementRecord
        Public Property Name As String
        Public Property Value As Double
        Public Property UnitName As String
        Public Property SourceReference As String
        Public Property Notes As String
        Public ReadOnly Property DisplayValue As String
            Get
                Return Name & ": " & Value.ToString("0.######") & " " & UnitName
            End Get
        End Property
    End Class

    Public Class MeasurementEngine
        Public Function MeasureComponentLength(component As CircuitComponent) As MeasurementRecord
            Return New MeasurementRecord With {.Name = component.Reference & " body length", .Value = component.LengthMm, .UnitName = "mm", .SourceReference = component.Reference, .Notes = "Physical component body length"}
        End Function
        Public Function MeasureComponentArea(component As CircuitComponent) As MeasurementRecord
            Dim area = component.LengthMm * component.BodyDiameterMm
            Return New MeasurementRecord With {.Name = component.Reference & " envelope area", .Value = area, .UnitName = "mm²", .SourceReference = component.Reference, .Notes = "Projected rectangular component envelope"}
        End Function
        Public Function MeasureWireLength(wire As CircuitWire) As MeasurementRecord
            Return New MeasurementRecord With {.Name = wire.Reference & " length", .Value = wire.LengthMm, .UnitName = "mm", .SourceReference = wire.Reference, .Notes = "Declared physical wire length"}
        End Function
        Public Function MeasureWireVolume(wire As CircuitWire) As MeasurementRecord
            Dim volume = Math.PI * wire.RadiusMm * wire.RadiusMm * wire.LengthMm
            Return New MeasurementRecord With {.Name = wire.Reference & " volume", .Value = volume, .UnitName = "mm³", .SourceReference = wire.Reference, .Notes = "Cylindrical conductor volume"}
        End Function
        Public Function MeasureWireMass(wire As CircuitWire) As MeasurementRecord
            Dim material = MaterialCatalog.Find(wire.Material)
            Dim volumeCm3 = Math.PI * wire.RadiusMm * wire.RadiusMm * wire.LengthMm / 1000.0
            Return New MeasurementRecord With {.Name = wire.Reference & " mass", .Value = volumeCm3 * material.Density, .UnitName = "g", .SourceReference = wire.Reference, .Notes = material.Name & " density"}
        End Function
        Public Function MeasureBoardBounds(project As CircuitProject) As MeasurementRecord
            Dim bounds = CircuitGeometry.BoundingBox(project.Components)
            Return New MeasurementRecord With {.Name = "Board envelope", .Value = bounds.Width * bounds.Height, .UnitName = "mm²", .SourceReference = "PROJECT", .Notes = bounds.Width.ToString("0.#") & " x " & bounds.Height.ToString("0.#") & " mm"}
        End Function
        Public Function MeasureAll(project As CircuitProject) As List(Of MeasurementRecord)
            Dim records As New List(Of MeasurementRecord) From {MeasureBoardBounds(project)}
            For Each component In project.Components.Where(Function(item) Not item.Hidden)
                records.Add(MeasureComponentLength(component))
                records.Add(MeasureComponentArea(component))
            Next
            For Each wire In project.Wires.Where(Function(item) Not item.Hidden)
                records.Add(MeasureWireLength(wire))
                records.Add(MeasureWireVolume(wire))
                records.Add(MeasureWireMass(wire))
            Next
            Return records
        End Function
        Public Function TotalConductorMass(project As CircuitProject) As Double
            Return project.Wires.Where(Function(item) Not item.Hidden).Sum(Function(item) MeasureWireMass(item).Value)
        End Function
        Public Function TotalWireVolume(project As CircuitProject) As Double
            Return project.Wires.Where(Function(item) Not item.Hidden).Sum(Function(item) MeasureWireVolume(item).Value)
        End Function
        Public Function LongestWire(project As CircuitProject) As CircuitWire
            Return project.Wires.Where(Function(item) Not item.Hidden).OrderByDescending(Function(item) item.LengthMm).FirstOrDefault()
        End Function
    End Class
End Namespace
