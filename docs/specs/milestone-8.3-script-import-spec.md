# Milestone 8.3: Script Import

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 8.3 — Script import (HIGHER PRIORITY)
**Project**: Phase 8 — Workflow Polish
**Related**: 4.2.5 (Multi-scene project structure), 1.2.4 (Subtitle overlay)

---

- ### 8.3. Script import (Milestone)
	*Import screenplays to auto-populate scenes, characters, and dialogue. Bridges the gap between script and previs.*

	- ##### 8.3.1. Script parsing (Feature)
		*Import Final Draft (.fdx) and Fountain (.fountain) files. Parse scene headings, character names, and action lines. Auto-create scenes (one scene per heading, NOT one shot).*

		**Supported formats:**
		1. **Final Draft (.fdx)** — Industry standard. XML-based, well-documented. Contains scene headings, characters, action, dialogue, transitions.
		2. **Fountain (.fountain)** — Open plaintext screenplay format. Popular with indie filmmakers (Highland, WriterSolo). Simpler to parse.

		**What gets imported:**

		| Script Element | Fram3d Action |
		|---------------|---------------|
		| Scene heading (INT. OFFICE - DAY) | Create a **scene** (not a shot), named from the heading |
		| Character name (first appearance) | Create a named mannequin character (project-level) |
		| Action line | Store as text metadata on the scene (reference, not auto-blocking) |
		| Dialogue | Store in project-level dialogue reference library |
		| Transition (CUT TO:, DISSOLVE TO:) | Scene boundary marker |

		**What does NOT get imported (v1):**
		- Action lines do NOT auto-place elements or characters (that's Phase 11 — AI blocking)
		- Parentheticals do NOT affect character poses
		- No environment generation from scene headings (future: "INT. OFFICE" → suggest office environment)

		**Import workflow:**
		1. File → Import Script (or drag .fdx/.fountain onto the app)
		2. Import dialog shows: detected scenes, detected characters, total page count
		3. User picks which scenes to import (can deselect unwanted scenes)
		4. User picks an environment per scene (environment picker during import)
		5. User confirms → scenes created (one per heading), characters placed in default lineup at scene origin
		6. User begins blocking

		**Dialogue as reference library:**
		- Script dialogue stored as project-level metadata (list of lines per character per scene)
		- When user creates a subtitle (1.2.4), dropdown shows "Import from script..." with available lines
		- User picks a line, it populates the subtitle text
		- Timing is manual (user sets start/end on subtitle track)
		- No auto-generated subtitles with guessed timing

		**Character name matching:**
		- Character names in script auto-match existing scene characters if they share a name
		- Partial script import supported (just one scene or a subset)

		**Multi-scene interaction:**
		- Each scene heading creates a separate scene tab (per 4.2.5 multi-scene project structure)
		- Character definitions are project-level; character state (pose, position) is scene-level

		**Future enhancement:**
		- Connect to Phase 11 (AI blocking): after import, offer "Auto-block this scene?"
		- Scene heading → environment mapping: "INT. OFFICE" auto-suggests Office environment
