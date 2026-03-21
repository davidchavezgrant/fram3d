using System.IO;
using System.Linq;
using Fram3d.Core.Camera;
using UnityEngine;

namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Loads the camera-lens-database.json from StreamingAssets at startup,
    /// parses it into Core types, and populates a CameraDatabase.
    /// </summary>
    public static class CameraDatabaseLoader
    {
        private const string DATABASE_FILENAME = "camera-lens-database.json";

        public static CameraDatabase Load()
        {
            var db   = new CameraDatabase();
            var path = Path.Combine(Application.streamingAssetsPath, DATABASE_FILENAME);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Camera database not found at {path}. Using generic defaults only.");

                return db;
            }

            var json  = File.ReadAllText(path);
            var rawDb = JsonUtility.FromJson<RawDatabase>(json);

            LoadBodies(db, rawDb);
            LoadLensSets(db, rawDb);

            Debug.Log($"Camera database loaded: {db.Bodies.Count} bodies, {db.LensSets.Count} lens sets.");

            return db;
        }

        private static void LoadBodies(CameraDatabase db, RawDatabase rawDb)
        {
            if (rawDb.cameras == null)
                return;

            foreach (var raw in rawDb.cameras)
            {
                db.AddBody(new CameraBody(
                    raw.name,
                    raw.manufacturer,
                    raw.sensor_width_mm,
                    raw.sensor_height_mm,
                    raw.format,
                    raw.mount,
                    raw.native_resolution,
                    raw.supported_fps));
            }
        }

        private static void LoadLensSets(CameraDatabase db, RawDatabase rawDb)
        {
            if (rawDb.lenses == null)
                return;

            var groupedBySet = rawDb.lenses
                .GroupBy(l => l.set)
                .OrderBy(g => g.Key);

            foreach (var group in groupedBySet)
            {
                var first = group.First();

                if (first.type == "Zoom")
                    LoadZoomLensSets(db, group);
                else
                    LoadPrimeLensSet(db, group.Key, group, first);
            }
        }

        private static void LoadZoomLensSets(CameraDatabase db, IGrouping<string, RawLens> group)
        {
            foreach (var lens in group)
            {
                var range = lens.focal_range_mm;

                if (range == null || range.Length != 2)
                    continue;

                db.AddLensSet(new LensSet(
                    lens.name,
                    range[0],
                    range[1],
                    lens.is_anamorphic,
                    lens.squeeze_factor));
            }
        }

        private static void LoadPrimeLensSet(CameraDatabase db,
                                             string         setName,
                                             IGrouping<string, RawLens> group,
                                             RawLens        representative)
        {
            var focalLengths = group
                .Where(l => l.focal_length_mm > 0)
                .Select(l => l.focal_length_mm)
                .OrderBy(f => f)
                .ToArray();

            if (focalLengths.Length == 0)
                return;

            db.AddLensSet(new LensSet(
                setName,
                focalLengths,
                representative.is_anamorphic,
                representative.squeeze_factor));
        }

        // Raw JSON structure matching camera-lens-database.json.
        // Using JsonUtility which requires serializable classes with public fields.

        [System.Serializable]
        private class RawDatabase
        {
            public RawCamera[] cameras;
            public RawLens[]   lenses;
        }

        [System.Serializable]
        private class RawCamera
        {
            public string format;
            public string manufacturer;
            public string mount;
            public string name;
            public int[]  native_resolution;
            public float  sensor_height_mm;
            public float  sensor_width_mm;
            public int[]  supported_fps;
        }

        [System.Serializable]
        private class RawLens
        {
            public float   focal_length_mm;
            public float[] focal_range_mm;
            public bool    is_anamorphic;
            public string  name;
            public string  set;
            public float   squeeze_factor;
            public string  type;
        }
    }
}
