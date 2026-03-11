# Milestone 3.7: Set Decoration Library

**Date**: 2026-03-10
**Status**: Draft
**Milestone**: 3.7 — Set decoration library
**Project**: 3 — Production features

---

- ### 3.7. Set decoration library (Milestone)

	A director says "I need a table" and a table appears. That's the bar. Not "download an FBX from TurboSquid, drag it into the project folder, fix the scale, generate a collider." One click. Table. In the scene.

	This milestone delivers three layers of asset access. First, a built-in library that ships with Fram3d — common props that cover 80% of previs needs out of the box. Second, marketplace integration that lets directors browse and import assets from existing 3D marketplaces without leaving the app. Third, a personal asset management system so that once a director has assembled their kit — their cop car, their interview room furniture, their period-appropriate street dressing — they can find and reuse it instantly.

	The built-in library is table stakes. Marketplace integration is the ambitious play — each marketplace has different APIs, auth flows, licensing models, file formats, and quality levels. The product owner has specifically prioritized this: "We really need to figure out a way to connect to existing marketplaces in the app, if possible." This spec acknowledges the complexity honestly and defines a phased approach.

	*Blocked by: 2.3 (asset import — marketplace integration builds on the import pipeline established by model import and the basic asset library panel)*

	---

	- ##### 3.4.1. Built-in asset library (Feature)

		***A curated set of low-poly props that ship with Fram3d, organized by category and searchable by name — so a director can dress a scene without importing anything.***

		*Related:
		- 2.3.2 (asset library panel — built-in assets appear alongside imported assets)
		- 1.3.1 (scene elements — placed assets become interactive scene elements with colliders)
		- 1.3.2 (transform gizmos — placed assets can be moved, rotated, and scaled immediately)*

		**Functional requirements:**
		- Fram3d ships with a built-in library of 3D props ready for immediate use
		- Visual style: clearly 'previs' — stylized/abstract aesthetic, not photo-realistic. Assets should look like blocking tools, not finished set pieces.
		- Assets are low-poly, optimized for previs — visual clarity at a distance, not photorealism
		- Asset count for first release will be user-sourced — the developer will provide the models.
		- Every built-in asset has a pre-generated collider for selection and gizmo interaction
		- Every built-in asset has a thumbnail for browsing
		- Built-in assets are organized into categories:
			- Furniture: tables, chairs, sofas, beds, desks, bookshelves, cabinets
			- Architectural: doors, windows, walls, floors, stairs, columns, arches
			- Vehicles: sedan, SUV, pickup truck, police car, ambulance, motorcycle, bicycle
			- Exterior: trees (deciduous, conifer), bushes, streetlights, benches, fences, fire hydrants
			- Interior: lamps, televisions, phones, books, cups, plates
			- Structural: boxes, barrels, crates, pallets, traffic cones, dumpsters
		- Minimum 5 distinct assets per category, minimum 50 total built-in assets
		- Each asset has a display name (e.g., "Dining Table", "Office Chair", "Oak Tree")
		- The library panel provides a text search field that filters assets by name
		- Search is case-insensitive and matches partial strings ("tab" matches "Dining Table" and "Coffee Table")
		- The library panel can filter by category
		- Clicking an asset in the library places it in the scene at a default position
			- Default position: on the ground plane, in front of the camera, at a reasonable distance
			- The placed asset becomes the selected element immediately
		- Built-in assets cannot be deleted from the library — they are always available
		- Built-in assets are available offline — no network connection required

		**Design constraints:**
		- Asset visual style must be consistent across the entire built-in library — all assets should look like they belong together
		- Assets should be proportioned to real-world scale (a table is table-height, a car is car-length)
		- File size for the complete built-in library should not make the application download unreasonably large

		**Expected behavior:**
		``` python
			# Browsing and placing
			.if the user opens the asset library panel >>
				<== categories are listed
				<== assets display as a grid of thumbnails with names
				<== built-in assets are immediately available without loading

			# Search
			.if the user types "chair" in the search field >>
				<== library filters to show only assets whose names contain "chair"
				<== results include assets from any category (e.g., "Office Chair", "Dining Chair")
				<== assets not matching the query are hidden

			# Category filter
			.if the user selects the "Vehicles" category >>
				<== only vehicle assets are shown
				<== other categories' assets are hidden

			# Combining search and category
			.if the user selects the "Furniture" category
			.if the user types "table" in the search field >>
				<== only furniture assets matching "table" are shown

			# Placement
			.if the user clicks a "Dining Table" asset in the library >>
				<== a dining table appears in the scene on the ground plane
				<== the table is positioned in front of the camera at a usable distance
				<== the table is selected and gizmo is visible
				<== the table is proportioned to real-world scale

			# Placing multiple instances
			.if a dining table is already in the scene
			.if the user clicks "Dining Table" in the library again >>
				<== a second dining table appears in the scene
				<== the second table does not overlap the first
				<== the second table is named "Dining Table (1)"

			# Offline availability
			.if the user has no network connection >>
				<== every built-in asset is available and can be placed
				!== "download required" or loading indicator for built-in assets
		```

		**Error cases:**
		``` python
			# No search results
			.if the user types a query that matches no assets >>
				<== the library shows an empty state with a clear message (e.g., "No assets match your search")
				!== the application crashes or shows a blank panel

			# Empty category after search
			.if the user selects "Vehicles" category
			.if the user types "sofa" in the search field >>
				<== no assets shown
				<== empty state message displayed
		```

	---

	- ##### 3.4.2. Marketplace integration (Feature)

		***Browse and import free 3D assets from existing online marketplaces directly within Fram3d — search, preview, download, and place without leaving the application. For paid assets, redirect to the marketplace website.***

		This is the most ambitious feature in this milestone. The system supports ALL three marketplaces — **Sketchfab**, **Unity Asset Store**, and **TurboSquid** — plus **Mixamo** for character assets. Each has different APIs, authentication flows, licensing terms, file formats, and quality guarantees. The in-app browser shows **FREE assets only**. No in-app purchases — for paid assets, redirect to the marketplace website.

		*Blocked by:
		- 2.3.1 (model import — marketplace assets require format conversion and collider generation)
		- 3.4.1 (built-in asset library — marketplace assets integrate into the same browsing UI)*

		*Related:
		- 3.4.3 (user asset management — downloaded marketplace assets can be organized into collections)*

		**Functional requirements:**

		*Marketplace browsing:*
		- The user can search all four marketplace catalogs (Sketchfab, Unity Asset Store, TurboSquid, Mixamo) from within the asset library panel
		- Marketplace search results display: thumbnail, name, author, free/paid status, and file size
		- The user can preview a marketplace asset before downloading — at minimum a larger thumbnail and basic metadata (polygon count, file format)
		- Search results are paginated — the user can load more results
		- Marketplace results are visually distinguished from built-in and local assets (e.g., a marketplace badge or separate tab)

		*Authentication:*
		- The user authenticates with each marketplace individually
		- Authentication state persists across sessions — the user does not re-authenticate on every launch
		- The user can disconnect a marketplace account from within Fram3d
		- Unauthenticated users can browse and search but cannot download

		*Download and import:*
		- Browse and import FREE assets only. No in-app purchase processing.
		- The user can download a free asset with one click
		- For paid assets, redirect to the marketplace website — do not handle payment within Fram3d
		- No license attribution enforcement on export
		- Downloaded assets are automatically converted to a format usable by Fram3d
		- Downloaded assets automatically receive generated colliders for selection and gizmo interaction
		- A progress indicator shows download status for large assets
		- Downloaded assets are cached locally — re-placing the same asset does not re-download it
		- The user can cancel an in-progress download

		*Placement:*
		- Once downloaded, a marketplace asset can be placed in the scene identically to a built-in asset
		- The placed asset becomes a selectable scene element with a collider
		- Marketplace assets can be re-placed from the local cache without re-downloading

		*Marketplace scope:*
		- All four marketplaces (Sketchfab, Unity Asset Store, TurboSquid, Mixamo) are supported
		- Each marketplace integration is independently toggleable — the user can enable/disable individual marketplace connections
		- Mixamo integration focuses on character models and animations specifically

		**Design constraints:**
		- Marketplace browsing must not block the main application — the user can continue working while a search executes or an asset downloads
		- Downloaded assets must work offline after the initial download — no phone-home requirement to use a previously downloaded asset

		**Expected behavior:**
		``` python
			# Browsing marketplace
			.if the user switches to the marketplace tab in the asset library
			.if the user is authenticated with a marketplace >>
				<== a search field is available
				||> .if the user searches for "wooden table" >>
					<== marketplace results appear with thumbnails, names, authors, and free/paid status
					<== results are visually marked as marketplace assets
					<== a "Load more" option is available if there are additional results

			# Unauthenticated browsing
			.if the user is not authenticated with any marketplace
			.if the user switches to the marketplace tab >>
				<== the user can search and browse results
				!== search is blocked for unauthenticated users
				||> .if the user attempts to download an asset >>
					<== the user is prompted to authenticate
					!== the download begins without authentication

			# Free asset download
			.if the user finds a free marketplace asset
			.if the user clicks to download it >>
				<== a progress indicator appears showing download progress
				<== the user can continue working in the scene during download
				||> .if the download completes >>
					<== the asset appears in the library as a locally available asset
					<== the asset can be placed in the scene with one click
					<== a collider is auto-generated for the asset

			# Paid asset redirect
			.if the user finds a paid marketplace asset
			.if the user clicks the asset's action >>
				<== the marketplace website opens in the user's browser to the asset's page
				!== Fram3d processes payment
				!== the asset downloads without purchase

			# Re-placing a cached asset
			.if a marketplace asset was previously downloaded
			.if the user places it in the scene again >>
				<== the asset is placed immediately from local cache
				!== a network request is made to re-download the asset

			# Download cancellation
			.if a marketplace asset is downloading
			.if the user clicks cancel >>
				<== the download stops
				<== no partial asset appears in the library
				<== the user can retry the download later

			# Offline with cached assets
			.if the user has previously downloaded marketplace assets
			.if the user has no network connection >>
				<== previously downloaded assets are available for placement
				<== marketplace search is unavailable (graceful message, not an error)
				!== previously downloaded assets become unusable
		```

		**Error cases:**
		``` python
			# Download failure (network interruption)
			.if a marketplace asset is downloading
			.if the network connection drops >>
				<== the download fails with a clear error message
				<== the user can retry when connectivity is restored
				!== partial or corrupted asset added to the library

			# Oversized asset
			.if a marketplace asset exceeds a size threshold (e.g., 500MB) >>
				<== the user is warned about the file size before download begins
				<== the user can choose to proceed or cancel
				!== the download begins without warning

			# Asset with incompatible format
			.if a marketplace asset cannot be converted to a usable format >>
				<== the user is notified that the asset is incompatible
				<== a clear message explains why (e.g., "This asset uses a format Fram3d cannot import")
				!== the asset silently fails to appear

			# Asset that looks different from thumbnail
			.if a marketplace asset is downloaded and placed
			.if its in-scene appearance differs significantly from the thumbnail >>
				<== the asset's actual geometry and materials are shown — no bait-and-switch
				<== the user can delete the asset from the scene and from their local cache

			# Authentication token expires
			.if the user's marketplace authentication expires during a session >>
				<== the user is prompted to re-authenticate when they next attempt a marketplace action
				<== previously downloaded assets remain accessible
				!== the application crashes or loses state

			# Marketplace API unavailable
			.if a marketplace API is temporarily unavailable >>
				<== the marketplace tab shows a clear "service unavailable" message
				<== built-in assets and locally cached marketplace assets remain fully functional
				!== the entire asset library becomes unusable
		```

	---

	- ##### 3.4.3. User asset management (Feature)

		***Organize all assets — built-in, imported, and marketplace — into personal collections with tags, favorites, and quick-access, so a director's curated kit travels with them across projects.***

		Once a director has imported a custom prop, downloaded a period-specific vehicle, and found the right style of streetlight, they need to find those assets again instantly. Not scroll through a flat list of 200 items. This feature turns a pile of assets into an organized toolkit.

		*Related:
		- 3.4.1 (built-in asset library — built-in assets can be favorited and added to collections)
		- 3.4.2 (marketplace integration — downloaded marketplace assets appear in the user's asset inventory)
		- 2.3.1 (model import — user-imported assets are managed here)*

		**Functional requirements:**

		*Collections:*
		- The user can create named collections (e.g., "Interview Room", "1970s Street", "Cop Car Chase")
		- Any asset (built-in, imported, or marketplace-downloaded) can be added to one or more collections
		- Collections are user-level, not project-level — they persist across projects
		- The user can rename and delete collections
		- Deleting a collection does not delete the assets within it — it only removes the organizational grouping
		- The user can browse assets by collection in the library panel

		*Favorites:*
		- The user can mark any asset as a favorite with a single action (e.g., star icon, heart)
		- A "Favorites" filter shows only favorited assets
		- Favoriting is a toggle — clicking again removes the favorite
		- Favorite state is user-level and persists across projects

		*Tags:*
		- The user can assign one or more text tags to any asset (e.g., "modern", "outdoor", "hero prop")
		- Tags are freeform text, not a fixed taxonomy
		- The search field matches against tags in addition to asset names
		- The user can filter the library by tag
		- The user can remove tags from an asset
		- Tags are user-level and persist across projects

		*Recent and frequent:*
		- The library tracks recently placed assets and shows them in a "Recent" section
		- The library tracks frequently placed assets and shows them in a "Most Used" section
		- Both sections update automatically based on usage

		*Asset metadata:*
		- Every asset displays: name, source (built-in / imported / marketplace name), date added, file size
		- Marketplace assets additionally display: author
		- The user can rename any asset locally without affecting the source

		*Cache and storage:*
		- No disk space cap on cached marketplace assets
		- Simple cache management UI (basic, not elaborate)
		- Local storage only — no cloud sync for v1
		- Asset management data location is an implementation detail — not specified

		**Expected behavior:**
		``` python
			# Creating a collection
			.if the user creates a collection named "Interrogation Room" >>
				<== the collection appears in the collections list
				<== the collection is initially empty

			# Adding assets to a collection
			.if the user adds "Metal Chair", "Desk Lamp", and "Filing Cabinet" to "Interrogation Room" >>
				<== all three assets appear when browsing the "Interrogation Room" collection
				<== the assets remain visible in their original categories as well

			# Asset in multiple collections
			.if "Metal Chair" is in both "Interrogation Room" and "Office Scene" collections >>
				<== "Metal Chair" appears when browsing either collection
				<== removing it from one collection does not remove it from the other

			# Favorites
			.if the user favorites "Oak Tree" >>
				<== "Oak Tree" appears in the Favorites filter
				||> .if the user unfavorites "Oak Tree" >>
					<== "Oak Tree" no longer appears in the Favorites filter
					<== "Oak Tree" remains in the library and any collections

			# Tag-based search
			.if the user tags "Sedan" and "Pickup Truck" with "vehicle" and "modern" >>
				||> .if the user searches for "modern" >>
					<== both "Sedan" and "Pickup Truck" appear in results
					<== other assets tagged "modern" also appear

			# Combining filters
			.if the user selects the "Interrogation Room" collection
			.if the user types "chair" in the search field >>
				<== only assets in "Interrogation Room" matching "chair" are shown

			# Recent assets
			.if the user places "Dining Table" in the scene >>
				<== "Dining Table" appears in the Recent section of the library

			# Deleting a collection
			.if the user deletes the "Office Scene" collection >>
				<== the collection is removed from the collections list
				<== assets that were in the collection are not deleted
				<== assets remain in other collections they belong to
				<== assets remain in the library

			# Cross-project persistence
			.if the user creates collections and favorites in Project A
			.if the user opens Project B >>
				<== the same collections and favorites are available
				<== tags assigned in Project A are visible in Project B

			# Local rename
			.if the user renames a marketplace asset from "Wooden_Table_v2" to "Hero Table" >>
				<== the asset displays as "Hero Table" in the library
				<== the original marketplace name is preserved in asset metadata
				!== the rename affects the source marketplace
		```

		**Error cases:**
		``` python
			# Duplicate collection name
			.if the user creates a collection with the same name as an existing collection >>
				<== the user is notified that the name is already taken
				!== the existing collection is overwritten
				!== a duplicate collection is silently created

			# Deleting an asset that's in collections
			.if the user deletes an imported asset from the library
			.if that asset is in one or more collections >>
				<== the asset is removed from the library and from every collection it belonged to
				<== a confirmation is shown before deletion since the asset is referenced in collections

			# Empty tag
			.if the user tries to assign an empty string as a tag >>
				<== the tag is not created
				!== an empty tag is added to the asset

			# Orphaned marketplace asset (source removed from marketplace)
			.if a previously downloaded marketplace asset is no longer available on the marketplace >>
				<== the locally cached asset remains usable
				<== the asset remains in any collections and retains its tags and favorites
				!== the asset disappears from the user's library
		```
