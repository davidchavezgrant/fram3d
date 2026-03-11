#!/usr/bin/env python3
"""Build camera-lens-database.json from the markdown source data."""
import json

data = {
    "meta": {
        "description": "Comprehensive camera and lens database for Fram3d FOV simulation. Every entry includes parameters needed for FOV calculation: sensor dimensions, focal length, aperture, and squeeze factor.",
        "fov_formula": "2 * atan(sensor_dimension / (2 * focal_length))",
        "anamorphic_hfov_formula": "2 * atan((sensor_width * squeeze_factor) / (2 * focal_length))",
        "source": "docs/research/2026-03-11-*.md",
        "generated": "2026-03-11"
    },
    "cameras": [],
    "lenses": [],
    "reference": {
        "film_formats": [],
        "sensor_formats": [],
        "focal_length_guide": [],
        "mount_compatibility": [],
        "anamorphic_squeeze_factors": []
    }
}

cameras = data["cameras"]
lenses = data["lenses"]

###############################################################################
# HELPER: parse fps strings like "1080: 24,25,30; 720: 50,60" into flat list
###############################################################################
def parse_fps(s):
    """Extract all unique numeric fps values from a frame rate string."""
    import re
    vals = set()
    # Find all numbers (int or float) that look like fps values
    # Handle ranges like "24-60" and lists like "24,25,30"
    # Remove text labels first
    s = s.replace("ext.pwr", "").replace("ext", "").replace("overdrive", "")
    s = s.replace("(crop)", "").replace("(no crop)", "").replace("(S35 crop)", "")
    s = s.replace("(S35)", "").replace("FF", "").replace("(OG)", "")
    s = s.replace("(OG RAW)", "").replace("(200Mbps)", "").replace("RAW", "")
    s = s.replace("fwd", "").replace("rev", "").replace("crystal", "")
    s = s.replace("spring", "").replace("burst", "").replace("VFR", "")
    # Split by semicolons first to get segments
    segments = s.split(";")
    for seg in segments:
        # Remove resolution labels like "1080:", "4K:", "DCI4K:", etc.
        parts = seg.split(":")
        if len(parts) > 1:
            fps_part = parts[-1]
        else:
            fps_part = parts[0]
        # Now parse fps values
        # Split by commas
        items = fps_part.replace(" ", "").split(",")
        for item in items:
            item = item.strip()
            if not item:
                continue
            if "-" in item:
                # Range like "24-60"
                range_parts = item.split("-")
                try:
                    lo = float(range_parts[0])
                    hi = float(range_parts[1])
                    # Add standard fps values in range
                    standard = [0.75, 1, 2, 3, 4, 5, 8, 12, 15, 16, 20, 24, 25, 30, 32, 36, 40, 48, 50, 60, 72, 75, 80, 90, 96, 100, 120, 150, 168, 180, 200, 240, 300, 330, 480, 660, 960]
                    for v in standard:
                        if lo <= v <= hi:
                            vals.add(v)
                except (ValueError, IndexError):
                    pass
            else:
                # Try single value
                item = item.rstrip("ip")  # Remove 'i' for interlaced, 'p' for progressive
                try:
                    vals.add(float(item))
                except ValueError:
                    pass
    result = sorted(vals)
    # Convert to int where possible
    return [int(v) if v == int(v) else v for v in result]

###############################################################################
# 1. DIGITAL CAMERAS
###############################################################################

