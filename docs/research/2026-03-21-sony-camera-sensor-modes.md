# Raw Research Findings: Sony Camera Sensor/Recording Modes

## Queries Executed
1. "Sony VENICE 1 sensor modes resolution frame rate" - 5 useful results
2. "Sony VENICE 2 sensor modes 8K 6K 4K resolution frame rate" - 4 useful results
3. "Sony BURANO recording modes resolution sensor area frame rate" - 5 useful results
4. "Sony PMW-F55 PMW-F5 F65 sensor modes recording formats" - 3 useful results
5. "Sony FX9 FX6 recording modes sensor modes resolution frame rate" - 4 useful results
6. "Sony FX3 FX30 video recording modes resolution frame rate" - 3 useful results
7. "Sony FR7 recording modes resolution frame rate" - 2 useful results
8. "Sony NEX-FS100 NEX-FS700 recording modes resolution frame rate" - 3 useful results
9. "Sony PXW-FS7 PXW-FS5 recording modes resolution frame rate" - 3 useful results
10. "Sony A7S III / A1 / A7IV / A7RV / A9III / A7C II / ZV-E1 video modes" - 8+ useful results
11. "Sony PMW-F3 specifications" - 2 useful results
12. "Sony VENICE HFR complete list" - 3 useful results
13. "Sony BURANO V2 firmware modes" - 3 useful results

---

## CINEMA LINE CAMERAS

---

### Sony F65
- **Sensor:** Super 35mm CMOS, 24.7 x 13.1 mm, 20 MP, mechanical rotary shutter
- **Confidence:** HIGH (multiple sources agree)

```
Camera: F65
  Mode: 8K (RAW readout),       Resolution: 8182x2160,  Sensor Area: 24.7x13.1 mm, Max FPS: 60
  Mode: 4K 17:9,                Resolution: 4096x2160,  Sensor Area: 24.7x13.1 mm, Max FPS: 60
  Mode: 2K 17:9,                Resolution: 2048x1080,  Sensor Area: 24.7x13.1 mm, Max FPS: 120
  Mode: HD 16:9,                Resolution: 1920x1080,  Sensor Area: 24.7x13.1 mm, Max FPS: 120
```

**Notes:** The F65 sensor is 8K but the 8K RAW output is 8182x2160 (a wide extraction). 4K is the primary delivery format. 16-bit linear RAW recording. S&Q from 1-120fps in 1fps increments.

