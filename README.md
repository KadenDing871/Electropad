# Electropad

Electropad is a .NET 10 Windows x64 WinForms circuit studio prototype written in VB.

## Build and run

Requires the .NET 10 SDK and Windows desktop runtime.

```powershell
dotnet build Electropad.vbproj -c Release -p:Platform=x64
dotnet run --project Electropad.vbproj -c Release -p:Platform=x64
```

Release staging, without publishing yet:

```powershell
dotnet publish Electropad.vbproj -c Release -r win-x64 --self-contained false -p:Platform=x64 -o publish/win-x64
```

The publish command is intentionally documented but has not been run. Test the Release build on Windows first, then publish to a clean output directory.

The application includes:

- Component palette for resistors, LEDs, batteries, switches, and ground.
- Schematic canvas with connected components and adjustable wire radius.
- Breadboard view and a 3D inspector summary.
- Isometric 3D board view with extruded component bodies and socket field.
- SolidWorks-style assembly tree in the left component rail.
- 2D ↔ 3D Exchange dialog with target selection and conversion progress bar.
- Component naming/value editing, component color, and circuit material controls.
- Simulation using battery voltage and resistor value to calculate test-point current.
- Wire resistance calculation based on length, radius, and copper resistivity.
- Circuit Exchange (`.cex`) JSON import and export.

The 3D view initializes a Vortice.Direct3D11 hardware device and swap chain when available, with the existing GDI+ scene retained as a compatibility overlay. The project does not require the legacy `dx8vb` type library; the renderer uses the modern Direct3D 11 API and can be extended with GPU mesh and material buffers without changing the project model or exchange dialog.