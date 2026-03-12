# Milestone 11.3: Camera Suggestions

**Date**: 2026-03-10
**Status**: Draft
**Blocked by**: 11.1 (Natural language shot description), 11.2 (Automatic blocking)
**Parent**: Phase 11 — AI Features
**Companion to**: `fram3d-roadmap.md`

---

This is the most speculative milestone in the roadmap. Coverage planning is deeply contextual -- an experienced DP's instinct for where to put the camera comes from thousands of hours of watching and shooting. We are not claiming to replicate that instinct. We are claiming that standard coverage patterns exist, that they can be enumerated for common scene types, and that generating them as a starting point is faster than placing cameras from scratch.

The value proposition splits cleanly by audience. A student filmmaker who has never heard of a "cowboy shot" gets a starting point that follows established conventions. An experienced director gets a rough layout in seconds that they'll immediately tear apart and rebuild -- but starting from six placed shots is faster than starting from zero. In both cases, every generated shot must be editable using the same tools as any manually created shot. Generated shots are not special. They are ordinary shots that happened to be placed by the system instead of the user.

This milestone depends on two capabilities from earlier milestones: the ability to position a camera from a natural language description (11.1) and the ability to place characters from a scene description (11.2). Coverage suggestions require knowing where the actors are. Shot list generation requires both blocking the scene and positioning cameras. If either upstream milestone underperforms, this milestone's quality degrades proportionally.

**Honest uncertainty**: Coverage patterns for two-person dialogue are well-established and should work reasonably well. Coverage for complex multi-character scenes, action sequences, and montage is far less formulaic. The system may produce usable results for standard dialogue and mediocre results for everything else. That is an acceptable first version -- dialogue scenes are the majority of what previs tools are used for.

---

## 11.3.1 Coverage Suggestions

***Given a blocked scene with characters in known positions, suggest a standard set of camera angles that provide adequate coverage -- master, over-the-shoulders, close-ups -- and generate each as a separate shot in the shot track.***

*Requires: characters placed in scene (from 11.2 or manual placement), shot creation (3.1), camera positioning (11.1)*

### Scene Analysis

- The system examines the current scene to determine: how many characters are present, where each character is positioned, which characters face each other (approximate facing direction), and what elements exist between or around them.
- Character identification uses whatever character system exists from milestone 6.1. Characters must be distinguishable from props and set pieces.

``` python
.if >> scene contains exactly zero characters
    <== coverage suggestion is unavailable
    <== system communicates that characters are needed ("Place characters in the scene to get coverage suggestions")
    !== system generates shots of empty space
```

``` python
.if >> scene contains characters but no spatial relationship is discernible (all characters overlapping at the same point)
    <== system communicates that characters need to be spread out
    !== system generates shots with cameras clipping through stacked characters
```

### Coverage Patterns

The system recognizes scene configurations and applies appropriate coverage patterns. Each pattern is a named set of camera setups.

**Two-person dialogue** (the baseline pattern -- this must work well):

``` python
.if >> scene contains exactly two characters facing each other (within approximately 90 degrees of opposed facing directions)
    <== system suggests the following coverage:
        Master wide: both characters visible in frame, camera perpendicular to the line between them, medium-long shot or wider
        Over-the-shoulder A: camera behind Character A's shoulder, Character B in frame, medium or medium close-up
        Over-the-shoulder B: camera behind Character B's shoulder, Character A in frame, medium or medium close-up
        Close-up A: Character A fills the frame, shot size between medium close-up and close-up
        Close-up B: Character B fills the frame, shot size between medium close-up and close-up
    <== each suggestion is generated as a separate shot in the shot track
    <== shots are named descriptively: "Master Wide", "OTS on [Character B]", "CU [Character A]", etc.
    <== shot order in the shot track follows a conventional shooting order: master first, then OTS pair, then CU pair
    <== camera height defaults to approximately eye level for OTS and CU shots
    <== the master shot uses a wider focal length than the close-ups
```

``` python
.if >> two characters are side by side rather than facing each other
    <== coverage pattern adapts: master from the front, singles rather than OTS (no shoulder to shoot over)
    !== system generates OTS shots that would show the back of both characters' heads
```

**Single character**:

``` python
.if >> scene contains exactly one character
    <== system suggests: wide establishing shot (character in environment), medium shot, close-up
    !== system suggests over-the-shoulder shots (no second character to anchor them)
    <== suggested shot count is 3 rather than 5
```

**Group scene (3-5 characters)**:

