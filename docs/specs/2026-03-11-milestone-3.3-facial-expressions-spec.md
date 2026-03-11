# Milestone 3.3: Facial Expressions

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 3.3 — Facial expressions
**Project**: 3 — Production features
**Blocked by**: 3.2 (Characters — expressions apply to character meshes)

---

- ### 3.3. Facial expressions (Milestone)
	*Blend shape-based facial expressions on characters. Presets, intensity control, keyframeable. Adds emotional context to blocking.*

	- ##### 3.3.1. Expression system (Feature)
		*10 preset expressions driven by blend shape targets on the character mesh. Each preset is a combination of blend shape weights.*

		**Presets:**

		| Expression | Description |
		|-----------|-------------|
		| Neutral | Default relaxed face |
		| Happy | Smile, slightly raised cheeks |
		| Sad | Downturned mouth, furrowed brow |
		| Angry | Furrowed brow, clenched jaw, narrowed eyes |
		| Surprised | Raised eyebrows, open mouth, wide eyes |
		| Concerned | Slight frown, raised inner brows |
		| Scared | Wide eyes, open mouth, raised brows |
		| Disgusted | Wrinkled nose, raised upper lip |
		| Thinking | Slightly narrowed eyes, one brow raised |
		| Smirk | Asymmetric half-smile |

		**Blend shape targets (minimum set):**

		| Target | What it deforms |
		|--------|----------------|
		| Brow_Raise_L / Brow_Raise_R | Left/right eyebrow up |
		| Brow_Furrow | Both brows down and together |
		| Eye_Wide | Eyes open wider |
		| Eye_Squint | Eyes narrow |
		| Mouth_Smile | Corners of mouth up |
		| Mouth_Frown | Corners of mouth down |
		| Mouth_Open | Jaw drops, mouth opens |
		| Nose_Wrinkle | Nose scrunches |
		| Jaw_Clench | Jaw tightens |

		Each preset expression = combination of targets at specific weights. Example: "Angry" = Brow_Furrow(1.0) + Eye_Squint(0.5) + Jaw_Clench(0.7).

		**UI:**
		- **Preset dropdown** in inspector panel when character is selected
		- **Intensity slider**: 0% (neutral) to 100% (full expression). Default 100%.
		- **Advanced dropdown**: Individual blend shape sliders for manual mixing
		- **Quick access**: Right-click character → "Expression" → submenu of presets
		- **Custom presets**: User can create a custom blend and save it. Saved presets appear alongside built-in presets. Cross-project.

		**Imported characters:**
		- If model has blend shapes with standard naming (ARKit or common names like "Smile", "Frown"), auto-map to our expression system
		- If no blend shapes exist, expressions are greyed out in UI
		- Mannequin characters always have expressions (we author the blend shapes)

	- ##### 3.3.2. Eye direction (Feature)
		*Separate controls for eye look direction, independent from expression presets.*

		**Additional blend shape targets:**

		| Target | What it deforms |
		|--------|----------------|
		| Eye_Look_Left | Both eyes look left |
		| Eye_Look_Right | Both eyes look right |
		| Eye_Look_Up | Both eyes look up |
		| Eye_Look_Down | Both eyes look down |

		Eye direction is its own control in the inspector panel. A character can be "Angry" while looking left. Keyframeable independently from expressions.

	- ##### 3.3.3. Expression animation (Feature)
		*Expression track as a sub-track under the character's track in the timeline.*

		- Shows the active expression name at each keyframe
		- Blend shape weights interpolate smoothly between keyframes
		- Independent of position/pose keyframing (follows per-track stopwatch model from 1.5.3)
		- Per-keyframe segment option: "hold" vs "interpolate" (like easing modes on other tracks)

		Example:
		```
		Character "Alice"
		  ├── Position: [keyframes]
		  ├── Rotation: [keyframes]
		  ├── Pose: [keyframes]
		  ├── Expression: [Neutral @ 0s] ——— [Angry @ 3s] ——— [Sad @ 7s]
		  └── Eye Direction: [keyframes]
		```

	**What this is NOT:**
	- Not lip sync (no phoneme shapes, no audio-driven animation)
	- Not a full facial rig (no individual muscle control, no blink)
	- Not motion capture driven
	- It's preset expressions with intensity control and keyframing. Readable emotion at previs fidelity.
