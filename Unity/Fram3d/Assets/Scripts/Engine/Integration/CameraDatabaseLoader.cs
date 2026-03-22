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
                var sensorModes = ParseSensorModes(raw.sensor_modes);

                db.AddBody(new CameraBody(raw.name,
                                          raw.manufacturer,
                                          raw.year,
                                          raw.sensor_width_mm,
                                          raw.sensor_height_mm,
                                          raw.format,
                                          raw.mount,
                                          raw.native_resolution,
                                          raw.supported_fps,
                                          sensorModes));
            }
        }

        private static void LoadLensSets(CameraDatabase db, RawDatabase rawDb)
        {
            if (rawDb.lenses == null)
                return;

            var groupedBySet = rawDb.lenses.GroupBy(l => l.set).OrderBy(g => g.Key);

            foreach (var group in groupedBySet)
            {
                var first = group.First();

                if (first.type == "Zoom")
                    LoadZoomLensSets(db, group);
                else
                    LoadPrimeLensSet(db,
                                     group.Key,
                                     group,
                                     first);
            }
        }

        private static void LoadPrimeLensSet(CameraDatabase             db,
                                             string                     setName,
                                             IGrouping<string, RawLens> group,
                                             RawLens                    representative)
        {
            var specs = group.Where(l => l.focal_length_mm > 0)
                             .OrderBy(l => l.focal_length_mm)
                             .Select(l => new LensSpec(l.focal_length_mm, l.max_aperture_tstop, l.close_focus_m))
                             .ToArray();

            if (specs.Length == 0)
                return;

            db.AddLensSet(new LensSet(setName,
                                      specs,
                                      representative.is_anamorphic,
                                      representative.squeeze_factor));
        }

        private static void LoadZoomLensSets(CameraDatabase db, IGrouping<string, RawLens> group)
        {
            foreach (var lens in group)
            {
                var range = lens.focal_range_mm;

                if (range == null || range.Length != 2)
                    continue;

                db.AddLensSet(new LensSet(lens.name,
                                          range[0],
                                          range[1],
                                          lens.is_anamorphic,
                                          lens.squeeze_factor,
                                          lens.max_aperture_tstop > 0? lens.max_aperture_tstop : 0f,
                                          lens.close_focus_m      > 0? lens.close_focus_m : 0f));
            }
        }

        private static SensorMode[] ParseSensorModes(RawSensorMode[] rawModes)
        {
            if (rawModes == null || rawModes.Length == 0)
                return null;

            var modes = new SensorMode[rawModes.Length];

            for (var i = 0; i < rawModes.Length; i++)
            {
                var raw        = rawModes[i];
                var res        = raw.resolution;
                var sensorArea = raw.sensor_area_mm;

                modes[i] = new SensorMode(raw.name,
                                          res        != null && res.Length        >= 2? res[0] : 0,
                                          res        != null && res.Length        >= 2? res[1] : 0,
                                          sensorArea != null && sensorArea.Length >= 2? sensorArea[0] : 0f,
                                          sensorArea != null && sensorArea.Length >= 2? sensorArea[1] : 0f,
                                          raw.max_fps);
            }

            return modes;
        }


        [System.Serializable]
        private class RawCamera
        {
            public string          format;
            public string          manufacturer;
            public string          mount;
            public string          name;
            public int[]           native_resolution;
            public float           sensor_height_mm;
            public RawSensorMode[] sensor_modes;
            public float           sensor_width_mm;
            public int[]           supported_fps;
            public int             year;
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
        private class RawLens
        {
            public float   close_focus_m;
            public float   focal_length_mm;
            public float[] focal_range_mm;
            public bool    is_anamorphic;
            public float   max_aperture_tstop;
            public string  name;
            public string  set;
            public float   squeeze_factor;
            public string  type;
        }


        [System.Serializable]
        private class RawSensorMode
        {
            public int     max_fps;
            public string  name;
            public int[]   resolution;
            public float[] sensor_area_mm;
        }
    }
}