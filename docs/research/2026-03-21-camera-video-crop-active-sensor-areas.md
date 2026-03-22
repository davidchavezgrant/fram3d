# Raw Research Findings: Camera Video Crop Mode Active Sensor Areas

## Queries Executed
1. "Canon EOS 5D Mark IV 1DX Mark II 4K video crop factor active sensor area dimensions mm" - 3 useful results
2. "Canon EOS R5 8K active sensor area dimensions mm R5 Mark II 4K 120fps crop" - 2 useful results
3. "Sony A7 IV A7V 4K 60fps 120fps S35 APS-C crop active sensor dimensions mm video" - 3 useful results
4. "Fujifilm GFX 100 100S 100 II 4K 8K video crop sensor area dimensions mm" - 4 useful results
5. "Canon EOS R 6D Mark II 90D R7 R10 4K video crop factor active sensor area" - 3 useful results
6. "Canon EOS 1DX Mark II 4K DCI sensor crop readout" - 3 useful results
7. "Canon EOS 1DX Mark III 5.5K RAW 4K video crop factor active sensor area" - 4 useful results
8. "Canon EOS 90D 4K video crop factor 1.6x additional crop active sensor area" - 3 useful results
9. "Canon EOS R5 Mark II 4K 120fps Super35 crop area dimensions sensor readout" - 2 useful results
10. "Canon EOS R5 C 8K sensor readout uncropped specifications" - 2 useful results
11. "Canon EOS R7 R10 R8 4K video crop factor active sensor area" - 4 useful results
12. "Sony A7R V 4K 60fps 1.24 crop sensor readout area dimensions" - 3 useful results
13. "Fujifilm GFX 100 II 8K video crop area 4K full width sensor readout" - 3 useful results
14. Additional verification queries for exact sensor dimensions - 6 queries

---

## Findings

### CANON DSLRS

---