``` python
.if >> scene contains 3 to 5 characters
    <== system suggests: master wide (all characters visible), plus individual coverage as appropriate
    <== for characters facing each other in pairs, OTS coverage is suggested for each pair
    <== for characters not clearly paired, singles (medium and close-up) are suggested
    <== total suggested shot count scales with character count but is capped at a reasonable limit
    !== system generates 20+ shots for a five-person scene
```

**Crowd scene (6+ characters)**:

``` python
.if >> scene contains 6 or more characters
    <== system suggests: master wide, group shots of subgroups, and close-ups of prominent characters only
    <== system acknowledges that individual coverage of every character is impractical
    <== total suggested shot count stays manageable (8-12 maximum)
```

### Camera Placement

- Every generated camera position must satisfy these constraints:
  - Camera does not clip through any scene element (wall, furniture, character)
  - Camera is not inside any character's bounding volume
  - The target character(s) are actually visible from the camera position (not occluded by walls or large elements)
  - Camera height is physically plausible (not underground, not floating at ceiling height -- unless the scene geometry requires it)

``` python
.if >> an ideal camera position would clip through a wall or element
    <== camera position adjusts to the nearest valid position that maintains approximately the same framing
    <== if no valid position exists that preserves the intended framing, that shot is omitted from the suggestion set with a note explaining why
```

``` python
.if >> the scene contains walls or enclosed spaces (detected from scene geometry)
    <== camera positions respect room boundaries where possible
    <== system prefers positions that could physically exist on a real set (though "cheating the wall" -- removing a wall for the camera -- is a valid production technique, the default should be realistic placement)
```

### Focal Length Selection

- Generated shots use cinematically appropriate focal lengths:
  - Master/wide shots: 24-35mm range
  - Medium shots and OTS: 35-50mm range
  - Close-ups: 50-85mm range
- Focal length selection respects the currently active camera body and lens set. If the selected lens set does not include a suggested focal length, the nearest available focal length is used.

``` python
.if >> scene is set in a small space (characters close together, tight environment)
    <== focal lengths skew wider to fit the coverage within the space
```

``` python
.if >> scene is set in a large open space
    <== focal lengths may use longer lenses, especially for close-ups
```

### Coverage Style

Three-way style toggle before generating: conservative (fewer shots, safe angles), minimal (bare minimum coverage), comprehensive (maximum coverage options). The user picks a style before generation.

``` python
.if >> user selects "conservative" style
    <== system generates the fewest shots that still provide basic coverage (e.g., master + one OTS + one CU for two-person dialogue)
    <== angles favor conventional, safe setups -- no unusual heights or extreme focal lengths
    !== system generates the full comprehensive set and hides some shots
```

``` python
.if >> user selects "minimal" style
    <== system generates the bare minimum coverage: typically master + one single per character
    <== shot count is the lowest of the three modes
    <== useful for rough blocking passes or animatics where full coverage is premature
```

``` python
.if >> user selects "comprehensive" style
    <== system generates maximum coverage options: standard set plus alternates (e.g., both tight and loose OTS, multiple CU sizes, high and low angle options)
    <== shot count is the highest of the three modes
```

``` python
.if >> user does not select a style (no toggle interaction)
    <== system defaults to conservative
```

### Camera Movement

Generated shots include camera movement (slow dolly, push-in, etc.) -- not all static locked-off angles.

``` python
.if >> system generates an OTS or single coverage shot
    <== shot may include subtle camera movement: slow push-in, gentle dolly, or creep
    <== movement is expressed as keyframes in the shot (start and end positions)
    <== movement speed defaults to slow/subtle -- the user can adjust or remove it
```

``` python
.if >> system generates a master/wide shot
    <== master shot defaults to static (locked-off) unless the scene description implies movement
    <== if the scene involves character movement (entrance, cross), the master may include a slow pan or track to follow
```

``` python
.if >> user prefers static shots only
    <== user can disable movement inclusion in the coverage settings
    <== all generated shots revert to single keyframe at t=0
```

### Shot Count and Pair Filtering

User-configurable maximum shots per generation request (default 3). Users can request coverage for a specific character pair only (e.g., "suggest coverage for characters A and B").

``` python
.if >> user sets maximum shots to N before generating
    <== system generates at most N shots
    <== if the coverage pattern would normally produce more than N shots, the system prioritizes: master first, then OTS, then close-ups
    !== system generates all shots and silently discards extras
```