# 1.1 Canon DSLR Video
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str, cat in [
    ("EOS 5D Mark II", 2008, 36.0, 24.0, "FF", "EF", 1920, 1080, "1080: 24,25,30; 720: 50,60", "DSLR"),
    ("EOS 5D Mark III", 2012, 36.0, 24.0, "FF", "EF", 1920, 1080, "1080: 24,25,30; 720: 50,60", "DSLR"),
    ("EOS 5D Mark IV", 2016, 36.0, 24.0, "FF", "EF", 4096, 2160, "DCI4K: 24-30; 1080: 24-60; 720: 100,120", "DSLR"),
    ("EOS 1D X", 2012, 36.0, 24.0, "FF", "EF", 1920, 1080, "1080: 24,25,30; 720: 50,60", "DSLR"),
    ("EOS 1D X Mark II", 2016, 36.0, 24.0, "FF", "EF", 4096, 2160, "DCI4K: 24-60; 1080: 24-120", "DSLR"),
    ("EOS 1D X Mark III", 2020, 36.0, 24.0, "FF", "EF", 5472, 2886, "5.5K: 60; DCI4K: 24-60; 1080: 24-120", "DSLR"),
    ("EOS 7D Mark II", 2014, 22.4, 15.0, "APS-C", "EF/EF-S", 1920, 1080, "1080: 24-60; 720: 50,60", "DSLR"),
    ("EOS 6D Mark II", 2017, 35.9, 24.0, "FF", "EF", 1920, 1080, "1080: 24-60", "DSLR"),
    ("EOS 90D", 2019, 22.3, 14.9, "APS-C", "EF/EF-S", 3840, 2160, "UHD4K: 25,30; 1080: 25-120", "DSLR"),
]:
    cameras.append({"name": name, "manufacturer": "Canon", "year": year, "category": cat,
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# Canon EOS R Mirrorless
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("EOS R", 2018, 36.0, 24.0, "FF", "RF", 3840, 2160, "4K: 24-30; 1080: 24-60"),
    ("EOS R3", 2021, 36.0, 24.0, "FF", "RF", 6000, 3164, "6K: 60; DCI4K: 24-60; 4K: 120; 1080: 240"),
    ("EOS R5", 2020, 36.0, 24.0, "FF", "RF", 8192, 4320, "8K: 24-30; 4K: 24-120; 1080: 24-120"),
    ("EOS R5 C", 2022, 36.0, 24.0, "FF", "RF", 8192, 4320, "8K: 30,60; 4K: 120"),
    ("EOS R5 Mark II", 2024, 36.0, 24.0, "FF", "RF", 8192, 4320, "8K: 60; 4K: 120; 2K: 240"),
    ("EOS R6", 2020, 36.0, 24.0, "FF", "RF", 3840, 2160, "4K: 24-60; 1080: 24-120"),
    ("EOS R6 Mark II", 2022, 35.9, 23.9, "FF", "RF", 3840, 2160, "4K: 24-60; 1080: 24-180"),
    ("EOS R7", 2022, 22.3, 14.8, "APS-C", "RF", 3840, 2160, "4K: 24-60; 1080: 24-120"),
    ("EOS R8", 2023, 35.9, 23.9, "FF", "RF", 3840, 2160, "4K: 24-60; 1080: 24-180"),
    ("EOS R10", 2022, 22.3, 14.9, "APS-C", "RF", 3840, 2160, "4K: 24-60; 1080: 24-120"),
    ("EOS R50", 2023, 22.3, 14.9, "APS-C", "RF", 3840, 2160, "4K: 24-30; 1080: 24-120"),
    ("EOS R100", 2023, 22.3, 14.9, "APS-C", "RF", 3840, 2160, "4K: 24-25; 1080: 24-60; 720: 100,120"),
]:
    cameras.append({"name": name, "manufacturer": "Canon", "year": year, "category": "Mirrorless",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# Canon Cinema EOS
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("EOS C100", 2012, 24.6, 13.8, "S35", "EF", 1920, 1080, "1080: 24,25,30"),
    ("EOS C100 Mark II", 2014, 24.6, 13.8, "S35", "EF", 1920, 1080, "1080: 24-60"),
    ("EOS C200", 2017, 24.4, 13.5, "S35", "EF", 4096, 2160, "DCI4K: 24-30; 4K: 24-60; 1080: 24-120"),
    ("EOS C300", 2012, 24.6, 13.8, "S35", "EF or PL", 1920, 1080, "1080: 24-30; VFR: 1-60"),
    ("EOS C300 Mark II", 2015, 24.6, 13.8, "S35", "EF/PL swap", 4096, 2160, "DCI4K: 24-30; 2K: 24-60; HD: 120"),
    ("EOS C300 Mark III", 2020, 26.2, 13.8, "S35", "EF/PL swap", 4096, 2160, "4K: 12-120; 2K: 12-180"),
    ("EOS C500", 2012, 24.6, 13.8, "S35", "EF or PL", 4096, 2160, "4K: 24-60; 2K: 24-60"),
    ("EOS C500 Mark II", 2019, 38.1, 20.1, "FF", "EF/PL swap", 5952, 3140, "5.9K: 15-60; 4K: 15-60; 2K: 15-120"),
    ("EOS C700 S35", 2016, 26.2, 13.8, "S35", "EF or PL", 4096, 2160, "DCI4K: 60; 4.5K: 100-120"),
    ("EOS C700 FF", 2018, 38.1, 20.1, "FF", "EF/PL swap", 5952, 3140, "5.9K: 30; 4K: 72; 2K: 168"),
    ("EOS C700 GS PL", 2017, 26.2, 13.8, "S35", "PL", 4096, 2160, "DCI4K: 60"),
    ("EOS C70", 2020, 26.2, 13.8, "S35", "RF", 4096, 2160, "DCI4K: 15-120; 2K: 15-180"),
    ("EOS C400", 2024, 36.0, 19.0, "FF", "RF", 6000, 3164, "6K: 60; 4K: 120; 2K: 180"),
    ("EOS C80", 2024, 36.0, 19.0, "FF", "RF", 6000, 3164, "6K: 30; 4K: 120; 2K: 180"),
]:
    cameras.append({"name": name, "manufacturer": "Canon", "year": year, "category": "Digital Cinema",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# Canon Compact Cinema
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("XC10", 2015, 12.8, 9.6, "1\"", "Fixed", 3840, 2160, "4K: 24-30; 1080: 24-60"),
    ("XC15", 2016, 12.8, 9.6, "1\"", "Fixed", 3840, 2160, "4K: 24-30; 1080: 24-60"),
]:
    cameras.append({"name": name, "manufacturer": "Canon", "year": year, "category": "Compact Cinema",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# 1.2 Sony CineAlta
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("F65", 2011, 24.7, 13.1, "S35", "PL", 8192, 2160, "4K: 1-60; 2K: 120"),
    ("PMW-F3", 2011, 23.6, 13.3, "S35", "FZ (PL adapter)", 1920, 1080, "1080: 24,25,30; 720: 60"),
    ("PMW-F5", 2012, 24.0, 12.7, "S35", "FZ (PL adapter)", 4096, 2160, "4K: 1-60; 2K: 1-240"),
    ("PMW-F55", 2012, 24.0, 12.7, "S35", "PL", 4096, 2160, "4K: 60; 2K: 180-240"),
    ("VENICE", 2017, 36.2, 24.1, "FF", "PL/E swap", 6048, 4032, "6K: 60; 4K: 120"),
    ("VENICE 2", 2022, 36.0, 24.0, "FF", "PL/E swap", 8640, 5760, "8.2K: 60; 4K: 120"),
    ("BURANO", 2023, 36.0, 24.0, "FF", "PL/E", 8640, 5760, "8K: 30; 4K: 120"),
]:
    cameras.append({"name": name, "manufacturer": "Sony", "year": year, "category": "Digital Cinema",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# Sony Cinema Line (FX/FR)
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("FX9", 2019, 35.7, 18.8, "FF", "E-mount", 4096, 2160, "4K: 60; HD: 180"),
    ("FX6", 2020, 35.7, 18.8, "FF", "E-mount", 4096, 2160, "DCI4K: 60; UHD: 120; HD: 240"),
    ("FX3", 2021, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 120; HD: 240"),
    ("FX30", 2022, 23.5, 15.6, "APS-C", "E-mount", 3840, 2160, "4K: 120; HD: 240"),
    ("FR7", 2022, 35.6, 23.8, "FF", "E-mount", 4096, 2160, "DCI4K: 60; UHD: 120; HD: 240"),
]:
    cameras.append({"name": name, "manufacturer": "Sony", "year": year, "category": "Compact Cinema",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# Sony Camcorder Line
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("NEX-FS100", 2011, 23.6, 13.3, "S35", "E-mount", 1920, 1080, "1080: 24-60"),
    ("NEX-FS700", 2012, 24.0, 12.7, "S35", "E-mount", 4096, 2160, "1080: 24-60; burst: 240"),
    ("PXW-FS5", 2015, 23.6, 13.3, "S35", "E-mount", 3840, 2160, "4K: 24-30; HD: 960"),
    ("PXW-FS5 II", 2018, 23.6, 13.3, "S35", "E-mount", 3840, 2160, "4K: 24-30; HD: 120"),
    ("PXW-FS7", 2014, 23.6, 13.3, "S35", "E-mount", 3840, 2160, "4K: 60; HD: 180"),
    ("PXW-FS7 II", 2016, 23.6, 13.3, "S35", "E-mount", 3840, 2160, "4K: 60; HD: 180"),
]:
    cameras.append({"name": name, "manufacturer": "Sony", "year": year, "category": "Camcorder",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# Sony Alpha Mirrorless
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("A7S", 2014, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 30; 1080: 60; 720: 120"),
    ("A7S II", 2015, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 24-30; 1080: 120"),
    ("A7S III", 2020, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 120; HD: 240"),
    ("A7 III", 2018, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 24-30; 1080: 120"),
    ("A7 IV", 2021, 35.9, 23.9, "FF", "E-mount", 3840, 2160, "4K: 60; 1080: 120"),
    ("A7 V", 2025, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 60,120; HD: 240"),
    ("A7R", 2013, 35.9, 24.0, "FF", "E-mount", 1920, 1080, "1080: 24-60"),
    ("A7R II", 2015, 35.9, 24.0, "FF", "E-mount", 3840, 2160, "4K: 24-30; 720: 120"),
    ("A7R III", 2017, 35.9, 24.0, "FF", "E-mount", 3840, 2160, "4K: 24-30; 1080: 120"),
    ("A7R IV", 2019, 35.7, 23.8, "FF", "E-mount", 3840, 2160, "4K: 24-30; 1080: 120"),
    ("A7R V", 2022, 35.7, 23.8, "FF", "E-mount", 7680, 4320, "8K: 25; 4K: 60; 1080: 60"),
    ("A9", 2017, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 24-30; 1080: 120"),
    ("A9 II", 2019, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 24-30; 1080: 120"),
    ("A9 III", 2024, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 120; 1080: 120"),
    ("A1", 2021, 35.9, 24.0, "FF", "E-mount", 7680, 4320, "8K: 30; 4K: 120; HD: 240"),
    ("A1 II", 2024, 35.9, 24.0, "FF", "E-mount", 7680, 4320, "8K: 30; 4K: 120; HD: 240"),
    ("A7C", 2020, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 24-30; 1080: 120"),
    ("A7C II", 2023, 35.9, 23.9, "FF", "E-mount", 3840, 2160, "4K: 60; 1080: 120"),
    ("A7CR", 2023, 35.7, 23.8, "FF", "E-mount", 3840, 2160, "4K: 60; 1080: 120"),
    ("ZV-E1", 2023, 35.6, 23.8, "FF", "E-mount", 3840, 2160, "4K: 120; HD: 240"),
    ("ZV-E10", 2021, 23.5, 15.6, "APS-C", "E-mount", 3840, 2160, "4K: 24-30; 1080: 120"),
    ("ZV-E10 II", 2024, 23.5, 15.6, "APS-C", "E-mount", 3840, 2160, "4K: 120; 1080: 120"),
]:
    cameras.append({"name": name, "manufacturer": "Sony", "year": year, "category": "Mirrorless",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# 1.3 ARRI Digital
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("Arriflex D-20", 2005, 23.76, 13.37, "S35", "PL", 2880, 1620, "1-60"),
    ("Arriflex D-21", 2008, 23.76, 13.37, "S35", "PL (LDS)", 2880, 1620, "20-60"),
    ("ALEXA Classic", 2010, 23.76, 13.37, "S35", "PL", 2880, 1620, "0.75-120"),
    ("ALEXA Plus", 2011, 23.76, 13.37, "S35", "PL (LDS)", 2880, 1620, "0.75-120"),
    ("ALEXA Plus 4:3", 2011, 23.76, 17.82, "S35", "PL (LDS)", 2880, 2160, "0.75-120"),
    ("ALEXA Studio", 2012, 23.76, 17.82, "S35", "PL (LDS)", 2880, 2160, "0.75-60"),
    ("ALEXA M", 2012, 23.76, 17.82, "S35", "PL (LDS)", 2880, 2160, "0.75-120"),
    ("ALEXA XT", 2013, 28.25, 18.17, "S35", "PL", 3424, 2202, "0.75-120"),
    ("ALEXA XT Plus", 2013, 28.25, 18.17, "S35", "PL (LDS)", 3424, 2202, "0.75-120"),
    ("ALEXA XT Studio", 2013, 28.25, 18.17, "S35", "PL (LDS)", 3424, 2202, "0.75-96"),
    ("AMIRA", 2014, 28.25, 18.17, "S35", "PL/EF/B4", 3840, 2160, "0.75-200"),
    ("ALEXA Mini", 2015, 28.25, 18.17, "S35", "PL/LPL/EF/B4", 3840, 2160, "0.75-200"),
    ("ALEXA 65", 2015, 54.12, 25.58, "65mm", "LPL", 6560, 3100, "20-60"),
    ("ALEXA SXT Plus", 2016, 28.25, 18.17, "S35", "PL (LDS)", 3424, 2202, "0.75-120"),
    ("ALEXA SXT W", 2017, 28.25, 18.17, "S35", "PL (LDS)", 3424, 2202, "0.75-120"),
    ("ALEXA LF", 2018, 36.70, 25.54, "LF", "LPL", 4448, 3096, "90"),
    ("ALEXA Mini LF", 2019, 36.70, 25.54, "LF", "LPL/EF", 4448, 3096, "40,90"),
    ("ALEXA 35", 2022, 27.99, 19.22, "S35", "PL/LPL/EF", 4608, 3164, "0.75-120"),
    ("ALEXA 265", 2025, 54.12, 25.58, "65mm", "LPL/PL/EF", 6560, 3100, "0.75-60"),
    ("ALEXA 35 Xtreme", 2025, 27.99, 19.22, "S35", "PL/LPL/EF", 4608, 3164, "0.75-330; overdrive: 660"),
]:
    cameras.append({"name": name, "manufacturer": "ARRI", "year": year, "category": "Digital Cinema",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# 1.4 RED - sensor lookup
red_sensors = {
    "Mysterium":      (24.4, 13.7, 4520, 2540),
    "Mysterium-X":    (27.7, 14.6, 5120, 2700),
    "Dragon 19.4MP":  (30.7, 15.8, 6144, 3160),
    "Dragon 13.8MP":  (25.6, 13.5, 5120, 2700),
    "Dragon 9.9MP":   (23.0, 10.8, 4608, 2160),
    "Helium":         (29.90, 15.77, 8192, 4320),
    "Gemini":         (30.72, 18.00, 5120, 3000),
    "Monstro":        (40.96, 21.60, 8192, 4320),
    "KOMODO GS":      (27.03, 14.26, 6144, 3240),
    "KOMODO-X GS":    (27.03, 14.26, 6144, 3240),  # same sensor family
    "V-RAPTOR VV":    (40.96, 21.60, 8192, 4320),
    "V-RAPTOR S35":   (26.21, 13.82, 8192, 4320),
    "V-RAPTOR VV GS": (40.96, 21.60, 8192, 4320),
    "24.5MP FF":      (36.0, 24.0, 6144, 4080),  # Nikon ZR - FF RED sensor
}

for name, year, sensor, fmt, mount, fps_str in [
    ("RED ONE", 2007, "Mysterium", "S35", "PL", "4K: 30; 2K: 120"),
    ("RED ONE MX", 2009, "Mysterium-X", "S35", "PL", "4K: 30; 2K: 120"),
    ("EPIC-X/M", 2011, "Mysterium-X", "S35", "PL/EF/NF", "5K: 60"),
    ("Scarlet-X", 2011, "Mysterium-X", "S35", "PL/EF/NF", "4K: 30; 2K: 60"),
    ("EPIC Dragon", 2013, "Dragon 19.4MP", "S35", "PL/EF/NF", "6K: 75; 4K: 120; 2K: 240"),
    ("Scarlet Dragon", 2013, "Dragon 19.4MP", "S35", "PL/EF/NF", "5K: 48; 4K: 60; 2K: 120"),
    ("Scarlet-W 5K", 2015, "Dragon 13.8MP", "S35", "PL/EF/NF", "5K: 50; 4K: 120; 2K: 240"),
    ("WEAPON Dragon 6K", 2015, "Dragon 19.4MP", "S35", "PL/EF/NF", "6K: 75; 4K: 120; 2K: 240"),
    ("WEAPON Helium 8K", 2016, "Helium", "S35", "PL/EF/NF", "8K: 60; 4K: 120; 2K: 240"),
    ("EPIC-W Helium 8K", 2016, "Helium", "S35", "PL/EF/NF", "8K: 30; 6K: 75; 4K: 120"),
    ("Raven 4.5K", 2016, "Dragon 9.9MP", "S35", "EF", "4.5K: 120; 4K: 150; 2K: 300"),
    ("WEAPON Monstro 8K VV", 2017, "Monstro", "VV", "PL/EF/NF", "8K: 60; 4K: 120; 2K: 240"),
    ("EPIC-W Gemini 5K", 2018, "Gemini", "S35", "PL/EF/NF", "5K: 96; 4K: 120; 2K: 300"),
    ("DSMC2 Dragon-X 5K", 2018, "Dragon 13.8MP", "S35", "PL/EF/NF", "5K: 96; 4K: 120; 2K: 240"),
    ("DSMC2 Dragon-X 6K", 2018, "Dragon 19.4MP", "S35", "PL/EF/NF", "6K: 75; 4K: 120; 2K: 240"),
    ("DSMC2 Helium 8K", 2018, "Helium", "S35", "PL/EF/NF", "8K: 60; 4K: 120; 2K: 240"),
    ("DSMC2 Gemini 5K", 2018, "Gemini", "S35", "PL/EF/NF", "5K: 96; 4K: 120; 2K: 300"),
    ("DSMC2 Monstro 8K VV", 2018, "Monstro", "VV", "PL/EF/NF", "8K: 60; 4K: 120; 2K: 300"),
    ("RANGER Monstro", 2018, "Monstro", "VV", "PL/EF/NF", "8K: 60; 4K: 120"),
    ("RANGER Helium", 2019, "Helium", "S35", "PL/EF/NF", "8K: 60"),
    ("RANGER Gemini", 2019, "Gemini", "S35", "PL/EF/NF", "5K: 96"),
    ("KOMODO 6K", 2020, "KOMODO GS", "S35", "RF", "6K: 40; 4K: 60; 2K: 120"),
    ("KOMODO-X 6K", 2023, "KOMODO-X GS", "S35", "RF/Z", "6K: 80; 4K: 120; 2K: 240"),
    ("V-RAPTOR 8K VV", 2021, "V-RAPTOR VV", "VV", "RF", "8K: 120; 4K: 240; 2K: 480"),
    ("V-RAPTOR XL 8K VV", 2022, "V-RAPTOR VV", "VV", "PL/EF swap", "8K: 120; 4K: 240; 2K: 480"),
    ("V-RAPTOR 8K S35", 2022, "V-RAPTOR S35", "S35", "RF", "8K: 120; 4K: 240; 2K: 480"),
    ("V-RAPTOR [X] 8K VV", 2024, "V-RAPTOR VV GS", "VV", "RF/Z", "8K: 120; 4K: 240; 2K: 480"),
    ("V-RAPTOR XE 8K VV", 2025, "V-RAPTOR VV GS", "VV", "RF/Z", "8K: 60; 4K: 120; 2K: 240"),
    ("Nikon ZR", 2025, "24.5MP FF", "FF", "Nikon Z", "6K: 60; 4K: 120"),
]:
    sw, sh, rw, rh = red_sensors[sensor]
    cameras.append({"name": name, "manufacturer": "RED", "year": year, "category": "Digital Cinema",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [rw, rh], "supported_fps": parse_fps(fps_str)})

# 1.5 Panasonic GH Series (MFT)
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("GH1", 2009, 17.3, 13.0, "MFT", "MFT", 1920, 1080, "24; 720: 50,60"),
    ("GH2", 2010, 17.3, 13.0, "MFT", "MFT", 1920, 1080, "24,60"),
    ("GH3", 2012, 17.3, 13.0, "MFT", "MFT", 1920, 1080, "24-60"),
    ("GH4", 2014, 17.3, 13.0, "MFT", "MFT", 4096, 2160, "C4K: 24; UHD: 24-30; 1080: 96"),
    ("GH5", 2017, 17.3, 13.0, "MFT", "MFT", 4096, 2160, "C4K: 60; UHD: 60; 1080: 180"),
    ("GH5S", 2018, 17.3, 13.0, "MFT", "MFT", 4096, 2160, "C4K: 60; 1080: 240"),
    ("GH5 II", 2021, 17.3, 13.0, "MFT", "MFT", 4096, 2160, "C4K: 60; 1080: 180"),
    ("GH6", 2022, 17.3, 13.0, "MFT", "MFT", 5728, 3024, "5.7K: 60; C4K: 120; 1080: 300"),
    ("GH7", 2024, 17.3, 13.0, "MFT", "MFT", 5728, 3024, "5.7K: 60; C4K: 120; 1080: 300"),
    ("BGH1", 2020, 17.3, 13.0, "MFT", "MFT", 4096, 2160, "C4K: 60; 1080: 240"),
]:
    cameras.append({"name": name, "manufacturer": "Panasonic", "year": year, "category": "Mirrorless",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# Panasonic Lumix S Series (FF)
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("S1", 2019, 35.6, 23.8, "FF", "L-Mount", 3840, 2160, "4K: 60; 1080: 180"),
    ("S1H", 2019, 35.6, 23.8, "FF", "L-Mount", 5952, 3968, "6K: 24; 5.9K: 30; C4K: 60; 1080: 120"),
    ("S1R", 2019, 36.0, 24.0, "FF", "L-Mount", 3840, 2160, "4K: 60; 1080: 180"),
    ("S1R II", 2025, 36.0, 24.0, "FF", "L-Mount", 7680, 4320, "8K: 30; 4K: 120"),
    ("S5", 2020, 35.6, 23.8, "FF", "L-Mount", 4096, 2160, "C4K: 30; 4K: 60; 1080: 180"),
    ("S5 II", 2023, 35.6, 23.8, "FF", "L-Mount", 6000, 4000, "6K: 30; C4K: 60; 1080: 180"),
    ("S5 IIX", 2023, 35.6, 23.8, "FF", "L-Mount", 6000, 4000, "6K: 30; C4K: 60; 1080: 180"),
    ("S9", 2024, 35.6, 23.8, "FF", "L-Mount", 6000, 4000, "6K: 30; 4K: 30; 1080: 120"),
    ("BS1H", 2021, 35.6, 23.8, "FF", "L-Mount", 5952, 3968, "6K: 24; C4K: 60; 1080: 180"),
]:
    cameras.append({"name": name, "manufacturer": "Panasonic", "year": year, "category": "Mirrorless",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# Panasonic Cinema (VariCam / EVA)
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("EVA1", 2017, 24.6, 13.0, "S35", "EF", 5720, 3016, "4K: 60; 2K: 240"),
    ("VariCam 35", 2014, 23.6, 13.3, "S35", "PL", 4096, 2160, "4K: 1-120"),
    ("VariCam LT", 2016, 23.6, 13.3, "S35", "EF", 4096, 2160, "4K: 60; 2K: 240"),
]:
    cameras.append({"name": name, "manufacturer": "Panasonic", "year": year, "category": "Digital Cinema",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# 1.6 Blackmagic Design
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("Cinema Camera 2.5K", 2012, 15.6, 8.8, "S16", "EF/MFT", 2432, 1366, "24-30"),
    ("Production Camera 4K", 2014, 21.12, 11.88, "S35", "EF/PL", 4000, 2160, "24-30"),
    ("Pocket Cinema Camera", 2013, 12.48, 7.02, "S16", "MFT", 1920, 1080, "24-30"),
    ("Micro Cinema Camera", 2015, 12.48, 7.02, "S16", "MFT", 1920, 1080, "24-60"),
    ("Pocket Cinema Camera 4K", 2018, 18.96, 10.0, "MFT", "MFT", 4096, 2160, "4K: 60; 2.6K: 120"),
    ("Pocket Cinema Camera 6K", 2019, 23.10, 12.99, "S35", "EF", 6144, 3456, "6K: 50; 4K: 60; 2.8K: 120"),
    ("Pocket Cinema Camera 6K G2", 2022, 23.10, 12.99, "S35", "EF", 6144, 3456, "6K: 50; 4K: 60; 2.8K: 120"),
    ("Pocket Cinema Camera 6K Pro", 2021, 23.10, 12.99, "S35", "EF", 6144, 3456, "6K: 50; 4K: 60; 2.8K: 120"),
    ("URSA Mini 4K", 2015, 22.0, 11.88, "S35", "EF or PL", 4000, 2160, "4K: 60; 1080: 120"),
    ("URSA Mini 4.6K", 2016, 25.34, 14.25, "S35", "EF or PL", 4608, 2592, "4.6K: 60"),
    ("URSA Mini 4.6K G2", 2018, 25.34, 14.25, "S35", "EF or PL", 4608, 2592, "4.6K: 60; UHD: 120"),
    ("URSA Mini Pro 4.6K", 2017, 25.34, 14.25, "S35", "EF/PL/B4 swap", 4608, 2592, "4.6K: 60; 1080: 120"),
    ("URSA Mini Pro 4.6K G2", 2019, 25.34, 14.25, "S35", "EF/PL/B4 swap", 4608, 2592, "4.6K: 120; UHD: 150; HD: 300"),
    ("URSA Mini Pro 12K", 2020, 27.03, 14.25, "S35", "PL/EF swap", 12288, 6480, "12K: 60; 8K: 120"),
    ("URSA Cine 12K", 2024, 27.03, 14.25, "S35", "PL/EF swap", 12288, 6480, "12K: 60"),
    ("Cinema Camera 6K", 2023, 36.0, 24.0, "FF", "L-Mount", 6048, 4032, "6K: 36; 4K: 60"),
    ("PYXIS 6K", 2024, 36.0, 24.0, "FF", "L-Mount/EF", 6048, 4032, "6K: 36; 4K: 60"),
]:
    cameras.append({"name": name, "manufacturer": "Blackmagic Design", "year": year, "category": "Digital Cinema",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# 1.7 Fujifilm X Series (APS-C)
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("X-T2", 2016, 23.5, 15.6, "APS-C", "X-mount", 3840, 2160, "4K: 24-30; 1080: 60"),
    ("X-T3", 2018, 23.5, 15.6, "APS-C", "X-mount", 4096, 2160, "DCI4K: 60; 1080: 120"),
    ("X-T4", 2020, 23.5, 15.6, "APS-C", "X-mount", 4096, 2160, "DCI4K: 60; 1080: 240"),
    ("X-T5", 2022, 23.5, 15.6, "APS-C", "X-mount", 6240, 4160, "6.2K: 30; 4K: 60; 1080: 240"),
    ("X-H1", 2018, 23.5, 15.6, "APS-C", "X-mount", 4096, 2160, "DCI4K: 24; 1080: 120"),
    ("X-H2", 2022, 23.5, 15.6, "APS-C", "X-mount", 8192, 4320, "8K: 30; 4K: 60"),
    ("X-H2S", 2022, 23.5, 15.6, "APS-C", "X-mount", 6240, 4160, "6.2K: 30; 4K: 120; 1080: 240"),
    ("X-S20", 2023, 23.5, 15.6, "APS-C", "X-mount", 6240, 4160, "6.2K: 30; 4K: 60; 1080: 240"),
]:
    cameras.append({"name": name, "manufacturer": "Fujifilm", "year": year, "category": "Mirrorless",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# Fujifilm GFX (Medium Format)
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("GFX 50S", 2017, 43.8, 32.9, "MF", "G-mount", 1920, 1080, "1080: 24-30"),
    ("GFX 50R", 2018, 43.8, 32.9, "MF", "G-mount", 1920, 1080, "1080: 24-30"),
    ("GFX 50S II", 2021, 43.8, 32.9, "MF", "G-mount", 3840, 2160, "4K: 24-30; 1080: 24-30"),
    ("GFX 100", 2019, 43.8, 32.9, "MF", "G-mount", 3840, 2160, "4K: 24-30; 1080: 24-60"),
    ("GFX 100S", 2021, 43.8, 32.9, "MF", "G-mount", 3840, 2160, "4K: 24-30; 1080: 24-60"),
    ("GFX 100S II", 2024, 43.8, 32.9, "MF", "G-mount", 3840, 2160, "4K: 24-30"),
    ("GFX 100 II", 2023, 43.8, 32.9, "MF", "G-mount", 7680, 4320, "8K: 20; 4K: 60; 1080: 120"),
    ("GFX 100RF", 2025, 43.8, 32.9, "MF", "G-mount", 3840, 2160, "4K: 24-30"),
]:
    cameras.append({"name": name, "manufacturer": "Fujifilm", "year": year, "category": "Mirrorless",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# 1.8 Panavision Digital
for name, year, sw, sh, fmt, mount, res_w, res_h, fps_str in [
    ("Genesis", 2004, 23.62, 13.28, "S35", "PV", 1920, 1080, "1-50"),
    ("Millennium DXL", 2016, 40.96, 21.60, "VV", "PV", 8192, 4320, "8K: 60"),
    ("Millennium DXL2", 2018, 40.96, 21.60, "VV", "PV", 8192, 4320, "8K: 60"),
]:
    cameras.append({"name": name, "manufacturer": "Panavision", "year": year, "category": "Digital Cinema",
        "sensor_width_mm": sw, "sensor_height_mm": sh, "format": fmt, "mount": mount,
        "native_resolution": [res_w, res_h], "supported_fps": parse_fps(fps_str)})

# DXL-M - varies by sensor, use Monstro defaults (most common config)
cameras.append({"name": "DXL-M", "manufacturer": "Panavision", "year": 2018, "category": "Digital Cinema",
    "sensor_width_mm": 40.96, "sensor_height_mm": 21.60, "format": "VV", "mount": "PV",
    "native_resolution": [8192, 4320], "supported_fps": [24, 25, 30, 48, 60]})

###############################################################################
# 2. ANALOG FILM CAMERAS
###############################################################################

# ARRI Film Cameras
for name, film_fmt, gw, gh, mount, fps_str, notes in [
    ("Arriflex 35 IIC", "35mm 4-perf", 21.95, 16.00, "ARRI Bayonet", "24-25,80", "MOS, turret"),
    ("Arriflex 35 BL III+", "35mm", 24.89, 18.66, "PL", "3-50", "Sync-sound; first ARRI PL mount"),
    ("ARRI 235", "35mm 2/3/4-perf", 24.89, 18.66, "PL", "1-60", "MOS, compact"),
    ("ARRI 435", "35mm 3/4-perf", 24.89, 18.66, "PL", "1-150", "High speed"),
    ("ARRI 535", "35mm S35/Academy", 24.89, 18.66, "PL", "3-60", "Sync-sound"),
    ("ARRICAM ST", "35mm 3/4-perf", 24.89, 18.66, "PL (LDS)", "1-60", "Flagship 35mm"),
    ("ARRICAM LT", "35mm 3/4-perf", 24.89, 18.66, "PL (LDS)", "1-48", "Lighter flagship"),
    ("ARRI 765", "65mm 5-perf", 48.59, 22.04, "Maxi PL", "2-100", "65mm sync-sound"),
    ("ARRI 16SR", "16mm / S16", 12.52, 7.42, "PL", "5-75,150", "Workhorse 16mm"),
    ("ARRI 416", "Super 16", 12.52, 7.42, "PL", "1-75,150", "Latest S16"),
    ("ARRI 16BL", "16mm", 10.26, 7.49, "ARRI Bayonet", "24", "Sync-sound 16mm"),
]:
    cameras.append({"name": name, "manufacturer": "ARRI", "year": None, "category": "Film Camera",
        "sensor_width_mm": gw, "sensor_height_mm": gh, "format": film_fmt, "mount": mount,
        "native_resolution": None, "supported_fps": parse_fps(fps_str), "film_format": film_fmt})

# Panavision Film Cameras
for name, film_fmt, gw, gh, mount, fps_str, notes in [
    ("PSR / SPSR", "35mm 4-perf", 21.95, 16.00, "PV", "1-24", "Based on Mitchell BNC"),
    ("Panaflex Gold / Gold II", "35mm 4-perf", 21.95, 16.00, "PV", "4-40", "First Panaflex"),
    ("Panaflex Platinum", "35mm 4-perf", 21.95, 16.00, "PV", "4-36", "Sync"),
    ("Panaflex Lightweight II", "35mm 4-perf", 21.95, 16.00, "PV", "4-36", "14 lbs body"),
    ("Panaflex Millennium", "35mm 3/4-perf", 24.89, 18.66, "PV", "3-50", "Electronic shutter"),
    ("Panaflex Millennium XL/XL2", "35mm 3/4-perf", 24.89, 18.66, "PV", "3-50", "Steadicam-ready"),
    ("System 65 Studio", "65mm 5-perf", 48.59, 22.04, "PV 65", "4-36", "Nolan's 65mm"),
    ("System 65 High Speed", "65mm 5-perf", 48.59, 22.04, "PV 65", "1-60", "High speed 65mm"),
]:
    cameras.append({"name": name, "manufacturer": "Panavision", "year": None, "category": "Film Camera",
        "sensor_width_mm": gw, "sensor_height_mm": gh, "format": film_fmt, "mount": mount,
        "native_resolution": None, "supported_fps": parse_fps(fps_str), "film_format": film_fmt})

# IMAX Cameras
for name, film_fmt, gw, gh, mount, fps_str, notes in [
    ("IMAX MSM 9802", "65mm 15-perf horiz", 70.41, 52.63, "Hasselblad", "24", "Workhorse IMAX camera"),
    ("IMAX MKIV Reflex", "65mm 15-perf horiz", 70.41, 52.63, "Hasselblad", "24", "Heavy-duty aerial variant"),
]:
    cameras.append({"name": name, "manufacturer": "IMAX", "year": None, "category": "Film Camera",
        "sensor_width_mm": gw, "sensor_height_mm": gh, "format": film_fmt, "mount": mount,
        "native_resolution": None, "supported_fps": parse_fps(fps_str), "film_format": film_fmt})

# Aaton Film Cameras
for name, film_fmt, gw, gh, mount, fps_str, notes in [
    ("XTR Prod", "Super 16", 12.35, 7.42, "PL", "3-75", "Sync-sound"),
    ("Xtera", "Super 16", 12.52, 7.42, "PL", "3-75", "Latest S16"),
    ("A-Minima", "Super 16", 12.52, 7.42, "PL / Nikon F", "1-50", "Camcorder-sized"),
    ("35-III", "35mm 3/4-perf", 24.89, 18.66, "PL", "2-40", "Sync-sound S35"),
    ("Penelope", "35mm 2/3-perf", 22.05, 9.47, "PL", "3-40", "Native 2-perf Techniscope"),
]:
    cameras.append({"name": name, "manufacturer": "Aaton", "year": None, "category": "Film Camera",
        "sensor_width_mm": gw, "sensor_height_mm": gh, "format": film_fmt, "mount": mount,
        "native_resolution": None, "supported_fps": parse_fps(fps_str), "film_format": film_fmt})

# Bolex Film Cameras
cameras.append({"name": "Bolex H16", "manufacturer": "Bolex", "year": None, "category": "Film Camera",
    "sensor_width_mm": 10.26, "sensor_height_mm": 7.49, "format": "16mm", "mount": "C-mount",
    "native_resolution": None, "supported_fps": [12, 16, 24, 25, 32, 48, 60], "film_format": "16mm"})
cameras.append({"name": "Bolex H8", "manufacturer": "Bolex", "year": None, "category": "Film Camera",
    "sensor_width_mm": 4.88, "sensor_height_mm": 3.68, "format": "Regular 8mm", "mount": "D-mount",
    "native_resolution": None, "supported_fps": [16, 24], "film_format": "Regular 8mm"})

# Other Film Cameras
for name, film_fmt, gw, gh, mount, fps_str in [
    ("Mitchell Standard", "35mm 4-perf", 21.95, 16.00, "Mitchell Std", "24"),
    ("Mitchell BNC", "35mm 4-perf", 21.95, 16.00, "BNC", "1-24"),
    ("Mitchell BNCR", "35mm 4-perf", 21.95, 16.00, "BNCR", "1-24"),
    ("Eclair NPR", "16mm / S16", 10.26, 7.49, "Cameflex / C-mount", "4-40"),
    ("Eclair ACL", "16mm", 10.26, 7.49, "C-mount", "24-25"),
    ("Eclair Cameflex CM3", "35mm 4-perf", 21.95, 16.00, "CA mount", "8-48"),
    ("Bell & Howell Filmo 70", "16mm", 10.26, 7.49, "C-mount", "8,16,32,64"),
    ("CP-16 / 16R", "16mm", 10.26, 7.49, "CP mount", "12-36"),
    ("Moviecam Compact MK2", "35mm 3/4-perf", 24.89, 18.66, "PL", "1-50"),
    ("Konvas 1M / 2M", "35mm 4-perf", 21.95, 16.00, "Konvas bayonet", "8-32"),
]:
    mfr_map = {
        "Mitchell Standard": "Mitchell", "Mitchell BNC": "Mitchell", "Mitchell BNCR": "Mitchell",
        "Eclair NPR": "Eclair", "Eclair ACL": "Eclair", "Eclair Cameflex CM3": "Eclair",
        "Bell & Howell Filmo 70": "Bell & Howell", "CP-16 / 16R": "Cinema Products",
        "Moviecam Compact MK2": "Moviecam", "Konvas 1M / 2M": "Konvas",
    }
    cameras.append({"name": name, "manufacturer": mfr_map[name], "year": None, "category": "Film Camera",
        "sensor_width_mm": gw, "sensor_height_mm": gh, "format": film_fmt, "mount": mount,
        "native_resolution": None, "supported_fps": parse_fps(fps_str), "film_format": film_fmt})

###############################################################################
# 3. SPHERICAL CINEMA PRIME LENSES
###############################################################################

def add_primes(set_name, mount, coverage, entries, is_anamorphic=False, squeeze=1.0):
    """entries: list of (fl_mm, tstop, close_focus_m_or_None)"""
    for fl, ts, cf in entries:
        lenses.append({
            "name": f"{set_name} {fl}mm",
            "set": set_name,
            "type": "Prime",
            "focal_length_mm": fl,
            "focal_range_mm": None,
            "max_aperture_tstop": ts,
            "coverage": coverage,
            "mount": mount,
            "is_anamorphic": is_anamorphic,
            "squeeze_factor": squeeze,
            "close_focus_m": cf
        })

def add_primes_simple(set_name, mount, coverage, fls_tstops, default_cf=None, is_anamorphic=False, squeeze=1.0):
    """fls_tstops: list of (fl_mm, tstop)"""
    for fl, ts in fls_tstops:
        lenses.append({
            "name": f"{set_name} {fl}mm",
            "set": set_name,
            "type": "Prime",
            "focal_length_mm": fl,
            "focal_range_mm": None,
            "max_aperture_tstop": ts,
            "coverage": coverage,
            "mount": mount,
            "is_anamorphic": is_anamorphic,
            "squeeze_factor": squeeze,
            "close_focus_m": default_cf
        })

# 3.1 ARRI/Zeiss Master Primes
add_primes("ARRI/Zeiss Master Primes", "PL", "S35", [
    (12, 1.3, 0.40), (14, 1.3, 0.35), (16, 1.3, 0.35), (18, 1.3, 0.35),
    (21, 1.3, 0.35), (25, 1.3, 0.35), (27, 1.3, 0.35), (32, 1.3, 0.35),
    (35, 1.3, 0.35), (40, 1.3, 0.40), (50, 1.3, 0.50), (65, 1.3, 0.65),
    (75, 1.3, 0.80), (100, 1.3, 1.00), (135, 1.3, 0.95), (150, 1.3, 1.50),
])

# 3.2 ARRI/Zeiss Ultra Primes
add_primes("ARRI/Zeiss Ultra Primes", "PL", "S35", [
    (8, 2.8, 0.35), (12, 2.0, 0.30), (14, 1.9, 0.22), (16, 1.9, 0.25),
    (20, 1.9, 0.28), (24, 1.9, 0.30), (28, 1.9, 0.28), (32, 1.9, 0.35),
    (40, 1.9, 0.38), (50, 1.9, 0.60), (65, 1.9, 0.65), (85, 1.9, 0.90),
    (100, 1.9, 1.00), (135, 1.9, 1.50), (180, 1.9, 2.60),
])

# 3.3 ARRI Signature Primes
add_primes("ARRI Signature Primes", "LPL", "LF", [
    (12, 1.8, 0.35), (15, 1.8, 0.35), (18, 1.8, 0.35), (21, 1.8, 0.35),
    (25, 1.8, 0.35), (29, 1.8, 0.35), (35, 1.8, 0.35), (40, 1.8, 0.35),
    (47, 1.8, 0.45), (58, 1.8, 0.45), (75, 1.8, 0.65), (95, 1.8, 0.85),
    (125, 1.8, 1.00), (150, 1.8, 1.50), (200, 2.5, 1.80), (280, 2.8, 2.50),
])

# 3.4 Zeiss Supreme Primes
add_primes_simple("Zeiss Supreme Primes", "PL", "FF", [
    (15, 1.8), (18, 1.5), (21, 1.5), (25, 1.5), (29, 1.5), (35, 1.5),
    (40, 1.5), (50, 1.5), (65, 1.5), (85, 1.5), (100, 1.5), (135, 1.5),
    (150, 1.8), (200, 2.2),
])

# Supreme Prime Radiance (same FLs minus 15, 150, 200)
add_primes_simple("Zeiss Supreme Prime Radiance", "PL", "FF", [
    (18, 1.5), (21, 1.5), (25, 1.5), (29, 1.5), (35, 1.5),
    (40, 1.5), (50, 1.5), (65, 1.5), (85, 1.5), (100, 1.5), (135, 1.5),
])

# 3.5 Zeiss Compact Prime CP.3
add_primes_simple("Zeiss Compact Prime CP.3", "PL/EF/NF/E/MFT", "FF", [
    (15, 2.9), (18, 2.9), (21, 2.9), (25, 2.1), (28, 2.1), (35, 2.1),
    (50, 2.1), (85, 2.1), (100, 2.1), (135, 2.1),
])

# 3.6 Zeiss Super Speed MkII/III
add_primes_simple("Zeiss Super Speed", "PL", "S35", [
    (18, 1.3), (25, 1.3), (35, 1.3), (50, 1.3), (85, 1.3),
])

# 3.7 ARRI/Zeiss Standard Speed
add_primes_simple("ARRI/Zeiss Standard Speed", "PL", "S35", [
    (10, 2.1), (12, 2.1), (14, 2.1), (16, 2.1), (20, 2.1), (24, 2.1),
    (28, 2.1), (32, 2.1), (40, 2.1), (50, 2.1), (85, 2.1), (100, 2.1), (135, 2.1),
])

# 3.8 Cooke S7/i Full Frame Plus
add_primes_simple("Cooke S7/i", "PL", "FF", [
    (16, 2.0), (18, 2.0), (21, 2.0), (25, 2.0), (27, 2.0), (32, 2.0),
    (40, 2.0), (50, 2.0), (65, 2.0), (75, 2.0), (100, 2.0), (135, 2.0),
    (180, 2.0), (300, 3.3),
])

# 3.9 Cooke S4/i
add_primes_simple("Cooke S4/i", "PL", "S35", [
    (12, 2.0), (14, 2.0), (16, 2.0), (18, 2.0), (21, 2.0), (25, 2.0),
    (27, 2.0), (32, 2.0), (35, 2.0), (40, 2.0), (50, 2.0), (65, 2.0),
    (75, 2.0), (100, 2.0), (135, 2.0), (150, 2.0), (180, 2.0), (300, 2.8),
])

# 3.10 Cooke 5/i
add_primes_simple("Cooke 5/i", "PL", "S35", [
    (18, 1.4), (25, 1.4), (32, 1.4), (40, 1.4), (50, 1.4), (65, 1.4),
    (75, 1.4), (100, 1.4), (135, 1.4),
])

# 3.11 Cooke Panchro/i Classic
add_primes_simple("Cooke Panchro/i Classic", "PL", "S35", [
    (18, 2.2), (21, 2.2), (25, 2.2), (27, 2.2), (32, 2.2), (40, 2.2),
    (50, 2.2), (65, 2.2), (75, 2.2), (100, 2.2), (135, 2.2), (152, 2.2),
])

# 3.12 Cooke SP3
add_primes_simple("Cooke SP3", "E-mount", "FF", [
    (25, 2.4), (32, 2.4), (50, 2.4), (65, 2.4), (75, 2.4), (100, 2.4),
])

# 3.13 Canon CN-E Primes
add_primes_simple("Canon CN-E Primes", "EF", "FF", [
    (14, 3.1), (20, 1.5), (24, 1.5), (35, 1.5), (50, 1.3), (85, 1.3), (135, 2.2),
])

# 3.14 Canon Sumire Primes
add_primes_simple("Canon Sumire Primes", "PL", "FF", [
    (14, 3.1), (20, 1.5), (24, 1.5), (35, 1.5), (50, 1.3), (85, 1.3), (135, 2.2),
])

# 3.15 Sigma Cine FF High Speed
add_primes_simple("Sigma Cine FF High Speed", "PL/EF", "FF", [
    (14, 2.0), (20, 1.5), (24, 1.5), (35, 1.5), (50, 1.5), (85, 1.5), (105, 1.5), (135, 2.0),
])

# 3.16 Sigma Cine FF Classic Art
add_primes_simple("Sigma Cine FF Classic Art", "PL/EF", "FF", [
    (14, 2.5), (20, 2.5), (24, 2.5), (28, 2.5), (35, 2.5), (40, 2.5),
    (50, 2.5), (65, 2.5), (85, 2.5), (105, 2.5),
])

# 3.17 Leica Summilux-C
add_primes_simple("Leica Summilux-C", "PL", "LF", [
    (16, 1.4), (18, 1.4), (21, 1.4), (25, 1.4), (29, 1.4), (35, 1.4),
    (40, 1.4), (50, 1.4), (65, 1.4), (75, 1.4), (100, 1.4), (135, 1.4),
])

# 3.18 Leica Summicron-C
add_primes_simple("Leica Summicron-C", "PL", "S35", [
    (15, 2.0), (18, 2.0), (21, 2.0), (25, 2.0), (29, 2.0), (35, 2.0),
    (40, 2.0), (50, 2.0), (75, 2.0), (100, 2.0), (135, 2.0),
])

# 3.19 Leica Thalia
add_primes_simple("Leica Thalia", "PL/LPL", "LF", [
    (24, 3.6), (30, 2.6), (35, 2.2), (45, 2.6), (55, 2.8), (70, 2.6),
    (90, 2.2), (100, 2.2), (120, 2.6), (180, 3.6),
])

# 3.20 Panavision Primo Primes
add_primes_simple("Panavision Primo Primes", "PV", "S35", [
    (10, 1.9), (14.5, 1.9), (17.5, 1.9), (21, 1.9), (24, 1.9), (27, 1.9),
    (35, 1.9), (40, 1.9), (50, 1.9), (65, 1.9), (75, 1.9), (85, 1.9),
    (100, 1.9), (125, 1.9), (150, 1.9),
])

# 3.21 Panavision Primo 70
add_primes_simple("Panavision Primo 70", "PV", "LF", [
    (27, 1.9), (35, 1.9), (40, 1.9), (50, 1.9), (60, 1.9), (65, 1.9),
    (80, 1.9), (100, 1.9), (125, 1.9), (150, 1.9), (200, 1.9),
])

# 3.22 Tokina Cinema Vista
add_primes_simple("Tokina Cinema Vista", "PL", "FF", [
    (18, 1.5), (21, 1.5), (25, 1.5), (35, 1.5), (40, 1.5), (50, 1.5), (85, 1.5), (105, 1.5),
])

# 3.23 Samyang XEEN CF
add_primes_simple("Samyang XEEN CF", "PL/EF/E/MFT", "FF", [
    (16, 1.5), (24, 1.5), (35, 1.5), (50, 1.5), (85, 1.5),
])

# 3.24 DZOFilm VESPID
add_primes_simple("DZOFilm VESPID", "PL/EF", "FF", [
    (16, 2.8), (21, 2.1), (25, 2.1), (35, 2.1), (50, 2.1), (75, 2.1),
    (90, 2.1), (100, 2.1), (125, 2.1),
])

# 3.25 Tribe7 Blackwing7
add_primes_simple("Tribe7 Blackwing7", "PL", "LF", [
    (27, 1.9), (37, 1.9), (47, 1.9), (57, 1.9), (77, 1.9), (107, 1.9), (137, 1.9),
])

###############################################################################
# 4. SPHERICAL CINEMA ZOOM LENSES
###############################################################################

def add_zoom(name, range_min, range_max, tstop, coverage, mount, is_anamorphic=False, squeeze=1.0, close_focus=None):
    lenses.append({
        "name": name,
        "set": name,
        "type": "Zoom",
        "focal_length_mm": None,
        "focal_range_mm": [range_min, range_max],
        "max_aperture_tstop": tstop,
        "coverage": coverage,
        "mount": mount,
        "is_anamorphic": is_anamorphic,
        "squeeze_factor": squeeze,
        "close_focus_m": close_focus
    })

# 4.1 Angenieux
add_zoom("Angenieux Optimo 15-40mm", 15, 40, 2.6, "S35", "PL")
add_zoom("Angenieux Optimo 28-76mm", 28, 76, 2.6, "S35", "PL")
add_zoom("Angenieux Optimo 45-120mm", 45, 120, 2.8, "S35", "PL")
add_zoom("Angenieux Optimo 17-80mm", 17, 80, 2.2, "S35", "PL")
add_zoom("Angenieux Optimo 19.5-94mm", 19.5, 94, 2.6, "S35", "PL")
add_zoom("Angenieux Optimo 24-290mm", 24, 290, 2.8, "S35", "PL")
add_zoom("Angenieux Optimo Style 16-40mm", 16, 40, 2.8, "S35", "PL")
add_zoom("Angenieux Optimo Style 30-76mm", 30, 76, 2.8, "S35", "PL")
add_zoom("Angenieux Optimo Style 48-130mm", 48, 130, 3.0, "S35", "PL")
add_zoom("Angenieux Optimo Style 25-250mm", 25, 250, 3.5, "S35", "PL")
add_zoom("Angenieux Optimo Ultra Compact 21-56mm", 21, 56, 2.9, "FF", "PL")
add_zoom("Angenieux Optimo Ultra Compact 37-102mm", 37, 102, 2.9, "FF", "PL")
add_zoom("Angenieux Optimo Ultra 12x S35 24-290mm", 24, 290, 2.8, "S35", "PL")
add_zoom("Angenieux Optimo Ultra 12x FF 36-435mm", 36, 435, 4.2, "FF", "PL")
add_zoom("Angenieux Type EZ-1 S35 30-90mm", 30, 90, 2.0, "S35", "PL")
add_zoom("Angenieux Type EZ-1 FF 45-135mm", 45, 135, 3.0, "FF", "PL")
add_zoom("Angenieux Type EZ-2 S35 15-40mm", 15, 40, 2.0, "S35", "PL")
add_zoom("Angenieux Type EZ-2 FF 22-60mm", 22, 60, 3.0, "FF", "PL")

# 4.2 ARRI / Fujinon
add_zoom("ARRI/Fujinon Alura LW 15.5-45mm", 15.5, 45, 2.8, "S35", "PL")
add_zoom("ARRI/Fujinon Alura LW 30-80mm", 30, 80, 2.8, "S35", "PL")
add_zoom("ARRI/Fujinon Alura Studio 18-80mm", 18, 80, 2.6, "S35", "PL")
add_zoom("ARRI/Fujinon Alura Studio 45-250mm", 45, 250, 2.6, "S35", "PL")
add_zoom("ARRI Signature Zoom 16-32mm", 16, 32, 2.8, "LF", "LPL")
add_zoom("ARRI Signature Zoom 24-75mm", 24, 75, 2.8, "LF", "LPL")
add_zoom("ARRI Signature Zoom 45-135mm", 45, 135, 2.8, "LF", "LPL")
add_zoom("ARRI Signature Zoom 65-300mm", 65, 300, 2.8, "LF", "LPL")

# 4.3 Fujinon
add_zoom("Fujinon Premista 19-45mm", 19, 45, 2.9, "LF", "PL")
add_zoom("Fujinon Premista 28-100mm", 28, 100, 2.9, "LF", "PL")
add_zoom("Fujinon Premista 80-250mm", 80, 250, 2.9, "LF", "PL")
add_zoom("Fujinon Cabrio ZK 14.5-45mm", 14.5, 45, 2.0, "S35", "PL")
add_zoom("Fujinon Cabrio ZK 19-90mm", 19, 90, 2.9, "S35", "PL")
add_zoom("Fujinon Cabrio ZK 25-300mm", 25, 300, 3.5, "S35", "PL")
add_zoom("Fujinon Cabrio ZK 85-300mm", 85, 300, 2.9, "S35", "PL")

# 4.4 Canon
add_zoom("Canon CN-E 15.5-47mm", 15.5, 47, 2.8, "S35", "PL/EF")
add_zoom("Canon CN-E 30-105mm", 30, 105, 2.8, "S35", "PL/EF")
add_zoom("Canon CN-E Flex 14-35mm", 14, 35, 1.7, "S35", "PL/EF")
add_zoom("Canon CN-E Flex 31.5-95mm", 31.5, 95, 1.7, "S35", "PL/EF")
add_zoom("Canon CN-E Flex 20-50mm", 20, 50, 2.4, "FF", "PL/EF")
add_zoom("Canon CN-E Flex 45-135mm", 45, 135, 2.4, "FF", "PL/EF")
add_zoom("Canon CN-E 18-80mm Compact Servo", 18, 80, 4.4, "S35", "EF")
add_zoom("Canon CN-E 70-200mm Compact Servo", 70, 200, 4.4, "S35", "EF")
add_zoom("Canon CN-E 30-300mm", 30, 300, 2.95, "S35", "PL/EF")
add_zoom("Canon CINE-SERVO 17-120mm", 17, 120, 2.95, "S35", "PL/EF")
add_zoom("Canon CINE-SERVO 25-250mm", 25, 250, 2.95, "S35", "PL/EF")
add_zoom("Canon CINE-SERVO 50-1000mm", 50, 1000, 5.0, "S35", "PL")

# 4.5 Zeiss
add_zoom("Zeiss CZ.2 15-30mm", 15, 30, 2.9, "FF", "PL/EF/NF/E/MFT")
add_zoom("Zeiss CZ.2 28-80mm", 28, 80, 2.9, "FF", "PL/EF/NF/E/MFT")
add_zoom("Zeiss CZ.2 70-200mm", 70, 200, 2.9, "FF", "PL/EF/NF/E/MFT")

# 4.6 Sigma Cine
add_zoom("Sigma Cine 18-35mm T2", 18, 35, 2.0, "S35", "PL/EF/E")
add_zoom("Sigma Cine 50-100mm T2", 50, 100, 2.0, "S35", "PL/EF/E")

# 4.7 Panavision
add_zoom("Panavision Primo 19-90mm", 19, 90, 2.8, "S35", "PV")
add_zoom("Panavision Primo 24-275mm", 24, 275, 2.8, "S35", "PV")
add_zoom("Panavision Primo 70 24-70mm", 24, 70, 2.8, "LF", "PV")
add_zoom("Panavision Primo 70 70-200mm", 70, 200, 2.8, "LF", "PV")

###############################################################################
# 5. ANAMORPHIC LENSES
###############################################################################

# 5.1 ARRI Master Anamorphic
add_primes("ARRI Master Anamorphic", "PL", "S35", [
    (28, 1.9, 0.65), (35, 1.9, 0.75), (40, 1.9, 0.70), (50, 1.9, 0.75),
    (60, 1.9, 0.90), (75, 1.9, 0.90), (100, 1.9, 0.95), (135, 1.9, 1.20),
    (180, 2.8, 1.50),
], is_anamorphic=True, squeeze=2.0)

# 5.2 ARRI Rental ALFA
add_primes_simple("ARRI Rental ALFA", "LPL", "LF", [
    (40, 2.5), (47, 2.5), (60, 2.5), (72, 2.5), (90, 2.5), (108, 2.5),
    (145, 2.5), (190, 3.0),
], is_anamorphic=True, squeeze=2.0)

# 5.3 Cooke Anamorphic/i S35
add_primes("Cooke Anamorphic/i S35", "PL", "S35", [
    (25, 2.3, 0.55), (32, 2.3, 0.46), (40, 2.3, 0.50), (50, 2.3, 0.51),
    (65, 2.6, 0.14), (75, 2.3, 0.66), (100, 2.3, 0.90), (135, 2.3, 1.30),
    (180, 2.8, 1.80), (300, 3.5, 2.58),
], is_anamorphic=True, squeeze=2.0)

# 5.4 Cooke Anamorphic/i FF — 1.8x squeeze
add_primes_simple("Cooke Anamorphic/i FF", "PL/LPL", "FF", [
    (32, 2.3), (40, 2.3), (50, 2.3), (75, 2.3), (85, 2.8), (100, 2.3),
    (135, 2.3), (180, 2.9),
], is_anamorphic=True, squeeze=1.8)

# 5.5 Hawk V-Lite
add_primes_simple("Hawk V-Lite", "PL", "S35", [
    (28, 2.2), (35, 2.2), (45, 2.2), (55, 2.2), (75, 2.2), (95, 2.2),
    (135, 2.8), (180, 3.0),
], is_anamorphic=True, squeeze=2.0)

# Hawk V-Plus
add_primes_simple("Hawk V-Plus", "PL", "S35", [
    (35, 2.0), (45, 2.0), (60, 2.0), (85, 2.0), (110, 2.0),
], is_anamorphic=True, squeeze=2.0)

# Hawk V-Lite Vintage 74
add_primes_simple("Hawk V-Lite Vintage 74", "PL", "S35", [
    (35, 1.3), (45, 1.3), (55, 1.3), (75, 1.3),
], is_anamorphic=True, squeeze=2.0)

# 5.6 Atlas Mercury
add_primes_simple("Atlas Mercury", "PL/EF/E", "FF", [
    (36, 2.2), (42, 2.2), (54, 2.2), (66, 2.2), (80, 2.2), (100, 2.2),
], is_anamorphic=True, squeeze=2.0)

# Atlas Orion
add_primes_simple("Atlas Orion", "PL/EF/E", "S35", [
    (32, 2.0), (40, 2.0), (50, 2.0), (65, 2.0), (80, 2.0), (100, 2.0),
], is_anamorphic=True, squeeze=2.0)

# 5.7 Laowa Nanomorph — 1.5x squeeze
add_primes_simple("Laowa Nanomorph", "MFT/E/EF-M/X/L", "S35", [
    (27, 2.4), (35, 2.4), (50, 2.4), (65, 2.4), (80, 2.4),
], is_anamorphic=True, squeeze=1.5)

# 5.8 Laowa Proteus — 2x squeeze
add_primes_simple("Laowa Proteus", "PL", "FF", [
    (28, 2.0), (35, 2.0), (45, 2.0), (60, 2.0), (85, 2.0),
], is_anamorphic=True, squeeze=2.0)

# 5.9 P+S Technik Technovision 1.5x
add_primes_simple("P+S Technik Technovision 1.5x", "PL", "FF", [
    (25, 2.0), (35, 2.0), (40, 2.0), (50, 2.0), (70, 2.5), (90, 2.5), (100, 3.0),
], is_anamorphic=True, squeeze=1.5)

# 5.10 SIRUI — 1.6x squeeze
add_primes_simple("SIRUI 1.6x Anamorphic", "E/L/RF/X", "FF", [
    (24, 2.8), (35, 2.8), (50, 2.8), (75, 2.8),
], is_anamorphic=True, squeeze=1.6)

# 5.11 Blazar Remus 1.5x
add_primes_simple("Blazar Remus 1.5x", "E/MFT/X/L/RF", "S35", [
    (27, 1.6), (33, 1.6), (40, 1.6), (55, 1.6), (65, 1.6), (80, 1.6),
], is_anamorphic=True, squeeze=1.5)

# Blazar Cato 2x
add_primes_simple("Blazar Cato 2x", "E/MFT/X/L/RF", "S35", [
    (28, 2.8), (42, 2.8), (56, 2.8), (80, 2.8),
], is_anamorphic=True, squeeze=2.0)

# 5.12 Vazen — 1.8x squeeze, MFT
add_primes_simple("Vazen 1.8x Anamorphic", "MFT", "MFT", [
    (28, 2.2), (40, 2.0), (65, 2.0),
], is_anamorphic=True, squeeze=1.8)

# 5.13 DZOFilm Pavo — 2x squeeze
add_primes_simple("DZOFilm Pavo 2x", "PL/EF", "S35", [
    (28, 2.1), (32, 2.1), (40, 2.1), (55, 2.1), (75, 2.1), (100, 2.1),
], is_anamorphic=True, squeeze=2.0)

# 5.14 Angenieux Anamorphic Zooms
add_zoom("Angenieux Optimo Anamorphic 30-72mm", 30, 72, 4.0, "S35", "PL", is_anamorphic=True, squeeze=2.0)
add_zoom("Angenieux Optimo Anamorphic 56-152mm", 56, 152, 4.0, "S35", "PL", is_anamorphic=True, squeeze=2.0)
add_zoom("Angenieux Optimo Anamorphic 44-440mm", 44, 440, 4.5, "S35", "PL", is_anamorphic=True, squeeze=2.0)

# 5.15 Cooke Anamorphic/i Zoom
add_zoom("Cooke Anamorphic/i Zoom 35-140mm", 35, 140, 3.1, "S35", "PL", is_anamorphic=True, squeeze=2.0)

###############################################################################
# 6. STILLS LENSES
###############################################################################

def add_stills_prime(name, set_name, aperture, close_focus, coverage, mount):
    import re
    fl_match = re.search(r'(\d+(?:\.\d+)?)\s*mm', name)
    fl = float(fl_match.group(1)) if fl_match else None
    lenses.append({
        "name": name,
        "set": set_name,
        "type": "Prime",
        "focal_length_mm": fl,
        "focal_range_mm": None,
        "max_aperture_tstop": aperture,
        "coverage": coverage,
        "mount": mount,
        "is_anamorphic": False,
        "squeeze_factor": 1.0,
        "close_focus_m": close_focus,
        "is_stills_lens": True
    })

def add_stills_zoom(name, set_name, range_min, range_max, aperture, coverage, mount, close_focus=None):
    lenses.append({
        "name": name,
        "set": set_name,
        "type": "Zoom",
        "focal_length_mm": None,
        "focal_range_mm": [range_min, range_max],
        "max_aperture_tstop": aperture,
        "coverage": coverage,
        "mount": mount,
        "is_anamorphic": False,
        "squeeze_factor": 1.0,
        "close_focus_m": close_focus,
        "is_stills_lens": True
    })

# 6.1 Canon EF L Primes
for nm, ap, cf in [
    ("Canon EF 14mm f/2.8L II", 2.8, 0.20),
    ("Canon EF 24mm f/1.4L II", 1.4, 0.25),
    ("Canon EF 35mm f/1.4L II", 1.4, 0.28),
    ("Canon EF 50mm f/1.2L", 1.2, 0.45),
    ("Canon EF 85mm f/1.2L II", 1.2, 0.95),
    ("Canon EF 100mm f/2.8L Macro IS", 2.8, 0.30),
    ("Canon EF 135mm f/2L", 2.0, 0.90),
]:
    add_stills_prime(nm, "Canon EF L Primes", ap, cf, "FF", "EF")

# 6.2 Canon RF L Primes
for nm, ap, cf in [
    ("Canon RF 14mm f/1.4L VCM", 1.4, 0.24),
    ("Canon RF 24mm f/1.4L VCM", 1.4, 0.24),
    ("Canon RF 35mm f/1.4L VCM", 1.4, 0.30),
    ("Canon RF 50mm f/1.2L USM", 1.2, 0.40),
    ("Canon RF 50mm f/1.4L VCM", 1.4, 0.40),
    ("Canon RF 85mm f/1.2L USM", 1.2, 0.85),
    ("Canon RF 85mm f/1.2L USM DS", 1.2, 0.85),
    ("Canon RF 100mm f/2.8L Macro IS", 2.8, 0.26),
    ("Canon RF 135mm f/1.8L IS", 1.8, 0.70),
]:
    add_stills_prime(nm, "Canon RF L Primes", ap, cf, "FF", "RF")

# 6.3 Sony G Master Primes
for nm, ap, cf in [
    ("Sony FE 14mm f/1.8 GM", 1.8, 0.25),
    ("Sony FE 24mm f/1.4 GM", 1.4, 0.24),
    ("Sony FE 35mm f/1.4 GM", 1.4, 0.27),
    ("Sony FE 50mm f/1.2 GM", 1.2, 0.40),
    ("Sony FE 85mm f/1.4 GM II", 1.4, 0.85),
    ("Sony FE 135mm f/1.8 GM", 1.8, 0.70),
]:
    add_stills_prime(nm, "Sony G Master Primes", ap, cf, "FF", "E")

# 6.4 Sigma Art DG DN Primes
for nm, ap, cf in [
    ("Sigma 14mm f/1.4 DG DN Art", 1.4, 0.30),
    ("Sigma 20mm f/1.4 DG DN Art", 1.4, 0.20),
    ("Sigma 24mm f/1.4 DG DN Art", 1.4, 0.25),
    ("Sigma 35mm f/1.4 DG DN Art", 1.4, 0.30),
    ("Sigma 50mm f/1.4 DG DN Art", 1.4, 0.45),
    ("Sigma 85mm f/1.4 DG DN Art", 1.4, 0.77),
    ("Sigma 135mm f/1.4 DG Art", 1.4, 1.10),
]:
    add_stills_prime(nm, "Sigma Art DG DN Primes", ap, cf, "FF", "E/L")

# 6.5 Key Stills Zoom Sets
add_stills_zoom("Canon EF 16-35mm f/2.8L III", "Canon EF L Zooms", 16, 35, 2.8, "FF", "EF")
add_stills_zoom("Canon EF 24-70mm f/2.8L II", "Canon EF L Zooms", 24, 70, 2.8, "FF", "EF")
add_stills_zoom("Canon EF 70-200mm f/2.8L IS III", "Canon EF L Zooms", 70, 200, 2.8, "FF", "EF")
add_stills_zoom("Canon RF 15-35mm f/2.8L IS", "Canon RF L Zooms", 15, 35, 2.8, "FF", "RF")
add_stills_zoom("Canon RF 24-70mm f/2.8L IS", "Canon RF L Zooms", 24, 70, 2.8, "FF", "RF")
add_stills_zoom("Canon RF 28-70mm f/2L", "Canon RF L Zooms", 28, 70, 2.0, "FF", "RF")
add_stills_zoom("Canon RF 70-200mm f/2.8L IS", "Canon RF L Zooms", 70, 200, 2.8, "FF", "RF")
add_stills_zoom("Sony FE 16-35mm f/2.8 GM II", "Sony G Master Zooms", 16, 35, 2.8, "FF", "E")
add_stills_zoom("Sony FE 24-70mm f/2.8 GM II", "Sony G Master Zooms", 24, 70, 2.8, "FF", "E")
add_stills_zoom("Sony FE 70-200mm f/2.8 GM OSS II", "Sony G Master Zooms", 70, 200, 2.8, "FF", "E")
add_stills_zoom("Sigma 18-35mm f/1.8 DC Art", "Sigma Art Zooms", 18, 35, 1.8, "APS-C", "EF/E/L")
add_stills_zoom("Sigma 24-70mm f/2.8 DG DN Art", "Sigma Art Zooms", 24, 70, 2.8, "FF", "E/L")

# 6.6 Popular Vintage/Adapted Lenses
for nm, set_nm, ap, cf, mount_str in [
    ("Helios 44-2 58mm f/2", "Helios", 2.0, 0.50, "M42"),
    ("Super Takumar 50mm f/1.4", "Super Takumar", 1.4, 0.45, "M42"),
    ("Jupiter-37A 135mm f/3.5", "Jupiter", 3.5, 1.20, "M42"),
    ("Canon FD 50mm f/1.4 SSC", "Canon FD", 1.4, 0.45, "FD"),
    ("Canon FD 85mm f/1.2", "Canon FD", 1.2, 0.90, "FD"),
    ("Nikon AI-S 50mm f/1.2", "Nikon AI-S", 1.2, 0.50, "F"),
    ("Nikon AI-S 35mm f/1.4", "Nikon AI-S", 1.4, 0.30, "F"),
    ("Minolta MC Rokkor 58mm f/1.2", "Minolta MC Rokkor", 1.2, 0.57, "SR"),
    ("Voigtlander Nokton 35mm f/1.2", "Voigtlander Nokton", 1.2, 0.30, "E-mount"),
]:
    add_stills_prime(nm, set_nm, ap, cf, "FF", mount_str)

###############################################################################
# 7. REFERENCE TABLES
###############################################################################

data["reference"]["film_formats"] = [
    {"format": "Regular 8mm", "gate_width_mm": 4.88, "gate_height_mm": 3.68, "aspect_ratio": "1.33:1", "notes": "Double-run 8mm"},
    {"format": "Super 8mm", "gate_width_mm": 5.63, "gate_height_mm": 4.22, "aspect_ratio": "1.33:1", "notes": "Larger gate than Reg 8"},
    {"format": "16mm Standard", "gate_width_mm": 10.26, "gate_height_mm": 7.49, "aspect_ratio": "1.37:1", "notes": "Single or double perf"},
    {"format": "Super 16mm", "gate_width_mm": 12.52, "gate_height_mm": 7.42, "aspect_ratio": "1.69:1", "notes": "Extended into soundtrack area"},
    {"format": "35mm Academy (4-perf)", "gate_width_mm": 21.95, "gate_height_mm": 16.00, "aspect_ratio": "1.375:1", "notes": "SMPTE standard"},
    {"format": "35mm Full Silent (4-perf)", "gate_width_mm": 24.89, "gate_height_mm": 18.67, "aspect_ratio": "1.33:1", "notes": "No soundtrack area"},
    {"format": "35mm 3-perf", "gate_width_mm": 24.89, "gate_height_mm": 13.90, "aspect_ratio": "1.79:1", "notes": "~16:9 native"},
    {"format": "35mm 2-perf (Techniscope)", "gate_width_mm": 22.05, "gate_height_mm": 9.47, "aspect_ratio": "2.33:1", "notes": "Half-height Academy"},
    {"format": "Super 35mm (4-perf)", "gate_width_mm": 24.89, "gate_height_mm": 18.66, "aspect_ratio": "1.33:1", "notes": "Full silent aperture"},
    {"format": "Super 35mm (3-perf)", "gate_width_mm": 24.89, "gate_height_mm": 13.90, "aspect_ratio": "1.79:1", "notes": "3-perf height"},
    {"format": "VistaVision (8-perf horiz)", "gate_width_mm": 37.72, "gate_height_mm": 24.92, "aspect_ratio": "1.51:1", "notes": "35mm run horizontally"},
    {"format": "65mm 5-perf", "gate_width_mm": 48.59, "gate_height_mm": 22.04, "aspect_ratio": "2.20:1", "notes": "Camera negative"},
    {"format": "IMAX 15-perf 65mm", "gate_width_mm": 70.41, "gate_height_mm": 52.63, "aspect_ratio": "1.34:1", "notes": "65mm run horizontally"},
]

data["reference"]["sensor_formats"] = [
    {"label": "Super 16", "approx_width_mm": "12-13", "image_circle_mm": 15, "digital_examples": "BM Pocket (orig), BM Micro", "film_equivalent": "Super 16mm film"},
    {"label": "Micro Four Thirds", "approx_width_mm": "17-19", "image_circle_mm": 22, "digital_examples": "BM Pocket 4K, Panasonic GH series", "film_equivalent": None},
    {"label": "APS-C", "approx_width_mm": "22-24", "image_circle_mm": 28, "digital_examples": "Canon R7, Sony FX30, Fuji X-H2S", "film_equivalent": None},
    {"label": "Super 35", "approx_width_mm": "23-28", "image_circle_mm": 32, "digital_examples": "ARRI Alexa 35, RED KOMODO, Canon C300", "film_equivalent": "Super 35mm film"},
    {"label": "Full Frame", "approx_width_mm": "35-36", "image_circle_mm": 43, "digital_examples": "Sony VENICE, Canon R5, ARRI Alexa LF", "film_equivalent": "VistaVision"},
    {"label": "Large Format", "approx_width_mm": "36-41", "image_circle_mm": 46, "digital_examples": "RED Monstro, ARRI Mini LF", "film_equivalent": "VistaVision+"},
    {"label": "VistaVision", "approx_width_mm": "40-41", "image_circle_mm": 46, "digital_examples": "RED V-RAPTOR VV, Panavision DXL2", "film_equivalent": "VistaVision film"},
    {"label": "65mm", "approx_width_mm": "48-54", "image_circle_mm": 60, "digital_examples": "ARRI 65, ARRI 265", "film_equivalent": "65mm 5-perf film"},
    {"label": "Medium Format", "approx_width_mm": "43-44", "image_circle_mm": 55, "digital_examples": "Fujifilm GFX series", "film_equivalent": None},
    {"label": "IMAX", "approx_width_mm": "70", "image_circle_mm": None, "digital_examples": None, "film_equivalent": "IMAX 15-perf 65mm"},
]

data["reference"]["focal_length_guide"] = [
    {"focal_length_mm": "12-14", "category": "Ultra-wide", "typical_use": "Extreme establishing, distortion effect"},
    {"focal_length_mm": "16-18", "category": "Wide", "typical_use": "Interior wide shots, dynamic movement"},
    {"focal_length_mm": "21", "category": "Wide", "typical_use": "Standard wide, room establishing"},
    {"focal_length_mm": "24-25", "category": "Moderate wide", "typical_use": "Versatile wide, documentary"},
    {"focal_length_mm": "28", "category": "Normal-wide", "typical_use": "Walk-and-talk, slight wide"},
    {"focal_length_mm": "32-35", "category": "Normal", "typical_use": "Standard narrative, naturalistic"},
    {"focal_length_mm": "40", "category": "Normal", "typical_use": "Slightly tight normal"},
    {"focal_length_mm": "50", "category": "Standard", "typical_use": "Classic 'normal' lens, closest to human eye"},
    {"focal_length_mm": "65-75", "category": "Short telephoto", "typical_use": "Portraits, medium close-ups"},
    {"focal_length_mm": "85", "category": "Portrait telephoto", "typical_use": "Classic portrait focal length"},
    {"focal_length_mm": "100", "category": "Medium telephoto", "typical_use": "Tight portraits, product"},
    {"focal_length_mm": "135", "category": "Telephoto", "typical_use": "Compressed backgrounds, intimate close-ups"},
    {"focal_length_mm": "180-200", "category": "Long telephoto", "typical_use": "Extreme compression, distant subjects"},
]

data["reference"]["mount_compatibility"] = [
    {"mount": "PL", "ffd_mm": 52, "diameter_mm": 54, "cameras": "ARRI, RED (DSMC1/2), Sony CineAlta, BMD URSA", "adapts_to": "Short FFD cameras via adapter"},
    {"mount": "LPL", "ffd_mm": 44, "diameter_mm": 62, "cameras": "ARRI LF/Mini LF/35/265, RED (XL)", "adapts_to": "PL cameras via adapter"},
    {"mount": "PV", "ffd_mm": 57.15, "diameter_mm": None, "cameras": "Panavision cameras", "adapts_to": "Rental only"},
    {"mount": "EF", "ffd_mm": 44, "diameter_mm": 54, "cameras": "Canon DSLRs, RED (DSMC2), BMD BMPCC6K", "adapts_to": "RF, E, MFT via adapter"},
    {"mount": "RF", "ffd_mm": 20, "diameter_mm": 54, "cameras": "Canon R series, RED DSMC3", "adapts_to": "Native, very short FFD"},
    {"mount": "E-mount", "ffd_mm": 18, "diameter_mm": 46.1, "cameras": "All Sony mirrorless, Sony FX series", "adapts_to": "Accepts PL, EF, NF via adapter"},
    {"mount": "L-Mount", "ffd_mm": 20, "diameter_mm": 51.6, "cameras": "Panasonic S series, BMD Cinema Camera 6K/PYXIS", "adapts_to": None},
    {"mount": "Nikon Z", "ffd_mm": 16, "diameter_mm": 55, "cameras": "Nikon Z series, RED (2025+), Nikon ZR", "adapts_to": "Accepts F, PL, EF via adapter"},
    {"mount": "Nikon F", "ffd_mm": 46.5, "diameter_mm": 44, "cameras": "Nikon DSLRs, RED (DSMC1/2)", "adapts_to": "Z, E via adapter"},
    {"mount": "MFT", "ffd_mm": 19.25, "diameter_mm": 38, "cameras": "Panasonic GH, BMD Pocket 4K", "adapts_to": None},
    {"mount": "X-mount", "ffd_mm": 17.7, "diameter_mm": 43.5, "cameras": "Fujifilm X series", "adapts_to": None},
    {"mount": "G-mount", "ffd_mm": 26.7, "diameter_mm": 65, "cameras": "Fujifilm GFX series", "adapts_to": None},
    {"mount": "Maxi PL", "ffd_mm": 73.5, "diameter_mm": 64, "cameras": "ARRI 765", "adapts_to": "65mm only"},
]

data["reference"]["anamorphic_squeeze_factors"] = [
    {"squeeze": 1.33, "desqueezed_on_178": "2.37:1", "desqueezed_on_190": "2.53:1", "typical_use": "S35 / FF, moderate widescreen"},
    {"squeeze": 1.5, "desqueezed_on_178": "2.67:1 (masked to 2.39)", "desqueezed_on_190": "2.85:1 (masked to 2.39)", "typical_use": "Full frame, moderate anamorphic"},
    {"squeeze": 1.8, "desqueezed_on_178": "3.20:1 (masked to 2.39)", "desqueezed_on_190": "3.42:1 (masked to 2.39)", "typical_use": "Full frame (Cooke FF)"},
    {"squeeze": 2.0, "desqueezed_on_178": "3.56:1 (masked to 2.39)", "desqueezed_on_190": "3.80:1 (masked to 2.39)", "typical_use": "Classic CinemaScope"},
]

###############################################################################
# Write JSON
###############################################################################

output_path = "/Users/davidgrant/Code/projects/fram3d/reference data/camera-lens-database.json"
with open(output_path, "w") as f:
    json.dump(data, f, indent=2, ensure_ascii=False)

print(f"Written {len(cameras)} cameras and {len(lenses)} lenses to {output_path}")
