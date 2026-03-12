# Milestone 10.1: Set Builder

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 10.1 — Set builder
**Phase**: 10 — Set Builder
**Related**: 5.3 (Premade environments), 8.2 (2D Designer)

---

- ### 10.1. Set builder (Milestone)
	*Lightweight environment building tool. Separate page in the application. Room shapes, materials, furniture placement, lighting presets.*

	- ##### 10.1.1. Room construction (Feature)
		*Pick from basic shapes or draw custom walls. Room dimensions via inputs or dragging. Material presets. Room wizard. Auto-furnish.*

		**Workflow:**
		1. User selects "Build Environment" from environment panel or File menu
		2. Opens as a **separate page** in the application (not a modal or sub-panel — its own full-screen workspace)
		3. Pick room shape, set dimensions, choose materials, place furniture, set lighting
		4. Save as reusable environment template

		**Room shape presets:**

		| Shape | Description |
		|-------|-------------|
		| Rectangle | Simple four-wall room. Most common. |
		| L-shape | Two connected rectangles. Offices, apartments with hallways. |
		| T-shape | Three connected areas. Lobbies, junctions. |
		| Open (no walls) | Exterior or stage. Just a floor plane. |

		**Custom room shapes:** Via wall drawing (10.1.2) for arbitrary layouts.

		**Material presets (cosmetic only — no lighting calculations):**
		- **Walls:** White drywall, grey concrete, red brick, wood paneling, glass
		- **Floors:** Hardwood, tile (white, black, checkered), carpet (grey, blue), concrete
		- **Ceilings:** White drywall, exposed beam, acoustic tile, open (no ceiling — exterior)

		**Furniture placement:** Uses standard asset library and existing drag-and-drop placement tools. No special set-builder-specific UI — same tools as regular scene editing.

		**Lighting presets:**

		| Preset | What it places |
		|--------|---------------|
		| Daytime window | Directional light outside one wall, warm white |
		| Night window | Dim blue fill from one wall, no direct light |
		| Overhead fluorescent | Even cool-white area light from ceiling |
		| Single source | One spot or point light, dramatic shadows |
		| Exterior daylight | Sun directional + sky ambient |

		Presets are starting points — user can modify placed lights.

		**Room wizard:** Guided flow asking questions ("How many walls? Interior or exterior? What kind of space?") → generates a starting room layout.

		**Auto-furnish:** Exposes the procedural system used for premade environments. User specifies room type and dimensions → system populates with appropriate furniture.

		**Saving and reuse:**
		- Saved environments appear in environment library alongside premade environments
		- Include name (user-specified), thumbnail (auto-captured from 2D overhead), all objects/lights/walls/materials
		- When placed in a scene, all objects become independent and editable (same as premade environments)
		- Shareable — export as file for sharing between users

	- ##### 10.1.2. Wall drawing (Feature)
		*Draw walls in 2D overhead view by clicking to place segments. Straight and curved segments. Doors and windows as cutouts. Two-sided materials.*

		**Drawing flow:**
		1. Select "Draw Wall" tool from toolbar in 2D overhead view (or keyboard shortcut)
		2. Click to place first point of a wall segment
		3. Move mouse — preview line follows cursor showing wall thickness
		4. Click again to place end point — wall segment created
		5. Continue clicking to chain segments (corners auto-connected)
		6. Double-click or Enter/Escape to finish the chain
		7. Walls snap to right angles by default (hold Shift for arbitrary angles)
		8. Supports curved/arc segments in addition to straight lines

		**Wall properties (adjustable after creation):**

		| Property | Default | Range |
		|----------|---------|-------|
		| Height | 2.5m | 1.0–6.0m |
		| Thickness | 0.15m | 0.05–0.5m |
		| Material | White drywall | From material preset list |

		**Two-sided materials:** Walls support different materials on each side (interior vs exterior face).

		**Close room:** Button to auto-connect last wall endpoint to first, forming a closed loop.

		**Room presets:** Click to place a pre-sized rectangular room as starting point.

		**Editing:**
		- Click wall segment to select (2D or 3D view)
		- Drag to reposition (connected walls adjust)
		- Drag endpoints to resize
		- Select + Delete removes a segment
		- Corner behavior: shared endpoints form corners; moving one adjusts the connected wall. Hold Alt to disconnect.

		**Doors and windows (rectangular cutouts):**

		| Property | Door Default | Window Default |
		|----------|-------------|----------------|
		| Width | 0.9m | 1.2m |
		| Height | 2.1m | 1.0m |
		| Sill height | 0m (floor) | 1.0m |

		- Right-click wall → "Add Door" or "Add Window" → click position on wall
		- Draggable along wall to reposition, resizable via inspector, deletable
		- 2D view: doors as gap with arc (standard floor plan convention), windows as gap with thin line
		- 3D view: holes in wall geometry. No frame mesh, no glass — just openings. This is previs.

		**2D/3D synchronization:**
		- 2D: thick lines (wall outlines from above)
		- 3D: extruded rectangles with selected material
		- Walls are scene objects — appear in hierarchy panel under "Walls" group
		- Walls cast and receive shadows, block camera views (useful for sight lines)

		**Walls are NOT keyframeable.** They are static set pieces. (For a moving wall, import a model.)

		**Wall drawing is only available in 2D view.** Editing walls (move, resize) works in both 2D and 3D.

	**What this is NOT:**
	- Not a CAD tool or floor plan editor (no dimension annotations, no construction details)
	- Not an architecture tool (no structural elements, no roof, no plumbing)
	- Not a level editor (no collision meshes, no navigation)
	- It's a quick way to sketch a room layout for previs. Accuracy matters (real-world scale), precision does not.