**Sources:**
- [Sony F65 Pro](https://pro.sony/ue_US/products/digital-cinema-cameras/f65)
- [VFX Camera Database - F65](https://vfxcamdb.com/sony-f65/)

---

### Sony PMW-F3
- **Sensor:** Super 35mm CMOS, 23.6 x 13.3 mm
- **Confidence:** MEDIUM

```
Camera: PMW-F3
  Mode: Full sensor readout,    Resolution: 2448x1377,  Sensor Area: 23.6x13.3 mm, Max FPS: 30
  Mode: HD 16:9,                Resolution: 1920x1080,  Sensor Area: 23.6x13.3 mm, Max FPS: 30
  Mode: HD 16:9 (1440),         Resolution: 1440x1080,  Sensor Area: 23.6x13.3 mm, Max FPS: 60
  Mode: 720p,                   Resolution: 1280x720,   Sensor Area: 23.6x13.3 mm, Max FPS: 60
```

**Notes:** HD-only camera. Overcranked 1-30fps at 1920x1080, 1-60fps at 1280x720. Dual-link HD-SDI for 10-bit 4:4:4 external recording. MPEG-2 long GOP 8-bit 4:2:0 internal.

**Sources:**
- [VFX Camera Database - PMW-F3](https://vfxcamdb.com/sony-pmw-f3/)
- [B&H - PMW-F3L](https://www.bhphotovideo.com/c/product/848144-REG/Sony_PMW_F3L_RGB_PMW_F3L_Super_35mm_Full_HD.html)

---

### Sony PMW-F5
- **Sensor:** Super 35mm CMOS, 24.0 x 12.7 mm, 8.9 MP
- **Confidence:** MEDIUM

```
Camera: PMW-F5
  Mode: 4K 17:9,                Resolution: 4096x2160,  Sensor Area: 24.0x12.7 mm, Max FPS: 60
  Mode: 2K 17:9,                Resolution: 2048x1080,  Sensor Area: 24.0x12.7 mm, Max FPS: 240
  Mode: HD 16:9,                Resolution: 1920x1080,  Sensor Area: 24.0x12.7 mm, Max FPS: 240
```

**Notes:** 4K RAW requires AXS-R5/R7 external recorder. 2K RAW up to 240fps with AXS-R5. Internal recording limited to HD codecs (MPEG-2, SStP, XAVC). Same sensor as F55 but different readout electronics.

**Sources:**
- [VFX Camera Database - F5](https://vfxcamdb.com/sony-f5/)
- [Sony Pro - F5/F55](https://pro.sony/ue_US/products/digital-cinema-cameras/pmw-f55)

---

### Sony PMW-F55
- **Sensor:** Super 35mm CMOS, 24.0 x 12.7 mm, 8.9 MP, global shutter
- **Confidence:** HIGH

```
Camera: PMW-F55
  Mode: 4K 17:9 (internal),     Resolution: 4096x2160,  Sensor Area: 24.0x12.7 mm, Max FPS: 60
  Mode: 4K 17:9 (AXS-R7 RAW),  Resolution: 4096x2160,  Sensor Area: 24.0x12.7 mm, Max FPS: 120
  Mode: 2K 17:9 (internal),     Resolution: 2048x1080,  Sensor Area: 24.0x12.7 mm, Max FPS: 180
  Mode: 2K 17:9 (AXS-R5 RAW),  Resolution: 2048x1080,  Sensor Area: 24.0x12.7 mm, Max FPS: 240
  Mode: HD 16:9,                Resolution: 1920x1080,  Sensor Area: 24.0x12.7 mm, Max FPS: 180
```

**Notes:** Global electronic shutter (no rolling shutter). AXS-R7 doubles 4K RAW from 60 to 120fps. AXS-R5 enables 2K RAW at 240fps. Internal XAVC supports 4K/2K up to 60fps, 2K/HD up to 180fps.

**Sources:**
- [B&H - PMW-F55](https://www.bhphotovideo.com/c/product/898428-REG/Sony_PMW_F55_CineAlta_4K_Digital.html)
- [VFX Camera Database - F55](https://vfxcamdb.com/sony-f55/)
- [AbelCine - HFR with F55](https://www.abelcine.com/articles/blog-and-knowledge/tutorials-and-guides/high-frame-rate-recording-with-the-sony-f55)
- [No Film School - AXS-R7 120fps](https://nofilmschool.com/2016/02/sony-axs-r7-recorder-120fps-4k-raw-f55)

---

### Sony VENICE (MPC-3610)
- **Sensor:** Full-frame CMOS, 36.2 x 24.1 mm (max active), 6048 x 4032 max resolution
- **Confidence:** HIGH (extensive cross-referencing)

```
Camera: VENICE
  Mode: 6K 3:2 (FF),            Resolution: 6048x4032,  Sensor Area: 35.9x24.0 mm,  Max FPS: 60
  Mode: 6K 2.39:1 (FF),         Resolution: 6048x2534,  Sensor Area: 35.9x15.0 mm,  Max FPS: 90
  Mode: 6K 1.85:1 (FF),         Resolution: 6054x3272,  Sensor Area: 36.0x19.4 mm,  Max FPS: 72
  Mode: 6K 17:9 (FF),           Resolution: 6054x3192,  Sensor Area: 36.0x19.0 mm,  Max FPS: 72
  Mode: 5.7K 16:9 (FF),         Resolution: 5674x3192,  Sensor Area: 33.7x18.9 mm,  Max FPS: 72
  Mode: 4K 6:5 Anamorphic,      Resolution: 4096x3432,  Sensor Area: 24.3x20.4 mm,  Max FPS: 72
  Mode: 4K 4:3 Anamorphic,      Resolution: 4096x3024,  Sensor Area: 24.3x18.0 mm,  Max FPS: 75
  Mode: 4K 17:9 (S35),          Resolution: 4096x2160,  Sensor Area: 24.3x12.8 mm,  Max FPS: 110
  Mode: 4K 2.39:1 (S35),        Resolution: 4096x1716,  Sensor Area: 24.3x10.2 mm,  Max FPS: 120
  Mode: 3.8K 16:9 (S35),        Resolution: 3840x2160,  Sensor Area: 22.8x12.8 mm,  Max FPS: 110
```

**Notes:** HFR requires optional license (CBK-Z-3610H). Standard (non-HFR) max is 60fps at 6K 3:2 and 59.94fps for most modes. All HFR modes support X-OCN recording. AXS-R7 recorder required for 6K modes.

**Sources:**
- [VFX Camera Database - VENICE](https://vfxcamdb.com/sony-venice/)
- [The Camera Department - VENICE](https://www.thecameradept.com/sony-venice)
- [Sony Cinematography - VENICE HFR](https://sony-cinematography.com/articles/sony-s-venice-continues-to-evolve-with-high-frame-rate-up-to-90-frames-per-second-at-6k/)
- [ProVideo Coalition - VENICE HFR](https://www.provideocoalition.com/sony-venice-gets-high-frame-rate-shooting-up-to-4k-120fps/)

---

### Sony VENICE 2 (8K Sensor - MPC-3628)
- **Sensor:** Full-frame CMOS, 36 x 24 mm, 8640 x 5760 max resolution
- **Confidence:** HIGH

```
Camera: VENICE 2 (8K Sensor)
  Mode: 8.6K 3:2 (FF),          Resolution: 8640x5760,  Sensor Area: 36.0x24.0 mm,  Max FPS: 30
  Mode: 8.2K 17:9 (FF),         Resolution: 8192x4320,  Sensor Area: ~34.1x18.0 mm, Max FPS: 60
  Mode: 7.6K 16:9 (FF),         Resolution: 7680x4320,  Sensor Area: ~32.0x18.0 mm, Max FPS: 60
  Mode: 5.8K 6:5 Anamorphic,    Resolution: 5792x4854,  Sensor Area: 24.1x20.2 mm,  Max FPS: 48
  Mode: 5.8K 17:9 (S35),        Resolution: 5792x3056,  Sensor Area: 24.1x12.7 mm,  Max FPS: 90
  Mode: 5.4K 16:9 (S35),        Resolution: 5452x3056,  Sensor Area: ~22.7x12.7 mm, Max FPS: 90
  Mode: 4K 6:5 Anamorphic,      Resolution: 4096x3432,  Sensor Area: ~17.1x14.3 mm, Max FPS: 72
  Mode: 4K 4:3 Anamorphic,      Resolution: 4096x3024,  Sensor Area: ~17.1x12.6 mm, Max FPS: 75
  Mode: 4K 17:9 (S35),          Resolution: 4096x2160,  Sensor Area: ~17.1x9.0 mm,  Max FPS: 110
  Mode: 4K 2.39:1 (S35),        Resolution: 4096x1716,  Sensor Area: ~17.1x7.1 mm,  Max FPS: 120
  Mode: 3.8K 16:9 (S35),        Resolution: 3840x2160,  Sensor Area: ~16.0x9.0 mm,  Max FPS: 110
```

**Notes:** Sensor area values marked with ~ are calculated from pixel pitch (~4.17um) and may not be exact. Internal X-OCN recording (no external recorder needed unlike VENICE 1 for some modes). Dual base ISO 800/3200.

**Sources:**
- [Sony Cinematography - VENICE 2](https://sony-cinematography.com/venice2/)
- [The Camera Department - VENICE 2](https://www.thecameradept.com/sony-venice-2)
- [CineD - VENICE 2 Announced](https://www.cined.com/sony-venice-2-camera-announced-8-6k-full-frame-sensor-and-internal-x-ocn-recording/)

---

### Sony VENICE 2 (6K Sensor - MPC-3626)
- **Sensor:** Full-frame CMOS, 36 x 24 mm, 6048 x 4032 max resolution (same as VENICE 1 sensor)
- **Confidence:** HIGH

```
Camera: VENICE 2 (6K Sensor)
  Mode: 6K 3:2 (FF),            Resolution: 6048x4032,  Sensor Area: 35.9x24.0 mm,  Max FPS: 60
  Mode: 6K 2.39:1 (FF),         Resolution: 6048x2534,  Sensor Area: 35.9x15.0 mm,  Max FPS: 90
  Mode: 6K 1.85:1 (FF),         Resolution: 6054x3272,  Sensor Area: 36.0x19.4 mm,  Max FPS: 60
  Mode: 6K 17:9 (FF),           Resolution: 6054x3192,  Sensor Area: 36.0x19.0 mm,  Max FPS: 72
  Mode: 5.7K 16:9 (FF),         Resolution: 5674x3192,  Sensor Area: 33.7x18.9 mm,  Max FPS: 60
  Mode: 4K 6:5 Anamorphic,      Resolution: 4096x3432,  Sensor Area: 24.3x20.4 mm,  Max FPS: 72
  Mode: 4K 4:3 Anamorphic,      Resolution: 4096x3024,  Sensor Area: 24.3x18.0 mm,  Max FPS: 75
  Mode: 4K 17:9 (S35),          Resolution: 4096x2160,  Sensor Area: 24.3x12.8 mm,  Max FPS: 110
  Mode: 4K 2.39:1 (S35),        Resolution: 4096x1716,  Sensor Area: 24.3x10.2 mm,  Max FPS: 120
  Mode: 3.8K 16:9 (S35),        Resolution: 3840x2160,  Sensor Area: 22.8x12.8 mm,  Max FPS: 110
```

**Notes:** The 6K sensor is the same sensor design as VENICE 1 but with internal recording capability. Dual base ISO 500/2500. Anamorphic license required for 4K 4:3 and 4K 6:5 modes.

**Sources:**
- [Sony Cinematography - VENICE 2](https://sony-cinematography.com/venice2/)
- [CBM-Cine - VENICE 2 6K](https://www.cbm-cine.com/Sony-VENICE-2-with-6K-image-sensor/VENICE-6K-MPC-3626)

---

### Sony BURANO (MPC-2610)
- **Sensor:** Full-frame CMOS, 35.9 x 24.0 mm, ~41.9 MP
- **Confidence:** HIGH

```
Camera: BURANO
  Mode: FF 8.6K 16:9,           Resolution: 8632x4856,  Sensor Area: 35.9x20.2 mm,  Max FPS: 30
  Mode: FF 8.6K 17:9,           Resolution: 8632x4552,  Sensor Area: 35.9x18.9 mm,  Max FPS: 30
  Mode: FFc 6K 16:9,            Resolution: 6052x3404,  Sensor Area: ~25.2x14.1 mm, Max FPS: 60
  Mode: FFc 6K 17:9,            Resolution: 6052x3192,  Sensor Area: ~25.2x13.3 mm, Max FPS: 60
  Mode: FFc 3.8K 16:9,          Resolution: 3840x2160,  Sensor Area: ~16.0x9.0 mm,  Max FPS: 120
  Mode: S35 5.8K 16:9,          Resolution: 5760x3240,  Sensor Area: ~24.0x13.5 mm, Max FPS: 60
  Mode: S35 5.8K 17:9,          Resolution: 5760x3036,  Sensor Area: ~24.0x12.6 mm, Max FPS: 60
  Mode: S35 4.3K 4:3 (Anamorphic), Resolution: ~4320x3240, Sensor Area: ~18.0x13.5 mm, Max FPS: 60
  Mode: S35c 4K 17:9,           Resolution: 4096x2160,  Sensor Area: ~17.0x9.0 mm,  Max FPS: 120
  Mode: S35 1.9K 16:9,          Resolution: 1920x1080,  Sensor Area: ~8.0x4.5 mm,   Max FPS: 240
```

**Notes:** Sensor area values marked with ~ are calculated estimates from pixel pitch (~4.16um). FFc = Full Frame crop. S35c = Super 35 crop. XAVC H-I modes also support 8192x4320 (8K UHD 17:9) and 7680x4320 (8K UHD 16:9) at up to 30fps. V2 firmware added: FFc 3.8K 120fps, S35 4.3K 4:3 anamorphic 60fps, S35 1.9K 240fps, and additional S&Q frame rates (66, 72, 75, 88, 90, 96, 110fps) for the S35c 4K mode. Dual base ISO 800/3200. IBIS. 16 stops DR.

**Sources:**
- [The Camera Department - BURANO](https://www.thecameradept.com/sony-burano)
- [Z Systems - BURANO in Detail](https://zsyst.com/2024/03/sony-burano-in-detail/)
- [PHFX.com - BURANO 8.6K sensor area](https://phfx.com/tools/frameAndFocus/fnf.cgi?wy=b&wx=c&wc=w&mr=8632x4856)
- [No Film School - BURANO V2](https://nofilmschool.com/sony-burano-upgrades-version-2)
- [Sony Cinematography - BURANO V2](https://sony-cinematography.com/top-5-new-features-in-the-expanded-burano-v2-0/)

---

### Sony PXW-FX9
- **Sensor:** Full-frame CMOS, 35.7 x 18.8 mm, 6K readout (19 MP)
- **Confidence:** HIGH

```
Camera: FX9
  Mode: FF 6K (downsampled to 4K), Resolution: 3840x2160, Sensor Area: 35.7x18.8 mm, Max FPS: 30
  Mode: FF 5K Crop,             Resolution: 3840x2160,  Sensor Area: ~29.8x15.7 mm, Max FPS: 60
  Mode: FF 2K,                  Resolution: 1920x1080,  Sensor Area: 35.7x18.8 mm,  Max FPS: 180
  Mode: S35 4K,                 Resolution: 3840x2160,  Sensor Area: ~23.8x12.5 mm, Max FPS: 60
  Mode: S35 2K,                 Resolution: 1920x1080,  Sensor Area: ~23.8x12.5 mm, Max FPS: 120
```

**Notes:** Output resolution is always 4K UHD or HD regardless of scan mode. FF 5K crop introduced in firmware v2 with 1.25x crop. S35 modes have 1.5x crop factor. FF 2K and S35 2K modes have reduced image quality. Maximum output is 4K DCI (not available in all modes). External RAW output via XDCA-FX9 extension unit enables 16-bit RAW at up to 60fps in FF and S35. Firmware v4 added 4K 120fps RAW output (S35 crop).

**Sources:**
- [CineD - FX9 Scan Modes Explained](https://www.cined.com/sony-fx9-sensor-scan-modes-explained/)
- [Sony Cinematography - FX9 Firmware v2 Deep Dive](https://sony-cinematography.com/articles/fx9-firmware-version-2-scan-modes-deep-dive/)
- [XDCAM-USER - FX9 Scan Modes](https://www.xdcam-user.com/2019/12/28/more-on-the-pxw-fx9s-scan-modes/)

---

### Sony FX6 (ILME-FX6)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.6 x 23.8 mm, 10.2 MP effective
- **Confidence:** HIGH

```
Camera: FX6
  Mode: FF UHD (full width),    Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 60
  Mode: FF 4K DCI (5% crop),    Resolution: 4096x2160,  Sensor Area: ~33.8x17.8 mm, Max FPS: 60
  Mode: FF UHD (10% crop),      Resolution: 3840x2160,  Sensor Area: ~32.0x18.0 mm, Max FPS: 120
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 240
```

**Notes:** Same sensor as A7S III. Full-frame UHD 1-60fps uses full sensor width. 4K DCI crops 5%. Above 60fps (100/120fps) the UHD mode reads at 3840 directly (10% narrower FOV). HD supports up to 240fps. 4:2:2 10-bit internally in all modes. Dual base ISO 800/12800.

**Sources:**
- [CineD - FX6 Released](https://www.cined.com/sony-fx6-released-full-frame-4k-120-fps-dual-native-iso/)
- [XDCAM-USER - FX6 is Full Frame Sometimes](https://www.xdcam-user.com/2020/11/22/the-sony-fx6-is-full-frame-sometimes/)
- [Sony Pro - FX6](https://pro.sony/ue_US/products/handheld-camcorders/ilme-fx6)

---

### Sony FX3 (ILME-FX3)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.6 x 23.8 mm, 12.1 MP
- **Confidence:** HIGH

```
Camera: FX3
  Mode: FF UHD,                 Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 60
  Mode: FF UHD (1.1x crop),     Resolution: 3840x2160,  Sensor Area: ~32.4x21.6 mm, Max FPS: 120
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 240
```

**Notes:** Essentially an A7S III in a cinema body. 4K 120p requires 1.1x crop. All modes support 10-bit 4:2:2 internally. S-Cinetone color science. No S35 crop mode (would just be center-cropping the 12MP sensor).

**Sources:**
- [DPReview - FX3 Specifications](https://www.dpreview.com/products/sony/slrs/sony_fx3/specifications)
- [Sony FX3 Help Guide](https://helpguide.sony.net/ilc/2210/v1/en/contents/TP1000886922.html)

---

### Sony FX30 (ILME-FX30)
- **Sensor:** APS-C back-illuminated CMOS (Exmor R), 25.1 x 16.7 mm, 26 MP
- **Confidence:** HIGH

```
Camera: FX30
  Mode: S35 UHD (6K oversample, 1.04x crop), Resolution: 3840x2160, Sensor Area: ~24.1x16.0 mm, Max FPS: 60
  Mode: S35 UHD (1.62x crop from FF equiv.), Resolution: 3840x2160, Sensor Area: ~15.5x8.7 mm, Max FPS: 120
  Mode: S35 HD,                 Resolution: 1920x1080,  Sensor Area: 25.1x16.7 mm,  Max FPS: 240
```

**Notes:** Native APS-C sensor (1.5x crop from full-frame equivalent). At 4K up to 60p, reads ~6K and oversamples with slight 1.04x additional crop. At 4K 120p, crops to 3840px window directly, giving 1.62x crop (relative to the APS-C sensor itself). 10-bit 4:2:2 internally.

**Sources:**
- [B&H - FX30](https://www.bhphotovideo.com/c/product/1729317-REG/sony_ilme_fx30_fx30_digital_cinema_camera.html)
- [Newsshooter - FX30](https://www.newsshooter.com/2022/09/28/sony-fx30-the-baby-fx3-with-an-aps-c-sensor/)

---

### Sony FR7 (ILME-FR7)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.6 x 23.8 mm, ~10.3 MP effective
- **Confidence:** MEDIUM

```
Camera: FR7
  Mode: FF 4K DCI,              Resolution: 4096x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 60
  Mode: FF UHD,                 Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 60
  Mode: S35 UHD S&Q,            Resolution: 3840x2160,  Sensor Area: ~23.7x13.3 mm, Max FPS: 120
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 60
  Mode: S35 HD S&Q,             Resolution: 1920x1080,  Sensor Area: ~23.7x13.3 mm, Max FPS: 120
```

**Notes:** PTZ cinema camera. Two imager scan modes: FF and S35. Standard recording up to 4K DCI 60p. S&Q (Slow & Quick) mode enables up to 120fps in S35 crop. Cannot switch scan mode during recording. XAVC-I and XAVC-L codecs.

**Sources:**
- [Sony FR7 Help Guide](https://helpguide.sony.net/ilc/2240/v1/en/contents/TP1000673490.html)
- [Newsshooter - FR7](https://www.newsshooter.com/2022/09/06/sony-fr7-a-ptz-camera-with-a-full-frame-sensor-and-interchangeable-lens-mount/)

---

### Sony NEX-FS100
- **Sensor:** Super 35mm CMOS, 23.6 x 13.3 mm, ~3.43 MP (2464 x 1394 native)
- **Confidence:** MEDIUM

```
Camera: NEX-FS100
  Mode: S35 HD,                 Resolution: 1920x1080,  Sensor Area: 23.6x13.3 mm,  Max FPS: 60
  Mode: S35 720p,               Resolution: 1280x720,   Sensor Area: 23.6x13.3 mm,  Max FPS: 60
```

**Notes:** HD-only camera. No 4K recording (internal or external). Variable frame rates 1-60fps. AVCHD 28Mbps internal. Uncompressed 4:2:2 HDMI output to external recorder. Very low-density sensor = exceptional low-light performance for its era.

**Sources:**
- [VFX Camera Database - NEX-FS100](https://vfxcamdb.com/sony-nex-fs100/)
- [B&H - NEX-FS100U](https://www.bhphotovideo.com/c/product/761578-REG/Sony_NEX_FS100U_NEX_FS100E_Super_35mm_Sensor.html)

---

### Sony NEX-FS700
- **Sensor:** Super 35mm CMOS, 24.0 x 12.7 mm, 11.6 MP (4352 x 2662 native)
- **Confidence:** HIGH

```
Camera: NEX-FS700
  Mode: S35 4K (external RAW),  Resolution: 4096x2160,  Sensor Area: 24.0x12.7 mm,  Max FPS: 60
  Mode: S35 HD (continuous),    Resolution: 1920x1080,  Sensor Area: 24.0x12.7 mm,  Max FPS: 60
  Mode: S35 HD (burst 240fps),  Resolution: 1920x1080,  Sensor Area: 24.0x12.7 mm,  Max FPS: 240
  Mode: S35 HD (burst 480fps),  Resolution: reduced,     Sensor Area: 24.0x12.7 mm,  Max FPS: 480
  Mode: S35 HD (burst 960fps),  Resolution: reduced,     Sensor Area: 24.0x12.7 mm,  Max FPS: 960
```

**Notes:** 4K requires external recorder (HXR-IFR5 + AXS-R5). Internal recording is AVCHD only (1080p up to 60fps continuous). Super slow motion modes are burst-limited: 240fps = full 1920x1080 (8-16 sec bursts), 480fps and 960fps have reduced resolution. In 60i mode: 120/240/480/960fps options.

**Sources:**
- [VFX Camera Database - NEX-FS700](https://vfxcamdb.com/sony-nex-fs700/)
- [B&H - NEX-FS700U](https://www.bhphotovideo.com/c/product/853273-REG/Sony_NEX_FS700_4K_Ready_High_Speed.html)
- [Philip Bloom - FS700](https://philipbloom.net/blog/fs700/)

---

### Sony PXW-FS5 / PXW-FS5 II
- **Sensor:** Super 35mm CMOS, 23.6 x 13.3 mm, 11.6 MP
- **Confidence:** MEDIUM

```
Camera: PXW-FS5 / PXW-FS5 II
  Mode: S35 UHD (internal),     Resolution: 3840x2160,  Sensor Area: 23.6x13.3 mm,  Max FPS: 60
  Mode: S35 4K RAW (external, burst), Resolution: 4096x2160, Sensor Area: 23.6x13.3 mm, Max FPS: 60
  Mode: S35 2K RAW (external),  Resolution: 2048x1080,  Sensor Area: 23.6x13.3 mm,  Max FPS: 240
  Mode: S35 HD (continuous),    Resolution: 1920x1080,  Sensor Area: 23.6x13.3 mm,  Max FPS: 120
  Mode: S35 HD (burst),         Resolution: 1920x1080,  Sensor Area: 23.6x13.3 mm,  Max FPS: 240
```

**Notes:** FS5 II includes RAW output and HFR features that were paid upgrades on original FS5. 4K RAW external is burst-limited (~4 seconds). HD 240fps is burst-limited (~8 seconds). Continuous HD at 120fps is unlimited. FS5 II adds Venice color science and mechanical ND filter. Internal UHD via XAVC-L.

**Sources:**
- [VFX Camera Database - PXW-FS5](https://vfxcamdb.com/sony-pxw-fs5/)
- [Sony Pro - FS5 II FAQ](https://pro.sony/ue_US/products/handheld-camcorders/broadcast-pxw-fs5m2-faqs)

---

### Sony PXW-FS7 / PXW-FS7 II
- **Sensor:** Super 35mm CMOS, 23.6 x 13.3 mm, 11.6 MP
- **Confidence:** HIGH

```
Camera: PXW-FS7 / PXW-FS7 II
  Mode: S35 4K DCI (internal),  Resolution: 4096x2160,  Sensor Area: 23.6x13.3 mm,  Max FPS: 60
  Mode: S35 UHD (internal),     Resolution: 3840x2160,  Sensor Area: 23.6x13.3 mm,  Max FPS: 60
  Mode: S35 4K RAW (external),  Resolution: 4096x2160,  Sensor Area: 23.6x13.3 mm,  Max FPS: 60
  Mode: S35 2K RAW (external),  Resolution: 2048x1080,  Sensor Area: 23.6x13.3 mm,  Max FPS: 240
  Mode: S35 HD (XAVC-I),        Resolution: 1920x1080,  Sensor Area: 23.6x13.3 mm,  Max FPS: 180
  Mode: S35 HD (XAVC-L),        Resolution: 1920x1080,  Sensor Area: 23.6x13.3 mm,  Max FPS: 120
```

**Notes:** Unlike FS5, the FS7 records 4K internally with unlimited record times. HD 180fps is continuous (no burst limit). RAW output requires XDCA-FS7 + HXR-IFR5 + AXS-R5. FS7 II adds variable ND, improved ergonomics, improved lock mount; recording specs are identical.

**Sources:**
- [VFX Camera Database - PXW-FS7](https://vfxcamdb.com/sony-pxw-fs7/)
- [Sony Pro - FS7 II](https://pro.sony/ue_US/products/handheld-camcorders/pxw-fs7m2)

---

## MIRRORLESS CAMERAS

---

### Sony A7S (ILCE-7S)
- **Sensor:** Full-frame CMOS, 35.8 x 23.9 mm, 12.2 MP
- **Confidence:** MEDIUM

```
Camera: A7S
  Mode: FF UHD (HDMI external),  Resolution: 3840x2160,  Sensor Area: 35.8x23.9 mm,  Max FPS: 30
  Mode: FF HD,                   Resolution: 1920x1080,  Sensor Area: 35.8x23.9 mm,  Max FPS: 60
  Mode: S35 720p,                Resolution: 1280x720,   Sensor Area: ~23.9x15.9 mm, Max FPS: 120
```

**Notes:** Cannot record 4K internally. 4K output via clean HDMI to external recorder only. Internal recording limited to 1080p AVCHD/XAVC S. 120fps only at 720p in APS-C crop mode.

**Sources:**
- [B&H - A7S](https://www.bhphotovideo.com/c/product/1044728-REG/sony_ilce7s_b_alpha_a7s_mirrorless_digital.html)
- [Imaging Resource - A7S](https://www.imaging-resource.com/PRODS/sony-a7s/sony-a7sA.HTM)

---

### Sony A7S II (ILCE-7SM2)
- **Sensor:** Full-frame CMOS, 35.6 x 23.8 mm, 12.2 MP
- **Confidence:** HIGH

```
Camera: A7S II
  Mode: FF UHD (internal),      Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 30
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 60
  Mode: Crop HD (2.2x crop),    Resolution: 1920x1080,  Sensor Area: ~16.2x9.1 mm,  Max FPS: 120
```

**Notes:** First A7S with internal 4K recording. 4K is full-frame only (no APS-C 4K mode - sensor is too low-res). 120fps in HD mode uses a small 2.2x crop from center of sensor. 8-bit 4:2:0 internal, 8-bit 4:2:2 HDMI.

**Sources:**
- [Suggestion of Motion - A7S II Recording Modes](https://suggestionofmotion.com/blog/sony-a7s2-recording-modes-overview/)
- [Imaging Resource - A7S II](https://www.imaging-resource.com/PRODS/sony-a7s-ii/sony-a7s-iiA.HTM)

---

### Sony A7S III (ILCE-7SM3)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.6 x 23.8 mm, 12.1 MP
- **Confidence:** HIGH

```
Camera: A7S III
  Mode: FF UHD,                 Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 60
  Mode: FF UHD (1.1x crop),     Resolution: 3840x2160,  Sensor Area: ~32.4x21.6 mm, Max FPS: 120
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 240
```

**Notes:** Full pixel readout for 4K (no binning). 4K 120p has mild 1.1x crop and 30-min limit. 10-bit 4:2:2 internally in all modes including 4K 120p. S-Cinetone, S-Log3. Same sensor as FX3 and FX6.

**Sources:**
- [DPReview - A7S III Review](https://www.dpreview.com/reviews/sony-a7s-iii-review/2)
- [CineD - A7S III Announced](https://www.cined.com/sony-a7s-iii-announced-4k120-10-bit-422-16-bit-raw-output/)

---

### Sony A7 III (ILCE-7M3)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.6 x 23.8 mm, 24.2 MP
- **Confidence:** HIGH

```
Camera: A7 III
  Mode: FF UHD (6K oversample),  Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 25
  Mode: FF UHD (1.2x crop),      Resolution: 3840x2160,  Sensor Area: ~29.7x16.7 mm, Max FPS: 30
  Mode: FF HD,                   Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 120
```

**Notes:** Full sensor width used at 4K 24p/25p with 6K oversampling. 4K 30p applies 1.2x crop with 5K oversampling. No 4K 60p. HD 120fps. 8-bit 4:2:0 internal, 8-bit 4:2:2 HDMI.

**Sources:**
- [Mirrorless Comparison - A7 III Video Settings](https://mirrorlesscomparison.com/guide/sony-a7-a9-video-settings-explained/)
- [DPReview - A7 III Review](https://www.dpreview.com/reviews/sony-a7-iii-review/10)

---

### Sony A7 IV (ILCE-7M4)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.9 x 24.0 mm, 33 MP
- **Confidence:** HIGH

```
Camera: A7 IV
  Mode: FF UHD (7K oversample),  Resolution: 3840x2160,  Sensor Area: 35.9x24.0 mm,  Max FPS: 30
  Mode: S35 UHD (4.6K oversample), Resolution: 3840x2160, Sensor Area: ~23.9x13.4 mm, Max FPS: 60
  Mode: FF HD,                   Resolution: 1920x1080,  Sensor Area: 35.9x24.0 mm,  Max FPS: 120
```

**Notes:** 4K 24-30p uses full sensor width with 7K oversampling. 4K 50/60p requires Super35 crop (1.5x) with 4.6K oversampling. No 4K 120p. 10-bit 4:2:2 internally. XAVC S-I available.

**Sources:**
- [CineD - A7 IV Announced](https://www.cined.com/sony-a7-iv-announced-allrounder-with-10-bit-422-video-and-new-33mp-sensor/)
- [4K Shooters - A7 IV Full-Frame 4K 60p](https://www.4kshooters.net/2022/03/04/shooting-full-frame-4k-60p-video-on-the-sony-a7-iv-no-crop/)

---

### Sony A7 V (ILCE-7M5)
- **Sensor:** Full-frame back-illuminated CMOS, ~35.9 x 24.0 mm, 33 MP
- **Confidence:** MEDIUM (recently announced Dec 2025)

```
Camera: A7 V
  Mode: FF UHD (7K oversample),  Resolution: 3840x2160,  Sensor Area: 35.9x24.0 mm,  Max FPS: 60
  Mode: S35 UHD,                 Resolution: 3840x2160,  Sensor Area: ~23.9x13.4 mm, Max FPS: 120
  Mode: FF HD,                   Resolution: 1920x1080,  Sensor Area: 35.9x24.0 mm,  Max FPS: 240
```

**Notes:** Key upgrade over A7 IV: 4K 60p is now full-frame (no crop), oversampled from 7K. 4K 120p available in S35 crop. 10-bit 4:2:2. "4K Angle of View Priority" setting lets you choose between FF 4K/60p or S35 4K/120p.

**Sources:**
- [DPReview - A7 V Review](https://www.dpreview.com/reviews/sony-a7-v-review)
- [Engadget - A7 V Announced](https://www.engadget.com/cameras/sonys-much-anticipated-a7-v-is-here-with-a-faster-33mp-sensor-and-4k-120p-video-140403371.html)

---

### Sony A7R (ILCE-7R)
- **Sensor:** Full-frame CMOS, 35.9 x 24.0 mm, 36.4 MP
- **Confidence:** MEDIUM

```
Camera: A7R
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.9x24.0 mm,  Max FPS: 60
```

**Notes:** No 4K video capability. HD only. AVCHD 2.0 recording. 60p/60i/24p options. This is a stills-focused camera with basic video.

**Sources:**
- [DPReview - A7R Review](https://www.dpreview.com/reviews/sony-alpha-a7r)

---

### Sony A7R II (ILCE-7RM2)
- **Sensor:** Full-frame back-illuminated CMOS, 35.9 x 24.0 mm, 42.4 MP
- **Confidence:** HIGH

```
Camera: A7R II
  Mode: FF UHD (pixel-binned),  Resolution: 3840x2160,  Sensor Area: 35.9x24.0 mm,  Max FPS: 30
  Mode: S35 UHD (full readout),  Resolution: 3840x2160,  Sensor Area: ~23.9x13.4 mm, Max FPS: 30
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.9x24.0 mm,  Max FPS: 60
```

**Notes:** First A7R with 4K. Full-frame 4K uses pixel binning. S35 crop 4K uses full pixel readout (better quality). No 4K 60p. Max HD is 60fps. 8-bit 4:2:0 internal. XAVC S 100Mbps.

**Sources:**
- [DPReview - A7R II Review](https://www.dpreview.com/reviews/sony-alpha-7r-ii)
- [Alik Griffin - A7R II Video Guide](https://alikgriffin.com/the-ultimate-sony-a7r-ii-video-guide/)

---

### Sony A7R III (ILCE-7RM3)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.9 x 24.0 mm, 42.4 MP
- **Confidence:** HIGH

```
Camera: A7R III
  Mode: FF UHD (6K oversample), Resolution: 3840x2160,  Sensor Area: 35.9x24.0 mm,  Max FPS: 30
  Mode: S35 UHD,                Resolution: 3840x2160,  Sensor Area: ~23.9x13.4 mm, Max FPS: 30
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.9x24.0 mm,  Max FPS: 120
```

**Notes:** Full sensor 4K with 6K oversampling (much better than A7R II's full-frame 4K). S35 mode at 1.5x crop. No 4K 60p. HD up to 120fps. 8-bit 4:2:0 internal.

**Sources:**
- [No Film School - A7R III](https://nofilmschool.com/2017/10/everything-we-know-about-new-sony-a7r-iii-full-frame-camera)
- [Mirrorless Comparison - A7/A7R/A9 Video Settings](https://mirrorlesscomparison.com/guide/sony-a7-a9-video-settings-explained/)

---

### Sony A7R IV (ILCE-7RM4)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.7 x 23.8 mm, 61.0 MP
- **Confidence:** MEDIUM

```
Camera: A7R IV
  Mode: FF UHD,                 Resolution: 3840x2160,  Sensor Area: 35.7x23.8 mm,  Max FPS: 30
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.7x23.8 mm,  Max FPS: 120
```

**Notes:** No 4K 60p. No 8K. 4K/30p with no crop in 8-bit 4:2:0. Limited video features compared to other models. HD 120fps available.

**Sources:**
- [DPReview - A7R V vs A7R IV comparison](https://www.dpreview.com/reviews/sony-a7rv-review)

---

### Sony A7R V (ILCE-7RM5)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.7 x 23.8 mm, 61.0 MP
- **Confidence:** HIGH

```
Camera: A7R V
  Mode: 8K UHD (1.24x crop),   Resolution: 7680x4320,  Sensor Area: ~28.8x16.2 mm, Max FPS: 25
  Mode: FF UHD,                 Resolution: 3840x2160,  Sensor Area: 35.7x23.8 mm,  Max FPS: 30
  Mode: FF/Crop UHD (1.24x),    Resolution: 3840x2160,  Sensor Area: ~28.8x16.2 mm, Max FPS: 60
  Mode: S35 UHD (6.2K oversample), Resolution: 3840x2160, Sensor Area: ~23.8x13.4 mm, Max FPS: 30
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.7x23.8 mm,  Max FPS: 120
```

**Notes:** 8K at 24/25p with 1.24x crop. 4K 24-30p full frame with no crop. 4K 60p requires 1.24x crop (from 8K region, not S35). 10-bit 4:2:0 internal. Significant rolling shutter in some modes.

**Sources:**
- [DPReview - A7R V Review](https://www.dpreview.com/reviews/sony-a7rv-review)
- [Photography Blog - A7R V Review](https://www.photographyblog.com/reviews/sony_a7r_v_review)

---

### Sony A9 (ILCE-9)
- **Sensor:** Full-frame stacked CMOS (Exmor RS), 35.6 x 23.8 mm, 24.2 MP
- **Confidence:** MEDIUM

```
Camera: A9
  Mode: FF UHD (6K oversample), Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 30
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 120
```

**Notes:** Stacked sensor designed for speed (stills). 4K uses full pixel readout from 6K oversampling. No 4K 60p. HD up to 120fps. 8-bit 4:2:0 internal.

**Sources:**
- [Imaging Resource - A9 Video Review](https://www.imaging-resource.com/PRODS/sony-a9/sony-a9VIDEO.HTM)

---

### Sony A9 II (ILCE-9M2)
- **Sensor:** Full-frame stacked CMOS (Exmor RS), 35.6 x 23.8 mm, 24.2 MP
- **Confidence:** MEDIUM

```
Camera: A9 II
  Mode: FF UHD (6K oversample), Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 30
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 120
```

**Notes:** Same video specs as A9. Sports/photojournalism body with networking improvements. 4K full pixel readout, no binning. 8-bit 4:2:0 internal. XAVC S 100Mbps.

**Sources:**
- [B&H - A9 II](https://www.bhphotovideo.com/c/product/1509600-REG/sony_ilce9m2_b_alpha_a9_ii_mirrorless.html)

---

### Sony A9 III (ILCE-9M3)
- **Sensor:** Full-frame global shutter stacked CMOS, 35.6 x 23.8 mm, 24.6 MP
- **Confidence:** HIGH

```
Camera: A9 III
  Mode: FF UHD (6K oversample), Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 60
  Mode: FF UHD (no oversample),  Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 120
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 120
  Mode: S&Q HD,                 Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 240
```

**Notes:** World's first full-frame global shutter mirrorless. 4K 120p with NO crop (unique among Sony full-frame mirrorless) but without oversampling (reads at 3840 directly above 60p). 10-bit 4:2:2 internally. S&Q mode reaches 240fps at 1080p (no audio).

**Sources:**
- [DPReview - A9 III Review](https://www.dpreview.com/reviews/sony-a9-iii-in-depth-review)
- [B&H - A9 III](https://www.bhphotovideo.com/c/product/1794764-REG/sony_ilce_9m3_a9_iii_mirrorless_camera.html)

---

### Sony A1 (ILCE-1)
- **Sensor:** Full-frame stacked CMOS (Exmor RS), 35.9 x 24.0 mm, 50.1 MP
- **Confidence:** HIGH

```
Camera: A1
  Mode: FF 8K UHD (8.6K oversample), Resolution: 7680x4320, Sensor Area: 35.9x24.0 mm, Max FPS: 30
  Mode: FF UHD (8.6K oversample),    Resolution: 3840x2160, Sensor Area: 35.9x24.0 mm, Max FPS: 60
  Mode: FF UHD (1.1x crop),          Resolution: 3840x2160, Sensor Area: ~32.6x18.3 mm, Max FPS: 120
  Mode: S35 UHD (5.8K oversample),   Resolution: 3840x2160, Sensor Area: ~23.9x13.4 mm, Max FPS: 60
  Mode: FF HD,                       Resolution: 1920x1080, Sensor Area: 35.9x24.0 mm, Max FPS: 240
```

**Notes:** 8K at 24-30p with no crop, oversampled from 8.6K. 4K 60p full-frame, oversampled from 8.6K. 4K 120p with 1.1x crop (still reads at high resolution). S35 4K oversampled from 5.8K at up to 60p. 10-bit 4:2:2 internal. XAVC HS (H.265) up to 600Mbps.

**Sources:**
- [DPReview - A1 Review](https://www.dpreview.com/reviews/sony-a1-review/7)
- [No Film School - A1 Video Features](https://nofilmschool.com/lets-dive-sony-alpha-1-video-features)
- [Wolfcrow - A1 Features](https://wolfcrow.com/important-quirks-and-features-of-sony-alpha-1-for-cinematography/)

---

### Sony A1 II (ILCE-1M2)
- **Sensor:** Full-frame stacked CMOS (Exmor RS), 35.9 x 24.0 mm, 50.1 MP
- **Confidence:** MEDIUM

```
Camera: A1 II
  Mode: FF 8K UHD,              Resolution: 7680x4320,  Sensor Area: 35.9x24.0 mm,  Max FPS: 30
  Mode: FF UHD,                 Resolution: 3840x2160,  Sensor Area: 35.9x24.0 mm,  Max FPS: 60
  Mode: FF UHD (1.13x crop),    Resolution: 3840x2160,  Sensor Area: ~31.8x17.8 mm, Max FPS: 120
  Mode: S35 UHD (5.8K oversample), Resolution: 3840x2160, Sensor Area: ~23.9x13.4 mm, Max FPS: 60
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.9x24.0 mm,  Max FPS: 240
```

**Notes:** Similar video specs to A1. Same sensor but updated processor. 8K at up to 30p. 4K 120p with 1.13x crop. No recording time limit on 4K 60p. External 16-bit RAW 4.3K/4K output at 60p via HDMI.

**Sources:**
- [Pro Moviemaker - A1 II Test](https://www.promoviemaker.net/reviews/big-test-sony-a1-ii/)
- [Cameralabs - A1 II Review](https://www.cameralabs.com/sony-a1-ii-review/)

---

### Sony A7C (ILCE-7C)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.6 x 23.8 mm, 24.2 MP
- **Confidence:** MEDIUM

```
Camera: A7C
  Mode: FF UHD (6K oversample), Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 25
  Mode: FF UHD (~1.2x crop),    Resolution: 3840x2160,  Sensor Area: ~29.7x16.7 mm, Max FPS: 30
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 120
```

**Notes:** Same sensor as A7 III. 4K 24/25p uses full sensor width with 6K oversampling. 4K 30p has mild crop. No 4K 60p. 8-bit 4:2:0 internal only. No 10-bit.

**Sources:**
- [Photography Blog - A7C Review](https://www.photographyblog.com/reviews/sony_a7c_review)

---

### Sony A7C II (ILCE-7CM2)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.9 x 24.0 mm, 33 MP
- **Confidence:** HIGH

```
Camera: A7C II
  Mode: FF UHD (7K oversample), Resolution: 3840x2160,  Sensor Area: 35.9x24.0 mm,  Max FPS: 30
  Mode: S35 UHD (4.6K oversample), Resolution: 3840x2160, Sensor Area: ~23.9x13.4 mm, Max FPS: 60
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.9x24.0 mm,  Max FPS: 120
```

**Notes:** Same sensor as A7 IV. 4K up to 30p full-frame with 7K oversampling. 4K 50/60p only in S35 crop (1.5x). No 4K 120p. 10-bit 4:2:2 internally.

**Sources:**
- [CineD - A7C II Announced](https://www.cined.com/sony-a7c-ii-announced-new-compact-full-frame-camera-with-33mp-10-bit-4k60-super35-video-and-more/)
- [DPReview - A7C II Review](https://www.dpreview.com/reviews/sony-a7c-ii-review)

---

### Sony A7CR (ILCE-7CR)
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.7 x 23.8 mm, 61.0 MP
- **Confidence:** MEDIUM

```
Camera: A7CR
  Mode: FF UHD,                 Resolution: 3840x2160,  Sensor Area: 35.7x23.8 mm,  Max FPS: 30
  Mode: FF UHD (1.2x crop),     Resolution: 3840x2160,  Sensor Area: ~29.8x16.7 mm, Max FPS: 60
  Mode: S35 UHD (6.2K oversample), Resolution: 3840x2160, Sensor Area: ~23.8x13.4 mm, Max FPS: 30
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.7x23.8 mm,  Max FPS: 120
```

**Notes:** Same sensor as A7R V but in compact A7C body. 4K 24-30p full frame. 4K 60p with 1.2x crop. S35 4K with 6.2K oversampling. 10-bit 4:2:2 internal.

**Sources:**
- [CineD - A7CR Announced](https://www.cined.com/sony-a7c-r-announced-61mp-and-full-frame-4k60-10-bit-video-in-a-compact-body/)
- [DPReview - A7CR Review](https://www.dpreview.com/reviews/sony-a7cr-review)

---

### Sony ZV-E1
- **Sensor:** Full-frame back-illuminated CMOS (Exmor R), 35.6 x 23.8 mm, 12.1 MP
- **Confidence:** HIGH

```
Camera: ZV-E1
  Mode: FF UHD,                 Resolution: 3840x2160,  Sensor Area: 35.6x23.8 mm,  Max FPS: 60
  Mode: FF UHD (~1.1x crop),    Resolution: 3840x2160,  Sensor Area: ~32.4x21.6 mm, Max FPS: 120
  Mode: FF HD,                  Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 120
  Mode: S&Q HD,                 Resolution: 1920x1080,  Sensor Area: 35.6x23.8 mm,  Max FPS: 240
```

**Notes:** Same 12.1MP sensor as A7S III / FX3. Full pixel readout 4K with no binning. 4K 120p added via paid license upgrade (initially 60p only). 10-bit 4:2:2 internal. XAVC S-I up to 600Mbps at 4K 60p. S-Cinetone.

**Sources:**
- [DPReview - ZV-E1 gains 4K/120](https://www.dpreview.com/news/8213800304/sony-zv-e1-gains-4k-120-and-1080-240-with-updated-license)
- [B&H - ZV-E1](https://www.bhphotovideo.com/c/product/1759472-REG/sony_zv_e1_mirrorless_camera_black.html)

---

### Sony ZV-E10 (original)
- **Sensor:** APS-C CMOS (Exmor), 23.5 x 15.6 mm, 24.2 MP
- **Confidence:** MEDIUM

```
Camera: ZV-E10
  Mode: S35 UHD,                Resolution: 3840x2160,  Sensor Area: 23.5x15.6 mm,  Max FPS: 30
  Mode: S35 HD,                 Resolution: 1920x1080,  Sensor Area: 23.5x15.6 mm,  Max FPS: 120
```

**Notes:** APS-C sensor. 4K limited to 30p. HD up to 120fps. 8-bit 4:2:0 internal. XAVC S 100Mbps. Budget vlogging camera.

**Sources:**
- [DPReview - ZV-E10 II Review](https://www.dpreview.com/reviews/sony-zv-e10-ii-vlogging-camera-review)

---

### Sony ZV-E10 II
- **Sensor:** APS-C back-illuminated CMOS (Exmor R), 23.5 x 15.6 mm, 26 MP
- **Confidence:** HIGH

```
Camera: ZV-E10 II
  Mode: S35 UHD (5.6K oversample), Resolution: 3840x2160, Sensor Area: 23.5x15.6 mm, Max FPS: 60
  Mode: S35 UHD (crop),         Resolution: 3840x2160,  Sensor Area: ~14.5x8.2 mm,  Max FPS: 120
  Mode: S35 HD,                 Resolution: 1920x1080,  Sensor Area: 23.5x15.6 mm,  Max FPS: 120
```

**Notes:** Major upgrade over original. 4K 60p with 5.6K oversampling. 4K 120p with additional crop. 10-bit 4:2:2 internal. XAVC HS and XAVC S-I formats.

**Sources:**
- [DPReview - ZV-E10 II Review](https://www.dpreview.com/reviews/sony-zv-e10-ii-vlogging-camera-review)
- [B&H - ZV-E10 II](https://www.bhphotovideo.com/c/product/1838825-REG/sony_zv_e10_ii_mirrorless_camera.html)

---

## Contradictions

### VENICE 5.7K 16:9 max FPS
- **The Camera Department** lists VENICE 5.7K 16:9 at up to 72fps (grouped with 6K 17:9 and 6K 1.85:1)
- **Sony's HFR press releases** mention 6K modes but do not specifically call out 5.7K 16:9 HFR
- Resolution: LOW confidence on 72fps for 5.7K specifically; it may be 60fps standard and 72fps HFR

### FX9 FF 2K max FPS
- **Sony Cinematography** deep dive says FF 2K goes up to 180fps
- **CineD scan modes article** mentions 120fps for S35 2K
- These may both be correct (FF 2K = 180fps, S35 2K = 120fps) rather than a contradiction

### A1 vs A1 II 4K 120p crop
- **A1**: Described as 1.1x crop at 4K 120p by DPReview (also quoted as 1.13x elsewhere)
- **A1 II**: Described as 1.13x crop at 4K 120p
- These are likely the same crop, with 1.1x being a rounding

### BURANO sensor areas
- Exact sensor area per mode is not published by Sony for most modes
- The 8.6K 16:9 mode is confirmed at 35.87 x 20.18 mm by PHFX.com
- All other mode areas are calculated estimates from pixel pitch

---

## Source Registry

| # | Title | URL | Date | Queries |
|---|-------|-----|------|---------|
| 1 | VFX Camera Database - VENICE | https://vfxcamdb.com/sony-venice/ | N/A | Q1, Q11 |
| 2 | Sony Cinematography - VENICE 2 | https://sony-cinematography.com/venice2/ | N/A | Q2 |
| 3 | The Camera Department - VENICE | https://www.thecameradept.com/sony-venice | N/A | Q1, Q12 |
| 4 | The Camera Department - VENICE 2 | https://www.thecameradept.com/sony-venice-2 | N/A | Q2 |
| 5 | The Camera Department - BURANO | https://www.thecameradept.com/sony-burano | N/A | Q3, Q13 |
| 6 | Z Systems - BURANO in Detail | https://zsyst.com/2024/03/sony-burano-in-detail/ | 2024/03 | Q3 |
| 7 | CineD - FX9 Scan Modes Explained | https://www.cined.com/sony-fx9-sensor-scan-modes-explained/ | N/A | Q5 |
| 8 | Sony Cinematography - FX9 FW2 Deep Dive | https://sony-cinematography.com/articles/fx9-firmware-version-2-scan-modes-deep-dive/ | N/A | Q5 |
| 9 | CineD - FX6 Released | https://www.cined.com/sony-fx6-released-full-frame-4k-120-fps-dual-native-iso/ | N/A | Q5 |
| 10 | XDCAM-USER - FX6 Full Frame | https://www.xdcam-user.com/2020/11/22/the-sony-fx6-is-full-frame-sometimes/ | 2020/11 | Q5 |
| 11 | B&H - PMW-F55 | https://www.bhphotovideo.com/c/product/898428-REG/Sony_PMW_F55_CineAlta_4K_Digital.html | N/A | Q4 |
| 12 | No Film School - AXS-R7 120fps | https://nofilmschool.com/2016/02/sony-axs-r7-recorder-120fps-4k-raw-f55 | 2016/02 | Q4 |
| 13 | AbelCine - HFR with F55 | https://www.abelcine.com/articles/blog-and-knowledge/tutorials-and-guides/high-frame-rate-recording-with-the-sony-f55 | N/A | Q4 |
| 14 | Sony Pro - F65 | https://pro.sony/ue_US/products/digital-cinema-cameras/f65 | N/A | Q4 |
| 15 | VFX Camera Database - F65 | https://vfxcamdb.com/sony-f65/ | N/A | Q4 |
| 16 | VFX Camera Database - F5 | https://vfxcamdb.com/sony-f5/ | N/A | Q4 |
| 17 | VFX Camera Database - F55 | https://vfxcamdb.com/sony-f55/ | N/A | Q4 |
| 18 | VFX Camera Database - PMW-F3 | https://vfxcamdb.com/sony-pmw-f3/ | N/A | Q4 |
| 19 | VFX Camera Database - NEX-FS100 | https://vfxcamdb.com/sony-nex-fs100/ | N/A | Q8 |
| 20 | VFX Camera Database - NEX-FS700 | https://vfxcamdb.com/sony-nex-fs700/ | N/A | Q8 |
| 21 | VFX Camera Database - PXW-FS7 | https://vfxcamdb.com/sony-pxw-fs7/ | N/A | Q9 |
| 22 | VFX Camera Database - PXW-FS5 | https://vfxcamdb.com/sony-pxw-fs5/ | N/A | Q9 |
| 23 | B&H - NEX-FS700U | https://www.bhphotovideo.com/c/product/853273-REG/Sony_NEX_FS700_4K_Ready_High_Speed.html | N/A | Q8 |
| 24 | Philip Bloom - FS700 | https://philipbloom.net/blog/fs700/ | N/A | Q8 |
| 25 | Sony Pro - FS5 II FAQ | https://pro.sony/ue_US/products/handheld-camcorders/broadcast-pxw-fs5m2-faqs | N/A | Q9 |
| 26 | Sony Pro - FS7 II | https://pro.sony/ue_US/products/handheld-camcorders/pxw-fs7m2 | N/A | Q9 |
| 27 | DPReview - A7S III Review | https://www.dpreview.com/reviews/sony-a7s-iii-review/2 | N/A | Q10 |
| 28 | CineD - A7S III Announced | https://www.cined.com/sony-a7s-iii-announced-4k120-10-bit-422-16-bit-raw-output/ | N/A | Q10 |
| 29 | DPReview - A1 Review | https://www.dpreview.com/reviews/sony-a1-review/7 | N/A | Q10 |
| 30 | No Film School - A1 Video | https://nofilmschool.com/lets-dive-sony-alpha-1-video-features | N/A | Q10 |
| 31 | DPReview - A9 III Review | https://www.dpreview.com/reviews/sony-a9-iii-in-depth-review | N/A | Q10 |
| 32 | CineD - A7 IV Announced | https://www.cined.com/sony-a7-iv-announced-allrounder-with-10-bit-422-video-and-new-33mp-sensor/ | N/A | Q10 |
| 33 | DPReview - A7R V Review | https://www.dpreview.com/reviews/sony-a7rv-review | N/A | Q10 |
| 34 | CineD - A7C II Announced | https://www.cined.com/sony-a7c-ii-announced-new-compact-full-frame-camera-with-33mp-10-bit-4k60-super35-video-and-more/ | N/A | Q10 |
| 35 | CineD - A7CR Announced | https://www.cined.com/sony-a7c-r-announced-61mp-and-full-frame-4k60-10-bit-video-in-a-compact-body/ | N/A | Q10 |
| 36 | DPReview - ZV-E1 4K/120 update | https://www.dpreview.com/news/8213800304/sony-zv-e1-gains-4k-120-and-1080-240-with-updated-license | N/A | Q10 |
| 37 | B&H - FX30 | https://www.bhphotovideo.com/c/product/1729317-REG/sony_ilme_fx30_fx30_digital_cinema_camera.html | N/A | Q6 |
| 38 | Newsshooter - FX30 | https://www.newsshooter.com/2022/09/28/sony-fx30-the-baby-fx3-with-an-aps-c-sensor/ | 2022/09 | Q6 |
| 39 | Sony FR7 Help Guide | https://helpguide.sony.net/ilc/2240/v1/en/contents/TP1000673490.html | N/A | Q7 |
| 40 | Newsshooter - FR7 | https://www.newsshooter.com/2022/09/06/sony-fr7-a-ptz-camera-with-a-full-frame-sensor-and-interchangeable-lens-mount/ | 2022/09 | Q7 |
| 41 | DPReview - A7 V Review | https://www.dpreview.com/reviews/sony-a7-v-review | 2025/12 | Q10 |
| 42 | Engadget - A7 V | https://www.engadget.com/cameras/sonys-much-anticipated-a7-v-is-here-with-a-faster-33mp-sensor-and-4k-120p-video-140403371.html | 2025/12 | Q10 |
| 43 | Pro Moviemaker - A1 II Test | https://www.promoviemaker.net/reviews/big-test-sony-a1-ii/ | N/A | Q10 |
| 44 | Cameralabs - A1 II Review | https://www.cameralabs.com/sony-a1-ii-review/ | N/A | Q10 |
| 45 | Sony Cinematography - VENICE HFR | https://sony-cinematography.com/articles/sony-s-venice-continues-to-evolve-with-high-frame-rate-up-to-90-frames-per-second-at-6k/ | N/A | Q12 |
| 46 | ProVideo Coalition - VENICE HFR | https://www.provideocoalition.com/sony-venice-gets-high-frame-rate-shooting-up-to-4k-120fps/ | N/A | Q12 |
| 47 | No Film School - BURANO V2 | https://nofilmschool.com/sony-burano-upgrades-version-2 | N/A | Q13 |
| 48 | Sony Cinematography - BURANO V2 | https://sony-cinematography.com/top-5-new-features-in-the-expanded-burano-v2-0/ | N/A | Q13 |
| 49 | PHFX.com - BURANO sensor area | https://phfx.com/tools/frameAndFocus/ | N/A | Q3 |
| 50 | CBM-Cine - VENICE 2 6K | https://www.cbm-cine.com/Sony-VENICE-2-with-6K-image-sensor/VENICE-6K-MPC-3626 | N/A | Q2 |
| 51 | Suggestion of Motion - A7S II modes | https://suggestionofmotion.com/blog/sony-a7s2-recording-modes-overview/ | N/A | Q10 |
| 52 | DPReview - ZV-E10 II Review | https://www.dpreview.com/reviews/sony-zv-e10-ii-vlogging-camera-review | N/A | Q10 |
| 53 | B&H - PMW-F3L | https://www.bhphotovideo.com/c/product/848144-REG/Sony_PMW_F3L_RGB_PMW_F3L_Super_35mm_Full_HD.html | N/A | Q11 |
| 54 | Mirrorless Comparison - A7/A9 Video | https://mirrorlesscomparison.com/guide/sony-a7-a9-video-settings-explained/ | N/A | Q10 |
| 55 | DPReview - A7 III Review | https://www.dpreview.com/reviews/sony-a7-iii-review/10 | N/A | Q10 |
| 56 | Imaging Resource - A7S | https://www.imaging-resource.com/PRODS/sony-a7s/sony-a7sA.HTM | N/A | Q10 |
| 57 | Imaging Resource - A7S II | https://www.imaging-resource.com/PRODS/sony-a7s-ii/sony-a7s-iiA.HTM | N/A | Q10 |
| 58 | DPReview - A7R II Review | https://www.dpreview.com/reviews/sony-alpha-7r-ii | N/A | Q10 |
| 59 | No Film School - A7R III | https://nofilmschool.com/2017/10/everything-we-know-about-new-sony-a7r-iii-full-frame-camera | 2017/10 | Q10 |
| 60 | Imaging Resource - A9 Video | https://www.imaging-resource.com/PRODS/sony-a9/sony-a9VIDEO.HTM | N/A | Q10 |