``` python
.if >> user requests coverage for a specific character pair (e.g., "A and B") in a multi-character scene
    <== system generates coverage only for the specified pair, ignoring other characters
    <== the master shot frames the specified pair, not the full group
    <== other characters remain in the scene but are not the subject of generated shots
```

``` python
.if >> user requests coverage for a pair that does not exist in the scene (names do not match any character)
    <== system communicates that the specified characters were not found
    !== system generates coverage for random characters instead
```

### Shot Generation

``` python
.if >> user triggers coverage suggestion
    <== system generates shots and appends them to the shot track after the current last shot
    <== existing shots are not modified or removed
    !== existing shot sequence is cleared or overwritten
    <== generated shots include camera movement keyframes when movement is enabled (see Camera Movement above)
    <== generated shot duration defaults to 5.0 seconds (matching the system default)
    <== element states in generated shots reflect the current scene blocking
```

``` python
.if >> user triggers coverage suggestion on a scene that already has coverage shots from a previous suggestion
    <== new shots are appended; old ones remain
    <== user is responsible for deleting unwanted shots
    !== previous suggestion is automatically replaced
```

### Editing Generated Shots

- Generated shots are indistinguishable from manually created shots in every way

``` python
.if >> user modifies a generated shot (moves camera, changes focal length, renames, adjusts duration)
    <== modification works identically to modifying any other shot
    !== system prevents editing because the shot was generated
    !== system marks the shot as "AI-generated" permanently (it may be labeled at creation time, but the label has no behavioral effect)
```

``` python
.if >> user deletes a generated shot
    <== shot is deleted normally
    !== system warns differently than for any other shot deletion
```

---

## 11.3.2 Shot List Generation

***Given a scene description or script excerpt in plain text, generate a complete sequence of shots -- blocked characters, positioned cameras, ordered in the shot track as a rough cut. The user types a paragraph and gets a previs scene.***

*Requires: scene-from-text (11.2.1), shot-from-text (11.1.1), coverage suggestions (11.3.1), shot track (3.1)*

This is the most ambitious feature in the product. It chains together blocking, camera placement, and coverage into a single operation. The quality of the output depends on every upstream system performing adequately. Expect this to be the feature most likely to produce disappointing results on first release, and most likely to improve dramatically as upstream capabilities mature.

### Input

- The user provides a text description of a scene. This can be:
  - A script excerpt in standard screenplay format ("INT. KITCHEN - DAY. Sarah sits at the table. Michael enters through the back door, holding a letter.")
  - A plain prose description ("Two people arguing in a kitchen, one sitting, one standing")
  - A brief scene slug with action lines

``` python
.if >> user provides text input for shot list generation
    <== system parses the input to identify: location/setting (if described), characters mentioned by name or role, physical actions and positions described, emotional tone or dramatic beats (if detectable)
    !== system requires rigidly formatted input -- it should handle natural language
```

``` python
.if >> input is ambiguous or underspecified ("two people talking")
    <== system makes reasonable defaults: generic environment, characters facing each other at conversational distance, neutral staging
    !== system refuses to generate or asks for clarification before producing any output
    <== generated output may include fewer shots than a fully specified scene
```

``` python
.if >> input describes a location but no characters ("INT. ABANDONED WAREHOUSE - NIGHT")
    <== system generates establishing shots of the environment (wide, detail shots)
    !== system generates character coverage for characters that don't exist
```

``` python
.if >> input is nonsensical or unparseable ("asdfghjkl")
    <== system communicates that it could not understand the input
    !== system generates random shots
    !== system crashes
```

### Character Matching and OTS Pairing

Shot list reuses existing characters by matching character names from the scene to names in the script/description. OTS (over-the-shoulder) pairing is determined by characters facing each other within a threshold distance in the scene.

``` python
.if >> input mentions a character name that matches a character already in the scene (case-insensitive name match)
    <== system reuses the existing character rather than creating a duplicate
    <== existing character's position is updated if the input describes a new position
    !== system creates a second "Sarah" when "Sarah" already exists in the scene
```

``` python
.if >> the AI system receives a prompt referencing a character by name (e.g., "close-up of Alice")
    <== the system matches the name against existing scene characters (case-insensitive)
    <== if "Alice" exists in the scene, the existing Alice character is used as the camera target
    !== the system creates a new character or ignores the existing one
```

``` python
.if >> input mentions a character name that does not match any existing character
    <== system creates a new character with that name
```

