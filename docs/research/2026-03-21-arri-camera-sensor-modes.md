# Raw Research Findings: ARRI Digital Cinema Camera Sensor Modes

## Queries Executed
1. "ARRI ALEXA Classic sensor modes resolution max frame rate specifications" - 3 useful results
2. "ARRI ALEXA Mini LF sensor modes resolution active sensor area max fps" - 3 useful results
3. "ARRI ALEXA 35 sensor modes resolution max frame rate specifications" - 3 useful results
4. "ARRI ALEXA 65 sensor modes resolution specifications" - 3 useful results
5. "ARRI AMIRA sensor modes resolution max frame rate" - 2 useful results
6. "ARRI Arriflex D-20 D-21 sensor specifications resolution frame rate" - 3 useful results
7. "ARRI ALEXA XT sensor modes recording resolution max frame rate" - 2 useful results
8. "ARRI ALEXA SXT Plus SXT W sensor modes resolution frame rates" - 2 useful results
9. "ARRI ALEXA LF sensor modes resolution max fps specifications" - 2 useful results
10. "ARRI ALEXA 265 sensor modes specifications 2025 2026" - 3 useful results
11. "ARRI ALEXA 35 Xtreme sensor modes specifications max frame rate 2025" - 4 useful results
12. Various targeted follow-up queries for missing data points

---

## Camera Specifications by Model

### Arriflex D-20
- **Confidence**: MEDIUM (2 independent sources, some frame rate details limited)
- **Sensor**: CMOS with Bayer pattern, ALEV I
- **Sensor dimensions**: 23.76 x 17.82 mm (full 4:3 area)

| Mode | Resolution | Sensor Area (mm) | Max FPS |
|------|-----------|-----------------|---------|
| Data 4:3 (ARRIRAW) | 2880 x 2160 | 23.76 x 17.82 | 30 (limited to 23.976/24/25/29.97/30 fps) |
| HD 16:9 | 1920 x 1080 (downsampled from 2880x1620) | 23.76 x 13.37 | 60 |

