#!/usr/bin/env python3
"""
Fixes sensor mode ordering so the widest mode (open gate) is first.
Also adds sensor_area_mm to full-width HD modes that were missing it.
The first sensor mode determines the default gate — it should be the
camera's widest video mode, not a crop.
"""
import json


def fix_camera(camera, modes):
    """Replace sensor_modes for a camera."""
    camera["sensor_modes"] = modes


def main():
    with open("camera-lens-db/camera-lens-database.json", "r") as f:
        db = json.load(f)

    cameras = {c["name"]: c for c in db["cameras"]}
    fixed = 0

    # ── Cameras where HD (full sensor width) should be first ──────────

    # Canon 5D Mark IV: HD is full-width, 4K is cropped
    fix_camera(cameras["EOS 5D Mark IV"], [
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [36.0, 20.25], "max_fps": 60},
        {"name": "4K DCI Crop",  "resolution": [4096, 2160], "sensor_area_mm": [20.7, 10.9],  "max_fps": 30},
    ])
    fixed += 1

    # Canon 1D X Mark II: has both uncropped and cropped 4K
    fix_camera(cameras["EOS 1D X Mark II"], [
        {"name": "4K DCI",       "resolution": [4096, 2160], "sensor_area_mm": [26.9, 14.2],  "max_fps": 60},
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [36.0, 20.25], "max_fps": 120},
    ])
    fixed += 1

    # Canon EOS R: 4K is heavily cropped, HD is full width
    fix_camera(cameras["EOS R"], [
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [36.0, 20.25], "max_fps": 60},
        {"name": "4K UHD Crop",  "resolution": [3840, 2160], "sensor_area_mm": [20.6, 11.6],  "max_fps": 30},
    ])
    fixed += 1

    # Canon 90D: 4K uses full APS-C width, crop mode is optional
    fix_camera(cameras["EOS 90D"], [
        {"name": "4K UHD",       "resolution": [3840, 2160], "sensor_area_mm": [22.3, 12.5],  "max_fps": 30},
        {"name": "4K UHD Crop",  "resolution": [3840, 2160], "sensor_area_mm": [18.5, 10.4],  "max_fps": 30},
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [22.3, 12.5],  "max_fps": 120},
    ])
    fixed += 1

    # Canon 5D Mark II: HD only, full sensor width
    fix_camera(cameras["EOS 5D Mark II"], [
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [36.0, 20.25], "max_fps": 30},
    ])
    fixed += 1

    # Canon 5D Mark III: HD only, full sensor width
    fix_camera(cameras["EOS 5D Mark III"], [
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [36.0, 20.25], "max_fps": 30},
    ])
    fixed += 1

    # Canon 7D Mark II: HD only, full APS-C width
    fix_camera(cameras["EOS 7D Mark II"], [
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [22.4, 12.6],  "max_fps": 60},
    ])
    fixed += 1

    # Canon 6D Mark II: no 4K, HD only at full width
    fix_camera(cameras["EOS 6D Mark II"], [
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [36.0, 20.25], "max_fps": 60},
    ])
    fixed += 1

    # Canon 1D X: HD only, full sensor width
    fix_camera(cameras["EOS 1D X"], [
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [36.0, 20.25], "max_fps": 30},
    ])
    fixed += 1

    # Canon EOS R50: APS-C, 4K at full width
    fix_camera(cameras["EOS R50"], [
        {"name": "4K UHD",       "resolution": [3840, 2160], "sensor_area_mm": [22.3, 12.5],  "max_fps": 30},
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [22.3, 12.5],  "max_fps": 60},
    ])
    fixed += 1

    # Canon EOS R100: APS-C, 4K at full width
    fix_camera(cameras["EOS R100"], [
        {"name": "4K UHD",       "resolution": [3840, 2160], "sensor_area_mm": [22.3, 12.5],  "max_fps": 24},
        {"name": "HD",           "resolution": [1920, 1080], "sensor_area_mm": [22.3, 12.5],  "max_fps": 60},
    ])
    fixed += 1

    # ── Sony cameras: ensure HD modes have sensor area ────────────────

    for name in ["A7S", "A7S II", "A7 III", "A7C", "A9", "A9 II",
                 "A7R", "A7R II", "A7R III", "A7R IV"]:
        if name in cameras and "sensor_modes" in cameras[name]:
            for mode in cameras[name]["sensor_modes"]:
                if "sensor_area_mm" not in mode or mode.get("sensor_area_mm") == [0, 0]:
                    body_w = cameras[name].get("sensor_width_mm", 35.6)
                    res = mode["resolution"]
                    res_ratio = res[0] / res[1] if res[1] > 0 else 1.78
                    mode["sensor_area_mm"] = [round(body_w, 1), round(body_w / res_ratio, 1)]

    with open("camera-lens-db/camera-lens-database.json", "w") as f:
        json.dump(db, f, indent=2)

    print(f"Done: {fixed} cameras reordered/fixed")


if __name__ == "__main__":
    main()