``` python
.if >> two characters are facing each other (within approximately 90 degrees of opposed facing directions) and are within conversational distance (roughly 1-3 meters apart)
    <== system identifies them as an OTS pair and generates over-the-shoulder coverage for them
    !== system generates OTS shots for characters facing the same direction or separated by a large distance
```

``` python
.if >> characters are facing each other but are far apart (e.g., across a large room)
    <== system generates singles or telephoto coverage rather than OTS
    <== OTS pairing requires proximity -- the shoulder must plausibly fill part of the frame
```

### Scene Construction

``` python
.if >> user triggers shot list generation with valid input
    <== system first blocks the scene: places characters and elements as described, using 11.2 capabilities
    ||> .if >> blocking is complete
        <== system then generates a shot sequence covering the blocked scene
        <== shot sequence follows dramatic logic: establish the space first, then move to coverage of the action
```

- The generated shot sequence follows cinematic conventions for the scene type:

**Dialogue scene**:
  - Establishing/master shot of the space
  - Master two-shot of the characters
  - Standard coverage (OTS, singles, CUs) for each character with dialogue
  - Tighter coverage (ECU, insert shots) for described emotional beats
  - Expected output: 5-10 shots depending on scene complexity

**Action scene**:
  - Wide establishing shot
  - Medium shots following the primary action
  - Close-ups on key moments
  - Variety of angles (high, low, lateral) to convey energy
  - Expected output: 6-12 shots depending on described action

**Montage or transitional scene**:
  - Series of shots at varied sizes and angles
  - Less conventional structure -- more visual variety
  - Expected output: 4-8 shots

``` python
.if >> input describes specific shots or camera directions ("close-up on her hands", "low angle as he enters")
    <== those specific shots are included in the generated sequence at the appropriate dramatic moment
    <== system-generated coverage fills in around the explicitly requested shots
```

### Shot Track Output

``` python
.if >> shot list generation completes successfully
    <== all generated shots appear in the shot track in dramatic sequence order
    <== shots are named descriptively based on their content: "Wide Establishing - Kitchen", "OTS on Sarah", "CU Michael - Reaction", etc.
    <== each shot has the camera positioned and framed for its described content
    <== character positions and poses reflect the described blocking for each shot's moment in the scene
    <== total generation creates a playable sequence -- the user can hit play and watch a rough cut
```

``` python
.if >> scene already contains existing shots before generation
    <== generated shots are appended after existing shots
    !== existing shots are modified or removed
```

``` python
.if >> scene already contains characters/elements before generation
    <== the user is warned that generation will add new characters and elements
    <== existing scene content is preserved (new content is additive)
    !== existing elements are silently moved or deleted
```

### Character Continuity Across Generated Shots

- Characters generated for the first shot must persist across all generated shots in the sequence

``` python
.if >> the input describes a character moving ("Michael stands up and walks to the window")
    <== earlier shots show Michael in his initial position
    <== later shots show Michael in his new position
    <== the transition shot (if generated) shows both positions or captures the movement
```

``` python
.if >> the input describes a character entering or exiting
    <== shots before the entrance do not include that character
    <== shots after the entrance include that character
    <== the entrance is marked in the sequence (establishing shot of the character arriving, or the character appearing in an existing setup)
```

### Progress and Feedback

- Shot list generation involves multiple steps (parsing, blocking, camera placement) and may take noticeable time

``` python
.if >> generation is in progress
    <== user sees a progress indicator showing what step is being performed
    <== user can cancel generation at any point
    !== UI freezes or becomes unresponsive during generation
```

``` python
.if >> user cancels mid-generation
    <== shots generated so far are kept in the shot track
    <== partially placed characters remain in the scene
    <== system returns to an interactive state
    !== cancellation rolls back all generated content (user might want to keep what was generated)
```

``` python
.if >> generation fails partway through (upstream system produces an error or unusable result)
    <== successfully generated shots up to the failure point are preserved
    <== user is informed of the failure with a description of what went wrong
    !== entire generation is silently discarded
```

### Quality and Limitations

This section documents known limitations that should be communicated to users rather than hidden.

- **Dialogue is the sweet spot.** Two-person dialogue scenes with clear spatial descriptions will produce the best results. This is where coverage conventions are most standardized and where the upstream systems (blocking, camera placement) have the most to work with.

- **Action scenes are unpredictable.** Action coverage is highly director-specific. Generated action coverage will follow generic patterns (wide-medium-close) but will lack the visual storytelling decisions that make action sequences work. Expect to replace most generated action shots.

