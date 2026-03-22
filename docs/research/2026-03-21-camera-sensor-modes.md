# Raw Research Findings: Camera Sensor/Recording Modes

## Queries Executed
1. "Phantom Flex high speed camera specifications sensor modes resolution max frame rate fps" - 3 useful results
2. "Phantom Flex4K specifications resolution frame rate sensor modes" - 4 useful results
3. "Phantom v2512 v2640 specifications resolution vs frame rate" - 3 useful results
4. "Phantom VEO 640 VEO4K-PL VEO4K 990 specifications resolution fps" - 3 useful results
5. "Phantom Miro M320S specifications resolution frame rate" - 3 useful results
6. "Phantom TMX 7510 specifications resolution frame rate datasheet" - 3 useful results
7. "Phantom T4040 T2540 T1340 specifications resolution fps" - 4 useful results
8. "Phantom VEO 1310 specifications resolution frame rate" - 2 useful results
9. "Panavision Genesis digital camera specifications sensor resolution" - 2 useful results
10. "Panavision Millennium DXL DXL2 DXL-M specifications sensor modes resolution" - 3 useful results
11. "RED Monstro 8K VV sensor modes resolution max frame rate fps" - 2 useful results
12. "RED Dragon 8K VV sensor frame rate resolution table" - 1 useful result

---

## VISION RESEARCH (PHANTOM) CAMERAS

---

### Phantom Flex

- **Confidence**: HIGH
- **Sensor**: CMOS, 25.6 x 16.0 mm (30.2 mm diagonal), pixel size 10 um
- **Max Resolution**: 2560 x 1600
- **Bit Depth**: 12-bit
- **Shutter**: Global

Standard Mode:

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor | 2560 x 1600 | 25.6 x 16.0 | 1,456 |
| 2.5K 1.78:1 | 2560 x 1440 | 25.6 x 14.4 | 1,617 |
| 2K 1.78:1 | 2048 x 1152 | 20.5 x 11.5 | 2,405 |
| HD 1080p | 1920 x 1080 | 19.2 x 10.8 | 2,564 |
| Anamorphic 2:1 | 1920 x 1600 | 19.2 x 16.0 | 1,736 |
| 720p | 1280 x 720 | 12.8 x 7.2 | 5,355 |

Note: HQ Mode runs at approximately half the standard frame rates (e.g., 1,275 fps at 1920x1080, 2,640 fps at 1280x720).

