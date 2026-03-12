# Milestone 11.2: Automatic Blocking — Specification

**Date**: 2026-03-10
**Milestone**: 11.2
**Status**: Draft (Speculative)

---

- ### 11.2. Automatic blocking (Milestone)

  Position characters and elements from a scene description. This is the spatial reasoning counterpart to shot description (11.1) — instead of telling the system where to put the camera, the director tells it where to put the characters.

  Blocking is a rehearsal concept: "She enters from the left, crosses to the window, turns to face him." A director communicates this verbally to actors, and the actors figure out the spatial details — how far apart to stand, which direction to face, where the furniture is relative to the architecture. The director refines from there. Automatic blocking replicates that workflow: describe a scene setup in natural language, get characters placed in roughly appropriate positions with roughly appropriate poses, then refine iteratively until it is close enough to be useful.

  This is genuinely hard. Shot description (11.1) translates cinematic vocabulary into camera parameters — a well-defined mapping between terms like "medium close-up" and concrete values like distance and focal length. Blocking requires spatial reasoning that has no such clean mapping. "Two people sitting across a table" requires knowing: how big is the table? How wide are the chairs? How far apart do people sit at a table? Which direction do they face? Where does the table go in the scene? If there is a window, where is it relative to the table? These are physical-world common-sense inferences, not vocabulary lookups. The system must invent plausible spatial arrangements from incomplete descriptions.

  **Honest assessment of uncertainty**: The quality of automatic blocking depends entirely on the spatial reasoning capabilities available at build time. The feature may produce results that range from "useful starting point that saves a minute of manual placement" to "roughly correct room but every character needs repositioning." Both outcomes are acceptable — the value is in speed, not accuracy. A bad starting point that takes 15 seconds to generate and 30 seconds to fix is still faster than 3 minutes of manual placement from scratch. The spec is written to be useful across that range of quality.

  *Blocked by: 6.1 (Characters — automatic blocking places and poses characters)*

  - ##### 11.2.1. Scene-from-text (Feature)

    ***Describe a scene setup in natural language — characters, furniture, spatial relationships — and get characters placed and posed in the 3D scene as a starting point for refinement.***

    Input is text-only for v1. No voice input.

    **Functional requirements:**
    - The user provides a natural language description of a scene setup via a text input field
    - The system interprets the description and places characters into the scene with positions, orientations, and poses
    - Placed characters are standard character elements from the character system (6.1) — they support posing, animation, and all existing scene interactions
    - Props and furniture referenced in the description are placed using elements from the built-in asset library (5.2.1) when a match exists
    - When the description references a prop or furniture item that has no match in the asset library, the system places a labeled placeholder element (a simple geometric shape with the element name displayed)
    - The system assigns names to placed characters (e.g., "Actor 1", "Actor 2", or names extracted from the description if provided)
    - All placed elements are fully editable after generation — the user can move, rotate, repose, or delete any of them using existing tools
    - Generation does not modify or remove elements already in the scene unless the user explicitly requests replacement
    - The user can invoke scene-from-text on an empty scene or on a scene with existing elements
    - When invoked on a scene with existing elements, new placements should respect existing element positions (not overlap or stack on top of existing content)
    - The result is a single atomic action in the undo stack (4.1) — one undo removes all placed elements from that generation
    - Descriptions that reference an environment (e.g., "two people in a kitchen") trigger best-effort environment generation using available assets from the asset library — walls, counters, furniture, etc. The result is not photorealistic; it uses whatever is available. If no kitchen counter exists in the library, a placeholder is used.
    - Descriptions that imply motion (e.g., "she walks to the table") produce keyframed animation: the character is placed with a walk animation keyframed between the starting position and the destination. This leverages the keyframe system (3.2) and the character animation system (6.1).

    **Spatial reasoning requirements:**
    - Characters described as "sitting" are placed in a seated pose at an appropriate height
    - Characters described as "standing" are placed upright on the ground plane
    - Relative spatial terms ("across from," "next to," "behind," "between") produce plausible arrangements
    - Facing direction is inferred from context: people in conversation face each other; a person "at the window" faces the window unless told otherwise
    - Element scale is physically plausible: tables are table-sized, chairs are chair-sized, distances between seated characters match real-world proportions
    - When the description omits spatial details, the system uses reasonable defaults rather than failing (e.g., if no room dimensions are given, characters are placed at conversational distances in open space)

    **Expected behavior:**
    ``` python
      # basic scene generation
      .if the scene is empty
      .if the user enters "Two people sitting across a table" >>
          <== two characters appear in the scene in seated poses
          <== a table element appears between them
          <== the characters face each other across the table
          <== the characters are at a plausible seated height relative to the table
          <== the spacing between characters is physically reasonable (not overlapping, not 20 meters apart)

      # scene with spatial references
      .if the user enters "A man standing by the window, a woman sitting in a chair facing him" >>
          <== two characters appear in the scene
          <== one character is in a standing pose near a window element (or the scene's existing window if one exists)
          <== one character is in a seated pose in a chair
          <== the seated character's orientation faces toward the standing character

      # named characters
      .if the user enters "Sarah sits at the desk. James stands in the doorway." >>
          <== two characters appear named "Sarah" and "James"
          <== Sarah is in a seated pose near a desk element
          <== James is in a standing pose near a door or doorway element

      # description with no furniture references
      .if the user enters "Three people standing in a circle talking" >>
          <== three characters appear in standing poses
          <== the characters are arranged in a roughly circular formation
          <== the characters face inward toward the center of the circle
          !== any furniture or props are placed (none were described)

      # preserving existing scene content
      .if the scene already contains a table and two chairs
      .if the user enters "A person standing behind the table" >>
          <== one character appears in a standing pose
          <== the character is positioned behind the existing table (not a new table)
          <== the existing table and chairs are not moved or removed

      # undoable as a single action
      .if the user generates a scene with five characters and three props
      .if the user presses undo >>
          <== all five characters and three props are removed
          <== the scene returns to its state before generation
          !== characters are removed one at a time requiring multiple undos

      # environment generation from description
      .if the scene is empty
      .if the user enters "Two people in a kitchen" >>
          <== two characters appear in the scene
          <== environment elements are placed using best-effort matches from the asset library (walls, counters, appliances, etc.)
          <== elements not found in the asset library are represented as labeled placeholders
          !== the system refuses to generate because no "kitchen" environment template exists
          !== the result is photorealistic — it is a rough spatial arrangement from available assets

      # motion description producing keyframed animation
      .if the scene is empty
      .if the user enters "She walks from the door to the table" >>
          <== one character appears with a walk animation
          <== the character is keyframed at the door position at the start and the table position at the end
          <== a door element and a table element appear (or existing ones are used if present)
          !== the character is placed in a static pose at a single position
    ```

    **Missing information handling:**
    ``` python
      # vague description — system uses defaults
      .if the user enters "Two people talking" >>
          <== two characters appear at conversational distance (roughly 1-2 meters apart)
          <== the characters face each other
          <== the characters are in neutral standing poses
          !== the system fails or asks for clarification before placing anything

      # unknown prop — placeholder created
      .if the user enters "A character sitting at a piano"
      .if no piano exists in the asset library >>
          <== a character appears in a seated pose
          <== a placeholder element labeled "piano" appears at an appropriate position
          <== the placeholder is roughly the right scale for a piano

      # contradictory description
      .if the user enters "He sits on the table while standing by the door" >>
          <== the system makes a best-effort interpretation
          <== at least one character is placed in the scene
          !== the system fails silently or produces no output
          !== the system blocks waiting for clarification

      # extremely complex scene
      .if the user enters a description with more than 10 characters and many spatial relationships >>
          <== the system places all described characters
          <== spatial accuracy may degrade for characters described later or with less spatial context
          <== the result is still a usable starting point even if some positions need manual adjustment
    ```

    **Error cases:**
    ``` python
      # empty or nonsensical input
      .if the user enters an empty string or text with no scene content (e.g., "hello") >>
          <== the system communicates that it could not interpret the input as a scene description
          !== any elements are placed in the scene
          !== the application produces an error

      # generation failure
      .if the system cannot process the description for any reason >>
          <== the user is informed that generation failed
          <== the scene is unchanged (no partial placements left behind)
          !== the scene is left in an inconsistent state
    ```

  - ##### 11.2.2. Blocking refinement (Feature)

    ***Iterative natural language adjustments to an existing scene — "move him closer to the table," "have her face the window" — without regenerating the entire layout.***

    Blocking refinement is the conversational half of the workflow. Scene-from-text gets the rough layout; refinement is how the director dials it in. This mirrors how blocking works on a real set: the director places actors, watches, then gives notes. "Cheat toward camera." "Give her more space." "You two are too close together." The tool must understand these incremental corrections and apply them to specific characters or elements already in the scene.

    The key constraint is that refinement must be surgical. Moving one character should not disturb the rest of the scene. The director has already accepted the positions of everyone else — they are adjusting one thing.

    **Functional requirements:**
    - The user provides a natural language instruction that modifies existing scene content
    - The system identifies which character(s) or element(s) the instruction refers to and applies the change
    - Refinement operates on the current scene state — it does not regenerate the scene from scratch
    - Only the referenced characters/elements are modified; all other scene content remains in place
    - The system maintains a conversational context within the current session so that pronouns and references resolve correctly ("move her" refers to the last-mentioned female character, "the table" refers to the table in the scene)
    - Conversational context persists across save/load. The AI remembers previous blocking instructions even after the project is saved and reopened. Context is serialized as part of the project file.
    - Each refinement is a single undoable action
    - Multiple refinements can be applied in sequence, each building on the previous result
    - Refinement works on scenes that were manually arranged, not only on scenes generated by scene-from-text
    - Refinement can add new elements to the scene: "add a chair next to her" creates a new chair (from the asset library or as a placeholder) and positions it relative to the referenced character
    - When the user's instruction contains an ambiguous reference to an element (e.g., "move her to the table" when two tables exist), the system prompts the user for clarification rather than guessing
    - The system does not communicate confidence or uncertainty about placements. It places elements and lets the user judge. The exception is true indeterminacy (like the two-table case above), where it must ask.

    **Types of refinement the system should understand:**
    - **Position adjustments**: "Move him closer to the table." "Put her by the window." "Spread them out more."
    - **Orientation changes**: "Have her face the camera." "Turn him toward the door." "They should face each other."
    - **Pose changes**: "She should be sitting." "Have him lean against the wall." "He raises his hand."
    - **Relative adjustments**: "Move her to his left." "Put the chair between them." "She should be further from the camera."
    - **Group operations**: "Move everyone closer together." "Spread the crowd out." "Have them form a line."
    - **Element addition**: "Add a chair next to her." "Put a lamp on the table." "Give him a briefcase."

    **Expected behavior:**
    ``` python
      # basic position refinement
      .if the scene contains Actor 1 near a table and Actor 2 by a window
      .if the user enters "Move Actor 2 closer to the table" >>
          <== Actor 2 moves toward the table
          <== Actor 1 remains in place
          <== Actor 2's pose is unchanged (only position changes)
          <== the table remains in place

      # orientation refinement
      .if the scene contains a character facing north
      .if the user enters "Have her face the window" >>
          <== the character rotates to face the window element in the scene
          <== the character's position does not change
          <== the character's pose does not change (only rotation)

      # pose refinement
      .if the scene contains a standing character
      .if the user enters "She should be sitting" >>
          <== the character's pose changes to a seated pose
          <== the character's height adjusts appropriately for a seated position
          <== the character's position and orientation are preserved as closely as possible

      # pronoun resolution from context
      .if the user previously referred to "Sarah" in a refinement
      .if the user enters "Move her a step to the left" >>
          <== Sarah moves approximately one step (roughly 0.5-0.7 meters) to the left
          !== the system asks "who do you mean by her?"

      # sequential refinements
      .if the user enters "Move him closer to the table"
      ||> .if the user enters "Actually, a bit more" >>
          <== the same character moves further in the same direction
          <== the second refinement builds on the result of the first

      # refinement on manually-placed scene
      .if the user has manually placed characters without using scene-from-text
      .if the user enters "Move the character on the left closer to the other one" >>
          <== the system identifies characters by their spatial positions in the scene
          <== the leftmost character moves toward the other character
          <== the result is the same as if the scene had been generated by scene-from-text

      # group operation
      .if the scene contains four characters spread across the space
      .if the user enters "Move everyone closer together" >>
          <== all four characters move inward toward the group's center
          <== relative positions are roughly preserved (the person on the left stays on the left)
          <== the spacing between characters decreases

      # camera-relative language
      .if the user enters "Cheat her toward camera" >>
          <== the referenced character rotates or moves slightly toward the current camera position
          <== the adjustment is subtle (a "cheat" is a small adjustment, not a full reposition)

      # undoable refinements
      .if the user applies three sequential refinements
      .if the user presses undo >>
          <== only the third refinement is reversed
          <== the first and second refinements remain applied
      ||> .if the user presses undo again >>
          <== the second refinement is reversed
          <== only the first refinement remains

      # adding a new element via refinement
      .if the scene contains a character named "Sarah"
      .if the user enters "Add a chair next to her" >>
          <== a chair element appears from the asset library (or as a placeholder if none exists)
          <== the chair is positioned adjacent to Sarah
          <== Sarah's position and pose are unchanged

      # context persists across save/load
      .if the user has been refining a scene with characters "Sarah" and "James"
      .if the user saves and closes the project
      ||> .if the user reopens the project
      .if the user enters "Move her closer to him" >>
          <== the system resolves "her" to Sarah and "him" to James from the saved context
          <== Sarah moves toward James
          !== the system asks who "her" refers to (context was preserved)
    ```

    **Ambiguity handling:**
    ``` python
      # ambiguous character reference
      .if the scene contains three standing male characters
      .if the user enters "Move him to the left" >>
          <== the system makes a best-effort guess (e.g., most recently placed, most recently referenced, or nearest to camera)
          !== the system moves all three characters
          !== the system silently does nothing

      # ambiguous element reference — system asks for clarification
      .if the scene contains two tables
      .if the user enters "Move her to the table" >>
          <== the system asks the user which table they mean (e.g., "There are two tables in the scene. Which one?")
          !== the system guesses which table
          !== the system moves the character to an arbitrary table
          !== the system silently does nothing

      # ambiguous direction
      .if the user enters "Move her over there" >>
          <== the system communicates that it needs more specific direction
          !== the system moves the character to an arbitrary position

      # conflicting with physics
      .if the user enters "Put him inside the table" >>
          <== the system places the character as close to the described position as possible without interpenetration
          !== the character clips through the table geometry
    ```

    **Edge cases:**
    ``` python
      # refinement referencing nonexistent elements
      .if the scene contains no window
      .if the user enters "Move her to the window" >>
          <== the system communicates that there is no window in the scene
          !== a window is created
          !== the character moves to a random position

      # adding new elements via refinement
      .if the scene contains a character named "Sarah"
      .if the user enters "Add a chair next to her" >>
          <== a chair is created (from the asset library or as a placeholder) and placed adjacent to Sarah
          <== this is a single undoable action

      # refinement with no characters in scene
      .if the scene is empty
      .if the user enters "Move the character to the left" >>
          <== the system communicates that there are no characters to move
          !== a new character is created

      # very small adjustment
      .if the user enters "Move him just a tiny bit to the right" >>
          <== the character moves a small amount (roughly 0.1-0.3 meters) to the right
          <== the movement is visually noticeable but subtle

      # conflicting sequential instructions
      .if the user enters "Move her to the left"
      ||> .if the user enters "Move her to the right" >>
          <== the character moves to the right from the current position (the left-moved position)
          <== the net result is approximately the original position
          !== the system tries to reconcile both instructions simultaneously
    ```

