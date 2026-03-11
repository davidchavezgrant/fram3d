# Raw Research Findings: RED Digital Cinema Cameras -- Complete Technical Specifications

## Queries Executed
1. "RED digital cinema cameras complete list all models specifications" - 5 useful results
2. "RED camera sensor dimensions mm Mysterium Dragon Helium Monstro Gemini specifications" - 4 useful results
3. "RED ONE EPIC Scarlet WEAPON camera specifications sensor size resolution" - 4 useful results
4. "RED KOMODO V-RAPTOR RHINO DSMC2 specifications 2025 2026" - 4 useful results
5. "RED camera sensor size comparison chart all models complete" - 3 useful results
6. "RED ONE original Mysterium sensor size 24.4 13.7 specifications" - 3 useful results
7. "RED DSMC2 Dragon-X 5K sensor dimensions specifications" - 4 useful results
8. "RED DSMC2 Gemini 5K sensor dimensions 30.72 18mm" - 3 useful results
9. "RED KOMODO-X 6K specifications sensor size frame rates" - 4 useful results
10. "RED V-RAPTOR 8K VV sensor dimensions specifications" - 4 useful results
11. "RED Raven 4.5K sensor size specifications" - 3 useful results
12. "RED V-RAPTOR 8K S35 sensor dimensions Rhino" - 4 useful results
13. "RED RANGER Monstro Gemini Helium specifications" - 3 useful results
14. "RED EPIC-W Helium 8K S35 specifications" - 4 useful results
15. "RED Scarlet-X Mysterium-X specifications" - 3 useful results
16. "RED V-RAPTOR [X] 8K VV global shutter specifications" - 4 useful results
17. "RED V-RAPTOR XE specifications 2025" - 3 useful results
18. "RED WEAPON Dragon 6K specifications" - 3 useful results
19. "RED Scarlet Dragon specifications" - 3 useful results
20. "Nikon ZR camera RED specifications 2025" - 3 useful results

---

## Camera Platform Overview

RED cameras are organized into platform generations:
- **DSMC1** (Digital Still and Motion Camera, Gen 1): RED ONE, EPIC, Scarlet (original bodies)
- **DSMC2** (Gen 2): WEAPON, EPIC-W, Scarlet-W, Raven, unified DSMC2 bodies, RANGER
- **DSMC3** (Gen 3): KOMODO, KOMODO-X, V-RAPTOR family

### Lens Mount Notes (applies across platforms)

- **DSMC1** (RED ONE, EPIC, Scarlet): Native **PL mount**. Interchangeable mounts available: Canon EF (Al/Ti), Nikon F (Al/Ti), Leica M (Al). Adapters available for PL-to-other.
- **DSMC2** (WEAPON, EPIC-W, Scarlet-W, Raven, unified DSMC2, RANGER): Interchangeable mounts: **PL** (Mg/Ti, captive and non-captive), **Canon EF** (Al/Ti), **Nikon F** (Al/Ti), **Leica M** (Al). Raven shipped with fixed Canon EF mount.
- **DSMC3** (KOMODO, KOMODO-X, V-RAPTOR, V-RAPTOR XE): Fixed **Canon RF mount** or **Nikon Z mount** (Z-mount versions introduced 2025). V-RAPTOR XL variants use interchangeable mounts including PL (with Cooke /i and Zeiss eXtended Data support).
- All RF/Z mount cameras support PL and EF lenses via adapters.

---

## Findings by Camera

---

### Finding 1: RED ONE (Mysterium) -- 2007

- **Confidence**: HIGH
- **Sensor**: Mysterium 12 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 24.4 x 13.7 mm
- **Sensor Format**: Super 35mm
- **Native Resolution**: 4K+ (4520 x 2540 active pixel array; full array 4900 x 2580)
- **Supported Resolutions**: 4K+ (4520x2540), 4K (4096x2304), 3K, 2K (2048x1080), 1080p, 1080i, 720p
- **Native Aspect Ratio**: ~1.78:1 (16:9) at 4K HD; sensor is natively wider at full active area
- **Frame Rates**:
  - 4K: up to 30 fps
  - 3K: higher fps (exact not specified in sources)
  - 2K: up to 120 fps
  - 720p: up to 120 fps