- **Supporting sources**:
  - [Love High Speed - Phantom Flex](https://www.lovehighspeed.com/high-speed-cameras/phantom-flex/) - Full resolution/fps table with sensor area per mode
  - [AbelCine - VRI Phantom Flex](https://www.abelcine.com/rent/cameras-accessories/digital-cinema-cameras/vri-phantom-flex-high-speed-digital-camera) - Confirms max fps tiers

---

### Phantom Miro M320S

- **Confidence**: HIGH
- **Sensor**: CMOS, 19.2 x 12.0 mm, pixel size 10 um
- **Max Resolution**: 1920 x 1200
- **Bit Depth**: 12-bit
- **Shutter**: Global
- **Throughput**: 3.2 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor | 1920 x 1200 | 19.2 x 12.0 | 1,320 |
| HD 1080p | 1920 x 1080 | 19.2 x 10.8 | 1,540 |
| Square | 1152 x 1152 | 11.5 x 11.5 | 2,250 |

- **Supporting sources**:
  - [AbelCine - VRI Introduces Phantom Miro M320S](https://www.abelcine.com/articles/news/product-news/vri-introduces-the-phantom-miro-m320s-high-speed-camera) - Resolution/fps data
  - [Studio Daily - Phantom Miro M320S](https://www.studiodaily.com/2012/04/phantoms-miro-m320s-shoots-1080p-at-1540fps/) - Confirms 1540fps at 1080p
  - [Y.M.Cinema - Phantom Miro LC320S Tribute](https://ymcinema.com/2022/07/12/a-tribute-to-the-phantom-miro-lc320s-simplification-of-ultra-high-speed-cinematic-capturing/) - Sensor dimensions 19.2x12mm

---

### Phantom Flex4K

- **Confidence**: HIGH
- **Sensor**: CMOS, 27.6 x 15.5 mm (31.7 mm diagonal, Super 35 format), pixel size 6.75 um
- **Max Resolution**: 4096 x 2304
- **Bit Depth**: 12-bit
- **Shutter**: Rolling (Flex4K), Global (Flex4K-GS variant)
- **Throughput**: 8.85 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 4K | 4096 x 2304 | 27.6 x 15.5 | 938 |
| 4K DCI 1.9:1 | 4096 x 2160 | 27.6 x 14.6 | 1,000 |
| 2K | 2048 x 1080 | 13.8 x 7.3 | 1,984 |
| HD 1080p | 1920 x 1080 | 13.0 x 7.3 | 1,984 |
| 720p | 1280 x 720 | 8.6 x 4.9 | 2,949 |

- **Supporting sources**:
  - [Phantom High Speed - Flex4K Product Page](https://www.phantomhighspeed.com/products/cameras/4kmedia/flex4k) - Official specs (blocked, data from search snippet)
  - [Love High Speed - Phantom Flex4K](https://www.lovehighspeed.com/high-speed-cameras/phantom-flex4k/) - Confirms resolution/fps tiers
  - [AbelCine - Flex4K FAQs](https://www.abelcine.com/articles/blog-and-knowledge/tools-charts-and-downloads/phantom-flex4k-faqs) - Additional detail
- **Notes**: Active sensor area for sub-4K modes estimated from pixel size (6.75 um). The Flex4K crops the sensor (windowing) at lower resolutions rather than using the full area.

---

### Phantom VEO 640

- **Confidence**: HIGH
- **Sensor**: CMOS, 25.6 x 16.0 mm (30.2 mm diagonal), pixel size 10 um
- **Max Resolution**: 2560 x 1600
- **Bit Depth**: 12-bit
- **Shutter**: Global
- **Throughput**: 6 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor | 2560 x 1600 | 25.6 x 16.0 | 1,400 |
| HD 1080p | 1920 x 1080 | 19.2 x 10.8 | 2,800 |
| 720p | 1280 x 720 | 12.8 x 7.2 | 5,700 |

Maximum at reduced resolution: 300,000 fps.

- **Supporting sources**:
  - [Phantom High Speed - VEO 640](https://www.phantomhighspeed.com/products/cameras/veo/veo640) - Official product page (blocked, data from search snippet)
  - [AbelCine - VEO 640S](https://www.abelcine.com/buy/cameras-accessories/digital-cinema-cameras/phantom-veo-640s-digital-high-speed-camera) - Confirms specs
  - [Delta Photonics - VEO 640](https://deltaphotonics.com/product/veo-640/) - Throughput data

---

### Phantom v2512

- **Confidence**: HIGH
- **Sensor**: CMOS, 35.8 x 22.4 mm (42.2 mm diagonal), pixel size 28 um
- **Max Resolution**: 1280 x 800
- **Bit Depth**: 12-bit
- **Shutter**: Global
- **Throughput**: 25.7 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 1 Mpx | 1280 x 800 | 35.8 x 22.4 | 25,700 |
| Reduced | 128 x 16 | -- | 663,280 |
| FAST option | 256 x 32 | -- | 1,000,000 |

Note: The v2512's max resolution is 1280x800 (1 Mpx). It does NOT support 1920x1080 -- its full sensor is smaller than HD. This is a scientific/industrial ultra-high-speed camera, not a cinema camera. The 28 um pixel size gives exceptional light sensitivity but low resolution.

- **Supporting sources**:
  - [Phantom High Speed - v2512](https://www.phantomhighspeed.com/products/cameras/ultrahighspeed/v2512) - Official specs (blocked, data from search snippet)
  - [EMIN - v2512 listing](https://emin.com.mm/phantomvriv2512288gmagm-phantom-v2512-ultrahigh-speed-camera-25700fps-mono-288gb-cinemag-96074/pr.html) - Confirms 25,700 fps
  - [CN Rood - v2512](https://shop.cnrood.com/phantom-v2512-high-speed-camera) - Additional specs

---

### Phantom VEO4K-PL

- **Confidence**: HIGH
- **Sensor**: CMOS, ~27.6 x 15.5 mm (31.7 mm diagonal, Super 35 format), pixel size 6.75 um
- **Max Resolution**: 4096 x 2304
- **Bit Depth**: 12-bit
- **Shutter**: Global (also supports Rolling Shutter mode)
- **Throughput**: 8.85 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 4K | 4096 x 2304 | 27.6 x 15.5 | 938 |
| 4K DCI 1.85:1 | 4096 x 2160 | 27.6 x 14.6 | 1,000 |
| UHD | 3840 x 2160 | 25.9 x 14.6 | 1,000 |
| Anamorphic | 2752 x 2304 | 18.6 x 15.5 | 938 |
| HD 1080p | 1920 x 1080 | 13.0 x 7.3 | 1,977 |
| 720p | 1280 x 720 | 8.6 x 4.9 | 2,932 |

- **Supporting sources**:
  - [Love High Speed - VEO4K-PL](https://www.lovehighspeed.com/high-speed-cameras/phantom-veo4k-pl/) - Full resolution/fps table
  - [CN Rood - VEO4K-PL](https://shop.cnrood.com/veo4k-pl-camera) - Confirms specs
  - [SLAM Solutions - VEO4K-PL](https://www.corpslam.com/en/products/phantom-veo-4k-pl) - Additional confirmation
- **Notes**: The VEO4K-PL is essentially the cinema-focused packaging of the VEO4K 990 sensor, tailored for media production with PL mount, CFast storage, on-camera controls, and SDI outputs.

---

### Phantom VEO4K 990

- **Confidence**: HIGH
- **Sensor**: CMOS, ~27.6 x 15.5 mm (31.7 mm diagonal, Super 35 format), pixel size 6.75 um
- **Max Resolution**: 4096 x 2304 (9.4 Mpx)
- **Bit Depth**: 12-bit
- **Shutter**: Global (also supports Rolling Shutter mode)
- **Throughput**: 8.85 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 4K | 4096 x 2304 | 27.6 x 15.5 | 938 |
| 4K DCI 1.9:1 | 4096 x 2160 | 27.6 x 14.6 | 1,000 |
| 4K Cropped | 4096 x 1152 | 27.6 x 7.8 | 1,850 |
| Square 2K | 2048 x 2048 | 13.8 x 13.8 | 1,050 |
| 2K | 2048 x 1152 | 13.8 x 7.8 | 1,850 |
| 2K DCI | 2048 x 1080 | 13.8 x 7.3 | 1,970 |

Maximum at reduced resolution: 64,300 fps.

- **Supporting sources**:
  - [MC-S Special Camera Systems - VEO4K 990](https://highspeed-cameras.nl/product/phantom-veo4k-990/) - Detailed resolution/fps table
  - [CN Rood - VEO4K 990](https://www.cnrood.com/en/veo4k-990l-990s-cameras) - Confirms specs
- **Notes**: Same sensor as VEO4K-PL. The 990 designation refers to the throughput model. Sensor area calculated from pixel size (6.75 um x 4096 = 27.6 mm, 6.75 um x 2304 = 15.6 mm).

---

### Phantom v2640

- **Confidence**: HIGH
- **Sensor**: CMOS, 27.6 x 26.3 mm (38.1 mm diagonal), pixel size 13.5 um
- **Max Resolution**: 2048 x 1952 (4 Mpx)
- **Bit Depth**: 12-bit
- **Shutter**: Global
- **Throughput**: 26.2 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 4 Mpx | 2048 x 1952 | 27.6 x 26.3 | 6,600 |
| HD 1080p | 1920 x 1080 | 25.9 x 14.6 | 11,750 |
| 1 Mpx crop | 1024 x 976 | 13.8 x 13.2 | 14,740 |
| Widescreen | 1792 x 720 | 24.2 x 9.7 | 19,690 |
| 640x480 | 640 x 480 | 8.6 x 6.5 | 28,760 |
| Binned 1 Mpx (mono) | 1024 x 976 | 27.6 x 26.3 | 25,030 |

Maximum at reduced resolution: 303,460 fps.

- **Supporting sources**:
  - [DPReview - Phantom v2640](https://www.dpreview.com/news/4514783704/the-4mp-phantom-v2640-can-shoot-6-600fps-at-full-resolution-11-750fps-at-1920x1080) - Key fps/resolution data
  - [Phantom High Speed - v2640 Press Release](https://www.phantomhighspeed.com/news/newsarticles/2018/january/phantom-v2640) - Sensor size, resolution, fps data
  - [CineD - v2640 hands-on](https://www.cined.com/11750-fps-going-high-speed-with-the-phantom-v2640/) - Confirms 11,750 fps at 1080p
- **Notes**: Near-square sensor (27.6 x 26.3 mm) is unusual. Binning mode (monochrome only) converts to 1 Mpx equivalent with doubled sensitivity. Sensor area for cropped modes estimated from pixel pitch.

---

### Phantom VEO 1310

- **Confidence**: HIGH
- **Sensor**: CMOS, 23.0 x 17.3 mm, pixel size 18 um
- **Max Resolution**: 1280 x 960 (1.2 Mpx)
- **Bit Depth**: 12-bit
- **Shutter**: Global
- **Throughput**: 13 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 1.2 Mpx | 1280 x 960 | 23.0 x 17.3 | 10,860 |
| 720p-wide | 1280 x 720 | 23.0 x 13.0 | 14,350 |
| 640x480 | 640 x 480 | 11.5 x 8.6 | 30,030 |
| Binned low-res (mono) | 320 x 24 | -- | 423,350 |

Note: The VEO 1310's max resolution is 1280x960 (1.2 Mpx). It does NOT natively support 1920x1080. This is a high-throughput scientific/industrial camera with large 18 um pixels for extreme light sensitivity (ISO 25,000 mono, ISO 5,000 color).

- **Supporting sources**:
  - [Newsshooter - Phantom VEO 1310](https://www.newsshooter.com/2020/02/05/phantom-veo-1310-10800fps/) - Sensor dimensions (23x17.3mm), fps data
  - [Phantom High Speed - VEO 1310](https://www.phantomhighspeed.com/products/cameras/veo/veo1310) - Official specs (blocked, search snippet data)
  - [Novus Light - VEO 1310 Release](https://www.novuslight.com/vision-research-releases-phantom-veo-1310-high-speed-camera_N10067.html) - Confirms throughput and sensitivity

---

### Phantom TMX 7510

- **Confidence**: HIGH
- **Sensor**: CMOS (Back Side Illuminated / BSI), 23.7 x 14.8 mm (27.94 mm diagonal), pixel size 18.5 um
- **Max Resolution**: 1280 x 800 (1 Mpx)
- **Bit Depth**: 12-bit
- **Shutter**: Global
- **Throughput**: 75 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 1 Mpx | 1280 x 800 | 23.7 x 14.8 | 76,000 |
| Reduced height | 1280 x 192 | 23.7 x 3.6 | 300,000+ |
| Extreme reduced | (small) | -- | 770,000+ |
| FAST option | 1280 x 32 | 23.7 x 0.6 | 1,750,000 |

Binned mode:
| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Binned | 1280 x 94 | 23.7 x 3.5 | 617,000+ |
| Binned | 640 x 192 | 11.8 x 7.1 | 617,000+ |

Note: The TMX 7510's max resolution is 1280x800 (1 Mpx). It does NOT support 1920x1080. This is a scientific ultra-high-speed camera where the BSI sensor architecture enables 75 Gpx/s throughput -- 3x faster than previous 1 Mpx Phantom cameras. The FAST option is export-controlled (ITAR).

- **Supporting sources**:
  - [PetaPixel - TMX 7510](https://petapixel.com/2021/03/03/new-phantom-tmx-7510-camera-can-record-an-insane-1750000-fps/) - Resolution/fps, sensor details
  - [Y.M.Cinema - Phantom TMX BSI](https://ymcinema.com/2021/03/03/phantom-tmx-the-first-ultra-high-speed-cameras-with-bsi-sensor/) - TMX 7510 and 6410 specs
  - [Phantom High Speed - TMX 7510](https://www.phantomhighspeed.com/products/cameras/tmx/7510) - Official product page (blocked, search snippet data)
- **Notes**: Sensor dimensions confirmed from search results (23.7 x 14.8 mm). The TMX 6410 sister model does 64,940 fps at full 1280x800 resolution.

---

### Phantom T1340

- **Confidence**: HIGH
- **Sensor**: CMOS, 27.6 x 26.3 mm (38 mm diagonal), pixel size 13.5 um
- **Max Resolution**: 2048 x 1952 (4 Mpx)
- **Bit Depth**: 12-bit
- **Shutter**: Global
- **Throughput**: 13 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 4 Mpx | 2048 x 1952 | 27.6 x 26.3 | 3,270 |
| 2K 1.42:1 | 2048 x 1440 | 27.6 x 19.4 | 4,390 |
| 2K 1.78:1 | 2048 x 1152 | 27.6 x 15.6 | 5,400 |
| HD 1080p | 1920 x 1080 | 25.9 x 14.6 | 6,160 |
| 720p | 1280 x 720 | 17.3 x 9.7 | ~13,000 |
| Extreme reduced | 640 x 8 | -- | 113,510 |

- **Supporting sources**:
  - [Y.M.Cinema - Phantom T1340](https://ymcinema.com/2020/09/17/meet-the-high-speed-phantom-t1340-that-shoots-3270-fps-at-2k-resolution/) - Sensor size (27.6x26.3mm), key fps/resolution data
  - [CineD - T1340 Announced](https://www.cined.com/phantom-t1340-four-megapixel-high-speed-camera-announced/) - Confirms 3,270 fps full and 6,160 fps at Full HD
  - [The ASC - T1340 Launch](https://theasc.com/articles/vision-research-launches-phantom-t1340) - Additional confirmation
- **Notes**: Same near-square 4 Mpx sensor as the v2640, but at ~1/2 the throughput (13 vs 26.2 Gpx/s). The 720p figure of ~13,000 fps is approximate based on throughput calculation.

---

### Phantom T4040

- **Confidence**: HIGH
- **Sensor**: CMOS (Back Side Illuminated / BSI), 23.7 x 15.4 mm (28.2 mm diagonal), pixel size 9.27 um
- **Max Resolution**: 2560 x 1664 (4.3 Mpx)
- **Bit Depth**: 12-bit
- **Shutter**: Global
- **Throughput**: 39.8 Gpx/s

Standard Mode:

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 2.5K | 2560 x 1664 | 23.7 x 15.4 | 9,350 |
| Square 1.5K | 1536 x 1536 | 14.2 x 14.2 | 10,130 |
| 2.5K 1.78:1 | 2560 x 1440 | 23.7 x 13.4 | 10,810 |
| 2K 1.78:1 | 2048 x 1152 | 19.0 x 10.7 | 13,510 |
| 2.5K strip | 2560 x 256 | 23.7 x 2.4 | 60,500 |
| 2.5K extreme | 2560 x 32 | 23.7 x 0.3 | 444,440 |

Binned Mode (monochrome only):

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Binned 1 Mpx | 1280 x 832 | 23.7 x 15.4 | 37,200 |
| Binned wide | 1280 x 640 | 23.7 x 11.9 | 48,190 |
| Binned 0.5 Mpx | 1024 x 512 | 19.0 x 9.5 | 60,150 |

Note: The T4040 does NOT have a native 1920x1080 mode in its published resolution table. Its 2048x1152 mode at 13,510 fps is the closest standard cinema resolution. 1080p output would require downscaling from 2560x1664 or cropping.

- **Supporting sources**:
  - [MC-S Special Camera Systems - T4040](https://highspeed-cameras.nl/product/phantom-t4040/) - Complete resolution/fps table
  - [Link Gulf - T4040](https://link-gulf.com/product/phantom-t4040-t-series-high-speed-camera/) - Confirms identical table
  - [Y.M.Cinema - T4040 Announcement](https://ymcinema.com/2023/03/06/meet-the-new-phantom-t4040-high-speed-camera-9350-fps-at-2-5k-of-resolution/) - Sensor size (23.7x15.4mm)

---

### Phantom T2540

- **Confidence**: HIGH
- **Sensor**: CMOS (Back Side Illuminated / BSI), 23.7 x 15.4 mm (28.2 mm diagonal), pixel size 9.27 um
- **Max Resolution**: 2560 x 1664 (4.3 Mpx)
- **Bit Depth**: 12-bit
- **Shutter**: Global
- **Throughput**: 25 Gpx/s

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| Full Sensor 2.5K | 2560 x 1664 | 23.7 x 15.4 | 5,840 |
| Square 1.5K | 1536 x 1536 | 14.2 x 14.2 | 6,330 |
| 2.5K 1.78:1 | 2560 x 1440 | 23.7 x 13.4 | 6,750 |
| 2K 1.78:1 | 2048 x 1152 | 19.0 x 10.7 | 8,440 |
| 2.5K strip | 2560 x 256 | 23.7 x 2.4 | 37,590 |
| 2.5K extreme | 2560 x 32 | 23.7 x 0.3 | 277,770 |

- **Supporting sources**:
  - [Y.M.Cinema - T2540 Announced](https://ymcinema.com/2023/08/28/phantom-t2540-announced-5840-fps-at-2-5k-resolution/) - Complete resolution/fps table, sensor dimensions
  - [Newsshooter - T2540](https://www.newsshooter.com/2023/08/28/phantom-t2540-5840-fps-at-2-5k/) - Confirms specs
  - [Phantom High Speed - T2540](https://www.phantomhighspeed.com/products/cameras/tseries/t2540) - Official product page (blocked, search snippet data)
- **Notes**: Same sensor as T4040 but at ~63% throughput (25 vs 39.8 Gpx/s). Sensor area per mode estimated from pixel size.

---

## PANAVISION CAMERAS

---

### Panavision Genesis

- **Confidence**: HIGH
- **Sensor**: CCD (true RGB, not Bayer), 23.6 x 13.3 mm (Super 35 format)
- **Sensor Array**: 5760 x 2160 (12.4 Mpx), pixel-binned to output resolution
- **Output Resolution**: 1920 x 1080 (single mode)
- **Bit Depth**: 10-bit (Panalog 4:4:4)
- **Shutter**: Global (CCD)
- **Discontinued**: 2012

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| HD 1080p (only mode) | 1920 x 1080 | 23.6 x 13.3 | 50 |

Note: The Genesis is a single-mode camera. It outputs only 1920x1080 at 1-50 fps. The 12.4 Mpx CCD is internally binned 3:1 horizontally to produce 1920 output pixels. It is NOT a variable-frame-rate or high-speed camera. The true RGB CCD (equal R, G, B pixel count) was its distinguishing feature vs Bayer-pattern competitors.

- **Supporting sources**:
  - [VFX Camera Database - Panavision Genesis](https://vfxcamdb.com/panavision-genesis/) - Sensor size (23.6x13.3mm), resolution, output format
  - [Wikipedia - Genesis (camera)](https://en.wikipedia.org/wiki/Genesis_(camera)) - Sensor array (5760x2160), frame rate (1-50fps), CCD details
  - [Panavision Genesis Brochure (PDF)](https://regmedia.co.uk/2013/06/24/panavision_genesis_digital_camera_system_brochure.pdf) - Official specs

---

### Panavision Millennium DXL

- **Confidence**: MEDIUM
- **Sensor**: RED Dragon 8K VV, CMOS, 40.96 x 21.60 mm (46.31 mm diagonal, VistaVision/Large Format)
- **Max Resolution**: 8192 x 4320 (35.5 Mpx)
- **Bit Depth**: 16-bit RAW (R3D)
- **Shutter**: Rolling
- **Native ISO**: 800
- **Dynamic Range**: 15 stops

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| 8K FF 1.9:1 | 8192 x 4320 | 40.96 x 21.60 | 60 |
| 8K 2.4:1 WS | 8192 x 3456 | 40.96 x 17.28 | 75 |
| 7K FF | 7168 x 3780 | 35.84 x 18.90 | 60 |
| 6K FF | 6144 x 3240 | 30.72 x 16.20 | 75 |
| 5K FF | 5120 x 2700 | 25.60 x 13.50 | 96 |
| 4K FF | 4096 x 2160 | 20.48 x 10.80 | 120 |
| 3K FF | 3072 x 1620 | 15.36 x 8.10 | 150 |
| 2K FF | 2048 x 1080 | 10.24 x 5.40 | 240 |

Note: The original DXL (2016) uses the RED Dragon 8K VV sensor. The frame rates at sub-8K resolutions are based on the RED Monstro specs (used in DXL2) which is a newer, faster sensor -- the Dragon may have slightly lower max fps at some tiers. The resolution modes and sensor areas are identical (same physical sensor size). Frame rates for 7K-2K on the original DXL with Dragon sensor may differ slightly from the Monstro numbers shown here; the 8K modes (60fps FF, 75fps 2.4:1) are confirmed for the Dragon.

- **Supporting sources**:
  - [VFX Camera Database - Millennium DXL](https://vfxcamdb.com/panavision-millennium-dxl/) - Resolution modes and sensor areas
  - [Newsshooter - DXL Announced](https://www.newsshooter.com/2016/06/01/panavision-announce-8k-dxl-cinema-camera-with-red-dragon-sensor-and-light-iron-colour-science/) - 60fps at 8K FF, 75fps at 8K 2.4:1, sensor diagonal
  - [Gearfocus - Millennium DXL](https://gearfocus.com/m/panavision-millennium-dxl) - General specs

---

### Panavision Millennium DXL2

- **Confidence**: HIGH
- **Sensor**: RED Monstro 8K VV, CMOS, 40.96 x 21.60 mm (46.31 mm diagonal, VistaVision/Large Format)
- **Max Resolution**: 8192 x 4320 (35.4 Mpx)
- **Bit Depth**: 16-bit RAW (R3D)
- **Shutter**: Rolling
- **Native ISO**: 1600
- **Dynamic Range**: 16+ stops

Full Frame (1.9:1) Modes:

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| 8K FF | 8192 x 4320 | 40.96 x 21.60 | 60 |
| 7.5K | 7680 x 4080 | 38.40 x 20.40 | 60 |
| 7K | 7168 x 3780 | 35.84 x 18.90 | 60 |
| 6.5K | 6656 x 3536 | 33.28 x 17.68 | 60 |
| 6K | 6144 x 3240 | 30.72 x 16.20 | 75 |
| 5K | 5120 x 2700 | 25.60 x 13.50 | 96 |
| 4K | 4096 x 2160 | 20.48 x 10.80 | 120 |
| 3K | 3072 x 1620 | 15.36 x 8.10 | 150 |
| 2K | 2048 x 1080 | 10.24 x 5.40 | 240 |

2.4:1 Widescreen Modes:

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| 8K 2.4:1 | 8192 x 3456 | 40.96 x 17.28 | 75 |
| 7K 2.4:1 | 7168 x 3024 | 35.84 x 15.12 | 75 |
| 6K 2.4:1 | 6144 x 2592 | 30.72 x 12.96 | 100 |
| 5K 2.4:1 | 5120 x 2160 | 25.60 x 10.80 | 120 |
| 4K 2.4:1 | 4096 x 1728 | 20.48 x 8.64 | 150 |
| 3K 2.4:1 | 3072 x 1296 | 15.36 x 6.48 | 200 |
| 2K 2.4:1 | 2048 x 864 | 10.24 x 4.32 | 300 |

Anamorphic 6:5 Modes:

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| 8K Ana | 8192 x 4320 | 40.96 x 21.60 | 60 |
| 6K Ana | 3840 x 3200 | 19.20 x 16.00 | ~75 |
| 4K Ana | 2592 x 2160 | 12.96 x 10.80 | ~120 |

- **Supporting sources**:
  - [RED Technical Docs - Monstro 8K VV Specs](https://docs.red.com/955-0160/WEAPONMONSTRO8KVVOperationGuide/en-us/Content/A_TechSpecs/Specs_DSMC2_MONSTRO.htm) - Complete frame rate table by resolution
  - [VFX Camera Database - DXL2](https://vfxcamdb.com/panavision-millennium-dxl2/) - Resolution modes with sensor area per mode
  - [Panavision - DXL2 Product Page](https://www.panavision.com/camera-and-optics/cameras/product-detail/dxl2-millennium-dxl2) - Official specs
  - [Newsshooter - DXL2 Unveiled](https://www.newsshooter.com/2018/02/02/panavision-unveils-the-millennium-dxl2-8k-camera/) - ISO 1600, 16+ stops
- **Notes**: Frame rate data comes from the RED Monstro 8K VV sensor specs. The DXL2 uses this sensor directly, so the frame rates are identical to a RED DSMC2 Monstro body. Anamorphic mode fps values for 6K and 4K are estimated from the Monstro's general throughput curve.

---

### Panavision DXL-M

- **Confidence**: MEDIUM
- **Sensor**: RED Monstro 8K VV, CMOS, 40.96 x 21.60 mm (46.31 mm diagonal)
- **Max Resolution**: 8192 x 4320 (35.4 Mpx)
- **Bit Depth**: 16-bit RAW (R3D)
- **Shutter**: Rolling
- **Native ISO**: 1600
- **Dynamic Range**: 16+ stops

The DXL-M uses the same RED Monstro 8K VV sensor as the DXL2, so resolution modes and max frame rates are identical:

| Mode Name | Resolution | Active Sensor Area (mm) | Max FPS |
|-----------|-----------|------------------------|---------|
| 8K FF | 8192 x 4320 | 40.96 x 21.60 | 60 |
| 8K 2.4:1 | 8192 x 3456 | 40.96 x 17.28 | 75 |
| 6K FF | 6144 x 3240 | 30.72 x 16.20 | 75 |
| 5K FF | 5120 x 2700 | 25.60 x 13.50 | 96 |
| 4K FF | 4096 x 2160 | 20.48 x 10.80 | 120 |
| 2K FF | 2048 x 1080 | 10.24 x 5.40 | 240 |
| 2K 2.4:1 | 2048 x 864 | 10.24 x 4.32 | 300 |

(See DXL2 section above for complete mode list)

- **Supporting sources**:
  - [Panavision - DXL2 Technical Specs](https://dxl.panavision.com/node/5482) - Confirms Monstro 8K VV sensor in DXL-M
  - [Panavision - DXL2 Landing](https://dxl.panavision.com/) - DXL-M description
- **Notes**: The DXL-M is physically smaller (designed for gimbals/drones) but uses the identical sensor and image processing as DXL2. It uses a RED DSMC2-style body. Frame rates should match the DXL2/Monstro exactly. Confidence is MEDIUM because no source explicitly lists per-mode frame rates for the DXL-M specifically -- the assumption is that identical sensor = identical capabilities.

---

## Contradictions

- **Phantom Flex 1080p Max FPS**: Love High Speed lists 2,564 fps at 1920x1080 in Standard Mode, while another source mentions 2,570 fps. These are within rounding error of each other.

- **VEO4K-PL vs VEO4K 990 at 1080p**: The VEO4K-PL is listed at 1,977 fps at 1920x1080, while the VEO4K 990 lists 1,970 fps at 2048x1080. These are different resolutions (1920 vs 2048 width) which accounts for the slight fps difference. Both are consistent given the throughput.

- **T4040 at HD resolution**: No source provides a 1920x1080 fps figure for the T4040. The closest published mode is 2048x1152 at 13,510 fps. One search result mentioned "1080p60" but this appears to refer to the video output/monitoring, not the recording mode.

- **DXL original vs DXL2 frame rates at sub-8K**: The original DXL (Dragon sensor) and DXL2 (Monstro sensor) have the same 8K frame rates (60fps FF, 75fps 2.4:1). At lower resolutions, I've used the Monstro specs for both, but the Dragon sensor may have different throughput at 4K-6K ranges. No source provides a separate Dragon-specific frame rate table for the DXL.

---

## Source Registry

| # | Title | URL | Date | Queries |
|---|-------|-----|------|---------|
| 1 | Love High Speed - Phantom Flex | https://www.lovehighspeed.com/high-speed-cameras/phantom-flex/ | -- | Q1 |
| 2 | AbelCine - VRI Phantom Flex | https://www.abelcine.com/rent/cameras-accessories/digital-cinema-cameras/vri-phantom-flex-high-speed-digital-camera | -- | Q1 |
| 3 | Love High Speed - Phantom Flex4K | https://www.lovehighspeed.com/high-speed-cameras/phantom-flex4k/ | -- | Q2 |
| 4 | AbelCine - Flex4K FAQs | https://www.abelcine.com/articles/blog-and-knowledge/tools-charts-and-downloads/phantom-flex4k-faqs | -- | Q2 |
| 5 | DPReview - Phantom v2640 | https://www.dpreview.com/news/4514783704/the-4mp-phantom-v2640-can-shoot-6-600fps-at-full-resolution-11-750fps-at-1920x1080 | 2018-01 | Q3 |
| 6 | Phantom High Speed - v2640 Press Release | https://www.phantomhighspeed.com/news/newsarticles/2018/january/phantom-v2640 | 2018-01 | Q3 |
| 7 | CineD - v2640 Hands-On | https://www.cined.com/11750-fps-going-high-speed-with-the-phantom-v2640/ | -- | Q3 |
| 8 | MC-S - VEO4K 990 | https://highspeed-cameras.nl/product/phantom-veo4k-990/ | -- | Q4 |
| 9 | Love High Speed - VEO4K-PL | https://www.lovehighspeed.com/high-speed-cameras/phantom-veo4k-pl/ | -- | Q4 |
| 10 | CN Rood - VEO4K 990 | https://www.cnrood.com/en/veo4k-990l-990s-cameras | -- | Q4 |
| 11 | AbelCine - Miro M320S Announcement | https://www.abelcine.com/articles/news/product-news/vri-introduces-the-phantom-miro-m320s-high-speed-camera | 2012-04 | Q5 |
| 12 | Studio Daily - Miro M320S | https://www.studiodaily.com/2012/04/phantoms-miro-m320s-shoots-1080p-at-1540fps/ | 2012-04 | Q5 |
| 13 | Y.M.Cinema - Miro LC320S Tribute | https://ymcinema.com/2022/07/12/a-tribute-to-the-phantom-miro-lc320s-simplification-of-ultra-high-speed-cinematic-capturing/ | 2022-07 | Q5 |
| 14 | PetaPixel - TMX 7510 | https://petapixel.com/2021/03/03/new-phantom-tmx-7510-camera-can-record-an-insane-1750000-fps/ | 2021-03 | Q6 |
| 15 | Y.M.Cinema - Phantom TMX BSI | https://ymcinema.com/2021/03/03/phantom-tmx-the-first-ultra-high-speed-cameras-with-bsi-sensor/ | 2021-03 | Q6 |
| 16 | Y.M.Cinema - T4040 | https://ymcinema.com/2023/03/06/meet-the-new-phantom-t4040-high-speed-camera-9350-fps-at-2-5k-of-resolution/ | 2023-03 | Q7 |
| 17 | Y.M.Cinema - T2540 | https://ymcinema.com/2023/08/28/phantom-t2540-announced-5840-fps-at-2-5k-resolution/ | 2023-08 | Q7 |
| 18 | Y.M.Cinema - T1340 | https://ymcinema.com/2020/09/17/meet-the-high-speed-phantom-t1340-that-shoots-3270-fps-at-2k-resolution/ | 2020-09 | Q7 |
| 19 | CineD - T1340 Announced | https://www.cined.com/phantom-t1340-four-megapixel-high-speed-camera-announced/ | 2020-09 | Q7 |
| 20 | Newsshooter - VEO 1310 | https://www.newsshooter.com/2020/02/05/phantom-veo-1310-10800fps/ | 2020-02 | Q8 |
| 21 | VFX Camera Database - Genesis | https://vfxcamdb.com/panavision-genesis/ | -- | Q9 |
| 22 | Wikipedia - Genesis (camera) | https://en.wikipedia.org/wiki/Genesis_(camera) | -- | Q9 |
| 23 | VFX Camera Database - DXL | https://vfxcamdb.com/panavision-millennium-dxl/ | -- | Q10 |
| 24 | VFX Camera Database - DXL2 | https://vfxcamdb.com/panavision-millennium-dxl2/ | -- | Q10 |
| 25 | Newsshooter - DXL2 Unveiled | https://www.newsshooter.com/2018/02/02/panavision-unveils-the-millennium-dxl2-8k-camera/ | 2018-02 | Q10 |
| 26 | RED Docs - Monstro 8K VV Specs | https://docs.red.com/955-0160/WEAPONMONSTRO8KVVOperationGuide/en-us/Content/A_TechSpecs/Specs_DSMC2_MONSTRO.htm | -- | Q11 |
| 27 | Newsshooter - DXL Announced | https://www.newsshooter.com/2016/06/01/panavision-announce-8k-dxl-cinema-camera-with-red-dragon-sensor-and-light-iron-colour-science/ | 2016-06 | Q12 |
| 28 | MC-S - T4040 | https://highspeed-cameras.nl/product/phantom-t4040/ | -- | Q7 |
| 29 | Link Gulf - T4040 | https://link-gulf.com/product/phantom-t4040-t-series-high-speed-camera/ | -- | Q7 |
| 30 | Delta Photonics - VEO 640 | https://deltaphotonics.com/product/veo-640/ | -- | Q4 |
| 31 | Panavision - DXL2 Tech Specs | https://dxl.panavision.com/node/5482 | -- | Q10 |
