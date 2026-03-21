using System.Collections.Generic;
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
        public static CameraDatabase Load()
        {
            var db   = new CameraDatabase();
            var path = Path.Combine(Application.streamingAssetsPath, "camera-lens-database.json");

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Camera database not found at {path}. Using generic defaults only.");

                return db;
            }

            var json    = File.ReadAllText(path);
            var rawDb   = JsonUtility.FromJson<RawDatabase>(json);

            if (rawDb.cameras != null)
            {
                foreach (var c in rawDb.cameras)
                {
                    db.AddBody(new CameraBody(
                        c.name,
                        c.manufacturer,
                        c.sensor_width_mm,
                        c.sensor_height_mm,
                        c.format,
                        c.mount,
                        c.native_resolution,
                        c.supported_fps));
                }
            }

            if (rawDb.lenses != null)
            {
                var grouped = rawDb.lenses
                    .GroupBy(l => l.set)
                    .OrderBy(g => g.Key);

                foreach (var group in grouped)
                {
                    var first = group.First();

                    if (first.type == "Zoom")
                    {
                        // Zoom lenses: one lens per set with a focal range
                        foreach (var lens in group)
                        {
                            if (lens.focal_range_mm != null && lens.focal_range_mm.Length == 2)
                            {
                                db.AddLensSet(new LensSet(
                                    lens.name,
                                    lens.focal_range_mm[0],
                                    lens.focal_range_mm[1],
                                    lens.is_anamorphic,
                                    lens.squeeze_factor));
                            }
                        }
                    }
                    else
                    {
                        // Prime lenses: group by set name, collect focal lengths
                        var focalLengths = group
                            .Where(l => l.focal_length_mm > 0)
                            .Select(l => l.focal_length_mm)
                            .OrderBy(f => f)
                            .ToArray();

                        if (focalLengths.Length > 0)
                        {
                            db.AddLensSet(new LensSet(
                                group.Key,
                                focalLengths,
                                first.is_anamorphic,
                                first.squeeze_factor));
                        }
                    }
                }
            }

            Debug.Log($"Camera database loaded: {db.Bodies.Count} bodies, {db.LensSets.Count} lens sets.");

            return db;
        }

        // Raw JSON structure matching camera-lens-database.json
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
            public string name;
            public string manufacturer;
            public float  sensor_width_mm;
            public float  sensor_height_mm;
            public string format;
            public string mount;
            public int[]  native_resolution;
            public int[]  supported_fps;
        }

        [System.Serializable]
        private class RawLens
        {
            public string  name;
            public string  set;
            public string  type;
            public float   focal_length_mm;
            public float[] focal_range_mm;
            public bool    is_anamorphic;
            public float   squeeze_factor;
        }
    }
}