- **Lens Mount**: PL mount (native). Adapters available for Nikon F, Canon EF, B4
- **Dynamic Range**: >66 dB (~11 stops)
- **Recording**: REDCODE RAW (.R3D) only
- **Weight**: 10 lbs body only
- **Supporting Sources**:
  - [Film School Online - RED ONE Camera](https://filmschoolonline.com/info/red_one_camera.htm) - sensor 24.4mm x 13.7mm, 12MP Mysterium, 4096 lines
  - [Dependent Media - Camera Technical](https://dependentmedia.com/camera-technical/) - 4520x2540 active array, frame rates
  - [VFX Camera Database - RED ONE MX](https://vfxcamdb.com/red-one-mx/) - format-dependent crop areas
- **Notes**: First RED camera ever produced. The original Mysterium sensor was later superseded by the Mysterium-X upgrade.

---

### Finding 2: RED ONE MX (Mysterium-X upgrade) -- 2009

- **Confidence**: HIGH
- **Sensor**: Mysterium-X 14 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: The physical sensor is the same ~30 x 15 mm silicon as the EPIC MX, but the RED ONE body uses a smaller active recording area. Maximum active area in the RED ONE MX body depends on format:
  - 4.5K WS: 24.19 x 10.37 mm (4480 x 1920)
  - 4K HD: 22.12 x 12.44 mm (4096 x 2304)
  - 4K 2:1: 22.12 x 11.06 mm (4096 x 2048)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 4.5K widescreen (4480 x 1920) max; 4K HD (4096 x 2304) most common
- **Supported Resolutions**: 4.5K WS, 4K (multiple aspect ratios), 3K, 2K
- **Native Aspect Ratio**: Multiple depending on format (2.33:1 at 4.5K WS, 16:9 at 4K HD, 2:1 at 4K 2:1)
- **Frame Rates**:
  - 4K: up to 30 fps (same limitation as original RED ONE body)
  - 2K: up to 120 fps
- **Lens Mount**: PL mount (native). Adapters for Nikon F, Canon EF
- **Dynamic Range**: Improved over original Mysterium
- **Recording**: REDCODE RAW (.R3D), 12-bit
- **Supporting Sources**:
  - [VFX Camera Database - RED ONE MX](https://vfxcamdb.com/red-one-mx/) - format-dependent sensor crops with exact mm
  - [REDUSER.NET](https://reduser.net/threads/is-the-red-one-mysterium-x-sensor-size-actually-larger-than-the-red-epic-mysterium-x.170931/) - same 30x15mm sensor as EPIC MX but different active areas
- **Notes**: The Mysterium-X was an upgrade path for existing RED ONE bodies. The RED ONE body architecture limited the usable resolution and frame rates compared to the later EPIC body with the same sensor.

---

### Finding 3: RED EPIC-X / EPIC-M (Mysterium-X) -- 2011

- **Confidence**: HIGH
- **Sensor**: Mysterium-X 14 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 27.7 x 14.6 mm (31.4 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 5K (5120 x 2700)
- **Supported Resolutions**: 5K (5120x2700), 4K, 3K, 2K
- **Native Aspect Ratio**: 17:9 (at 5K full format)
- **Frame Rates**: 23.98, 24, 25, 29.97, 47.95, 48, 50, 59.94 fps at all resolutions
- **Lens Mount**: Interchangeable -- PL, Canon EF, Nikon F, Leica M
- **Dynamic Range**: 13.5 stops (up to 18 stops with HDRx)
- **Weight**: 5 lbs (BRAIN only)
- **Recording**: REDCODE RAW; delivery via REDCINE-X PRO to DPX, TIFF, OpenEXR, QuickTime, H.264
- **Supporting Sources**:
  - [RED Official Docs - EPIC MX Specs](https://docs.red.com/955-0156/EPICSCARLETOperationGuide/en-us/Content/A_TechSpecs/Specs_EPIC_MX.htm) - 27.7x14.6mm, 5120x2700, 14MP, 13.5 stops
  - [AbelCine](https://www.abelcine.com/rent/cameras-accessories/red-epic-mysterium-x-5k-digital-cinema-camera-pl-mount) - PL mount rental listing
- **Notes**: EPIC-X = base model, EPIC-M = "Meizler" monochrome variant. Same sensor/body design. DSMC1 platform.

---

### Finding 4: RED Scarlet-X (Mysterium-X) -- 2011

- **Confidence**: HIGH
- **Sensor**: Mysterium-X 14 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 27.7 x 14.6 mm (31.4 mm diagonal) -- same sensor as EPIC MX
- **Sensor Format**: Super 35mm
- **Native Resolution**: 5K (5120 x 2700) -- but limited by processing
- **Supported Resolutions**: 5K (limited to 12 fps burst), 4K, 3K, 2K, 1K
- **Native Aspect Ratio**: 17:9 (at full format)
- **Frame Rates**:
  - 5K: 12 fps (burst mode only)
  - 4K / 4K HD 16:9: 24, 30 fps
  - 3K: up to 48 fps
  - 2K: up to 60 fps
  - 1K: up to 120 fps
- **Lens Mount**: Interchangeable -- PL, Canon EF, Nikon F, Leica M
- **Dynamic Range**: 13.5 stops (up to 18 stops with HDRx)
- **Weight**: 5 lbs (BRAIN only)
- **Supporting Sources**:
  - [RED Official Docs - Scarlet MX Specs](https://docs.red.com/955-0156/EPICSCARLETOperationGuide/en-us/Content/A_TechSpecs/Specs_SCARLET_MX.htm) - 27.7x14.6mm, 5120x2700, same sensor
  - [TV Tech Review](https://www.tvtechnology.com/equipment/the-red-scarlet-x-4k-camera-review) - 4K at 24/30fps
- **Notes**: Budget-oriented sibling to EPIC. Same sensor but with significant frame rate and processing limitations. DSMC1 platform.

---

### Finding 5: RED EPIC Dragon -- 2013

- **Confidence**: HIGH
- **Sensor**: RED Dragon 19.4 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 30.7 x 15.8 mm (34.5 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 6K (6144 x 3160)
- **Supported Resolutions**: 6K, 5K, 4K, 3K, 2K (each with full format and 2.4:1 options)
- **Native Aspect Ratio**: ~17:9 (at 6K full format)
- **Frame Rates**:
  - 6K FF: 75 fps; 6K 2.4:1: 100 fps
  - 5K FF: 96 fps; 5K 2.4:1: 120 fps
  - 4K FF: 120 fps; 4K 2.4:1: 150 fps
  - 3K FF: 150 fps; 3K 2.4:1: 200 fps
  - 2K FF: 240 fps; 2K 2.4:1: 300 fps
- **Lens Mount**: Interchangeable -- PL, Canon EF, Nikon F, Leica M
- **Dynamic Range**: 16.5+ stops
- **Data Rate**: Up to 200 MB/s (RED MINI-MAG)
- **Weight**: 5 lbs (BRAIN only)
- **Supporting Sources**:
  - [RED Official Docs - EPIC Dragon Specs](https://docs.red.com/955-0156/EPICSCARLETOperationGuide/en-us/Content/A_TechSpecs/Specs_EPIC_DRAGON.htm) - 30.7x15.8mm, 6144x3160, 19.4MP, 16.5+ stops, all frame rates
  - [No Film School](https://nofilmschool.com/2013/04/red-epic-dragon-scarlet-specs-upgrade-price-cost-warranty) - Dragon upgrade announcement
- **Notes**: Available as upgrade to existing EPIC bodies. Significant improvement in DR (13.5 to 16.5 stops) and resolution (5K to 6K). DSMC1 platform.

---

### Finding 6: RED Scarlet Dragon -- 2013

- **Confidence**: HIGH
- **Sensor**: RED Dragon 19.4 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 30.7 x 15.8 mm (34.5 mm diagonal) -- same sensor as EPIC Dragon
- **Sensor Format**: Super 35mm
- **Native Resolution**: 6K (6144 x 3160) -- but heavily frame-rate limited at 6K
- **Supported Resolutions**: 6K (burst only), 5K, 4K, 3K, 2K
- **Native Aspect Ratio**: ~17:9 (at full format)
- **Frame Rates**:
  - 6K FF: 12 fps (burst mode only)
  - 5K FF: 48 fps; 5K 2.4:1: 60 fps
  - 4K FF: 60 fps; 4K 2.4:1: 75 fps
  - 3K FF: 75 fps; 3K 2.4:1: 100 fps
  - 2K FF: 120 fps; 2K 2.4:1: 150 fps
- **Lens Mount**: Interchangeable -- PL, Canon EF, Nikon F, Leica M
- **Dynamic Range**: 16.5+ stops
- **Data Rate**: Up to 72 MB/s
- **Weight**: 5 lbs (BRAIN only)
- **Supporting Sources**:
  - [RED Official Docs - Scarlet Dragon Specs](https://docs.red.com/955-0156/EPICSCARLETOperationGuide/en-us/Content/A_TechSpecs/Specs_SCARLET_DRAGON.htm) - 30.7x15.8mm, all frame rates, 72 MB/s max
- **Notes**: Same Dragon sensor as EPIC Dragon but with Scarlet processing limitations. Much lower data rate ceiling (72 vs 200 MB/s). DSMC1 platform.

---

### Finding 7: RED Raven 4.5K -- 2016

- **Confidence**: HIGH
- **Sensor**: RED Dragon 9.9 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 23.0 x 10.8 mm (25.5 mm diagonal)
- **Sensor Format**: Sub-Super 35 (closer to APS-C / Academy 35mm)
- **Native Resolution**: 4.5K (4608 x 2160)
- **Supported Resolutions**: 4.5K, 4K, 3K, 2K (multiple aspect ratios: FF, 2.4:1, 2:1, UHD 16:9, 3:2)
- **Native Aspect Ratio**: ~2.13:1 (at 4.5K full format 4608x2160)
- **Frame Rates**:
  - 4.5K: up to 120 fps
  - 4K: up to 150 fps
  - 3K: up to 200 fps
  - 2K: up to 300 fps
- **Lens Mount**: Fixed Canon EF mount (compatible with DSMC2 accessories)
- **Dynamic Range**: 16.5+ stops
- **Data Rate**: Up to 140 MB/s (RED MINI-MAG)
- **Weight**: 3.5 lbs (BRAIN with Integrated Media Bay and Canon Lens Mount)
- **Supporting Sources**:
  - [RED Official Docs - RAVEN Specs](https://docs.red.com/955-0154/REDRAVENOperationGuide/en-us/Content/A_TechSpecs/Specs_RAVEN.htm) - 23.0x10.8mm, 4608x2160, 9.9MP, all frame rates
  - [B&H Photo](https://www.bhphotovideo.com/c/product/1359258-REG/red_digital_cinema_710_0223_red_raven_4_5k_dsmc2.html) - EF mount, DSMC2 compatible
- **Notes**: Entry-level RED. Smallest sensor in the RED lineup. Uses a cut-down Dragon sensor. Initially sold exclusively through Apple stores and RED.com. DSMC2 accessory compatible but with fixed EF mount.

---

### Finding 8: RED Scarlet-W 5K (Dragon) -- 2015

- **Confidence**: HIGH
- **Sensor**: RED Dragon 13.8 MP Super 35 CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 25.6 x 13.5 mm (28.9 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 5K (5120 x 2700)
- **Supported Resolutions**: 5K, 4K, 3K, 2K (each with FF and 2.4:1 options, plus 2:1, 16:9, 3:2, 4:3, 5:4, 6:5, 4:1, 8:1, anamorphic 2x, 1.3x)
- **Native Aspect Ratio**: 17:9 (at 5K full format)
- **Frame Rates**:
  - 5K FF: 50 fps; 5K 2.4:1: 60 fps
  - 4K FF: 120 fps; 4K 2.4:1: 150 fps
  - 3K FF: 150 fps; 3K 2.4:1: 200 fps
  - 2K FF: 240 fps; 2K 2.4:1: 300 fps
- **Lens Mount**: Interchangeable DSMC2 mounts -- PL, Canon EF, Nikon F, Leica M
- **Dynamic Range**: 16.5+ stops
- **Weight**: 3.5 lbs (BRAIN with Integrated Media Bay)
- **Supporting Sources**:
  - [RED Official Docs - Scarlet-W 5K Specs](https://docs.red.com/955-0116_v7.4/SCARLET_DRAGON_7_4/en-us/Content/A_TechSpecs/Specs_SW_5K.htm) - 25.6x13.5mm, 5120x2700, 13.8MP, all frame rates
  - [No Film School](https://nofilmschool.com/2015/12/red-scarlet-w-price-cost-release-date-5k-weapon) - launch details, pricing from $9,950
- **Notes**: DSMC2 platform body. "W" = Weapon-class processing in a smaller form factor. Different sensor cut (25.6x13.5mm) than EPIC Dragon (30.7x15.8mm) despite both being "Dragon" sensors.

---

### Finding 9: RED WEAPON Dragon 6K (Carbon Fiber) -- 2015

- **Confidence**: HIGH
- **Sensor**: RED Dragon 19.4 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 30.7 x 15.8 mm (34.5 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 6K (6144 x 3160)
- **Supported Resolutions**: 6K, 5K, 4K, 3K, 2K (each with FF, 2:1, 2.4:1, 16:9, 6:5, 4:3, 5:4, 4:1, 8:1, anamorphic 2x, 1.3x)
- **Native Aspect Ratio**: ~17:9 (at 6K full format)
- **Frame Rates**:
  - 6K FF: 75 fps; 6K 2.4:1: 100 fps
  - 5K FF: 96 fps; 5K 2.4:1: 120 fps
  - 4K FF: 120 fps; 4K 2.4:1: 150 fps
  - 3K FF: 150 fps; 3K 2.4:1: 200 fps
  - 2K FF: 240 fps; 2K 2.4:1: 300 fps
- **Lens Mount**: Interchangeable DSMC2 mounts -- PL (Carbon Fiber body came with PL), Canon EF, Nikon F
- **Dynamic Range**: 16.5+ stops
- **Data Rate**: Up to 300 MB/s
- **Weight**: 3.3 lbs (BRAIN with Integrated Media Bay)
- **Supporting Sources**:
  - [RED Official Docs - WEAPON Dragon CF Specs](https://docs.red.com/955-0177_v7.4/DSMC2_DRAGON_7_4/en-us/Content/A_TechSpecs/Specs_WEAPON_6K_CF.htm) - all specs confirmed
  - [B&H Photo](https://www.bhphotovideo.com/c/product/1346772-REG/red_digital_cinema_710_0201_std_weapon_carbon_fiber_brain.html) - PL mount, Dragon 6K
- **Notes**: First DSMC2 platform camera. Carbon Fiber body variant. Same Dragon sensor as EPIC Dragon but in new body form factor with higher data rates.

---

### Finding 10: RED WEAPON Dragon 6K (Magnesium) -- 2015

- **Confidence**: HIGH
- **Sensor**: RED Dragon 19.4 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 30.7 x 15.8 mm (34.5 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 6K (6144 x 3160)
- **Supported Resolutions**: Same as WEAPON Dragon 6K CF
- **Native Aspect Ratio**: ~17:9
- **Frame Rates**: Same as WEAPON Dragon 6K CF (6K 75fps to 2K 300fps)
- **Lens Mount**: Interchangeable DSMC2 mounts -- PL, Canon EF, Nikon F
- **Dynamic Range**: 16.5+ stops
- **Data Rate**: Up to 225 MB/s
- **Weight**: 3.35 lbs
- **Supporting Sources**:
  - [RED Official Docs - WEAPON Dragon Mg Specs](https://docs.red.com/955-0177_v7.4/DSMC2_DRAGON_7_4/en-us/Content/A_TechSpecs/Specs_WEAPON_6K_MG.htm) - all specs, 225 MB/s
- **Notes**: Magnesium body variant. Slightly lower max data rate than Carbon Fiber variant (225 vs 300 MB/s), same sensor and frame rates.

---

### Finding 11: RED WEAPON Helium 8K S35 -- 2016

- **Confidence**: HIGH
- **Sensor**: Helium 35.4 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 29.90 x 15.77 mm (33.80 mm diagonal)
- **Sensor Format**: Super 35mm (8K in S35!)
- **Native Resolution**: 8K (8192 x 4320)
- **Supported Resolutions**: 8K, 7K, 6K, 5K, 4K, 3K, 2K (each with multiple aspect ratios including 2:1, 2.4:1, 16:9, 8:9, 4:3, anamorphic)
- **Native Aspect Ratio**: 17:9 (at 8K full format)
- **Frame Rates**:
  - 8K FF: 60 fps; 8K 2.4:1: 75 fps
  - 7K FF: 60 fps
  - 6K FF: 75 fps
  - 5K FF: 96 fps
  - 4K FF: 120 fps
  - 2K FF: 240 fps
- **Lens Mount**: Interchangeable DSMC2 mounts -- PL, Canon EF, Nikon F, Leica M
- **Dynamic Range**: 16.5+ stops
- **Data Rate**: Up to 300 MB/s
- **Weight**: 3.35 lbs (BRAIN with Integrated Media Bay)
- **Supporting Sources**:
  - [RED Official Docs - WEAPON Helium Specs](https://docs.red.com/955-0155_v7.0/WEAPONEPICWOperationGuide/en-us/Content/A_TechSpecs/Specs_DSMC2_HELIUM.htm) - 29.90x15.77mm, 8192x4320, 35.4MP, all frame rates
  - [FDTimes](https://www.fdtimes.com/2016/10/11/red-8k-helium-super35mm-cameras/) - launch coverage
- **Notes**: First 8K RED sensor. Despite being 8K, fits in a Super 35mm format factor. DSMC2 platform.

---

### Finding 12: RED EPIC-W (Helium 8K S35) -- 2016

- **Confidence**: HIGH
- **Sensor**: Helium 35.4 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 29.90 x 15.77 mm (33.80 mm diagonal) -- same Helium sensor
- **Sensor Format**: Super 35mm
- **Native Resolution**: 8K (8192 x 4320)
- **Supported Resolutions**: 8K, 7K, 6K, 5K, 4K, 3K, 2K
- **Native Aspect Ratio**: 17:9
- **Frame Rates** (lower than WEAPON due to EPIC-W processing):
  - 8K FF: 30 fps; 8K 2.4:1: 30 fps
  - 7K FF: 30 fps; 7K 2.4:1: 40 fps
  - 6K FF: 75 fps; 6K 2.4:1: 100 fps
  - 5K FF: 96 fps; 5K 2.4:1: 120 fps
  - 4K FF: 120 fps; 4K 2.4:1: 150 fps
  - 3K FF: 150 fps; 3K 2.4:1: 200 fps
  - 2K FF: 240 fps; 2K 2.4:1: 300 fps
- **Lens Mount**: Interchangeable DSMC2 mounts -- PL, Canon EF, Nikon F, Leica M
- **Dynamic Range**: 16.5+ stops
- **Data Rate**: Up to 275 MB/s
- **Weight**: 3.35 lbs
- **Construction**: Magnesium and Aluminum Alloy
- **Supporting Sources**:
  - [RED Official Docs - EPIC-W 8K S35 Specs](https://docs.red.com/955-0155_v7.0/WEAPONEPICWOperationGuide/en-us/Content/A_TechSpecs/Specs_EPIC-W_8KS35.htm) - all specs confirmed, 30fps at 8K
  - [B&H Photo](https://www.bhphotovideo.com/c/product/1346516-REG/red_digital_cinema_710_0263_std_epic_w_helium_8k_s35.html) - pricing and specs
- **Notes**: Same Helium sensor as WEAPON but in the EPIC-W body with lower processing power. 8K capture limited to 30fps vs WEAPON's 60fps. DSMC2 platform.

---

### Finding 13: RED WEAPON Monstro 8K VV -- 2017

- **Confidence**: HIGH
- **Sensor**: Monstro 35.4 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 40.96 x 21.60 mm (46.31 mm diagonal)
- **Sensor Format**: VistaVision / Large Format (sometimes called "Full Format" by RED; larger than standard full frame 36x24mm in width)
- **Native Resolution**: 8K (8192 x 4320)
- **Supported Resolutions**: 8K, 7K, 6K, 5K, 4K, 3K, 2K (each with multiple aspect ratios: 2:1, 2.4:1, 16:9, 8:9, 3:2, 4:3, 5:4, 6:5, 4:1, 8:1, anamorphic 2x, 1.3x, 1.25x)
- **Native Aspect Ratio**: 17:9 (at 8K full format)
- **Frame Rates**:
  - 8K FF: 60 fps; 8K 2.4:1: 75 fps
  - 7K FF: 60 fps; 7K 2.4:1: 75 fps
  - 6K FF: 75 fps; 6K 2.4:1: 100 fps
  - 5K FF: 96 fps; 5K 2.4:1: 120 fps
  - 4K FF: 120 fps; 4K 2.4:1: 150 fps
  - 3K FF: 150 fps; 3K 2.4:1: 200 fps
  - 2K FF: 240 fps; 2K 2.4:1: 300 fps
- **Lens Mount**: Interchangeable DSMC2 mounts -- PL, Canon EF, Nikon F, Leica M (note: some lens mounts may vignette at full VV sensor area)
- **Dynamic Range**: 17+ stops
- **Data Rate**: Up to 300 MB/s
- **Weight**: 3.35 lbs (BRAIN with Integrated Media Bay)
- **Supporting Sources**:
  - [RED Official Docs - Monstro 8K VV Specs](https://docs.red.com/955-0160/WEAPONMONSTRO8KVVOperationGuide/en-us/Content/A_TechSpecs/Specs_DSMC2_MONSTRO.htm) - 40.96x21.60mm, all frame rates, 17+ stops
  - [FDTimes](https://www.fdtimes.com/2017/10/08/red-8k-vv-monstro/) - launch coverage
- **Notes**: First large-format RED sensor. Same 8K resolution as Helium but with much larger photosites (5 micron vs 3.65 micron). DSMC2 platform. Initially sold as WEAPON body, later unified into DSMC2 body.

---

### Finding 14: RED EPIC-W Gemini 5K S35 -- 2018

- **Confidence**: HIGH
- **Sensor**: Gemini 15.4 MP Dual Sensitivity CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 30.72 x 18.00 mm (35.61 mm diagonal)
- **Sensor Format**: Super 35mm (slightly taller than standard S35; full-height 18mm enables better anamorphic lens coverage)
- **Native Resolution**: 5K (5120 x 3000 at full height; 5120 x 2700 at standard 17:9)
- **Supported Resolutions**: 5K (full height 1.7:1 and standard), 4K, 3K, 2K
- **Native Aspect Ratio**: 1.7:1 at 5K full height (5120x3000); 17:9 at 5K standard
- **Frame Rates**:
  - 5K 1.7:1 (5120x3000): 75 fps
  - 5K FF (5120x2700): 96 fps; 5K 2.4:1: 120 fps
  - 4K FF: 120 fps; 4K 2.4:1: 150 fps
  - 3K: up to 200 fps (2.4:1)
  - 2K: up to 300 fps (2.4:1)
- **Lens Mount**: Interchangeable DSMC2 mounts -- PL, Canon EF, Nikon F, Leica M
- **Dynamic Range**: 16.5+ stops (both sensitivities)
- **Dual Sensitivity**: Standard mode (ISO 800 native) and Low Light mode (ISO 3200 native)
- **Data Rate**: Up to 275 MB/s
- **Weight**: 3.35 lbs
- **Supporting Sources**:
  - [RED Official Docs - EPIC-W Gemini Specs](https://docs.red.com/955-0166_v7.0/EPIC-W5KS35OperationGuide/Content/A_TechSpecs/Specs_GEMINI.htm) - 30.72x18.00mm, 5120x3000, 15.4MP, dual sensitivity
  - [RED Official Announcement](https://www.red.com/news/red-introduces-the-new-5k-s35-gemini-sensor-for-red-epic-w) - dual sensitivity details
- **Notes**: Unique sensor with dual native ISO modes. Largest pixel pitch (6 micron) of any RED sensor. Sensor technology originally developed for aerospace applications. Available in both EPIC-W and DSMC2 body. DSMC2 platform.

---

### Finding 15: DSMC2 Unified Bodies (2018 lineup consolidation)

In May 2018, RED consolidated their DSMC2 lineup. All camera names (WEAPON, EPIC-W, Scarlet-W) were retired in favor of a single "DSMC2 BRAIN" body sold with one of four sensor options. The sensors and their specifications are identical to those documented in Findings 8-14 above. The four unified DSMC2 sensor options:

#### 15a: DSMC2 Dragon-X 5K S35

- **Confidence**: HIGH
- **Sensor**: Dragon-X 13.8 MP Super 35 CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 25.6 x 13.5 mm (28.9 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 5K (5120 x 2700)
- **Supported Resolutions**: 5K, 4K, 3K, 2K (FF, 2:1, 2.4:1, 16:9, 3:2, 4:3, 5:4, 6:5, 4:1, 8:1, anamorphic 2x, 1.3x)
- **Native Aspect Ratio**: 17:9
- **Frame Rates**:
  - 5K FF: 96 fps; 5K 2.4:1: 120 fps
  - 4K FF: 120 fps; 4K 2.4:1: 150 fps
  - 3K FF: 150 fps; 3K 2.4:1: 200 fps
  - 2K FF: 240 fps; 2K 2.4:1: 300 fps
- **Lens Mount**: Interchangeable DSMC2 mounts
- **Dynamic Range**: 16.5+ stops
- **Data Rate**: Up to 300 MB/s
- **Weight**: 3.35 lbs
- **Supporting Sources**:
  - [RED Official Docs - DSMC2 Dragon-X Specs](https://docs.red.com/955-0182/DSMC2DRAGONXOperationGuide/Content/A_TechSpecs/Specs_DSMC2_DRAGONX.htm) - 25.6x13.5mm, all specs
- **Notes**: Essentially the Scarlet-W sensor with WEAPON-class processing. Entry-level DSMC2 option. Later offered a free firmware upgrade to 6K (see 15b).

#### 15b: DSMC2 Dragon-X 6K S35 (firmware upgrade)

- **Confidence**: HIGH
- **Sensor**: Dragon-X 19.4 MP Super 35 CMOS (rolling shutter) -- same physical sensor, full resolution unlocked
- **Sensor Size (W x H mm)**: 30.7 x 15.8 mm (34.5 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 6K (6144 x 3160)
- **Frame Rates**: Same as WEAPON Dragon 6K (6K 75fps FF to 2K 300fps)
- **Supporting Sources**:
  - [RED Official Docs - DSMC2 Dragon-X 6K Specs](https://docs.red.com/955-0181_v7.4/DSMC2_DRAGONX_7_4/Content/A_TechSpecs/Specs_DSMC2_DRAGONX_6K.htm) - 30.7x15.8mm, 6144x3160, 19.4MP
  - [REDUSER.NET](https://reduser.net/threads/current-dsmc2-dragon-x-customers-upgrade-to-6k-today-gemini-upgrade-future.183340/) - free upgrade announcement
- **Notes**: Free firmware upgrade made available to Dragon-X 5K owners, unlocking the full 6K resolution.

#### 15c: DSMC2 Helium 8K S35

- **Confidence**: HIGH
- **Sensor**: Helium 35.4 MP CMOS (rolling shutter); also available as Monochrome variant
- **Sensor Size (W x H mm)**: 29.90 x 15.77 mm (33.80 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 8K (8192 x 4320)
- **Frame Rates**: Same as WEAPON Helium (8K 60fps FF to 2K 240fps)
- **Supporting Sources**:
  - [RED Official Docs](https://docs.red.com/955-0155_v7.0/WEAPONEPICWOperationGuide/en-us/Content/A_TechSpecs/Specs_DSMC2_HELIUM.htm) - same specs as WEAPON Helium
  - [B&H Photo - Monochrome](https://www.bhphotovideo.com/c/product/1425792-REG/red_digital_cinema_710_0307_dsmc2_brain_w_helium.html) - monochrome variant
- **Notes**: Identical sensor to WEAPON Helium. Monochrome variant available by special order -- same resolution/frame rates, no Bayer filter.

#### 15d: DSMC2 Gemini 5K S35

- **Confidence**: HIGH
- **Sensor**: Gemini 15.4 MP Dual Sensitivity CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 30.72 x 18.00 mm (35.61 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 5K (5120 x 3000 full height)
- **Frame Rates**:
  - 5K 1.7:1 (5120x3000): 75 fps
  - 5K FF (5120x2700): 96 fps
  - 5K 2.4:1: 120 fps
  - 4K FF: 120 fps; 4K 2.4:1: 150 fps
  - 3K: up to 200 fps; 2K: up to 300 fps
- **Supporting Sources**:
  - [RED Official Docs - DSMC2 Gemini Specs](https://docs.red.com/955-0172_v7.0/DSMC2GEMINIOperationGuide/Content/A_TechSpecs/Specs_DSMC2_GEMINI.htm) - 30.72x18mm, 5120x3000, all frame rates
  - [FDTimes](https://www.fdtimes.com/2018/05/21/red-simplified-monstro-helium-gemini/) - Gemini specs in unified lineup
- **Notes**: Same Gemini sensor as EPIC-W Gemini, now in unified DSMC2 body.

#### 15e: DSMC2 Monstro 8K VV

- **Confidence**: HIGH
- **Sensor**: Monstro 35.4 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 40.96 x 21.60 mm (46.31 mm diagonal)
- **Sensor Format**: VistaVision / Large Format
- **Native Resolution**: 8K (8192 x 4320)
- **Frame Rates**: Same as WEAPON Monstro (8K 60fps FF to 2K 300fps)
- **Supporting Sources**:
  - [RED Official Docs](https://docs.red.com/955-0160/WEAPONMONSTRO8KVVOperationGuide/en-us/Content/A_TechSpecs/Specs_DSMC2_MONSTRO.htm) - same specs
  - [B&H Photo](https://www.bhphotovideo.com/c/product/1411131-REG/red_digital_cinema_710_0303_dsmc2_brain_with_monstro.html) - unified DSMC2 body
- **Notes**: Same Monstro sensor as WEAPON, now in unified DSMC2 body (aluminum alloy or carbon fiber options).

---

### Finding 16: RED RANGER Monstro 8K VV -- 2018

- **Confidence**: HIGH
- **Sensor**: Monstro 35.4 MP CMOS -- identical to DSMC2 Monstro
- **Sensor Size (W x H mm)**: 40.96 x 21.60 mm (46.31 mm diagonal)
- **Sensor Format**: VistaVision / Large Format
- **Native Resolution**: 8K (8192 x 4320)
- **Frame Rates**: Same as DSMC2 Monstro
- **Lens Mount**: Interchangeable -- PL (with Cooke /i support), Canon EF, Nikon F
- **Weight**: ~7.5 lbs
- **Power**: Wide voltage 11.5V to 32V
- **Supporting Sources**:
  - [AbelCine](https://www.abelcine.com/articles/blog-and-knowledge/tech-news/all-in-one-for-everyone-red-ranger-now-available-with-gemini-helium-and-monstro) - specs and features
  - [CineD](https://www.cined.com/red-ranger-now-available-to-order-helium-8k-and-gemini-5k-announced/) - Ranger details
- **Notes**: Non-modular, studio-oriented body with integrated I/O, XLR audio input, larger fan for quieter operation, wider voltage range. Initially rental-only. Same sensors as DSMC2 equivalents.

---

### Finding 17: RED RANGER Helium 8K S35 -- 2019

- **Confidence**: HIGH
- **Sensor**: Helium 35.4 MP CMOS -- identical to DSMC2 Helium
- **Sensor Size (W x H mm)**: 29.90 x 15.77 mm (33.80 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 8K (8192 x 4320)
- **Frame Rates**: 60 fps at 8K Full Format (same as DSMC2 Helium)
- **Weight**: ~7.5 lbs
- **Price**: $29,950
- **Supporting Sources**:
  - [Y.M.Cinema Magazine](https://ymcinema.com/2019/09/09/red-announces-the-ranger-helium-8k-and-ranger-gemini-5k/) - Ranger Helium specs and pricing
  - [RedShark News](https://www.redsharknews.com/production/item/6628-ranger-helium-and-ranger-gemini-red-s-brand-new-camera-range) - details
- **Notes**: RANGER body with Helium sensor. Available for purchase (unlike initial Monstro RANGER which was rental-only).

---

### Finding 18: RED RANGER Gemini 5K S35 -- 2019

- **Confidence**: HIGH
- **Sensor**: Gemini 15.4 MP Dual Sensitivity CMOS -- identical to DSMC2 Gemini
- **Sensor Size (W x H mm)**: 30.72 x 18.00 mm (35.61 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 5K (5120 x 3000 full height)
- **Frame Rates**: 96 fps at 5K Full Format (same as DSMC2 Gemini)
- **Weight**: ~7.5 lbs
- **Price**: $24,950
- **Supporting Sources**:
  - [Y.M.Cinema Magazine](https://ymcinema.com/2019/09/09/red-announces-the-ranger-helium-8k-and-ranger-gemini-5k/) - Ranger Gemini specs and pricing
- **Notes**: RANGER body with Gemini sensor.

---

### Finding 19: RED KOMODO 6K -- 2020

- **Confidence**: HIGH
- **Sensor**: KOMODO 19.9 MP Super 35 Global Shutter CMOS
- **Sensor Size (W x H mm)**: 27.03 x 14.26 mm (30.56 mm diagonal)
- **Sensor Format**: Super 35mm (sometimes called "Super 35 Plus" by RED -- slightly wider than traditional S35)
- **Native Resolution**: 6K (6144 x 3240)
- **Supported Resolutions**: 6K, 5K, 4K, 2K (17:9, 2:1, 2.4:1, 16:9, 1:1, anamorphic)
- **Native Aspect Ratio**: 17:9 (at 6K)
- **Frame Rates**:
  - 6K 17:9 (6144x3240): 40 fps
  - 6K 2.4:1 (6144x2592): 50 fps
  - 5K 17:9 (5120x2700): 48 fps
  - 4K 17:9 (4096x2160): 60 fps
  - 2K 17:9 (2048x1080): 120 fps
- **Lens Mount**: Fixed Canon RF mount (with electronic communication). PL and EF via adapters.
- **Dynamic Range**: 16+ stops
- **Data Rate**: Up to 280 MB/s (CFast 2.0)
- **Weight**: 2.10 lbs (without body cap and media card)
- **Built-in Display**: 2.9" 1440x1440 touchscreen LCD
- **Global Shutter**: Yes -- first RED camera with global shutter
- **Supporting Sources**:
  - [RED Official Docs - KOMODO 6K Specs](https://docs.red.com/955-0190_v1.3/955-0190_v1.3_REV-1.1_1_RED_PS_KOMODO_Operation_Guide/Content/A_TechSpecs/Specs_KOMODO_6K.htm) - 27.03x14.26mm, 6144x3240, 19.9MP, 16+ stops, all frame rates
  - [No Film School](https://nofilmschool.com/red-reveals-details-komodo-sensor-size) - sensor size reveal
- **Notes**: DSMC3 platform. First RED with global shutter, integrated touchscreen, and Canon RF mount. Compact cube form factor. Dual BP-900 battery slots. No interchangeable lens mount.

---

### Finding 20: RED KOMODO-X 6K -- 2023

- **Confidence**: HIGH
- **Sensor**: KOMODO-X 19.9 MP Super 35 Global Shutter CMOS
- **Sensor Size (W x H mm)**: 27.03 x 14.26 mm (30.56 mm diagonal) -- same size as KOMODO
- **Sensor Format**: Super 35mm
- **Native Resolution**: 6K (6144 x 3240)
- **Supported Resolutions**: 6K, 5K, 4K, 2K (17:9, 2:1, 2.4:1, 16:9, 1:1, anamorphic 2x/1.8x/1.6x/1.5x/1.3x/1.25x)
- **Native Aspect Ratio**: 17:9
- **Frame Rates** (double the KOMODO):
  - 6K 17:9 (6144x3240): 80 fps
  - 5K 17:9 (5120x2700): 96 fps
  - 4K 17:9 (4096x2160): 120 fps
  - 2K 17:9 (2048x1080): 240 fps
- **Lens Mount**: Fixed Canon RF mount (also available in Nikon Z mount as of Feb 2025). PL and EF via adapters.
- **Dynamic Range**: 16.5+ stops
- **Data Rate**: Up to 560 MB/s (CFexpress Type B)
- **Weight**: 2.62 lbs (without body cap and CFexpress card)
- **Built-in Display**: 2.9" 1440x1440 touchscreen LCD
- **Global Shutter**: Yes
- **Supporting Sources**:
  - [RED Official Docs - KOMODO-X Specs](https://docs.red.com/955-0219/955-0219_V1.0%20Rev-A%20RED%20PS,%20KOMODO-X%20Operation%20Guide/Content/A_TechSpecs/Specs_KOMODO-X.htm) - 27.03x14.26mm, all frame rates, 16.5+ stops
  - [PetaPixel](https://petapixel.com/2023/05/16/red-unveils-komodo-x-6k-cine-camera-with-new-sensor-and-faster-framerates/) - launch coverage
- **Notes**: DSMC3 platform. Same sensor size as KOMODO but redesigned sensor with improved shadow detail and 2x frame rates. Upgraded to Micro V-Lock battery. CFexpress Type B (vs CFast 2.0 on KOMODO). Z-mount variant introduced February 2025.

---

### Finding 21: RED V-RAPTOR 8K VV -- 2021

- **Confidence**: HIGH
- **Sensor**: V-RAPTOR 8K VV 35.4 MP CMOS (rolling shutter)
- **Sensor Size (W x H mm)**: 40.96 x 21.60 mm (46.31 mm diagonal)
- **Sensor Format**: VistaVision / Large Format
- **Native Resolution**: 8K (8192 x 4320)
- **Supported Resolutions**: 8K, 7K, 6K, 5K, 4K, 3K, 2K (17:9, 2:1, 2.4:1, 16:9, 1:1, anamorphic)
- **Native Aspect Ratio**: 17:9
- **Frame Rates**:
  - 8K 17:9: 120 fps
  - 6K 17:9: 160 fps
  - 4K 17:9: 240 fps
  - 2K 17:9: 480 fps
- **Lens Mount**: Fixed Canon RF mount (integrated, locking, with electronic communication). PL and EF via adapters.
- **Dynamic Range**: 17+ stops
- **Data Rate**: Up to 800 MB/s (CFexpress Type B)
- **Weight**: 4.03 lbs (without body cap)
- **Supporting Sources**:
  - [RED Official Docs - V-RAPTOR Specs](https://docs.red.com/955-0199/955-0199_V1.2_Rev_A_RED_PS_V-RAPTOR_Operation_Guide/Content/C_TechSpecs/Specs_V-RAPTOR.htm) - 40.96x21.60mm, 8192x4320, all frame rates, 17+ stops
  - [CVP](https://cvp.com/product/red_v-raptor_8k_vv) - specs listing
- **Notes**: DSMC3 platform flagship. First DSMC3 camera. Massive improvement in frame rates over DSMC2 Monstro at same resolution/sensor size (120fps vs 60fps at 8K). Rolling shutter (NOT global shutter -- the [X] variant added that later).

---

### Finding 22: RED V-RAPTOR XL 8K VV -- 2022

- **Confidence**: HIGH
- **Sensor**: Same as V-RAPTOR 8K VV -- 35.4 MP, 40.96 x 21.60 mm
- **Sensor Format**: VistaVision / Large Format
- **Native Resolution**: 8K (8192 x 4320)
- **Frame Rates**: Same as V-RAPTOR 8K VV
- **Lens Mount**: Interchangeable -- PL (with Cooke /i and Zeiss eXtended Data), Canon EF. NOT a fixed RF mount like the standard V-RAPTOR.
- **Dynamic Range**: 17+ stops
- **Weight**: ~8 lbs (with PL mount)
- **Dimensions**: 8.5" x 6.5" x 6"
- **Additional Features vs Standard V-RAPTOR**:
  - 3x SDI outputs
  - Dedicated Gig-E and CTRL ports
  - 3x front-facing AUX ports
  - Integrated electronic ND (1/4 stop increments)
  - Dual voltage battery support (14V and 26V, V-Lock or Gold Mount)
- **Supporting Sources**:
  - [AbelCine](https://www.abelcine.com/buy/cameras-accessories/digital-cinema-cameras/red-v-raptor-xl-8k-vv-gold-mount) - XL specs and features
  - [B&H Photo](https://www.bhphotovideo.com/lit_files/976196.pdf) - comparison sheet
  - [RED Support](https://support.red.com/hc/en-us/articles/9479001592595-V-RAPTOR-XL-Lens-and-Lens-Mount-Compatibility) - lens mount compatibility
- **Notes**: Studio-oriented version of V-RAPTOR. Same sensor and processing, larger body with professional I/O. Uses DSMC-style interchangeable lens mounts (not fixed RF like standard V-RAPTOR).

---

### Finding 23: RED V-RAPTOR 8K S35 -- 2022 (November)

- **Confidence**: HIGH
- **Sensor**: V-RAPTOR 35.4 MP CMOS (rolling shutter) -- S35 crop of the sensor design
- **Sensor Size (W x H mm)**: 26.21 x 13.82 mm (29.63 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 8K (8192 x 4320)
- **Supported Resolutions**: 8K, 7K, 6K, 5K, 4K, 3K, 2K
- **Native Aspect Ratio**: 17:9
- **Frame Rates**:
  - 8K: 120 fps
  - 4K (UHD): 240 fps
  - 2K: 480 fps (17:9); up to 600 fps (2.4:1)
- **Lens Mount**: Fixed Canon RF mount. PL and EF via adapters.
- **Dynamic Range**: 16.5+ stops
- **Weight**: ~4 lbs
- **Supporting Sources**:
  - [B&H Photo](https://www.bhphotovideo.com/c/product/1709224-REG/red_digital_cinema_v_raptor_8k_s35_sensor.html) - 26.21x13.82mm, 35.4MP
  - [RED Official](https://www.red.com/v-raptor-8k-s35-black) - product page
  - [CineD](https://www.cined.com/red-v-raptor-and-v-raptor-xl-8k-s35-cinema-cameras-released/) - launch coverage
- **Notes**: Same DSMC3 architecture as V-RAPTOR VV but with a smaller S35-sized sensor. Optimized for S35 cinema lenses. Also available as V-RAPTOR XL 8K S35 with interchangeable mounts and studio I/O.

---

### Finding 24: RED V-RAPTOR 8K VV + 6K S35 (Dual Format) -- 2022

- **Confidence**: MEDIUM
- **Sensor**: Same VV sensor as V-RAPTOR 8K VV (40.96 x 21.60 mm)
- **Sensor Format**: VistaVision (full sensor) or Super 35 (cropped to ~6K)
- **Native Resolution**: 8K at VV; 6K in S35 mode
- **Supporting Sources**:
  - [AbelCine](https://www.abelcine.com/buy/cameras-accessories/digital-cinema-cameras/red-v-raptor-8k-vv-6k-s35-dual-format) - product listing
  - [Filmtools](https://www.filmtools.com/red-v-raptor-8k-vv-6k-s35-dual-format-black.html) - dual format specs
- **Notes**: This is a software/license variant of the V-RAPTOR 8K VV that also unlocks optimized S35 crop modes. Same hardware as V-RAPTOR 8K VV.

---

### Finding 25: RED V-RAPTOR [RHINO] 8K S35 -- 2022 (November)

- **Confidence**: HIGH
- **Sensor**: Same S35 sensor as V-RAPTOR 8K S35
- **Sensor Size (W x H mm)**: 26.21 x 13.82 mm (29.63 mm diagonal)
- **Sensor Format**: Super 35mm
- **Native Resolution**: 8K (8192 x 4320)
- **Frame Rates**: Same as V-RAPTOR 8K S35 (120fps at 8K)
- **Lens Mount**: Fixed Canon RF
- **Dynamic Range**: 16.5+ stops
- **Supporting Sources**:
  - [Y.M.Cinema Magazine](https://ymcinema.com/2023/01/26/red-v-raptor-rhino-8k-s35-120p-test-footage/) - Rhino S35 120fps test footage
  - [CineD](https://www.cined.com/red-v-raptor-and-v-raptor-xl-8k-s35-cinema-cameras-released/) - Rhino is limited edition S35
- **Notes**: Limited edition version of V-RAPTOR with S35 sensor. Light grey color scheme. Technically identical to V-RAPTOR 8K S35 in all specs. "RHINO" is a branding/cosmetic variant.

---

### Finding 26: RED V-RAPTOR [X] 8K VV (Global Shutter) -- 2024 (January)

- **Confidence**: HIGH
- **Sensor**: V-RAPTOR [X] 8K VV 35.4 MP Global Shutter CMOS
- **Sensor Size (W x H mm)**: 40.96 x 21.60 mm (46.31 mm diagonal)
- **Sensor Format**: VistaVision / Large Format
- **Native Resolution**: 8K (8192 x 4320)
- **Supported Resolutions**: 8K, 7K, 6K, 5K, 4K, 3K, 2K (17:9, 2:1, 2.4:1, 16:9, 1:1, anamorphic)
- **Native Aspect Ratio**: 17:9
- **Frame Rates**:
  - 8K 17:9: 120 fps; 8K 2.4:1: 150 fps
  - 6K 17:9: 160 fps; 6K 2.4:1: 200 fps
  - 4K 17:9: 240 fps; 4K 2.4:1: 300 fps
  - 2K 17:9: 480 fps; 2K 2.4:1: 600 fps
- **Lens Mount**: Fixed Canon RF mount (locking, with electronic communication). Also available in Nikon Z mount (Feb 2025). PL and EF via adapters.
- **Dynamic Range**: 17+ stops
- **Global Shutter**: Yes -- world's first large-format global shutter cinema camera
- **Data Rate**: Up to 800 MB/s (CFexpress Type B)
- **Weight**: 4.03 lbs
- **Global Vision Suite**: Extended Highlights, improved audio pre-amps, redesigned sensor cavity for better contrast
- **Supporting Sources**:
  - [RED Official Docs - V-RAPTOR [X] Specs](https://docs.red.com/955-0225/955-0225_V1.7+Rev-B+RED+PS,+V-RAPTOR+%5BX%5D+8K+VV+Operation+Guide+HTML/Content/B_TechSpecs/Specs_V-RAPTOR%20%5BX%5D.htm) - all specs confirmed
  - [FDTimes](https://www.fdtimes.com/2024/01/25/red-v-raptor-global-shutter/) - global shutter announcement
  - [AbelCine](https://www.abelcine.com/articles/blog-and-knowledge/tech-news/reds-new-global-vision-v-raptor-x-and-v-raptor-xl-x-) - detailed comparison with original
- **Notes**: DSMC3 platform. Same sensor dimensions as original V-RAPTOR 8K VV but with global shutter replacing rolling shutter. Same frame rates. Upgraded audio pre-amps and sensor cavity design.

---

### Finding 27: RED V-RAPTOR XL [X] 8K VV (Global Shutter) -- 2024

- **Confidence**: HIGH
- **Sensor**: Same as V-RAPTOR [X] -- 35.4 MP Global Shutter CMOS, 40.96 x 21.60 mm
- **Frame Rates**: Same as V-RAPTOR [X]
- **Lens Mount**: Interchangeable -- PL (Cooke /i, Zeiss eXtended Data), Canon EF
- **Additional Features**: Same as V-RAPTOR XL (integrated ND, 3x SDI, dual voltage battery, etc.)
- **Supporting Sources**:
  - [RED Official Docs - V-RAPTOR XL [X] Specs](https://docs.red.com/955-0227/955-0227_V1.7%20Rev-A%20RED%20PS,%20V-RAPTOR%20XL%20%5BX%5D%208K%20VV%20Operation%20Guide%20HTML/Content/B_TechSpecs/Specs_V-RAPTOR_XL%20%5BX%5D.htm)
  - [RED Official](https://www.red.com/v-raptor-xl-x-black) - product page
- **Notes**: Studio version of the V-RAPTOR [X]. Same sensor/processing, larger body with professional I/O and interchangeable mounts.

---

### Finding 28: RED V-RAPTOR XE 8K VV -- 2025 (September, shipping October)

- **Confidence**: HIGH
- **Sensor**: V-RAPTOR XE 8K VV 35.4 MP Global Shutter CMOS
- **Sensor Size (W x H mm)**: 40.96 x 21.60 mm (46.31 mm diagonal) -- same sensor dimensions
- **Sensor Format**: VistaVision / Large Format
- **Native Resolution**: 8K (8192 x 4320)
- **Supported Resolutions**: 8K, 7K, 6K, 5K, 4K, 3K, 2K
- **Native Aspect Ratio**: 17:9
- **Frame Rates** (approximately half of V-RAPTOR [X]):
  - 8K 17:9: 60 fps; 8K 2.4:1: 75 fps
  - 7K 17:9: 70 fps
  - 4K 17:9: 120 fps
  - 2K 17:9: 240 fps; 2K 2.4:1: 300 fps
- **Lens Mount**: Available in both Canon RF mount and Nikon Z mount variants
- **Dynamic Range**: 17+ stops
- **Global Shutter**: Yes
- **Data Rate**: Up to 800 MB/s (CFexpress Type B)
- **Weight**: 4.03 lbs
- **Price**: $14,995 (body); $19,995 (Cine Essentials Pack)
- **Supporting Sources**:
  - [RedShark News](https://www.redsharknews.com/red-v-raptor-xe-8k-z-mount-rf-mount) - full specs and pricing
  - [CineD](https://www.cined.com/red-v-raptor-xe-announced-half-the-frame-rates-half-the-price/) - "half the frame rates, half the price"
  - [Newsshooter](https://www.newsshooter.com/2025/09/09/red-v-raptor-xe-14995-usd/) - pricing details
- **Notes**: Budget-oriented version of V-RAPTOR [X]. Same sensor and global shutter but with reduced maximum frame rates (roughly half). Significantly lower price point ($14,995 vs ~$30,000+ for [X]). First RED to launch simultaneously in both RF and Z mount.

---

### Finding 29: Nikon ZR -- 2025 (October)

- **Confidence**: HIGH
- **Sensor**: 24.5 MP partially stacked Full-Frame CMOS (Expeed 7 processor)
- **Sensor Size (W x H mm)**: 35.9 x 23.9 mm
- **Sensor Format**: Full Frame (standard 35mm still photo format, NOT VistaVision)
- **Native Resolution**: 6K (6048 x 4032 for stills; 6K/59.94p video)
- **Supported Resolutions**: 6K at 59.94p, 4K at 119.88p (R3D NE codec)
- **Lens Mount**: Nikon Z mount (native)
- **Dynamic Range**: 15+ stops
- **Recording Format**: R3D NE (a variant of REDCODE RAW co-developed by Nikon and RED)
- **Weight**: 540g body (630g with battery and media)
- **Display**: Fully articulating 4.0" LCD touchscreen, 3.07M dots, 1000 nits. No EVF.
- **Supporting Sources**:
  - [Nikon Official](https://www.nikon.com/company/news/2024/0910_imaging_01.html) - press release
  - [FDTimes](https://www.fdtimes.com/2025/09/22/nikon-zr/) - specifications
  - [DPReview](https://www.dpreview.com/products/nikon/slrs/nikon_zr/specifications) - full spec sheet
- **Notes**: First camera co-developed by Nikon and RED following Nikon's acquisition of RED in March 2024. Part of Nikon's "Z Cinema" series. Uses R3D NE codec (not standard REDCODE RAW). This is a Nikon-branded product, not a RED-branded camera, but it is the first fruit of the RED acquisition.

---

## Sensor Summary Table

| Sensor Name | Dimensions (W x H mm) | Diagonal | Format | Resolution | MP | DR (stops) | Pixel Pitch | Cameras Using It |
|---|---|---|---|---|---|---|---|---|
| Mysterium | 24.4 x 13.7 | ~28 mm | Super 35 | 4K+ (4520x2540) | 12 | ~11 | ~5.4 um | RED ONE |
| Mysterium-X | 27.7 x 14.6 | 31.4 mm | Super 35 | 5K (5120x2700) | 14 | 13.5 | ~5.4 um | RED ONE MX*, EPIC-X/M, Scarlet-X |
| Dragon (19.4MP) | 30.7 x 15.8 | 34.5 mm | Super 35 | 6K (6144x3160) | 19.4 | 16.5+ | ~5.0 um | EPIC Dragon, Scarlet Dragon, WEAPON Dragon 6K, DSMC2 Dragon-X 6K |
| Dragon (13.8MP) | 25.6 x 13.5 | 28.9 mm | Super 35 | 5K (5120x2700) | 13.8 | 16.5+ | ~5.0 um | Scarlet-W, DSMC2 Dragon-X 5K |
| Dragon (9.9MP) | 23.0 x 10.8 | 25.5 mm | Sub-S35/APS-C | 4.5K (4608x2160) | 9.9 | 16.5+ | ~5.0 um | Raven |
| Helium | 29.90 x 15.77 | 33.80 mm | Super 35 | 8K (8192x4320) | 35.4 | 16.5+ | ~3.65 um | WEAPON Helium, EPIC-W Helium, DSMC2 Helium, RANGER Helium |
| Gemini | 30.72 x 18.00 | 35.61 mm | Super 35 | 5K (5120x3000) | 15.4 | 16.5+ | ~6.0 um | EPIC-W Gemini, DSMC2 Gemini, RANGER Gemini |
| Monstro | 40.96 x 21.60 | 46.31 mm | VistaVision/LF | 8K (8192x4320) | 35.4 | 17+ | ~5.0 um | WEAPON Monstro, DSMC2 Monstro, RANGER Monstro |
| KOMODO (GS) | 27.03 x 14.26 | 30.56 mm | Super 35 | 6K (6144x3240) | 19.9 | 16+ | ~4.4 um | KOMODO 6K |
| KOMODO-X (GS) | 27.03 x 14.26 | 30.56 mm | Super 35 | 6K (6144x3240) | 19.9 | 16.5+ | ~4.4 um | KOMODO-X |
| V-RAPTOR VV | 40.96 x 21.60 | 46.31 mm | VistaVision/LF | 8K (8192x4320) | 35.4 | 17+ | ~5.0 um | V-RAPTOR 8K VV, V-RAPTOR XL |
| V-RAPTOR S35 | 26.21 x 13.82 | 29.63 mm | Super 35 | 8K (8192x4320) | 35.4 | 16.5+ | ~3.2 um | V-RAPTOR 8K S35, V-RAPTOR XL S35, RHINO |
| V-RAPTOR [X] VV (GS) | 40.96 x 21.60 | 46.31 mm | VistaVision/LF | 8K (8192x4320) | 35.4 | 17+ | ~5.0 um | V-RAPTOR [X], V-RAPTOR XL [X], V-RAPTOR XE |

*RED ONE MX uses the same Mysterium-X sensor silicon but the camera body limits the usable active area to smaller crops (max ~24.19 x 10.37 mm at 4.5K WS).

GS = Global Shutter

---

## Contradictions

### V-RAPTOR 8K S35 Sensor Dimensions
- **B&H Photo and multiple retailer sources**: 26.21 x 13.82 mm (29.63 mm diagonal)
- **Earlier FDTimes source on Helium**: 29.90 x 15.77 mm (this is the HELIUM sensor, not the V-RAPTOR S35 sensor)
- **Resolution**: The V-RAPTOR S35 sensor has significantly smaller dimensions than the DSMC2 Helium despite both being Super 35 and 8K. This is because the V-RAPTOR S35 uses smaller photosites (~3.2 um vs ~3.65 um) to pack 8K into a smaller sensor area.

### EPIC-W Helium 8K Frame Rates
- **Some B&H listings**: State "up to 30fps at 8K" for EPIC-W
- **RED Official Docs**: Confirm 30fps at 8K for EPIC-W (vs 60fps for WEAPON Helium)
- **Resolution**: Both agree. The EPIC-W uses WEAPON processing at a lower power, limiting 8K to 30fps. This is confirmed by official RED documentation.

### Dragon Sensor Variants -- Same Silicon or Different?
- **REDUSER discussion**: Suggests the Dragon 13.8MP (Scarlet-W/Dragon-X 5K) and Dragon 19.4MP (EPIC Dragon/WEAPON Dragon) are different cuts of the same wafer design.
- **RED Official**: Lists them with different sensor dimensions (25.6x13.5mm vs 30.7x15.8mm), confirming they are physically different sensor sizes despite sharing the "Dragon" name.
- **Dragon-X 6K upgrade**: The Dragon-X 5K was later upgraded to 6K (30.7x15.8mm), suggesting the physical sensor was always 19.4MP capable but was originally marketed/sold at 5K.

---

## Source Registry

| # | Title | URL | Date | Queries that surfaced it |
|---|-------|-----|------|--------------------------|
| 1 | RED Official Docs - EPIC MX Specs | https://docs.red.com/955-0156/EPICSCARLETOperationGuide/en-us/Content/A_TechSpecs/Specs_EPIC_MX.htm | N/A | Q1, Q3 |
| 2 | RED Official Docs - EPIC Dragon Specs | https://docs.red.com/955-0156/EPICSCARLETOperationGuide/en-us/Content/A_TechSpecs/Specs_EPIC_DRAGON.htm | N/A | Q2, Q3 |
| 3 | RED Official Docs - Scarlet MX Specs | https://docs.red.com/955-0156/EPICSCARLETOperationGuide/en-us/Content/A_TechSpecs/Specs_SCARLET_MX.htm | N/A | Q15 |
| 4 | RED Official Docs - Scarlet Dragon Specs | https://docs.red.com/955-0156/EPICSCARLETOperationGuide/en-us/Content/A_TechSpecs/Specs_SCARLET_DRAGON.htm | N/A | Q19 |
| 5 | RED Official Docs - RAVEN Specs | https://docs.red.com/955-0154/REDRAVENOperationGuide/en-us/Content/A_TechSpecs/Specs_RAVEN.htm | N/A | Q5, Q11 |
| 6 | RED Official Docs - Scarlet-W 5K Specs | https://docs.red.com/955-0116_v7.4/SCARLET_DRAGON_7_4/en-us/Content/A_TechSpecs/Specs_SW_5K.htm | N/A | Q12 |
| 7 | RED Official Docs - WEAPON Helium Specs | https://docs.red.com/955-0155_v7.0/WEAPONEPICWOperationGuide/en-us/Content/A_TechSpecs/Specs_DSMC2_HELIUM.htm | N/A | Q13, Q14 |
| 8 | RED Official Docs - EPIC-W Helium Specs | https://docs.red.com/955-0155_v7.0/WEAPONEPICWOperationGuide/en-us/Content/A_TechSpecs/Specs_EPIC-W_8KS35.htm | N/A | Q14 |
| 9 | RED Official Docs - EPIC-W Gemini Specs | https://docs.red.com/955-0166_v7.0/EPIC-W5KS35OperationGuide/Content/A_TechSpecs/Specs_GEMINI.htm | N/A | Q17 |
| 10 | RED Official Docs - DSMC2 Dragon-X 5K Specs | https://docs.red.com/955-0182/DSMC2DRAGONXOperationGuide/Content/A_TechSpecs/Specs_DSMC2_DRAGONX.htm | N/A | Q7 |
| 11 | RED Official Docs - DSMC2 Dragon-X 6K Specs | https://docs.red.com/955-0181_v7.4/DSMC2_DRAGONX_7_4/Content/A_TechSpecs/Specs_DSMC2_DRAGONX_6K.htm | N/A | Q7, Q18 |
| 12 | RED Official Docs - DSMC2 Gemini Specs | https://docs.red.com/955-0172_v7.0/DSMC2GEMINIOperationGuide/Content/A_TechSpecs/Specs_DSMC2_GEMINI.htm | N/A | Q8 |
| 13 | RED Official Docs - Monstro 8K VV Specs | https://docs.red.com/955-0160/WEAPONMONSTRO8KVVOperationGuide/en-us/Content/A_TechSpecs/Specs_DSMC2_MONSTRO.htm | N/A | Q10, Q20 |
| 14 | RED Official Docs - WEAPON Dragon Mg Specs | https://docs.red.com/955-0177_v7.4/DSMC2_DRAGON_7_4/en-us/Content/A_TechSpecs/Specs_WEAPON_6K_MG.htm | N/A | Q18 |
| 15 | RED Official Docs - WEAPON Dragon CF Specs | https://docs.red.com/955-0177_v7.4/DSMC2_DRAGON_7_4/en-us/Content/A_TechSpecs/Specs_WEAPON_6K_CF.htm | N/A | Q18 |
| 16 | RED Official Docs - KOMODO 6K Specs | https://docs.red.com/955-0190_v1.3/955-0190_v1.3_REV-1.1_1_RED_PS_KOMODO_Operation_Guide/Content/A_TechSpecs/Specs_KOMODO_6K.htm | N/A | Q4, Q9 |
| 17 | RED Official Docs - KOMODO-X Specs | https://docs.red.com/955-0219/955-0219_V1.0%20Rev-A%20RED%20PS,%20KOMODO-X%20Operation%20Guide/Content/A_TechSpecs/Specs_KOMODO-X.htm | N/A | Q9 |
| 18 | RED Official Docs - V-RAPTOR Specs | https://docs.red.com/955-0199/955-0199_V1.2_Rev_A_RED_PS_V-RAPTOR_Operation_Guide/Content/C_TechSpecs/Specs_V-RAPTOR.htm | N/A | Q4, Q10 |
| 19 | RED Official Docs - V-RAPTOR [X] Specs | https://docs.red.com/955-0225/955-0225_V1.7+Rev-B+RED+PS,+V-RAPTOR+%5BX%5D+8K+VV+Operation+Guide+HTML/Content/B_TechSpecs/Specs_V-RAPTOR%20%5BX%5D.htm | N/A | Q16 |
| 20 | RED Official Docs - V-RAPTOR XL [X] Specs | https://docs.red.com/955-0227/955-0227_V1.7%20Rev-A%20RED%20PS,%20V-RAPTOR%20XL%20%5BX%5D%208K%20VV%20Operation%20Guide%20HTML/Content/B_TechSpecs/Specs_V-RAPTOR_XL%20%5BX%5D.htm | N/A | Q16 |
| 21 | RED Official Docs - DSMC2 Lens Mounts | https://docs.red.com/955-0167_v7.4/DSMC2_MONSTRO_7_4/en-us/Content/2_Components/13_Lens_Mounts.htm | N/A | Q20 |
| 22 | FDTimes - RED Simplified Monstro Helium Gemini | https://www.fdtimes.com/2018/05/21/red-simplified-monstro-helium-gemini/ | 2018-05-21 | Q2 |
| 23 | FDTimes - V-RAPTOR Global Shutter | https://www.fdtimes.com/2024/01/25/red-v-raptor-global-shutter/ | 2024-01-25 | Q16 |
| 24 | Film School Online - RED ONE Camera | https://filmschoolonline.com/info/red_one_camera.htm | N/A | Q6 |
| 25 | VFX Camera Database - RED ONE MX | https://vfxcamdb.com/red-one-mx/ | N/A | Q6 |
| 26 | B&H Photo - V-RAPTOR 8K S35 | https://www.bhphotovideo.com/c/product/1709224-REG/red_digital_cinema_v_raptor_8k_s35_sensor.html | N/A | Q12 |
| 27 | B&H Photo - V-RAPTOR [X] 8K VV | https://www.bhphotovideo.com/c/product/1807129-REG/red_digital_cinema_710_0390_v_raptor_x_8k_vv.html | N/A | Q4, Q16 |
| 28 | No Film School - KOMODO Sensor Size | https://nofilmschool.com/red-reveals-details-komodo-sensor-size | N/A | Q5, Q9 |
| 29 | PetaPixel - KOMODO-X Launch | https://petapixel.com/2023/05/16/red-unveils-komodo-x-6k-cine-camera-with-new-sensor-and-faster-framerates/ | 2023-05-16 | Q9 |
| 30 | AbelCine - V-RAPTOR [X] Analysis | https://www.abelcine.com/articles/blog-and-knowledge/tech-news/reds-new-global-vision-v-raptor-x-and-v-raptor-xl-x- | 2024-01 | Q16 |
| 31 | RedShark News - V-RAPTOR XE | https://www.redsharknews.com/red-v-raptor-xe-8k-z-mount-rf-mount | 2025-09 | Q17 |
| 32 | CineD - V-RAPTOR XE | https://www.cined.com/red-v-raptor-xe-announced-half-the-frame-rates-half-the-price/ | 2025-09 | Q17 |
| 33 | Newsshooter - V-RAPTOR XE | https://www.newsshooter.com/2025/09/09/red-v-raptor-xe-14995-usd/ | 2025-09 | Q17 |
| 34 | Y.M.Cinema - RANGER Helium/Gemini | https://ymcinema.com/2019/09/09/red-announces-the-ranger-helium-8k-and-ranger-gemini-5k/ | 2019-09-09 | Q13 |
| 35 | AbelCine - RANGER All Sensors | https://www.abelcine.com/articles/blog-and-knowledge/tech-news/all-in-one-for-everyone-red-ranger-now-available-with-gemini-helium-and-monstro | 2019 | Q13 |
| 36 | Y.M.Cinema - V-RAPTOR Rhino | https://ymcinema.com/2023/01/26/red-v-raptor-rhino-8k-s35-120p-test-footage/ | 2023-01-26 | Q4 |
| 37 | CineD - V-RAPTOR S35 Launch | https://www.cined.com/red-v-raptor-and-v-raptor-xl-8k-s35-cinema-cameras-released/ | 2022-11 | Q12 |
| 38 | RED Support - V-RAPTOR XL Lens Mounts | https://support.red.com/hc/en-us/articles/9479001592595-V-RAPTOR-XL-Lens-and-Lens-Mount-Compatibility | N/A | Q20 |
| 39 | PHFX - V-Raptor Resources | https://www.phfx.com/articles/redRaptorResources/ | N/A | Q10 |
| 40 | Nikon Official - ZR Press Release | https://www.nikon.com/company/news/2024/0910_imaging_01.html | 2025-09-10 | Q20 |
| 41 | FDTimes - Nikon ZR | https://www.fdtimes.com/2025/09/22/nikon-zr/ | 2025-09-22 | Q20 |
| 42 | No Film School - Scarlet-W Launch | https://nofilmschool.com/2015/12/red-scarlet-w-price-cost-release-date-5k-weapon | 2015-12 | Q3 |
| 43 | B&H Photo - Helium Monochrome | https://www.bhphotovideo.com/c/product/1425792-REG/red_digital_cinema_710_0307_dsmc2_brain_w_helium.html | N/A | Q20 |
