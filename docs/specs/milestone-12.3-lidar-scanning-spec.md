# Milestone 12.3: LiDAR Scanning

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 12.3 — LiDAR scanning
**Phase**: 12 — AI Generation
**Competitor reference**: Previs Pro (LiDAR scan import via iPad Pro)

---

- ### 12.3. LiDAR scanning (Milestone)
	*Scan real locations with iPhone/iPad LiDAR and import as scene backgrounds. Requires companion iOS app (eventually).*

	- ##### 12.3.1. LiDAR import (Feature)
		*Import point cloud or mesh data from LiDAR scans. Use as scene backdrop for accurate spatial reference. Placeable and rotatable. Multiple scans per project.*

		**Supported formats (v1 — third-party scanning apps):**
		- .ply (point cloud — Polycam, Scaniverse)
		- .obj (mesh — most scanning apps export this)
		- .usdz (Apple's format — native to iPhone scanning)

		**Behavior:**
		- Scan imported as a placeable, rotatable static mesh
		- Display-only — not editable, not selectable as an element (it's a backdrop)
		- Objects, characters, and cameras placed on top of the scan
		- Scale calibration: LiDAR scans are real-world scale (1 unit = 1 meter matches our convention)
		- Toggle scan visibility (sometimes useful to hide it)
		- Multiple scans per project supported (different rooms/locations)

	- ##### 12.3.2. Companion iOS app (Feature)
		*Minimal iOS app for capturing LiDAR scans. Scan capture only. Built after v1 third-party format support is proven.*

		**Scope:**
		- Uses ARKit's LiDAR capabilities (iPhone Pro, iPad Pro)
		- Captures room-scale 3D data
		- Exports as .usdz, .obj, or .ply
		- User transfers file to desktop (AirDrop, email, cloud storage)
		- Minimal app — not a previs tool, just a scanner

	**Why this is lower priority:**
	- Requires building and maintaining an iOS app (separate codebase, App Store review)
	- Only useful for productions with location access during pre-production
	- Many productions previs before locations are locked
	- The scan is just a backdrop — doesn't interact with scene system
	- v1 with third-party format support delivers most of the value without the iOS app
