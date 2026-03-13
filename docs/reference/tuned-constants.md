# Tuned Constants

Values from the prior codebase that were empirically tuned. Starting points — expect to re-tune.

---

## Camera Movement Speeds

- `DollyScrollSpeed`: 0.01
- `PanTiltSpeed`: 0.2
- `RollSpeed`: 0.03
- `CraneSpeed`: 0.02
- `TruckSpeed`: 0.02
- `DefaultPosition`: (0, 1.6, -5)
- `DefaultRotation`: Identity quaternion

## Focus

- Lerp speed: 2.0
- Distance multiplier: 1.5x (breathing room when focusing on an element)
- Minimum distance: 0.1 units

## Camera Shake

- Perlin noise-based
- Default amplitude: 0.1, frequency: 1.0
- Position scale: 0.01
- Rotation scale: 0.5 (X/Y only, no Z roll)
- Rotation time offset for decorrelation: 100.0

## Lens

- Default focal length: 50mm
- Common focal lengths: {14, 18, 24, 35, 50, 85, 100, 135, 200}
- Focal length adjustment multiplier: 0.5 per scroll unit
- Dolly zoom speed: 0.5
- Lens smoothing lerp speed: 10

## Recording Thresholds

- Near existing keyframe: 0.1 seconds. If near: update existing. If not: create new.
- Camera change detection: position 0.001 units, rotation 0.01 degrees, focal length 0.01mm
- Element change detection: position 0.001, rotation 0.01, scale 0.001

## Timeline

- Default shot duration: 5.0 seconds
- Minimum shot duration: 0.1 seconds
- Keyframe snap: frame boundaries (1/fps — e.g., 1/24 = 0.04167s at 24fps)
- Keyframe time tolerance (same-time conflict): 0.01 seconds

## Input Sensitivities

- Drag sensitivity: 0.2
- Scroll sensitivity: 0.02
- Scroll deadzone: 0.01
- Sideways movement threshold: 0.01

## Undo

- Coalescing timeout: 1000ms inactivity for scroll gestures

## Performance Budgets

- Editor target: 60fps with < 50 elements, 30fps acceptable with 50–200 elements
- Max elements per scene: 500 (soft limit — show performance warning)
- Max keyframes per track: 1,000
- Max total keyframes per scene: 10,000
- Export render: offline (no frame rate target), < 1s per frame at 1080p
- Startup to interactive: < 5s on Apple Silicon
- Project load: < 3s for typical project (< 50MB)