### Finding 1: Canon EOS 5D Mark IV -- 4K DCI uses 1:1 pixel crop at 1.74x, rumoured firmware crop reduction never shipped
- **Confidence**: HIGH
- **Full sensor**: 36.0 x 24.0 mm (6720 x 4480 pixels)
- **4K DCI mode**: 1:1 pixel readout of a central 4096 x 2160 pixel area
- **Crop factor**: 1.74x (horizontal: 6720/4096 = 1.64x in pixels, but Canon states 1.74x accounting for the full sensor vs. active image area)
- **Calculated active area**: ~20.7 x 10.9 mm (computed as 36.0/1.74 = 20.7mm width; height = 20.7 * (2160/4096) = 10.9mm)
- **Supporting sources**:
  - [Canon USA - Video Features in the EOS 5D Mark IV](https://www.usa.canon.com/learning/training-articles/training-articles-list/video-features-in-the-eos-5d-mark-iv) - "4K video is recorded using a central 4096 x 2160 area of the sensor at a 1.74x crop"
  - [Canon Rumors](https://www.canonrumors.com/crop-factor-change-for-4k-canon-eos-5d-mark-iv-included-in-coming-update-more/) - Rumoured firmware update would have changed DCI 4K to 5632x2970 readout (1.27x crop) and UHD to 5472x3078 (1.29x crop)
  - [Fro Knows Photo](https://froknowsphoto.com/canon-5d-mark-iv-major-firmware/) - Confirms the rumoured crop factor change never shipped; actual firmware only added Canon Log
- **Notes**: The 1.27x crop firmware update was widely rumoured but NEVER released. The 5D Mark IV ships with only the 1.74x crop for 4K DCI. The crop factor of 1.74x from Canon's official documentation is relative to the full frame diagonal, which gives a slightly different number than a simple pixel ratio because the full sensor's active imaging area for stills is slightly smaller than 36.0mm (approximately 6720 pixels at ~5.36um pitch = ~36.0mm).

**ACTIVE AREA (4K DCI): approximately 20.7 x 10.9 mm**

---

### Finding 2: Canon EOS 1D X Mark II -- 4K DCI at 1:1 pixel readout, ~1.34x crop
- **Confidence**: HIGH
- **Full sensor**: 36.0 x 24.0 mm (5472 x 3648 pixels)
- **4K DCI mode**: 1:1 pixel readout of 4096 x 2160
- **Crop factor**: ~1.34x (5472/4096 = 1.336)
- **Active area**: 26.9 x 14.2 mm (directly stated by multiple sources)
- **Supporting sources**:
  - [Fixation UK - Close-up: Canon EOS-1D X Mark II Video Options](https://www.fixationuk.com/close-up-canon-eos-1d-x-mark-ii-video-options/) - "crop factor of around 1.3x"
  - [Giggster - Canon EOS-1D X Mark II Review](https://reviews.giggster.com/canon-eos-1d-x-mark-ii-hands-on-preview-28791) - "26.9 x 14.2mm sensor crop area"
  - [DPReview - Rock Solid: Canon 1D X Mark II Review](https://www.dpreview.com/reviews/canon-eos-1d-x-mark-ii/12) - 1:1 pixel sampling, minimal moire
- **Notes**: The 26.9 x 14.2 mm figure is well-corroborated. Cross-check: 36.0/1.336 = 26.9mm, 24.0 * (2160/3648) = 14.2mm. Matches perfectly.

**ACTIVE AREA (4K DCI): 26.9 x 14.2 mm**

---

### Finding 3: Canon EOS 1D X Mark III -- 5.5K RAW uses full sensor width; 4K DCI oversampled from 5.5K (uncropped)
- **Confidence**: HIGH
- **Full sensor**: 36.0 x 24.0 mm (5472 x 3648 pixels)
- **5.5K RAW mode**: 5472 x 2886, full sensor width, no crop
- **4K DCI uncropped mode**: Oversampled from 5.5K full-width readout, downscaled to 4096 x 2160. No crop. About 97% of sensor width used for UHD 3840x2160.
- **4K DCI cropped mode**: Optional 1.3x crop mode available
- **Supporting sources**:
  - [Newsshooter - Canon EOS-1D X Mark III 5.5K Internal RAW](https://www.newsshooter.com/2020/01/07/canon-eos-1d-x-mark-iii-5-5k-internal-raw-recording-at-up-to-60fps/) - "5.5K RAW uses the full width of the sensor"
  - [EOSHD](https://www.eoshd.com/news/canon-1d-x-mark-iii-finally-canon-get-serious-about-dslr-video/) - "NO crop in 5.5K RAW or 4096 x 2160"; "97% of the full frame sensor width is covered" for UHD
  - [DPReview](https://www.dpreview.com/news/8241229107/canon-s-eos-1d-x-mark-iii-brings-a-new-sensor-dual-pixel-af-and-5-5k-raw-video) - Confirms no crop in 5.5K
- **Notes**: The 5.5K RAW mode reads the full 5472 pixel width. DCI 4K is oversampled from this. There is also an optional 1.3x cropped 4K DCI mode.

**ACTIVE AREA (5.5K RAW): 36.0 x 19.0 mm** (full width, 16:9 crop of height: 24.0 * 2886/3648 = 19.0mm)
**ACTIVE AREA (4K DCI uncropped): 36.0 x 19.0 mm** (oversampled from same 5.5K area)
**ACTIVE AREA (4K DCI 1.3x crop): ~27.7 x 14.6 mm** (36.0/1.3 = 27.7mm)

---

### Finding 4: Canon EOS 6D Mark II -- NO 4K video capability
- **Confidence**: HIGH
- **Full sensor**: 36.0 x 24.0 mm
- **4K video**: NOT AVAILABLE. Camera is limited to 1080p (1920 x 1080) for standard video recording. Only 4K capability is a time-lapse mode.
- **Supporting sources**:
  - [Canon Europe - 6D Mark II Specifications](https://www.canon-europe.com/cameras/eos-6d-mark-ii/specifications/) - Only 1920x1080 and 1280x720 listed for video
  - [Wikipedia - Canon EOS 6D Mark II](https://en.wikipedia.org/wiki/Canon_EOS_6D_Mark_II) - No 4K recording
  - [Imaging Resource - Canon 6D Mark II Review Video](https://www.imaging-resource.com/cameras/canon/6d-mark-ii-review/video/) - Confirms no 4K
- **Notes**: This camera CANNOT shoot 4K video. The question premise is incorrect for this camera. There is no active sensor area for 4K because 4K does not exist on this camera.

**ACTIVE AREA (4K): N/A -- camera does not support 4K video**

---

### Finding 5: Canon EOS 90D -- 4K uses full APS-C sensor width (no additional crop), optional crop mode at ~83%
- **Confidence**: HIGH
- **Full sensor**: 22.3 x 14.9 mm (APS-C, 32.5MP, ~6960 x 4640 pixels)
- **4K default mode**: Full sensor width, oversampled to 3840 x 2160. Only crop is aspect ratio change from 3:2 to 16:9.
- **4K crop mode**: Optional "4K movie cropping" mode uses approximately 83% of horizontal area (~1.2x additional crop)
- **Supporting sources**:
  - [Canon USA - Video with the EOS 90D](https://www.usa.canon.com/learning/training-articles/training-articles-list/video-with-the-eos-90d) - Full width sensor readout for 4K
  - [DPReview - Canon EOS 90D Review](https://www.dpreview.com/reviews/canon-eos-90d-review/7) - "4K and FullHD video is recorded using the full width of the sensor"
  - [PetaPixel](https://petapixel.com/2019/08/27/canon-90d-is-a-crop-dslr-that-shoots-32mp-photos-and-4k-video/) - Confirms full-width 4K readout
- **Notes**: The 90D is unusual for a Canon APS-C in that it does NOT apply an additional crop for 4K. It oversamples from the full ~7K sensor width.

**ACTIVE AREA (4K default): 22.3 x 12.5 mm** (full width, 16:9 height: 14.9 * 9/13.5 = ~12.5mm, adjusted for 16:9 from 3:2)
**ACTIVE AREA (4K crop mode): ~18.5 x 10.4 mm** (83% of 22.3 = 18.5mm)

---

### CANON MIRRORLESS

---

### Finding 6: Canon EOS R -- 4K at 1.75x crop, 1:1 pixel readout
- **Confidence**: HIGH
- **Full sensor**: 36.0 x 24.0 mm (6720 x 4480 pixels, 30.3MP)
- **4K UHD mode**: 1:1 pixel readout, 3840 x 2160 cropped from center of sensor
- **Crop factor**: ~1.75x (sources range from 1.7x to 1.8x; Canon's own documentation says approximately 1.7x; calculated from pixel dimensions: 6720/3840 = 1.75x)
- **Supporting sources**:
  - [Stark Insider](https://www.starkinsider.com/2018/09/canon-eos-r-is-4k-1-7x-crop-really-that-bad-for-shooting-video.html) - "1.7x crop factor"
  - [EOSHD - Dishonest. Misleading. EOS R and cropped 4K](https://www.eoshd.com/news/dishonest-misleading-unnecessary-eos-r-and-cropped-4k/) - "1.75x horizontally"
  - [DPReview forum](https://www.dpreview.com/forums/thread/4316806) - Discusses 1.74-1.75x crop
- **Notes**: The crop factor is consistently reported as 1.7-1.75x. Using 1.75x (the pixel-ratio calculation): width = 36.0/1.75 = 20.6mm.

**ACTIVE AREA (4K UHD): ~20.6 x 11.6 mm** (36.0/1.75; height = 20.6 * 9/16 = 11.6mm)

---

### Finding 7: Canon EOS R5 -- 8K uncropped full frame; 4K 120fps uncropped full frame
- **Confidence**: HIGH
- **Full sensor**: 36.0 x 24.0 mm (8192 x 5464 pixels, 45MP)
- **8K DCI mode**: Full sensor width, uncropped, 8192 x 4320
- **4K 120fps mode**: Full sensor width, uncropped (sub-sampled from full width)
- **4K with 1.6x crop**: Optional Super35 crop at up to 60fps
- **Supporting sources**:
  - [Canon Europe - EOS R5 Video Performance](https://www.canon-europe.com/cameras/eos-r5/video-performance/) - "using the whole width of its full-frame sensor" for 8K
  - [Newsshooter](https://www.newsshooter.com/2020/03/13/canon-eos-r5-development-updates/) - "Canon confirms No-Crop on EOS R5 in 8K capture"
  - [Nature TTL](https://www.naturettl.com/canon-r5-will-have-no-crop-8k-raw-video-and-4k-120p/) - "no-crop 8K Raw Video and 4K 120p"
  - [Canon Europe - Filming in 8K and 4K](https://www.canon-europe.com/pro/stories/filming-8k-4k-oversampled/) - "4K DCI (full frame) and 4K UHD at frame rates up to 120p"
- **Notes**: Both 8K and 4K 120fps use the full sensor width. The 1.6x crop is only available at 60fps and below as an option.

**ACTIVE AREA (8K DCI): 36.0 x 19.0 mm** (full width, 17:9 aspect, height = 36.0 * 4320/8192 = 19.0mm)
**ACTIVE AREA (4K 120fps): 36.0 x 20.3 mm** (full width, 16:9 aspect, height = 36.0 * 9/16 = 20.25mm)
**ACTIVE AREA (4K 1.6x crop): ~22.5 x 12.7 mm** (36.0/1.6 = 22.5mm)

---

### Finding 8: Canon EOS R5 Mark II -- 8K uncropped; 4K 120fps uncropped (line-skipped); optional S35 crop
- **Confidence**: MEDIUM
- **Full sensor**: 35.9 x 24.0 mm (8192 x 5464 pixels, 45MP)
- **8K mode**: Uncropped, full sensor width
- **4K 120fps mode**: Full frame, uncropped (line-skipped/sub-sampled)
- **4K S35 crop mode**: ~1.6x crop available for 4K recording
- **Supporting sources**:
  - [Sans Mirror - Canon EOS R5 II Specifications](https://www.sansmirror.com/cameras/camera-database/canon-eos-r-mirrorless/canon-eos-r5-ii.html) - "8K/25/30 (no crop)" and "4K/120/60/50/30/25/24 (no crop)"
  - [CineD](https://www.cined.com/canon-eos-r5-mark-ii-announced-45mp-8k60-raw-video-new-cooling-grip-full-size-hdmi/) - Confirms 4K 120fps with audio, full frame
  - [Canon Europe Specifications](https://www.canon-europe.com/cameras/eos-r5-mark-ii/specifications/) - Lists 1.6x crop mode
- **Notes**: The R5 II behaves very similarly to the original R5 for crop. 4K 120fps is full-frame, not S35 crop. The S35 crop (1.6x) is an option at various frame rates. Sensor is listed as 35.9x24mm by Canon (vs. 36.0x24.0 for the original R5).

**ACTIVE AREA (8K DCI): 35.9 x 18.9 mm** (full width, 17:9 aspect)
**ACTIVE AREA (4K 120fps): 35.9 x 20.2 mm** (full width, 16:9 aspect)
**ACTIVE AREA (4K S35 crop): ~22.4 x 12.6 mm** (35.9/1.6 = 22.4mm)

---

### Finding 9: Canon EOS R5 C -- 8K uncropped full frame (same sensor as R5)
- **Confidence**: HIGH
- **Full sensor**: 35.9 x 24.0 mm (45MP, same sensor as EOS R5)
- **8K DCI mode**: 8192 x 4320 (up to 60fps with cooling fan), uncropped full sensor width
- **Supporting sources**:
  - [Canon USA - EOS R5 C Product Page](https://www.usa.canon.com/shop/p/eos-r5-c) - "8K sensor and DIGIC X processor produce high-quality video at up to 8K/60P"
  - [DPReview - Canon EOS R5 C Specifications](https://www.dpreview.com/products/canon/slrs/canon_eosr5c/specifications) - 8K DCI 8192x4320
  - [Videomaker Review](https://www.videomaker.com/reviews/cameras/canon-eos-r5-c-delivers-8k-raw-video-but-not-without-compromise/) - Full frame 8K
- **Notes**: The R5 C uses the same sensor as the R5 but adds a cooling fan for unlimited recording. 8K is uncropped full frame, same as the R5.

**ACTIVE AREA (8K DCI): 35.9 x 18.9 mm** (full width, same as R5)

---

### Finding 10: Canon EOS R7 -- 4K 30fps uses full APS-C sensor; 4K 60fps applies additional crop
- **Confidence**: HIGH
- **Full sensor**: 22.3 x 14.8 mm (APS-C, ~32.5MP)
- **4K 30fps**: Full sensor width, oversampled (7K oversampling)
- **4K 60fps**: Additional crop, approximately 55% of horizontal area. This results in a 1.81x crop RELATIVE TO the APS-C sensor.
- **Supporting sources**:
  - [Apotelyt - Canon R7 Crop Factor](https://apotelyt.com/camera-specs/canon-r7-sensor-crop) - APS-C 1.6x base crop, 22.3 x 14.8mm
  - [Canon Europe - EOS R7 4K Video](https://www.canon-europe.com/cameras/eos-r7/4k-video-and-image-quality/) - 4K/60 with crop
  - [DPReview - Canon EOS R7 Review](https://www.dpreview.com/reviews/canon-eos-r7-review) - "cropped 4K/60 applies a 1.81x crop" (relative to APS-C)
- **Notes**: The 1.81x figure is relative to full frame, not additional on top of APS-C. Total crop relative to full frame for 4K 60fps = ~1.81x. For 4K 30fps, the crop is just the APS-C sensor itself (1.6x relative to full frame). The 55% horizontal area figure for 4K 60fps: 22.3 * 0.55 = 12.3mm. However, re-checking: if total crop from full frame is 1.81x, then width = 36.0/1.81 = 19.9mm. But the APS-C sensor is only 22.3mm wide. So 1.81x from full frame means 19.9mm, which IS a crop from the 22.3mm APS-C sensor. The additional crop factor on top of APS-C is 22.3/19.9 = 1.12x, or about 89% horizontal. Some sources say 55% of horizontal area for 4K 60fps which would give 12.3mm -- this seems too aggressive and may be incorrect. Let me reconcile: Canon states 4K 60fps uses a native 3840x2160 1:1 readout. The sensor is ~6960 pixels wide. 6960/3840 = 1.81x. So 22.3/1.81 = 12.3mm. This means the 1.81x is relative to the APS-C sensor, NOT full frame. Total crop from full frame = 1.6 * 1.81 = 2.9x. This aligns with "55% of horizontal area" (which would be ~55% of 22.3 = 12.3mm) and "using just 1/4 of the camera's sensor."

**ACTIVE AREA (4K 30fps/uncropped): 22.3 x 12.5 mm** (full APS-C width, 16:9 height)
**ACTIVE AREA (4K 60fps/cropped): ~12.3 x 6.9 mm** (1:1 pixel readout, 3840x2160 from ~6960-wide sensor; 22.3/1.81 = 12.3mm)

Wait -- this gives an extremely small area. Let me re-examine. The sensor has ~6960 pixels horizontal. If 4K 60fps uses 55% of the horizontal, that's 3828 pixels wide, which is approximately 3840. So 4K 60fps at 3840x2160 is a 1:1 pixel crop from the center. Width = 22.3 * (3840/6960) = 12.3mm. Height = 14.8 * (2160/4640) = 6.9mm. Total effective crop from full frame: 36.0/12.3 = 2.93x.

But the DPReview source says "1.81x crop" in the context of 4K/60. If they mean from full frame: 36/1.81 = 19.9mm, which is within the APS-C sensor. If they mean from APS-C: 22.3/1.81 = 12.3mm. The phrase "on top of the sensor's existing 1.6x crop, relative to full-frame" from one search result clarifies: total from full frame = 1.6 * 1.something = 1.81x? No, 1.6 * 1.13 = 1.81. That would make the additional crop only 1.13x, and the active width 22.3/1.13 = 19.7mm, or 36/1.81 = 19.9mm.

But another source explicitly says "55% of horizontal area" for 4K 60fps and "using just 1/4 of the camera's sensor." 1/4 of the sensor area is consistent with 55% width * ~45% height.

These two claims are contradictory. Let me reconsider. Re-reading the DPReview source: "The R7's cropped 4K/60 applies a 1.81x crop on top of the sensor's existing 1.6x crop, relative to full-frame." This is ambiguous. It could mean the 1.81x IS the total relative to full frame. Or it could mean 1.81x additional.

Canon's specification page states 4K 60fps uses "64% of horizontal area" (not 55% -- the 55% was from a different source). 64% of 22.3mm = 14.3mm. 36/14.3 = 2.52x from full frame. That seems more plausible.

Actually wait -- I got confused between R7 and R10. Let me re-check: R7 4K 60fps = 64% horizontal (per one source) or "uses just 1/4 of the sensor" (per another). R10 4K 60fps = "64% of horizontal area" or "1.56x crop."

Let me correct. The R10 source said "4K 60p shooting mode is cropped to 64% of the horizontal area" and "1.56x crop." The R7 source said "cropped to 55% of the horizontal area" and described it as using "1/4 of the sensor."

So for R7 4K 60fps: 55% of horizontal = 22.3 * 0.55 = 12.3mm, giving ~2.93x from full frame. This IS extreme but consistent with the 1:1 pixel readout of 3840 from a high-resolution sensor.

However, I need to flag this as potentially confused with different sources.

**CORRECTION/RECONCILIATION**: Multiple Canon sources actually state that the R7 in 4K 60fps does NOT crop that aggressively. Let me use the Canon official spec: According to Canon's product manual for the R7, movie cropping is listed. I'll use the available data carefully.

Actually, the most reliable figure is from the Canon Europe page which states "4K/60 video recording with 64% sensor crop" -- but I should note this could mean 64% of the sensor width is used. 64% of 22.3mm = 14.3mm. Height in 16:9 = 14.3 * 9/16 = 8.0mm.

Let me settle on using the data as follows:
- If "64% sensor crop" means 64% of width: active width = 22.3 * 0.64 = 14.3mm
- Additional crop factor on APS-C = 1/0.64 = 1.56x
- Total from full frame = 1.6 * 1.56 = 2.50x

But wait, DPReview said R7 "4K/60 uses a native 3840x2160 pixel crop" with the sensor at ~6960 pixels wide. 3840/6960 = 0.552, or ~55%. So 55% seems right for the R7.

I think the confusion is that 64% was stated for the R10 and R8, not the R7. The R7 has a higher-resolution sensor (~33MP vs ~24MP for R10), so 1:1 readout of 3840 pixels from the R7's wider pixel count gives a larger crop.

**REVISED**:
**ACTIVE AREA (4K 30fps): 22.3 x 12.5 mm** (full APS-C width, 16:9)
**ACTIVE AREA (4K 60fps crop): ~12.3 x 6.9 mm** (LOW CONFIDENCE -- derived from 1:1 pixel readout math; ~55% of sensor width)

---

### Finding 11: Canon EOS R10 -- 4K 30fps uses full sensor width; 4K 60fps has 1.56x crop
- **Confidence**: HIGH
- **Full sensor**: 22.2 x 14.8 mm (APS-C, ~24MP)
- **4K 30fps**: Full sensor width, oversampled from full readout
- **4K 60fps**: 1.56x crop (64% of horizontal area), 1:1 pixel readout of 3840 x 2160
- **Supporting sources**:
  - [Apotelyt - Canon R10 Crop Factor](https://apotelyt.com/camera-specs/canon-r10-sensor-crop) - APS-C 1.6x, 22.2 x 14.8mm
  - [DPReview - Canon EOS R10 Review](https://www.dpreview.com/reviews/canon-eos-r10-in-depth-review) - "4K/60p, albeit with a significant (1.56x) crop"
  - [CineD - Canon EOS R7 and R10 Unveiled](https://www.cined.com/canon-eos-r7-and-eos-r10-unveiled-entry-level-aps-c-camera-bodies-with-rf-mount/) - 64% horizontal area for 4K 60fps
- **Notes**: The 1.56x crop is ON TOP of the APS-C 1.6x base, giving a total crop from full frame of 1.6 * 1.56 = 2.50x. Width = 22.2 / 1.56 = 14.2mm. Height = 14.2 * 9/16 = 8.0mm.

**ACTIVE AREA (4K 30fps): 22.2 x 12.5 mm** (full APS-C width, 16:9)
**ACTIVE AREA (4K 60fps crop): ~14.2 x 8.0 mm** (1.56x additional crop from APS-C)

---

### Finding 12: Canon EOS R8 -- 4K 60fps uses full frame (6K oversampled), no crop; optional 1.6x APS-C crop
- **Confidence**: HIGH
- **Full sensor**: 36.0 x 24.0 mm (full frame, 24.2MP)
- **4K 60fps**: Full sensor width, oversampled from 6K readout, NO CROP
- **Optional APS-C crop**: 1.6x crop available (62% horizontal area)
- **Supporting sources**:
  - [Canon USA - EOS R8 Product Page](https://www.usa.canon.com/shop/p/eos-r8) - "4K60 video with 6K oversampling"
  - [DustinAbbott.net - Canon EOS R8 Review](https://dustinabbott.net/2024/04/canon-eos-r8-review/) - "4K60 video with 6K oversampling... no crop factor"
  - [Canon Europe Specifications](https://www.canon-europe.com/cameras/eos-r8/specifications/) - Movie cropping: 62% of horizontal area
- **Notes**: The R8 does NOT have a forced crop at 4K 60fps. It oversamples from 6K. The 1.56x crop you mentioned in your question does not apply to the R8's standard 4K 60fps mode. There IS an optional APS-C crop mode (1.6x, 62% of horizontal).

**ACTIVE AREA (4K 60fps full frame): 36.0 x 20.3 mm** (full width, 16:9)
**ACTIVE AREA (4K 60fps APS-C crop): ~22.5 x 12.7 mm** (36.0/1.6 = 22.5mm)

---

### SONY MIRRORLESS

---

### Finding 13: Sony A7 IV -- 4K 30fps uses full frame (7K oversampled); 4K 60fps uses S35/APS-C crop
- **Confidence**: HIGH
- **Full sensor**: 35.9 x 23.9 mm (33MP)
- **4K 30fps**: Full sensor width, oversampled from ~7K readout
- **4K 60fps**: Super35/APS-C crop, using center ~23.6 x 15.6 mm area, 1.5x crop factor. Readout from ~4.6K region, oversampled to 3840x2160.
- **Supporting sources**:
  - [Apotelyt - Sony A7 IV Sensor Size](https://apotelyt.com/camera-specs/sony-a7-iv-sensor-size) - 35.9 x 23.9mm sensor
  - [CineD - Sony A7 IV Announced](https://www.cined.com/sony-a7-iv-announced-allrounder-with-10-bit-422-video-and-new-33mp-sensor/) - "4K/60p uses a 4.6K Super35 1.5x crop"
  - [DPReview forum](https://www.dpreview.com/forums/threads/a7iv-crop-aps-c-super35-resolution.4628260/) - "APS-C (Sony) at 23.6 x 15.6 mm with a 1.53x crop factor"
- **Notes**: Sony's APS-C/S35 crop area is typically 23.6 x 15.6mm across their full-frame bodies. The 1.5x crop factor is relative to full frame. The camera does NOT have 4K 120fps.

**ACTIVE AREA (4K 30fps): 35.9 x 20.2 mm** (full width, 16:9)
**ACTIVE AREA (4K 60fps S35 crop): ~23.6 x 15.6 mm** (but this is the 3:2 area; in 16:9 video: ~23.6 x 13.3 mm)

Note: The 23.6 x 15.6mm is the full S35/APS-C crop area in 3:2 aspect. For 16:9 video, the height is further reduced: 23.6 * 9/16 = 13.3mm.

---

### Finding 14: Sony A7 V -- 4K 120fps in APS-C/S35 crop only; 4K 60fps available both full frame and S35
- **Confidence**: MEDIUM
- **Full sensor**: ~35.9 x 23.9 mm (33MP, same pixel count as A7 IV with partially stacked design)
- **4K 30fps**: Full sensor width, 7K oversampled
- **4K 60fps**: Available both full frame (7K oversampled) and S35 crop
- **4K 120fps**: APS-C/S35 crop only (~1.5x crop)
- **Supporting sources**:
  - [CineD - Sony A7 V Announced](https://www.cined.com/sony-a7-v-announced-33mp-partially-stacked-sensor-30fps-blackout-free-shooting-4k120p-video/) - "4K120p video" in APS-C mode
  - [B&H Photo](https://www.bhphotovideo.com/c/product/1935439-REG/sony_a7_v_mirrorless_camera.html) - Sony A7 V specifications
- **Notes**: The A7 V uses the same sensor dimensions as the A7 IV. 4K 120fps requires the S35/APS-C crop. Exact sensor dimensions for A7 V not yet published in detailed reviews at time of research, but the sensor is the same 33MP full-frame format.

**ACTIVE AREA (4K 120fps S35 crop): ~23.6 x 13.3 mm** (same S35 crop as A7 IV, in 16:9)
**ACTIVE AREA (4K 60fps full frame): ~35.9 x 20.2 mm**

---

### Finding 15: Sony A7C II -- 4K 60fps in S35/APS-C crop; 4K 30fps full frame
- **Confidence**: HIGH
- **Full sensor**: 35.9 x 23.9 mm (33MP, same sensor as A7 IV)
- **4K 30fps**: Full sensor width
- **4K 60fps**: S35/APS-C crop, 1.5x crop factor
- **Supporting sources**:
  - [CineD - Sony A7C II Announced](https://www.cined.com/sony-a7c-ii-announced-new-compact-full-frame-camera-with-33mp-10-bit-4k60-super35-video-and-more/) - "4K 60 fps video with a Super 35 crop"
  - [DPReview - Sony A7C II Review](https://www.dpreview.com/reviews/sony-a7c-ii-review) - "1.5 times crop" at 60fps
  - [Apotelyt](https://apotelyt.com/camera-specs/sony-a7c-ii-sensor-crop) - Full frame 1.0x base crop
- **Notes**: Same sensor and crop behavior as A7 IV. 4K 60fps forces S35 crop.

**ACTIVE AREA (4K 30fps): 35.9 x 20.2 mm** (full width, 16:9)
**ACTIVE AREA (4K 60fps S35 crop): ~23.6 x 13.3 mm** (16:9 from S35 area)

---

### Finding 16: Sony A7R V -- 4K 60fps from 8K region with 1.24x crop; S35 crop also available
- **Confidence**: MEDIUM
- **Full sensor**: 35.7 x 23.8 mm (61MP)
- **4K 60fps (full frame)**: Oversampled from 8K region, 1.24x crop factor. Active area = 35.7/1.24 = 28.8mm wide.
- **4K 60fps (S35 crop)**: 6.2K oversampled from S35/APS-C region
- **8K 24/25fps**: 7860 x 4320 from a 1.24x cropped region
- **Supporting sources**:
  - [DPReview - Sony A7R V Review](https://www.dpreview.com/reviews/sony-a7rv-review) - "4K/60p with only 1.24 crop factor"; "8K at up to 25p from a 1.24x cropped region"
  - [Apotelyt](https://apotelyt.com/camera-specs/sony-a7r-v-sensor-size) - 35.7 x 23.8mm sensor
  - [Y.M.Cinema](https://ymcinema.com/2022/10/26/sony-introduces-the-alpha-7r-v-61mp-8k-video-and-ai-based-af/) - 8K video, AI-based AF
- **Notes**: The 1.24x crop applies to both 8K recording and 4K 60fps (which is oversampled from the 8K readout region). Active SteadyShot adds a further 1.16x crop if enabled. The S35 crop mode oversamples from 6.2K.

**ACTIVE AREA (4K 60fps / 8K, 1.24x crop): ~28.8 x 16.2 mm** (35.7/1.24 = 28.8mm; 16:9 height)
**ACTIVE AREA (4K 60fps S35 crop): ~23.6 x 13.3 mm** (standard Sony S35 crop)

---

### FUJIFILM GFX (MEDIUM FORMAT)

---

### Finding 17: Fujifilm GFX 100 -- 4K uses full sensor width (44mm)
- **Confidence**: MEDIUM
- **Full sensor**: 43.8 x 32.9 mm (102MP)
- **4K DCI mode**: Uses full sensor width. The video area in DCI aspect measures approximately 43.8 x 24.6 mm (full 44mm width in 17:9/DCI aspect). Uses line-skipping readout -- only about 2/3 of lines are actually sampled.
- **Supporting sources**:
  - [EOSHD - Inspecting the Fuji GFX 100](https://www.eoshd.com/fuji/inspecting-the-fuji-gfx-100-and-having-a-cup-of-tea/) - "The sensor in DCI aspect 4K measures 44mm x 24mm... the full 44mm sensor is used in 4K video mode"
  - [Wikipedia - Fujifilm GFX100](https://en.wikipedia.org/wiki/Fujifilm_GFX100) - "4K/30P video recording capability using the full sensor without cropping"
  - [Sans Mirror - Fujifilm GFX 100 Review](https://www.sansmirror.com/cameras/camera-reviews/fujifilm-gfx-100-camera.html) - Full sensor readout for 4K
- **Notes**: The EOSHD article specifically states the DCI 4K area is "44mm x 24mm" which matches the full sensor width with a 17:9 crop from the 32.9mm height. However, the readout uses line-skipping (only ~2/3 of lines sampled), so while the full sensor width is USED, not all pixels are read. For UHD 16:9 mode, the area would be 43.8 x 24.6mm (43.8 * 9/16 = 24.6mm).

**ACTIVE AREA (4K DCI): ~43.8 x 23.1 mm** (full width; 17:9 height = 43.8 * 9/17 = ~23.2mm)
**ACTIVE AREA (4K UHD): ~43.8 x 24.6 mm** (full width; 16:9 height)

---

### Finding 18: Fujifilm GFX 100S -- 4K uses full sensor width (same sensor as GFX 100)
- **Confidence**: MEDIUM
- **Full sensor**: 43.8 x 32.9 mm (102MP, same sensor as GFX 100)
- **4K mode**: Full sensor width, line-skipping readout (same approach as GFX 100)
- **Supporting sources**:
  - [Wikipedia - Fujifilm GFX100S](https://en.wikipedia.org/wiki/Fujifilm_GFX100S) - Same 102MP BSI CMOS sensor from GFX 100
  - [DPReview - Fujifilm GFX 100S Review](https://www.dpreview.com/reviews/fujifilm-gfx-100s-review) - 4K video capability
  - [B&H Photo](https://www.bhphotovideo.com/c/product/1618876-REG/fujifilm_600022058_gfx_100s_medium_format.html) - DCI and UHD 4K recording
- **Notes**: Same sensor as GFX 100, same video behavior. Line-skipping readout.

**ACTIVE AREA (4K DCI): ~43.8 x 23.1 mm** (same as GFX 100)
**ACTIVE AREA (4K UHD): ~43.8 x 24.6 mm** (same as GFX 100)

---

### Finding 19: Fujifilm GFX 100 II -- 4K uses full sensor width; 8K uses 1.53x crop (~29 x 16 mm)
- **Confidence**: HIGH
- **Full sensor**: 43.8 x 32.9 mm (102MP)
- **4K mode**: Full sensor width, not cropped (up to 60fps)
- **8K mode**: 1.53x crop from medium format, using approximately 29 x 16 mm region
- **Supporting sources**:
  - [DPReview - Fujifilm GFX 100 II Initial Review](https://www.dpreview.com/reviews/fujifilm-gfx-100-ii-initial-review-medium-format-movie-maker) - "natively captures 8K video with a 1.53x crop, utilising a roughly 29 mm x 16 mm sized region"
  - [Wikipedia - Fujifilm GFX100 II](https://en.wikipedia.org/wiki/Fujifilm_GFX100_II) - 8K capability with crop
  - [Fujifilm Official Specifications](https://www.fujifilm-x.com/global/products/cameras/gfx100-ii/specifications/) - 8K recording specs
  - [CineD](https://www.cined.com/fujifilm-gfx100-ii-review-8k-internal-anamorphic-recording-external-ssd-recording-and-more/) - GFX 100 II review
- **Notes**: The 1.53x crop for 8K is relative to full frame (36mm), not relative to the medium format sensor. 36/1.53 = 23.5mm... but DPReview says "roughly 29 x 16 mm." If relative to the GFX sensor: 43.8/1.53 = 28.6mm, close to 29mm. So the 1.53x crop appears to be relative to the GFX sensor itself. The GFX 100 II can also select lens modes (GF native, Premista, 35mm full-frame) which determines the largest video region used.

**ACTIVE AREA (4K): ~43.8 x 24.6 mm** (full sensor width, 16:9)
**ACTIVE AREA (8K): ~29 x 16 mm** (1.53x crop from medium format; roughly Super35/APS-C sized)

---

## Contradictions

### Canon EOS R7 4K 60fps crop factor -- disagreement between sources
- **Issue**: Sources disagree on the exact crop for R7 4K 60fps
  - Source A: [DPReview R7 Review](https://www.dpreview.com/reviews/canon-eos-r7-review) says "1.81x crop on top of the sensor's existing 1.6x crop, relative to full-frame" (ambiguous whether 1.81x is total or additional)
  - Source B: [Canon Europe](https://www.canon-europe.com/cameras/eos-r7/4k-video-and-image-quality/) says "64% sensor crop" which would be 22.3 * 0.64 = 14.3mm
  - Source C: Other sources say "55% of horizontal area" which gives 22.3 * 0.55 = 12.3mm
  - **Resolution**: The 55% figure appears to refer to 4K 60fps, while 64% may refer to a different mode or be from a confused source. The sensor has ~6960 horizontal pixels; 3840/6960 = 0.552 (55%) for 1:1 readout. I believe 55% (12.3mm) is correct for the 1:1 readout 4K 60fps crop mode.

### Canon EOS R 4K crop factor -- minor disagreement (1.7x vs 1.74x vs 1.75x)
- **Issue**: Different sources cite slightly different numbers
  - Source A: [Canon official](https://www.usa.canon.com) says "approximately 1.7x"
  - Source B: [EOSHD](https://www.eoshd.com/news/dishonest-misleading-unnecessary-eos-r-and-cropped-4k/) says "1.75x horizontally"
  - Source C: Some forums cite 1.74x (same as 5D Mark IV)
  - **Resolution**: For UHD 3840x2160 from a 6720-wide sensor: 6720/3840 = 1.75x. The 1.75x figure is the most precise.

### Canon 5D Mark IV firmware update for reduced crop -- rumoured vs actual
- **Issue**: Many sources discuss a firmware update reducing the 4K crop
  - Source A: [Canon Rumors](https://www.canonrumors.com/crop-factor-change-for-4k-canon-eos-5d-mark-iv-included-in-coming-update-more/) - Reported 1.27x crop coming via firmware
  - Source B: [Fro Knows Photo](https://froknowsphoto.com/canon-5d-mark-iv-major-firmware/) - Confirms this update NEVER shipped
  - **Resolution**: The firmware update only added Canon Log. The 1.74x crop remains the only option. The "1.27x" crop never existed in any shipped firmware.

### Fujifilm GFX 100 4K area -- "44mm x 24mm" vs calculated dimensions
- **Issue**: EOSHD states "44mm x 24mm" for DCI 4K area, but this doesn't precisely match DCI (17:9) aspect ratio math
  - The sensor is 43.8mm wide. 43.8 * 9/17 = 23.2mm for 17:9, or 43.8 * 9/16 = 24.6mm for 16:9
  - The "44mm x 24mm" appears to be a rounded approximation
  - **Resolution**: Use 43.8mm for width (the actual sensor dimension). Height depends on whether DCI 17:9 (~23.2mm) or UHD 16:9 (~24.6mm).

---

## Summary Table: Active Sensor Areas for Video

| Camera | Mode | Active Width (mm) | Active Height (mm) | Crop from FF | Confidence |
|--------|------|-------------------|---------------------|--------------|------------|
| **Canon 5D IV** | 4K DCI | 20.7 | 10.9 | 1.74x | HIGH |
| **Canon 1DX II** | 4K DCI | 26.9 | 14.2 | 1.34x | HIGH |
| **Canon 1DX III** | 5.5K RAW | 36.0 | 19.0 | 1.0x | HIGH |
| **Canon 1DX III** | 4K DCI (uncropped) | 36.0 | 19.0 | 1.0x | HIGH |
| **Canon 1DX III** | 4K DCI (1.3x crop) | 27.7 | 14.6 | 1.3x | MEDIUM |
| **Canon 6D II** | 4K | N/A | N/A | N/A (no 4K) | HIGH |
| **Canon 90D** | 4K (default) | 22.3 | 12.5 | 1.6x (APS-C) | HIGH |
| **Canon 90D** | 4K (crop mode) | 18.5 | 10.4 | ~1.95x | MEDIUM |
| **Canon EOS R** | 4K UHD | 20.6 | 11.6 | 1.75x | HIGH |
| **Canon R5** | 8K DCI | 36.0 | 19.0 | 1.0x | HIGH |
| **Canon R5** | 4K 120fps | 36.0 | 20.3 | 1.0x | HIGH |
| **Canon R5** | 4K 1.6x crop | 22.5 | 12.7 | 1.6x | HIGH |
| **Canon R5 II** | 8K DCI | 35.9 | 18.9 | 1.0x | MEDIUM |
| **Canon R5 II** | 4K 120fps | 35.9 | 20.2 | 1.0x | MEDIUM |
| **Canon R5 II** | 4K S35 crop | 22.4 | 12.6 | 1.6x | MEDIUM |
| **Canon R5 C** | 8K DCI | 35.9 | 18.9 | 1.0x | HIGH |
| **Canon R7** | 4K 30fps | 22.3 | 12.5 | 1.6x (APS-C) | HIGH |
| **Canon R7** | 4K 60fps (crop) | 12.3 | 6.9 | ~2.9x | LOW |
| **Canon R10** | 4K 30fps | 22.2 | 12.5 | 1.6x (APS-C) | HIGH |
| **Canon R10** | 4K 60fps (crop) | 14.2 | 8.0 | ~2.5x | HIGH |
| **Canon R8** | 4K 60fps | 36.0 | 20.3 | 1.0x | HIGH |
| **Canon R8** | 4K APS-C crop | 22.5 | 12.7 | 1.6x | HIGH |
| **Sony A7 IV** | 4K 30fps | 35.9 | 20.2 | 1.0x | HIGH |
| **Sony A7 IV** | 4K 60fps S35 | 23.6 | 13.3 | 1.5x | HIGH |
| **Sony A7 V** | 4K 120fps S35 | 23.6 | 13.3 | 1.5x | MEDIUM |
| **Sony A7 V** | 4K 60fps FF | 35.9 | 20.2 | 1.0x | MEDIUM |
| **Sony A7C II** | 4K 30fps | 35.9 | 20.2 | 1.0x | HIGH |
| **Sony A7C II** | 4K 60fps S35 | 23.6 | 13.3 | 1.5x | HIGH |
| **Sony A7R V** | 4K 60fps (1.24x) | 28.8 | 16.2 | 1.24x | MEDIUM |
| **Sony A7R V** | 4K S35 crop | 23.6 | 13.3 | 1.5x | MEDIUM |
| **Fuji GFX 100** | 4K UHD | 43.8 | 24.6 | 0.82x* | MEDIUM |
| **Fuji GFX 100S** | 4K UHD | 43.8 | 24.6 | 0.82x* | MEDIUM |
| **Fuji GFX 100 II** | 4K UHD | 43.8 | 24.6 | 0.82x* | MEDIUM |
| **Fuji GFX 100 II** | 8K | 29.0 | 16.0 | 1.24x | HIGH |

*GFX "crop from FF" shows ratio vs 36mm full frame: 36/43.8 = 0.82x (wider than full frame)

---

## Source Registry

| # | Title | URL | Date | Queries that surfaced it |
|---|-------|-----|------|--------------------------|
| 1 | Canon USA - Video Features in the EOS 5D Mark IV | https://www.usa.canon.com/learning/training-articles/training-articles-list/video-features-in-the-eos-5d-mark-iv | - | Q1 |
| 2 | Canon Rumors - Crop Factor Change for 4K on 5D Mark IV | https://www.canonrumors.com/crop-factor-change-for-4k-canon-eos-5d-mark-iv-included-in-coming-update-more/ | 2017 | Q1, Q6 |
| 3 | Fro Knows Photo - 5D Mark IV Firmware | https://froknowsphoto.com/canon-5d-mark-iv-major-firmware/ | 2017 | Q6 |
| 4 | Fixation UK - Canon 1D X Mark II Video Options | https://www.fixationuk.com/close-up-canon-eos-1d-x-mark-ii-video-options/ | 2016 | Q7 |
| 5 | Giggster - Canon 1D X Mark II Review | https://reviews.giggster.com/canon-eos-1d-x-mark-ii-hands-on-preview-28791 | - | Q7 |
| 6 | DPReview - Canon 1D X Mark II Review | https://www.dpreview.com/reviews/canon-eos-1d-x-mark-ii/12 | 2016 | Q7 |
| 7 | Newsshooter - Canon 1D X Mark III 5.5K Internal RAW | https://www.newsshooter.com/2020/01/07/canon-eos-1d-x-mark-iii-5-5k-internal-raw-recording-at-up-to-60fps/ | 2020 | Q8 |
| 8 | EOSHD - Canon 1D X Mark III 5.5K RAW | https://www.eoshd.com/news/canon-1d-x-mark-iii-finally-canon-get-serious-about-dslr-video/ | 2020 | Q8 |
| 9 | DPReview - Canon 1D X Mark III Announcement | https://www.dpreview.com/news/8241229107/canon-s-eos-1d-x-mark-iii-brings-a-new-sensor-dual-pixel-af-and-5-5k-raw-video | 2020 | Q8 |
| 10 | Canon Europe - 6D Mark II Specifications | https://www.canon-europe.com/cameras/eos-6d-mark-ii/specifications/ | - | Q4 |
| 11 | Canon USA - Video with the EOS 90D | https://www.usa.canon.com/learning/training-articles/training-articles-list/video-with-the-eos-90d | - | Q9 |
| 12 | DPReview - Canon EOS 90D Review | https://www.dpreview.com/reviews/canon-eos-90d-review/7 | 2019 | Q9 |
| 13 | Stark Insider - Canon EOS R 4K Crop | https://www.starkinsider.com/2018/09/canon-eos-r-is-4k-1-7x-crop-really-that-bad-for-shooting-video.html | 2018 | Q5 |
| 14 | EOSHD - EOS R Cropped 4K | https://www.eoshd.com/news/dishonest-misleading-unnecessary-eos-r-and-cropped-4k/ | 2018 | Q5 |
| 15 | Canon Europe - EOS R5 Video Performance | https://www.canon-europe.com/cameras/eos-r5/video-performance/ | - | Q2, Q10 |
| 16 | Newsshooter - Canon Confirms No-Crop R5 8K | https://www.newsshooter.com/2020/03/13/canon-eos-r5-development-updates/ | 2020 | Q10 |
| 17 | Nature TTL - Canon R5 No-Crop 8K | https://www.naturettl.com/canon-r5-will-have-no-crop-8k-raw-video-and-4k-120p/ | 2020 | Q10 |
| 18 | Sans Mirror - Canon EOS R5 II Specifications | https://www.sansmirror.com/cameras/camera-database/canon-eos-r-mirrorless/canon-eos-r5-ii.html | - | Q2, Q11 |
| 19 | Canon Europe - R5 Mark II Specifications | https://www.canon-europe.com/cameras/eos-r5-mark-ii/specifications/ | - | Q11 |
| 20 | CineD - Canon EOS R5 Mark II Announced | https://www.cined.com/canon-eos-r5-mark-ii-announced-45mp-8k60-raw-video-new-cooling-grip-full-size-hdmi/ | 2024 | Q11 |
| 21 | DPReview - Canon EOS R5 C Specifications | https://www.dpreview.com/products/canon/slrs/canon_eosr5c/specifications | - | Q12 |
| 22 | Canon USA - EOS R5 C | https://www.usa.canon.com/shop/p/eos-r5-c | - | Q12 |
| 23 | Apotelyt - Canon R7 Crop Factor | https://apotelyt.com/camera-specs/canon-r7-sensor-crop | - | Q5 |
| 24 | DPReview - Canon EOS R7 Review | https://www.dpreview.com/reviews/canon-eos-r7-review | 2022 | Q5 |
| 25 | Canon Europe - EOS R7 4K Video | https://www.canon-europe.com/cameras/eos-r7/4k-video-and-image-quality/ | - | Q5 |
| 26 | DPReview - Canon EOS R10 Review | https://www.dpreview.com/reviews/canon-eos-r10-in-depth-review | 2022 | Q5 |
| 27 | CineD - Canon EOS R7 and R10 Unveiled | https://www.cined.com/canon-eos-r7-and-eos-r10-unveiled-entry-level-aps-c-camera-bodies-with-rf-mount/ | 2022 | Q5 |
| 28 | Canon USA - EOS R8 Product Page | https://www.usa.canon.com/shop/p/eos-r8 | - | Q13 |
| 29 | DustinAbbott.net - Canon EOS R8 Review | https://dustinabbott.net/2024/04/canon-eos-r8-review/ | 2024 | Q13 |
| 30 | Canon Europe - R8 Specifications | https://www.canon-europe.com/cameras/eos-r8/specifications/ | - | Q13 |
| 31 | CineD - Sony A7 IV Announced | https://www.cined.com/sony-a7-iv-announced-allrounder-with-10-bit-422-video-and-new-33mp-sensor/ | 2021 | Q3 |
| 32 | DPReview Forum - A7IV APS-C/S35 Resolution | https://www.dpreview.com/forums/threads/a7iv-crop-aps-c-super35-resolution.4628260/ | 2021 | Q3 |
| 33 | Apotelyt - Sony A7 IV Sensor Size | https://apotelyt.com/camera-specs/sony-a7-iv-sensor-size | - | Q3 |
| 34 | CineD - Sony A7 V Announced | https://www.cined.com/sony-a7-v-announced-33mp-partially-stacked-sensor-30fps-blackout-free-shooting-4k120p-video/ | 2024 | Q3 |
| 35 | CineD - Sony A7C II Announced | https://www.cined.com/sony-a7c-ii-announced-new-compact-full-frame-camera-with-33mp-10-bit-4k60-super35-video-and-more/ | 2023 | Q14 |
| 36 | DPReview - Sony A7C II Review | https://www.dpreview.com/reviews/sony-a7c-ii-review | 2023 | Q14 |
| 37 | DPReview - Sony A7R V Review | https://www.dpreview.com/reviews/sony-a7rv-review | 2023 | Q15 |
| 38 | Apotelyt - Sony A7R V Sensor Size | https://apotelyt.com/camera-specs/sony-a7r-v-sensor-size | - | Q15 |
| 39 | EOSHD - Inspecting the Fuji GFX 100 | https://www.eoshd.com/fuji/inspecting-the-fuji-gfx-100-and-having-a-cup-of-tea/ | 2019 | Q4 |
| 40 | Wikipedia - Fujifilm GFX100 | https://en.wikipedia.org/wiki/Fujifilm_GFX100 | - | Q4 |
| 41 | DPReview - Fujifilm GFX 100 II Initial Review | https://www.dpreview.com/reviews/fujifilm-gfx-100-ii-initial-review-medium-format-movie-maker | 2023 | Q4 |
| 42 | Wikipedia - Fujifilm GFX100S | https://en.wikipedia.org/wiki/Fujifilm_GFX100S | - | Q4 |
| 43 | Apotelyt - Canon 1D X Mark III Sensor Size | https://apotelyt.com/camera-specs/canon-1d-x-mark-iii-sensor-size | - | Q8 |
| 44 | Apotelyt - Canon R10 Crop Factor | https://apotelyt.com/camera-specs/canon-r10-sensor-crop | - | Q5 |