---

## Open questions

These are unresolved design decisions that affect scope and behavior. They should be answered before implementation begins.

1. ~~**Input modality**: Is blocking described via a text input field, a voice input, or both?~~ **Resolved**: Text input only for v1. Voice input may be added in a future milestone.

2. ~~**Scene-from-text scope**: Should scene-from-text also handle environment setup (walls, rooms, outdoor spaces), or only characters and props within a pre-existing environment?~~ **Resolved**: Yes, scene-from-text handles environment generation on a best-effort basis using available assets from the asset library. Not photorealistic — uses whatever is available.

3. ~~**Existing element recognition**: When the user says "move her to the table" and there are two tables in the scene, how does the system disambiguate?~~ **Resolved**: The system prompts the user for clarification when the reference is ambiguous.

4. ~~**Blocking and animation**: Scene-from-text produces static poses. Should it also support motion descriptions?~~ **Resolved**: Yes. Motion descriptions like "she walks to the table" produce keyframed animation using the keyframe system (3.2).

5. ~~**Confidence feedback**: Should the system communicate its confidence level to the user?~~ **Resolved**: No. The system does not communicate confidence or uncertainty. It places elements and the user judges. The only exception is true indeterminacy (e.g., ambiguous element references), where the system must ask for clarification rather than guessing.

6. **Generation speed expectation**: What is the acceptable latency for scene-from-text? Is 2 seconds acceptable? 10 seconds? 30 seconds? This affects whether the system needs a progress indicator and whether the user can continue working during generation.

7. ~~**Refinement context persistence**: Should conversational context survive across sessions, or only within the current session?~~ **Resolved**: Context persists across save/load. The conversational context is serialized as part of the project file.
