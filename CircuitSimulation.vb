Imports System.Linq

Namespace Electropad
    Public Class SimulationPoint
        Public Property TimeSeconds As Double
        Public Property Voltage As Double
        Public Property CurrentAmps As Double
        Public Property PowerWatts As Double
        Public Property TemperatureCelsius As Double
    End Class

    Public Class SimulationTrace
        Public Property StartedAt As DateTime
        Public Property DurationSeconds As Double
        Public Property StepSeconds As Double
        Public Property Points As New List(Of SimulationPoint)
        Public ReadOnly Property PeakCurrent As Double
            Get
                Return If(Points.Count = 0, 0, Points.Max(Function(item) item.CurrentAmps))
            End Get
        End Property
        Public ReadOnly Property PeakTemperature As Double
            Get
                Return If(Points.Count = 0, 0, Points.Max(Function(item) item.TemperatureCelsius))
            End Get
        End Property
    End Class

    Public Class CircuitSimulationEngine
        Private ReadOnly analyzer As New CircuitAnalyzer()
        Public Function Run(project As CircuitProject, durationSeconds As Double, stepSeconds As Double) As SimulationTrace
            Dim trace As New SimulationTrace With {.StartedAt = DateTime.Now, .DurationSeconds = Math.Max(0, durationSeconds), .StepSeconds = Math.Max(0.001, stepSeconds)}
            Dim result = analyzer.Analyze(project)
            Dim material = MaterialCatalog.Find(project.Material)
            Dim area = Math.PI * Math.Pow(Math.Max(0.1, project.WireRadius), 2)
            Dim time = 0.0
            While time <= trace.DurationSeconds
                Dim decay = Math.Exp(-time / Math.Max(0.001, trace.DurationSeconds + 0.001))
                Dim current = result.CurrentAmps * (0.95 + 0.05 * decay)
                Dim power = result.Voltage * current
                Dim temperature = 25 + analyzer.EstimateTemperatureRise(power, material, area)
                trace.Points.Add(New SimulationPoint With {.TimeSeconds = time, .Voltage = result.Voltage, .CurrentAmps = current, .PowerWatts = power, .TemperatureCelsius = temperature})
                time += trace.StepSeconds
            End While
            Return trace
        End Function
        Public Function RunSteadyState(project As CircuitProject) As SimulationPoint
            Dim result = analyzer.Analyze(project)
            Return New SimulationPoint With {.TimeSeconds = 0, .Voltage = result.Voltage, .CurrentAmps = result.CurrentAmps, .PowerWatts = result.PowerWatts, .TemperatureCelsius = 25 + analyzer.EstimateTemperatureRise(result.PowerWatts, MaterialCatalog.Find(project.Material), Math.PI * Math.Pow(Math.Max(0.1, project.WireRadius), 2))}
        End Function
        Public Function IsThermallySafe(trace As SimulationTrace, material As MaterialSpec) As Boolean
            If trace Is Nothing OrElse material Is Nothing Then Return False
            Return trace.PeakTemperature <= material.MaximumTemperature
        End Function
        Public Function AverageCurrent(trace As SimulationTrace) As Double
            Return If(trace Is Nothing OrElse trace.Points.Count = 0, 0, trace.Points.Average(Function(item) item.CurrentAmps))
        End Function
        Public Function IntegrateEnergy(trace As SimulationTrace) As Double
            If trace Is Nothing OrElse trace.Points.Count < 2 Then Return 0
            Return trace.Points.Sum(Function(item) item.PowerWatts * trace.StepSeconds)
        End Function
        Public Function FormatTrace(trace As SimulationTrace) As IEnumerable(Of String)
            If trace Is Nothing Then Return Enumerable.Empty(Of String)()
            Return trace.Points.Select(Function(item) item.TimeSeconds.ToString("0.###") & " s | " & item.Voltage.ToString("0.###") & " V | " & (item.CurrentAmps * 1000).ToString("0.###") & " mA | " & item.PowerWatts.ToString("0.###") & " W")
        End Function
    End Class
End Namespace
