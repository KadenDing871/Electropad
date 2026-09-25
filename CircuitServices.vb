Imports System.Linq

Namespace Electropad
    Public Module CircuitServices
        Public Function WireResistance(material As MaterialSpec, lengthMm As Double, radiusMm As Double) As Double
            If material Is Nothing OrElse radiusMm <= 0 Then Return Double.PositiveInfinity
            Return material.Resistivity * (lengthMm / 1000.0) / (Math.PI * radiusMm * radiusMm)
        End Function

        Public Function TotalWireResistance(project As CircuitProject) As Double
            If project Is Nothing Then Return 0
            If project.Wires.Count = 0 Then Return WireResistance(MaterialCatalog.Find(project.Material), project.WireLength, project.WireRadius)
            Return project.Wires.Where(Function(wire) Not wire.Hidden).Sum(Function(wire) WireResistance(MaterialCatalog.Find(If(String.IsNullOrWhiteSpace(wire.Material), project.Material, wire.Material)), wire.LengthMm, wire.RadiusMm))
        End Function

        Public Function ComponentReferenceIssues(project As CircuitProject) As List(Of String)
            Dim issues As New List(Of String)
            If project Is Nothing Then Return issues
            For Each reference In project.Components.GroupBy(Function(item) item.Reference).Where(Function(group) group.Count() > 1).Select(Function(group) group.Key)
                issues.Add("Duplicate component reference: " & reference)
            Next
            For Each wire In project.Wires
                If project.Components.All(Function(item) item.Reference <> wire.StartReference) Then issues.Add(wire.Reference & " has no start component")
                If project.Components.All(Function(item) item.Reference <> wire.EndReference) Then issues.Add(wire.Reference & " has no end component")
                If wire.LengthMm <= 0 Then issues.Add(wire.Reference & " has an invalid length")
                If wire.RadiusMm <= 0 Then issues.Add(wire.Reference & " has an invalid radius")
            Next
            Return issues
        End Function

        Public Function CircuitCurrent(voltage As Double, componentResistance As Double, wireResistanceValue As Double) As Double
            Dim totalResistance = componentResistance + wireResistanceValue
            If totalResistance <= 0 Then Return 0
            Return voltage / totalResistance
        End Function
    End Module
End Namespace
