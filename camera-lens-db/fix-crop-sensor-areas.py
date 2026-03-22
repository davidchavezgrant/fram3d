#!/usr/bin/env python3
"""
Fixes sensor_area_mm for cameras with center-crop video modes.
The original add-sensor-modes.py used full photo sensor dimensions,
but many cameras use a smaller active area for video recording.
"""
import json

# Corrections: camera name → mode name → corrected [width, height] in mm
FIXES = {
    # ── Canon DSLRs ──────────────────────────────────────────────────
    "EOS 5D Mark IV": {
        "4K DCI Crop": [20.7, 10.9],      # 1.74x crop from 36mm FF
    },
    "EOS 1D X Mark II": {
        "4K DCI Crop": [26.9, 14.2],       # 1.34x crop (directly sourced)
    },
    "EOS 1D X Mark III": {
        "5.5K RAW":    [36.0, 19.0],       # full width, 17:9 crop height
        "4K DCI Crop": [27.7, 14.6],       # 1.3x crop
    },
    # 6D Mark II: no 4K video at all — no fix needed (HD only)
    "EOS 90D": {
        "4K UHD Crop": [18.5, 10.4],       # ~1.2x additional crop from APS-C
    },

    # ── Canon Mirrorless ─────────────────────────────────────────────
    "EOS R": {
        "4K UHD Crop": [20.6, 11.6],       # 1.75x crop from 36mm FF
    },
    "EOS R5": {
        "8K DCI RAW":      [36.0, 19.0],   # full width, confirmed uncropped
        "4K UHD":          [36.0, 20.3],    # full width, 16:9 height
    },
    "EOS R5 C": {
        "8K DCI RAW":      [35.9, 18.9],   # full width
        "8K DCI":          [35.9, 18.9],    # full width
        "4K UHD":          [35.9, 20.2],    # full width, 16:9 height
    },
    "EOS R5 Mark II": {
        "8K DCI RAW":       [35.9, 18.9],  # full width
        "4K UHD FF":        [35.9, 20.2],   # full width, 16:9 height
        "4K UHD S35 Crop":  [22.4, 12.6],  # 1.6x APS-C crop
    },
    "EOS R7": {
        "4K UHD":          [22.3, 12.5],    # full APS-C sensor width
        "4K UHD Crop":     [12.3, 6.9],     # heavy 1:1 readout crop
    },
    "EOS R10": {
        "4K UHD":          [22.2, 12.5],    # full APS-C sensor width
        "4K UHD Crop":     [14.2, 8.0],     # ~1.56x crop from APS-C
    },
    "EOS R8": {
        "4K UHD":          [36.0, 20.3],    # full width, oversampled from 6K
        "4K UHD Crop":     [22.5, 12.7],    # 1.6x APS-C crop
    },
    "EOS R6": {
        "4K UHD":          [35.9, 20.2],    # full width, 16:9
    },
    "EOS R6 Mark II": {
        "6K RAW ext":      [35.9, 19.0],    # full width, 17:9
        "4K UHD":          [35.9, 20.2],    # full width
        "4K UHD Crop":     [22.4, 12.6],    # 1.6x crop
    },

    # ── Sony Mirrorless (S35/APS-C crop modes) ───────────────────────
    "A7 IV": {
        "4K UHD S35": [23.6, 13.3],        # 1.5x APS-C crop
    },
    "A7 V": {
        "4K UHD S35": [23.6, 13.3],        # 1.5x APS-C crop
    },
    "A7C II": {
        "4K UHD S35": [23.6, 13.3],        # 1.5x APS-C crop
    },
    "A7R V": {
        "4K UHD":     [28.8, 16.2],         # 1.24x crop from 8K readout
    },

    # ── Fujifilm GFX (medium format video crop) ──────────────────────
    "GFX 100": {
        "DCI 4K":     [43.8, 24.6],         # full width, 16:9 height crop of 4:3 sensor
        "4K UHD":     [43.8, 24.6],
    },
    "GFX 100S": {
        "DCI 4K":     [43.8, 24.6],
        "4K UHD":     [43.8, 24.6],
    },
    "GFX 100S II": {
        "4K UHD":     [43.8, 24.6],
    },
    "GFX 100 II": {
        "8K DCI":     [29.0, 16.0],         # significant crop for 8K (directly sourced)
        "8K UHD":     [29.0, 16.0],
        "DCI 4K":     [43.8, 24.6],         # full width for 4K
        "4K UHD":     [43.8, 24.6],
    },
    "GFX 100RF": {
        "DCI 4K":     [43.8, 24.6],
        "4K UHD":     [43.8, 24.6],
    },
}


def main():
    with open("camera-lens-db/camera-lens-database.json", "r") as f:
        db = json.load(f)

    fixes_applied = 0
    modes_fixed = 0

    for camera in db["cameras"]:
        name = camera["name"]
        if name not in FIXES:
            continue
        if "sensor_modes" not in camera:
            continue

        mode_fixes = FIXES[name]
        for mode in camera["sensor_modes"]:
            if mode["name"] in mode_fixes:
                new_area = mode_fixes[mode["name"]]
                old_area = mode.get("sensor_area_mm", [0, 0])
                mode["sensor_area_mm"] = new_area
                print(f"  {name} / {mode['name']}: [{old_area[0]:.1f}, {old_area[1]:.1f}] → [{new_area[0]:.1f}, {new_area[1]:.1f}]")
                modes_fixed += 1

        fixes_applied += 1

    with open("camera-lens-db/camera-lens-database.json", "w") as f:
        json.dump(db, f, indent=2)

    print(f"\nDone: {fixes_applied} cameras, {modes_fixed} modes fixed")


if __name__ == "__main__":
    main()