- **Interior spaces may produce invalid cameras.** The system may suggest camera positions that would be inside a wall or behind furniture. Collision avoidance (from 11.3.1) mitigates this but cannot guarantee perfect results in complex environments.

- **Tone is not well-captured.** The difference between a horror scene and a comedy scene in the same kitchen with the same characters is primarily expressed through camera angle, lens choice, and lighting -- not blocking. The system may detect tone from the text but the influence on generated shots will be subtle at best.

- **Long scripts produce diminishing returns.** A single scene description (1-3 paragraphs) will produce better results than feeding in multiple pages of script. The system is designed for scene-level generation, not sequence-level.

``` python
.if >> generated shots produce obviously wrong results (camera underground, characters floating, cameras pointing at empty space)
    <== these are bugs in upstream systems (blocking, camera placement) not in coverage logic
    <== the user's recourse is to edit or delete the offending shots manually
```

---

## Acceptance Criteria

### Coverage Suggestions -- Two-Person Dialogue

1. Scene contains two characters facing each other across a table. User triggers coverage suggestion.
``` python
   <== 5 shots generated: Master Wide, OTS on Character A, OTS on Character B, CU Character A, CU Character B
   <== all shots appear in the shot track after any existing shots
   <== each shot's camera is positioned at a plausible height and distance
   <== master shot uses a wider focal length than close-ups
   <== OTS shots show the back of one character's head/shoulder with the other character in frame
```

2. User selects the generated "CU Character A" shot and moves the camera.
``` python
   <== camera moves normally -- no restrictions from being a generated shot
```

3. User deletes the generated "Master Wide" shot.
``` python
   <== shot is deleted; remaining 4 generated shots are unaffected
```

### Coverage Suggestions -- Single Character

4. Scene contains one character. User triggers coverage suggestion.
``` python
   <== 3 shots generated: wide establishing, medium shot, close-up
   !== OTS shots generated
   !== system errors due to insufficient characters
```

### Coverage Suggestions -- Empty Scene

5. Scene contains no characters. User triggers coverage suggestion.
``` python
   <== no shots generated
   <== user is informed that characters are needed
   !== shots generated of empty space
```

### Coverage Suggestions -- Collision Avoidance

6. Two characters are positioned in a narrow hallway with walls on both sides.
``` python
   <== generated camera positions do not clip through the walls
   <== if a standard OTS position would be inside a wall, the camera adjusts to the nearest valid position or that shot is omitted
```

### Shot List Generation -- Dialogue Scene

7. User enters: "INT. KITCHEN - DAY. Sarah sits at the table reading a letter. Michael enters through the door and sits across from her."
``` python
   <== characters are placed in the scene: Sarah at a table, Michael near a door then at the table
   <== 5-10 shots are generated in the shot track
   <== first shot is a wide establishing shot of the kitchen
   <== sequence includes coverage of both characters
   <== shots are named descriptively
   <== user can press play and watch the sequence
```

### Shot List Generation -- Minimal Input

8. User enters: "Two people talking."
``` python
   <== system generates a scene with two characters facing each other
   <== standard dialogue coverage is generated
   !== system refuses the input or generates nothing
```

### Shot List Generation -- Cancellation

9. User triggers shot list generation and cancels after 3 shots have been generated.
``` python
   <== the 3 generated shots remain in the shot track
   <== any placed characters remain in the scene
   <== system returns to normal interactive state
```

### Shot List Generation -- Nonsense Input

10. User enters: "xyzzy plugh"
``` python
    <== system reports that it could not understand the input
    <== no shots or characters are generated
    !== system crashes
```

### Shot List Generation -- Character Continuity

11. User enters: "Sarah is sitting. She stands up and walks to the window."
``` python
    <== earlier shots show Sarah sitting
    <== later shots show Sarah standing at the window
    <== Sarah is the same character across all generated shots
```

### Edge Cases

12. User triggers coverage suggestion, then triggers it again on the same scene.
``` python
    <== second set of shots is appended after the first
    <== user now has duplicate coverage (their responsibility to clean up)
    !== first set is overwritten
```

13. Scene contains two characters but one is completely occluded behind a large element.
``` python
    <== system attempts coverage but may omit shots where the occluded character cannot be framed
    !== system generates shots pointing at the back of a bookshelf
```

14. User triggers shot list generation while existing shots are in the shot track.
``` python
    <== generated shots are appended after existing shots
    <== user is not asked "delete existing shots?" -- existing work is always preserved
```
