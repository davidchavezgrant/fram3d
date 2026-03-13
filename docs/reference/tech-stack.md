# Tech Stack

- **Engine**: Unity 6000.1.11f1
- **Rendering**: URP (Universal Render Pipeline) with post-processing Volumes for DOF (Bokeh mode)
- **Camera**: Plain `UnityEngine.Camera` with `usePhysicalProperties = true`. No Cinemachine — all camera computation in domain code.
- **Input**: Unity Input System (`UnityEngine.InputSystem`) with direct `Keyboard.current` / `Mouse.current` access
- **UI framework**: UI Toolkit (`UnityEngine.UIElements`) — VisualElement, Button, Label, TextField, etc.
- **UI construction**: All UI built programmatically in C# at runtime. No UXML, no USS files. Styling via inline `style.*` properties.
- **Domain math**: `System.Numerics` (Vector3, Quaternion) in Core and Services. Conversion to Unity types at the Engine boundary.
- **Testing**: xUnit for Core and Services (pure C#). Unity Test Runner for Engine and UI.