- **Sources**:
  - [VFX Camera Database - D-20](https://vfxcamdb.com/arri-arriflex-d-20/)
  - [Wikipedia - Arriflex D-20](https://en.wikipedia.org/wiki/Arriflex_D-20)
- **Notes**: The D-20 hardware supports 1-60 fps but Data/RAW mode is restricted to standard film/broadcast rates. HD mode downsamples from 2880x1620 to 1920x1080.

---

### Arriflex D-21
- **Confidence**: MEDIUM (2 independent sources)
- **Sensor**: CMOS with Bayer pattern, ALEV I
- **Sensor dimensions**: 23.76 x 17.82 mm (full 4:3 area)

| Mode | Resolution | Sensor Area (mm) | Max FPS |
|------|-----------|-----------------|---------|
| Data 4:3 (ARRIRAW) | 2880 x 2160 | 23.76 x 17.82 | 25 |
| Data 16:9 (ARRIRAW) | 2880 x 1620 | 23.76 x 13.37 | 30 |
| HD 16:9 | 1920 x 1080 (downsampled from 2880x1620) | 23.76 x 13.37 | 60 |

- **Sources**:
  - [VFX Camera Database - D-21](https://vfxcamdb.com/arri-arriflex-d-21/)
  - [Cameraquip - D-21](http://www.cameraquip.com.au/rentals/digital-cameras/arriflex-d-21/)
  - [ARRI D-21 Technical Data](https://www.arri.com/resource/blob/75852/a4ef1fa0e3f6183dcc1027a68a429a4c/1-7-0-arriflex-d-21-data.pdf)
- **Notes**: The D-21 also has an "M-scope" mode. D-21 HD variant is limited to 30 fps. Frame rates are crystal controlled, settable to 0.001 fps precision.

---

### ALEXA Classic, ALEXA Plus, ALEXA Plus 4:3, ALEXA Studio, ALEXA M
- **Confidence**: HIGH (multiple sources agree on key specs)
- **Sensor**: ALEV III CMOS with Bayer pattern
- **Sensor maximum photosites**: 3424 x 2202
- **Sensor physical size**: ~28.25 x 18.17 mm (full Open Gate area)

All ALEXA Classic family cameras share the same ALEV III sensor. Key differences:
- **ALEXA Classic / ALEXA Plus**: 16:9 sensor mode only. Plus adds LDS, wireless, lens motors.
- **ALEXA Plus 4:3, ALEXA Studio, ALEXA M**: Add 4:3 sensor mode for anamorphic shooting.
- **ALEXA Studio**: Adds optical viewfinder with mechanical mirror shutter.
- **ALEXA M**: Miniaturized remote head; sensor separated from body via cable.
- Standard fps is 0.75-60. High Speed license unlocks up to 120 fps.

| Mode | Resolution | Sensor Area (mm) | Max FPS | Available On |
|------|-----------|-----------------|---------|-------------|
| 16:9 2.8K (ARRIRAW external) | 2880 x 1620 | 23.76 x 13.37 | 60 | All Classic models |
| 16:9 HD (ProRes) | 1920 x 1080 | 23.76 x 13.37 | 120 (w/ High Speed license) | All Classic models |
| 16:9 2K (ProRes) | 2048 x 1152 | 23.76 x 13.37 | 60 | All Classic models |
| 4:3 2.8K (ARRIRAW external) | 2880 x 2160 | 23.76 x 17.82 | 60 | Plus 4:3, Studio, M |
| 4:3 HD (ProRes) | 1920 x 1080 | 23.76 x 17.82 | 60 | Plus 4:3, Studio, M |

- **Sources**:
  - [ARRI ALEXA Classic Technical Data](https://www.arri.com/resource/blob/75886/2653b2727b6204327852e34f0eb5344b/1-2-3-alexa-classic-data.pdf)
  - [VA Hire - ALEXA Classic EV](https://www.vahire.com/product/arri-alexa-classic-ev)
  - [VFX Camera Database - ALEXA M](https://vfxcamdb.com/arri-alexa-m/)
  - [ARRI ALEXA Classic & XT Recording Areas](https://www.arri.com/resource/blob/178012/d6a32bdbaee788486bce45ec1de9e4f1/alexa-classic-and-xt-recording-areas-surround-views-framelines-sup-11-data.pdf)
- **Notes**: ALEXA Classic cameras cannot record ARRIRAW internally -- they output via HD-SDI T-link to external recorders. ProRes recorded internally to SxS cards. The 120 fps High Speed mode is ProRes HD 16:9 only (422 HQ or lower). ALEXA Studio mirror shutter limits some frame rates (artifacts above 37 fps in 4:3, above 47 fps in 16:9 when mirror shutter is active).

---

### ALEXA XT, ALEXA XT Plus, ALEXA XT Studio
- **Confidence**: HIGH (multiple sources agree)
- **Sensor**: ALEV III CMOS with Bayer pattern (same as Classic)
- **Sensor physical size**: 28.25 x 18.17 mm (full Open Gate area)

The XT family adds in-camera ARRIRAW recording to XR Capture Drives, plus Open Gate mode (XT exclusive, not on Classic).

- **ALEXA XT**: 16:9 sensor mode. No 4:3.
- **ALEXA XT Plus**: 16:9 + 4:3 sensor modes. Adds LDS, wireless.
- **ALEXA XT Studio**: 16:9 + 4:3 + Open Gate. Adds optical viewfinder with mirror shutter.

| Mode | Resolution | Sensor Area (mm) | Max FPS | Available On |
|------|-----------|-----------------|---------|-------------|
| Open Gate (ARRIRAW) | 3414 x 2198 | 28.17 x 18.13 | 75 | XT Studio only |
| 4:3 2.8K (ARRIRAW) | 2880 x 2160 | 23.76 x 17.82 | 96 | XT Plus, XT Studio |
| 16:9 2.8K (ARRIRAW) | 2880 x 1620 | 23.76 x 13.37 | 120 | All XT models |
| 16:9 3.2K (ARRIRAW) | 3164 x 1778 | 26.14 x 14.70 | 120 | All XT models |
| 16:9 HD (ProRes) | 1920 x 1080 | 23.76 x 13.37 | 120 | All XT models |
| 16:9 2K (ProRes) | 2048 x 1152 | 23.66 x 13.32 | 60 | All XT models |
| 4:3 2K (ProRes/ARRIRAW) | 2048 x 1536 | 23.66 x 17.75 | 60 | XT Plus, XT Studio |

- **Sources**:
  - [VFX Camera Database - ALEXA XT](https://vfxcamdb.com/arri-alexa-xt/)
  - [VFX Camera Database - ALEXA XT Plus](https://vfxcamdb.com/arri-alexa-xt-plus/)
  - [ARRI ALEXA XT Open Gate White Paper](https://www.arri.com/resource/blob/178018/d685e369d764aa36e4a32eab9e227adc/alexa-xt-open-gate-white-paper-data.pdf)
  - [AbelCine - ALEXA XT Open Gate](https://www.abelcine.com/articles/blog-and-knowledge/tech-news/arri-alexa-xt-open-gate-and-resolve-support)
- **Notes**: Open Gate mode is ARRIRAW only (no ProRes). ALEXA XT Studio mirror shutter causes artifacts at high fps when active. The 120 fps requires High Speed license on non-XT-Plus models.

---

### AMIRA
- **Confidence**: HIGH (3 sources agree)
- **Sensor**: ALEV III CMOS with Bayer pattern (same as ALEXA)
- **Sensor maximum photosites**: 3424 x 2202

| Mode | Resolution | Sensor Area (mm) | Max FPS |
|------|-----------|-----------------|---------|
| 4K UHD 16:9 | 3840 x 2160 | 26.40 x 14.85 | 60 |
| 3.2K 16:9 | 3200 x 1800 | 26.40 x 14.85 | 60 |
| ARRIRAW 2.8K 16:9 | 2880 x 1620 | 23.76 x 13.37 | 48 |
| 2K 16:9 | 2048 x 1152 | 23.66 x 13.30 | 200 |
| HD 16:9 | 1920 x 1080 | 23.76 x 13.37 | 200 |
| S16 HD 16:9 | 1920 x 1080 | 13.20 x 7.43 | 200 |

- **Sources**:
  - [Offshoot Rentals - AMIRA Sensor Modes](https://offshoot.rentals/articles/arri-amira-sensor-modes)
  - [ARRI AMIRA Technical Data](https://www.videocineimport.com/wp-content/uploads/2020/03/Technical-Data-AMIRA.pdf)
  - [ARRI AMIRA Product Page](https://www.arri.com/en/camera-systems/cameras/amira)
- **Notes**: AMIRA is 16:9 only (no 4:3 or Open Gate). ARRIRAW 2.8K requires optional license key. 4K UHD requires optional license. 200 fps available in 2K, HD, and S16 modes.

---

### ALEXA Mini
- **Confidence**: HIGH (detailed source with all modes)
- **Sensor**: ALEV III CMOS with Bayer pattern
- **Sensor maximum photosites**: 3424 x 2202

| Mode | Resolution | Sensor Area (mm) | Max FPS |
|------|-----------|-----------------|---------|
| Open Gate 3.4K (ARRIRAW) | 3424 x 2202 | 28.25 x 18.17 | 30 |
| ARRIRAW 2.8K 16:9 | 2880 x 1620 | 23.76 x 13.37 | 48 |
| 4:3 2.8K | 2880 x 2160 | 23.76 x 17.82 | 50 |
| 4K UHD 16:9 | 3840 x 2160 | 26.40 x 14.85 | 60 |
| 3.2K 16:9 | 3200 x 1800 | 26.40 x 14.85 | 60 |
| 2.39:1 2K Anamorphic | 2048 x 858 | 21.12 x 17.70 | 120 |
| HD Anamorphic | 1920 x 1080 | 15.84 x 17.82 | 120 |
| 2K 16:9 | 2048 x 1152 | 23.66 x 13.30 | 200 |
| HD 16:9 | 1920 x 1080 | 23.76 x 13.37 | 200 |
| S16 HD 16:9 | 1920 x 1080 | 13.20 x 7.43 | 200 |

- **Sources**:
  - [Offshoot Rentals - ALEXA Mini Sensor Modes](https://offshoot.rentals/articles/arri-alexa-mini-sensor-modes)
  - [ARRI ALEXA Mini Product Page](https://www.arri.com/en/camera-systems/cameras/legacy-camera-systems/alexa-mini)
- **Notes**: Open Gate is ARRIRAW only and limited to 30 fps. The Mini is one of the most versatile ALEXA Classic-era cameras with 10 sensor modes. S16 mode uses a Super 16-sized crop for vintage lens compatibility.

---

### ALEXA 65
- **Confidence**: MEDIUM (resolution/modes confirmed by 3+ sources, but per-mode max fps for crop modes is sparse)
- **Sensor**: A3X (three vertically tiled ALEV III sensors)
- **Sensor physical size**: 54.12 x 25.58 mm (5-perf 65mm format)
- **Recording**: ARRIRAW only

| Mode | Resolution | Sensor Area (mm) | Max FPS |
|------|-----------|-----------------|---------|
| Open Gate 6.5K | 6560 x 3100 | 54.12 x 25.58 | 60 |
| 16:9 (1.78:1 Crop) | 5120 x 2880 | 42.24 x 23.76 | 60 |
| 3:2 (1.50:1 Crop) | 4320 x 2880 | 35.64 x 23.76 | 60 |
| LF Open Gate | 4448 x 3096 | 36.70 x 25.54 | 60 |
| 4K UHD | 3840 x 2160 | (not specified) | 60 |

- **Sources**:
  - [ARRI Rental - ALEXA 65](https://www.arrirental.com/en/cameras/digital-65-mm-cameras/alexa-65)
  - [VFX Camera Database - ALEXA 65](https://vfxcamdb.com/arri-alexa-65/)
  - [Illumination Dynamics - ALEXA 65](https://www.illuminationdynamics.com/alexa-65)
- **Notes**: Rental-only camera (not sold). Open Gate max fps was initially 27 fps with XR Capture Drives, later upgraded to 60 fps with SXR Capture Drives. All modes ARRIRAW only. Per-mode max fps for crop modes is not well documented; sources generally cite 60 fps as the camera's overall maximum. Crop modes produce less data so likely match or exceed Open Gate fps.

---

### ALEXA SXT Plus, ALEXA SXT W
- **Confidence**: HIGH (ARRI FAQ is authoritative, multiple corroborating sources)
- **Sensor**: ALEV III CMOS with Bayer pattern
- **Sensor maximum photosites**: 3424 x 2202
- **Sensor physical size**: 28.25 x 18.17 mm

The SXT family is the successor to the XT, with improved image processing and in-camera ProRes upscaling to 4K UHD. SXT Plus includes LDS and wireless; SXT W adds a built-in wireless video transmitter. Both share identical sensor modes and frame rates.

**16:9 Sensor Mode:**

| Recording Format | Resolution | Sensor Area (mm) | Max FPS |
|-----------------|-----------|-----------------|---------|
| ARRIRAW 16:9 3.2K | 3200 x 1800 | 26.40 x 14.85 | 120 |
| ARRIRAW 16:9 2.8K | 2880 x 1620 | 23.76 x 13.37 | 120 |
| ProRes 4K UHD | 3840 x 2160 | 26.40 x 14.85 | 60 |
| ProRes 3.2K | 3200 x 1800 | 26.40 x 14.85 | 60 |
| ProRes 2K | 2048 x 1152 | 23.76 x 13.37 | 120 |
| ProRes HD | 1920 x 1080 | 23.76 x 13.37 | 120 |

**6:5 Sensor Mode (Anamorphic):**

| Recording Format | Resolution | Sensor Area (mm) | Max FPS |
|-----------------|-----------|-----------------|---------|
| ARRIRAW 6:5 | 2578 x 2160 | 21.38 x 17.82 | 96 |
| ProRes 4K Cine Anamorphic | 4096 x 1716 | 21.12 x 17.70 | 60 |
| ProRes 2K Anamorphic | 2048 x 858 | 21.12 x 17.70 | 96 |

**4:3 Sensor Mode:**

| Recording Format | Resolution | Sensor Area (mm) | Max FPS |
|-----------------|-----------|-----------------|---------|
| ARRIRAW 4:3 | 2880 x 2160 | 23.76 x 17.82 | 96 |
| ProRes 2.8K | 2880 x 2160 | 23.76 x 17.82 | 96 |

**Open Gate Sensor Mode:**

| Recording Format | Resolution | Sensor Area (mm) | Max FPS |
|-----------------|-----------|-----------------|---------|
| ARRIRAW Open Gate | 3424 x 2202 | 28.25 x 18.17 | 90 |
| ProRes 4K Cine Open Gate | 4096 x 2636 | 28.17 x 18.13 | 60 |
| ProRes 3.4K Open Gate | 3424 x 2202 | 28.25 x 18.17 | 90 |

- **Sources**:
  - [ARRI ALEXA SXT FAQ](https://www.arri.com/en/learn-help/learn-help-camera-system/frequently-asked-questions/alexa-sxt-faq)
  - [VFX Camera Database - ALEXA SXT](https://vfxcamdb.com/arri-alexa-sxt/)
  - [ARRI ALEXA SXT W Product Page](https://www.arri.com/en/camera-systems/cameras/legacy-camera-systems/alexa-sxt-w)
  - [ALEXA SXT Plus Technical Data](https://www.videofax.com/userfiles/file/ALEXASXTPlusTechnicalData.pdf)
- **Notes**: The SXT FAQ is the authoritative source for max fps per mode. ProRes 4K UHD is generated by upscaling the 3.2K 16:9 sensor readout. The 3168 x 1782 resolution variant exists as an intermediate format.

---

### ALEXA LF
- **Confidence**: HIGH (ARRI FAQ is authoritative)
- **Sensor**: ALEV III LF CMOS with Bayer pattern
- **Sensor maximum photosites**: 4448 x 3096
- **Sensor physical size**: 36.70 x 25.54 mm

| Mode | Resolution | Sensor Area (mm) | Max FPS |
|------|-----------|-----------------|---------|
| LF Open Gate (3:2) | 4448 x 3096 | 36.70 x 25.54 | 90 |
| LF 16:9 (UHD) | 3840 x 2160 | 31.68 x 17.82 | 90 |
| LF 2.39:1 | 4448 x 1856 | 36.70 x 15.31 | 150 |

- **Sources**:
  - [ARRI ALEXA LF FAQ](https://www.arri.com/en/learn-help/learn-help-camera-system/frequently-asked-questions/alexa-lf-faq)
  - [ARRI ALEXA LF Product Page](https://www.arri.com/en/camera-systems/cameras/alexa-lf)
- **Notes**: Records to SXR Capture Drives. ARRIRAW and ProRes available. Actual max fps depends on recording medium and codec. The LF 2.39:1 widescreen crop allows significantly higher frame rates due to reduced data. The ALEXA LF was the first ARRI camera with a large-format sensor.

---

### ALEXA Mini LF
- **Confidence**: HIGH (detailed source with all 9 modes)
- **Sensor**: ALEV III LF CMOS with Bayer pattern (same as ALEXA LF)
- **Sensor maximum photosites**: 4448 x 3096
- **Sensor physical size**: 36.70 x 25.54 mm

| Mode | Resolution | Sensor Area (mm) | Max FPS |
|------|-----------|-----------------|---------|
| 4.5K LF 3:2 Open Gate | 4448 x 3096 | 36.70 x 25.54 | 40 |
| 4.5K LF 2.39:1 | 4448 x 1856 | 36.70 x 15.31 | 60 |
| 4.3K LF 16:9 | 3840 x 2160 (UHD) | 35.64 x 20.05 | 48 |
| 3.8K LF 16:9 | 3840 x 2160 (UHD) | 31.68 x 17.82 | 60 |
| 2.8K LF 1:1 | 2880 x 2880 | 23.76 x 23.76 | 60 |
| 3.4K S35 3:2 | 3424 x 2202 | 28.25 x 18.17 | 60 |
| 3.2K S35 16:9 | 3200 x 1800 | 26.40 x 14.85 | 75 |
| 3.2K S35 4:3 | 2880 x 2160 | 23.76 x 17.82 | 75 |
| 2.8K S35 16:9 | 2880 x 1620 | 23.76 x 13.37 | 100 |

- **Sources**:
  - [Offshoot Rentals - ALEXA Mini LF Sensor Modes](https://offshoot.rentals/articles/arri-alexa-mini-lf-recording-resolutions)
  - [ARRI ALEXA Mini LF Technical Data Sheet](https://www.arri.com/resource/blob/261224/0654da7b1dda042b23b1b6c254c1f8c3/alexa-%20mini-lf-flyer-technical-data-sheet-data.pdf)
  - [ARRI ALEXA Mini LF FAQ](https://www.arri.com/en/learn-help/learn-help-camera-system/frequently-asked-questions/alexa-mini-lf-faq)
- **Notes**: The Mini LF has lower max fps than the full ALEXA LF in most modes (40 vs 90 in Open Gate) due to its more compact processing pipeline. The S35 modes provide backward compatibility with Super 35 lenses. Max fps varies by codec (ARRIRAW vs ProRes) -- the numbers above represent the maximum achievable in any codec for each mode.

---

### ALEXA 35
- **Confidence**: HIGH (multiple detailed sources)
- **Sensor**: ALEV 4 CMOS with Bayer pattern (new generation)
- **Sensor maximum photosites**: 4608 x 3164
- **Sensor physical size**: 27.99 x 19.22 mm (Super 35)
- **Dynamic range**: 17 stops
- **Color science**: REVEAL

| Mode | Resolution | Sensor Area (mm) | Max FPS (ARRIRAW 2TB) | Max FPS (ARRIRAW 1TB) | Max FPS (ProRes) |
|------|-----------|-----------------|----------------------|----------------------|-----------------|
| 4.6K 3:2 Open Gate | 4608 x 3164 | 28.0 x 19.2 | 75 | 35 | 60 |
| 4.6K 16:9 | 4608 x 2592 | 28.0 x 15.7 | 75 | 45 | 75 |
| 4K 16:9 | 4096 x 2304 | 24.9 x 14.0 | 120 | 55 | 100 |
| 4K 2:1 | 4096 x 2048 | 24.9 x 12.4 | 120 | 65 | 120 |
| 3.8K 16:9 (UHD) | 3840 x 2160 | 23.3 x 13.1 | 120 | (not documented) | 120 |
| 3.3K 6:5 | 3328 x 2790 | 20.22 x 16.95 | 100 | 55 | 75 |
| 3K 1:1 | 3072 x 3072 | 18.7 x 18.7 | 100 | 55 | 90 |
| 2.7K 8:9 | 2743 x 3086 | 16.7 x 18.7 | (not documented) | (not documented) | 100 |
| 2K 16:9 S16 | 2048 x 1152 | 12.4 x 7.0 | 120 | 120 | 120 |

- **Sources**:
  - [Offshoot Rentals - ALEXA 35 Sensor Modes](https://offshoot.rentals/articles/arri-alexa-35-sensor-modes)
  - [ARRI ALEXA 35 Product Page](https://www.arri.com/en/camera-systems/cameras/legacy-camera-systems/alexa-35)
  - [Panavision - ALEXA 35](https://www.panavision.com/camera-and-optics/cameras/product-detail/alex35-alexa-35)
  - [Chater Camera - ALEXA 35](https://www.chatercamera.com/content/arri-alexa-35-46k)
- **Notes**: The ALEXA 35 uses the new ALEV 4 sensor -- first new ARRI sensor since the original ALEXA. 2TB Codex Compact Drives are required for the highest frame rates. ProRes frame rates are the same regardless of 1TB vs 2TB drive. The 2.7K 8:9 mode outputs a de-squeezed UHD 16:9 image for anamorphic workflows. The 3.8K UHD mode was added later via firmware update.

---

### ALEXA 265
- **Confidence**: MEDIUM (announced Dec 2024, limited detailed specifications published)
- **Sensor**: A3X Rev.B CMOS with Bayer pattern (revised ALEXA 65 sensor)
- **Sensor maximum photosites**: 6560 x 3100
- **Sensor physical size**: 54.12 x 25.58 mm
- **Dynamic range**: 15 stops
- **Recording**: ARRIRAW only
- **Rental-only camera** (through ARRI Rental)

| Mode | Resolution | Sensor Area (mm) | Max FPS |
|------|-----------|-----------------|---------|
| 6.5K 2.12:1 Open Gate | 6560 x 3100 | 54.12 x 25.58 | 60 |
| 5.1K 1.65:1 | 5120 x 3100 | 42.24 x 25.58 | 60 |
| 4.5K LF 3:2 (LF OG) | 4448 x 3096 | 36.70 x 25.54 | 60 |

- **Sources**:
  - [ARRI Rental - ALEXA 265](https://www.arrirental.com/en/cameras/digital-65-mm-cameras/alexa-265)
  - [Y.M.Cinema - ALEXA 265 FAQ](https://ymcinema.com/2024/12/06/arri-alexa-265-faq-sample-footage-and-concept/)
  - [ARRI Press Release - ALEXA 265](https://www.arri.com/en/company/press/press-releases-2024/arri-announces-the-small-and-lightweight-alexa-265-camera-revolutionizing-65-mm-cinematography)
- **Notes**: The ARRI Rental page states 29/60 fps for 6.5K OG and 37/60 fps for 5.1K -- the first number may represent a standard drive max and the second an upgraded drive max. The 4.5K LF mode matches ALEXA Mini LF Open Gate resolution exactly, enabling lens/workflow compatibility. Codex HDE (High Density Encoding) reduces file sizes ~40%.

---

### ALEXA 35 Xtreme
- **Confidence**: MEDIUM-HIGH (announced July 2025, detailed specs from ARRI website and press coverage)
- **Sensor**: ALEV 4 CMOS with Bayer pattern (same as ALEXA 35)
- **Sensor maximum photosites**: 4608 x 3164
- **Sensor physical size**: 27.99 x 19.22 mm (Super 35)
- **Dynamic range**: 17 stops (standard) / 11 stops (Sensor Overdrive)
- **Color science**: REVEAL
- **New codecs**: ARRICORE (RGB, 50% smaller than ARRIRAW)

**Standard Mode (full 17-stop dynamic range):**

| Mode | Resolution | Sensor Area (mm) | Max FPS |
|------|-----------|-----------------|---------|
| 4.6K 3:2 Open Gate | 4608 x 3164 | 28.0 x 19.2 | 120 |
| 4.6K 16:9 | 4608 x 2592 | 28.0 x 15.7 | 120 |
| 4K 16:9 | 4096 x 2304 | 24.9 x 14.0 | 150 |
| 3.8K 16:9 (UHD) | 3840 x 2160 | 23.3 x 13.1 | 150 |
| 3.8K 2.39:1 (NEW) | 3840 x 1608 | 23.3 x 9.8 | 240 |
| 3.3K 6:5 | 3328 x 2790 | 20.22 x 16.95 | (not specified) |
| 2K 16:9 S16 | 2048 x 1152 | 12.4 x 7.0 | 330 |
| HD 16:9 S16 | 1920 x 1080 | 11.7 x 6.6 | 330 |

**Sensor Overdrive Mode (11-stop dynamic range, EI 1600 base):**

| Mode | Resolution | Max FPS (Overdrive) |
|------|-----------|-------------------|
| 4.6K 3:2 Open Gate | 4608 x 3164 | 165 |
| 4.6K 16:9 | 4608 x 2592 | 165 |
| 3.8K 16:9 (UHD) | 3840 x 2160 | 240 |
| 3.8K 2.39:1 | 3840 x 1608 | 240 |
| 2K 16:9 S16 | 2048 x 1152 | 660 |
| HD 16:9 S16 | 1920 x 1080 | 660 |

- **Sources**:
  - [ARRI ALEXA 35 Xtreme Product Page](https://www.arri.com/en/camera-systems/cameras/alexa-35-xtreme)
  - [Newsshooter - ALEXA 35 Xtreme](https://www.newsshooter.com/2025/07/31/arri-alexa-35-xtreme/)
  - [CineD - ALEXA 35 Xtreme](https://www.cined.com/arri-alexa-35-xtreme-unveiled-up-to-660fps-and-new-arricore-codec/)
  - [Digital Production - ALEXA 35 Xtreme](https://digitalproduction.com/2025/08/01/arri-alexa-35-xtreme-frame-rates-up-data-rates-down-everything-else-still-arri/)
- **Notes**: Sensor Overdrive reduces dynamic range by 6 stops (from 17 to 11) and shifts base sensitivity to EI 1600. The new 3.8K 2.39:1 mode is exclusive to the Xtreme. ARRICORE codec is available on both ALEXA 35 and Xtreme via firmware update. Recording formats: ARRIRAW, ARRICORE, ProRes 4444 XQ / 4444 / 422 HQ. The "Base License" caps at 60 fps; Premium or additional licenses unlock higher rates.

---

## Contradictions

### ALEXA 65 Maximum Frame Rate in Open Gate
- **ARRI Rental page** and **Illumination Dynamics** cite 60 fps as max in Open Gate (with SXR drive).
- **Wolfcrow** and early press coverage cite 20-28 fps initially, with 60 fps as a planned upgrade.
- **Resolution**: The 60 fps figure is the current specification after firmware/drive upgrades. Early units were limited to ~27 fps.

### ALEXA Classic 16:9 ARRIRAW Max FPS
- Some sources imply 60 fps for ARRIRAW via T-link external recording.
- Other sources suggest only ProRes HD reached 120 fps, with ARRIRAW potentially limited to 60 fps.
- **Resolution**: The Classic outputs ARRIRAW externally, so the max fps depends on the external recorder's capabilities, but the sensor readout in 16:9 supports up to 120 fps (for ProRes). ARRIRAW external was typically limited to 60 fps by T-link bandwidth.

### ALEXA XT 16:9 ARRIRAW Max FPS
- Multiple sources cite 120 fps for 16:9 ARRIRAW on XT cameras.
- The SXT FAQ explicitly states "16:9 ARRIRAW up to 120 fps" which applies to SXT; XT may have been slightly lower.
- **Resolution**: Both XT and SXT support 120 fps in 16:9 ARRIRAW, confirmed by multiple sources. The XT internal recording to XR Capture Drives supports this rate.

### ALEXA 35 Xtreme Open Gate Resolution
- **ARRI product page** lists 4608 x 3164 for Open Gate.
- **Newsshooter** lists 4608 x 3080 for Open Gate.
- **Resolution**: 4608 x 3164 is the correct sensor photosite count from ARRI's official spec. The 3080 figure may represent a slightly different active recording area or may be an error in the press coverage.

---

## Source Registry

| # | Title | URL | Date | Queries |
|---|-------|-----|------|---------|
| 1 | VFX Camera Database - Arriflex D-20 | https://vfxcamdb.com/arri-arriflex-d-20/ | n/a | Q6 |
| 2 | VFX Camera Database - Arriflex D-21 | https://vfxcamdb.com/arri-arriflex-d-21/ | n/a | Q6 |
| 3 | Offshoot Rentals - ALEXA Mini LF Sensor Modes | https://offshoot.rentals/articles/arri-alexa-mini-lf-recording-resolutions | n/a | Q2 |
| 4 | Offshoot Rentals - ALEXA 35 Sensor Modes | https://offshoot.rentals/articles/arri-alexa-35-sensor-modes | n/a | Q3 |
| 5 | Offshoot Rentals - AMIRA Sensor Modes | https://offshoot.rentals/articles/arri-amira-sensor-modes | n/a | Q5 |
| 6 | Offshoot Rentals - ALEXA Mini Sensor Modes | https://offshoot.rentals/articles/arri-alexa-mini-sensor-modes | n/a | Q5 |
| 7 | ARRI ALEXA LF FAQ | https://www.arri.com/en/learn-help/learn-help-camera-system/frequently-asked-questions/alexa-lf-faq | n/a | Q9 |
| 8 | ARRI ALEXA SXT FAQ | https://www.arri.com/en/learn-help/learn-help-camera-system/frequently-asked-questions/alexa-sxt-faq | n/a | Q8 |
| 9 | VFX Camera Database - ALEXA SXT | https://vfxcamdb.com/arri-alexa-sxt/ | n/a | Q8 |
| 10 | VFX Camera Database - ALEXA XT | https://vfxcamdb.com/arri-alexa-xt/ | n/a | Q7 |
| 11 | VFX Camera Database - ALEXA XT Plus | https://vfxcamdb.com/arri-alexa-xt-plus/ | n/a | Q7 |
| 12 | VFX Camera Database - ALEXA M | https://vfxcamdb.com/arri-alexa-m/ | n/a | Q1 |
| 13 | ARRI Rental - ALEXA 65 | https://www.arrirental.com/en/cameras/digital-65-mm-cameras/alexa-65 | n/a | Q4 |
| 14 | VFX Camera Database - ALEXA 65 | https://vfxcamdb.com/arri-alexa-65/ | n/a | Q4 |
| 15 | Illumination Dynamics - ALEXA 65 | https://www.illuminationdynamics.com/alexa-65 | n/a | Q4 |
| 16 | ARRI Rental - ALEXA 265 | https://www.arrirental.com/en/cameras/digital-65-mm-cameras/alexa-265 | 2024 | Q10 |
| 17 | Y.M.Cinema - ALEXA 265 FAQ | https://ymcinema.com/2024/12/06/arri-alexa-265-faq-sample-footage-and-concept/ | 2024-12 | Q10 |
| 18 | ARRI - ALEXA 35 Xtreme Product Page | https://www.arri.com/en/camera-systems/cameras/alexa-35-xtreme | 2025 | Q11 |
| 19 | Newsshooter - ALEXA 35 Xtreme | https://www.newsshooter.com/2025/07/31/arri-alexa-35-xtreme/ | 2025-07 | Q11 |
| 20 | CineD - ALEXA 35 Xtreme | https://www.cined.com/arri-alexa-35-xtreme-unveiled-up-to-660fps-and-new-arricore-codec/ | 2025-07 | Q11 |
| 21 | VA Hire - ALEXA Classic EV | https://www.vahire.com/product/arri-alexa-classic-ev | n/a | Q1 |
| 22 | ARRI ALEXA Mini LF FAQ | https://www.arri.com/en/learn-help/learn-help-camera-system/frequently-asked-questions/alexa-mini-lf-faq | n/a | Q2 |
| 23 | ARRI Press Release - ALEXA 265 | https://www.arri.com/en/company/press/press-releases-2024/arri-announces-the-small-and-lightweight-alexa-265-camera-revolutionizing-65-mm-cinematography | 2024-12 | Q10 |
| 24 | Panavision - ALEXA 35 | https://www.panavision.com/camera-and-optics/cameras/product-detail/alex35-alexa-35 | n/a | Q3 |
| 25 | ARRI ALEXA XT Open Gate White Paper | https://www.arri.com/resource/blob/178018/d685e369d764aa36e4a32eab9e227adc/alexa-xt-open-gate-white-paper-data.pdf | 2014-12 | Q7 |
| 26 | Chater Camera - ALEXA 35 | https://www.chatercamera.com/content/arri-alexa-35-46k | n/a | Q3 |
| 27 | Digital Production - ALEXA 35 Xtreme | https://digitalproduction.com/2025/08/01/arri-alexa-35-xtreme-frame-rates-up-data-rates-down-everything-else-still-arri/ | 2025-08 | Q11 |
