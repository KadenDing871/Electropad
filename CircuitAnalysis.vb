Imports System.Linq

Namespace Electropad
    Public Class AnalysisResult
        Public Property Voltage As Double
        Public Property CurrentAmps As Double
        Public Property CurrentMilliamps As Double
        Public Property WireResistance As Double
        Public Property ComponentResistance As Double
        Public Property TotalResistance As Double
        Public Property PowerWatts As Double
        Public Property WirePowerWatts As Double
        Public Property ComponentPowerWatts As Double
        Public Property IsValid As Boolean
        Public Property Warnings As New List(Of String)
        Public ReadOnly Property Summary As String
            Get
                Return "V=" & Voltage.ToString("0.######") & " V, I=" & CurrentMilliamps.ToString("0.######") & " mA, R=" & TotalResistance.ToString("0.######") & " Ω"
            End Get
        End Property
    End Class

    Public Class CircuitAnalyzer
        Public Function Analyze(project As CircuitProject) As AnalysisResult
            Dim result As New AnalysisResult()
            If project Is Nothing Then
                result.Warnings.Add("No project is loaded.")
                Return result
            End If
            Dim battery = project.Components.FirstOrDefault(Function(item) item.Kind = "Battery" AndAlso Not item.Hidden)
            Dim resistor = project.Components.FirstOrDefault(Function(item) item.Kind = "Resistor" AndAlso Not item.Hidden)
            result.Voltage = If(battery Is Nothing, 0, battery.Value)
            result.ComponentResistance = If(resistor Is Nothing, 0, Math.Max(0, resistor.Value))
            result.WireResistance = CircuitServices.TotalWireResistance(project)
            result.TotalResistance = result.ComponentResistance + result.WireResistance
            If result.TotalResistance > 0 Then result.CurrentAmps = result.Voltage / result.TotalResistance
            result.CurrentMilliamps = result.CurrentAmps * 1000
            result.PowerWatts = result.Voltage * result.CurrentAmps
            result.WirePowerWatts = result.CurrentAmps ^ 2 * result.WireResistance
            result.ComponentPowerWatts = result.CurrentAmps ^ 2 * result.ComponentResistance
            result.IsValid = True
            ValidateTopology(project, result)
            Return result
        End Function
        Private Sub ValidateTopology(project As CircuitProject, result As AnalysisResult)
            For Each wire In project.Wires.Where(Function(item) Not item.Hidden)
                If project.Components.All(Function(item) item.Reference <> wire.StartReference) Then result.Warnings.Add(wire.Reference & " start node is missing")
                If project.Components.All(Function(item) item.Reference <> wire.EndReference) Then result.Warnings.Add(wire.Reference & " end node is missing")
            Next
            If project.Components.Where(Function(item) item.Kind = "Battery" AndAlso Not item.Hidden).Count() = 0 Then result.Warnings.Add("No active voltage source found.")
            If project.Components.Where(Function(item) item.Kind = "Resistor" AndAlso Not item.Hidden).Count() = 0 Then result.Warnings.Add("No active resistive load found.")
        End Sub
        Public Function EstimateTemperatureRise(powerWatts As Double, material As MaterialSpec, areaMm2 As Double) As Double
            If material Is Nothing OrElse material.ThermalConductivity <= 0 OrElse areaMm2 <= 0 Then Return Double.PositiveInfinity
            Return powerWatts / (material.ThermalConductivity * areaMm2 / 1000000.0)
        End Function
        Public Function VoltageDrop(currentAmps As Double, resistance As Double) As Double
            Return currentAmps * resistance
        End Function
        Public Function EnergyOverTime(powerWatts As Double, seconds As Double) As Double
            Return powerWatts * seconds
        End Function
        Public Function SafeOperatingCurrent(material As MaterialSpec, lengthMm As Double, radiusMm As Double, voltage As Double) As Double
            Dim resistance = CircuitServices.WireResistance(material, lengthMm, radiusMm)
            If resistance <= 0 OrElse Double.IsInfinity(resistance) Then Return 0
            Return voltage / resistance
        End Function
        Public Function CompareMaterials(first As MaterialSpec, second As MaterialSpec) As String
            If first Is Nothing OrElse second Is Nothing Then Return "No comparison available."
            Dim resistanceRatio = If(second.Resistivity = 0, Double.PositiveInfinity, first.Resistivity / second.Resistivity)
            Return first.Name & " is " & resistanceRatio.ToString("0.###") & "x the resistivity of " & second.Name & "."
        End Function
    End Class
End Namespace
