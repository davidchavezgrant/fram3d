#!/usr/bin/env python3
"""
Adds sensor_area_mm to HD/1080p modes on mirrorless and DSLR cameras where
HD uses full-sensor readout (downsampled), NOT a sensor crop. Without this,
ComputeGateWidth's proportional scaling would incorrectly compute a tiny
crop area for HD modes.

Cinema cameras (RED, Blackmagic, Vision Research, ARRI) are excluded — their
HD modes ARE genuine sensor crops (windowed readout for high frame rates).
"""
import json

# Manufacturers whose HD modes use full-sensor readout (downsampled)
FULL_SENSOR_HD_MANUFACTURERS = {
    "Canon", "Sony", "Panasonic", "Fujifilm",
}

# Camera categories where HD is full-sensor
FULL_SENSOR_HD_CATEGORIES = {
    "DSLR", "Mirrorless",
}

# Specific cinema cameras where HD is also full-sensor (not cropped)
FULL_SENSOR_HD_CAMERAS = {
    # Canon Cinema cameras at standard frame rates use full S35 for HD
    "EOS C100", "EOS C100 Mark II", "EOS C200",
    "EOS C300", "EOS C300 Mark II", "EOS C300 Mark III",
    "EOS C500", "EOS C500 Mark II",
    "EOS C700 S35", "EOS C700 FF", "EOS C700 GS PL",
    "EOS C70", "EOS C400", "EOS C80",
    # Sony cinema cameras
    "PMW-F3", "PMW-F5", "PMW-F55",
    "FX9", "FX6", "FX3", "FX30", "FR7",
    "NEX-FS100",
    # Panasonic cinema
    "EVA1", "VariCam 35", "VariCam LT",
    # Panavision
    "Genesis",
}

# HD mode names to fix
HD_MODE_NAMES = {"HD", "Full HD", "1080p", "1080i", "HD 16:9"}


def should_fix(camera, mode_name):
    """Returns True if this camera's HD mode uses full-sensor readout."""
    name = camera["name"]
    category = camera.get("category", "")
    manufacturer = camera.get("manufacturer", "")

    if name in FULL_SENSOR_HD_CAMERAS:
        return True

    if manufacturer in FULL_SENSOR_HD_MANUFACTURERS and category in FULL_SENSOR_HD_CATEGORIES:
        return True

    return False


def get_full_sensor_hd_area(camera):
    """Computes the full-sensor HD active area: full width, 16:9 height."""
    # Use the first sensor mode's width if available, else body sensor
    modes = camera.get("sensor_modes", [])
    if modes and modes[0].get("sensor_area_mm") and modes[0]["sensor_area_mm"][0] > 0:
        width = modes[0]["sensor_area_mm"][0]
    else:
        width = camera["sensor_width_mm"]

    # HD is 16:9
    height = width / (16.0 / 9.0)
    return [round(width, 2), round(height, 2)]


def main():
    with open("camera-lens-db/camera-lens-database.json", "r") as f:
        db = json.load(f)

    fixed = 0

    for camera in db["cameras"]:
        if "sensor_modes" not in camera:
            continue

        for mode in camera["sensor_modes"]:
            if mode["name"] not in HD_MODE_NAMES:
                continue

            # Skip if already has sensor area
            if mode.get("sensor_area_mm") and mode["sensor_area_mm"][0] > 0:
                continue

            if not should_fix(camera, mode["name"]):
                continue

            area = get_full_sensor_hd_area(camera)
            mode["sensor_area_mm"] = area
            print(f"  {camera['manufacturer']} {camera['name']} / {mode['name']}: added sensor_area_mm {area}")
            fixed += 1

    with open("camera-lens-db/camera-lens-database.json", "w") as f:
        json.dump(db, f, indent=2)

    print(f"\nDone: {fixed} HD modes fixed")


if __name__ == "__main__":
    main()
