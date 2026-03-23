using System.IO;
using Fram3d.Core.Camera;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Thin adapter: reads JSON from StreamingAssets, deserializes into
    /// Unity-serializable raw classes, maps them to Core raw types, and
    /// delegates all parsing logic to CameraDatabaseParser.
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
            var rawDb = JsonUtility.FromJson<SerializableDatabase>(json);
            CameraDatabaseParser.LoadBodies(db, MapCameras(rawDb.cameras));
            CameraDatabaseParser.LoadLensSets(db, MapLenses(rawDb.lenses));
            Debug.Log($"Camera database loaded: {db.Bodies.Count} bodies, {db.LensSets.Count} lens sets.");
            return db;
        }

        private static RawCamera[] MapCameras(SerializableCamera[] cameras)
        {
            if (cameras == null)
            {
                return null;
            }

            var result = new RawCamera[cameras.Length];

            for (var i = 0; i < cameras.Length; i++)
            {
                var c = cameras[i];

                result[i] = new RawCamera
                {
                    Name             = c.name,
                    Manufacturer     = c.manufacturer,
                    Year             = c.year,
                    SensorWidthMm    = c.sensor_width_mm,
                    SensorHeightMm   = c.sensor_height_mm,
                    Format           = c.format,
                    Mount            = c.mount,
                    NativeResolution = c.native_resolution,
                    SupportedFps     = c.supported_fps,
                    SensorModes      = MapSensorModes(c.sensor_modes)
                };
            }

            return result;
        }

        private static RawLens[] MapLenses(SerializableLens[] lenses)
        {
            if (lenses == null)
            {
                return null;
            }

            var result = new RawLens[lenses.Length];

            for (var i = 0; i < lenses.Length; i++)
            {
                var l = lenses[i];

                result[i] = new RawLens
                {
                    Name             = l.name,
                    Set              = l.set,
                    Type             = l.type,
                    FocalLengthMm    = l.focal_length_mm,
                    FocalRangeMm     = l.focal_range_mm,
                    MaxApertureTstop = l.max_aperture_tstop,
                    CloseFocusM      = l.close_focus_m,
                    IsAnamorphic     = l.is_anamorphic,
                    SqueezeFactor    = l.squeeze_factor
                };
            }

            return result;
        }

        private static RawSensorMode[] MapSensorModes(SerializableSensorMode[] modes)
        {
            if (modes == null)
            {
                return null;
            }

            var result = new RawSensorMode[modes.Length];

            for (var i = 0; i < modes.Length; i++)
            {
                var m = modes[i];

                result[i] = new RawSensorMode
                {
                    Name         = m.name,
                    Resolution   = m.resolution,
                    SensorAreaMm = m.sensor_area_mm,
                    MaxFps       = m.max_fps
                };
            }

            return result;
        }

        // --- Unity-serializable raw classes (JsonUtility requires [Serializable]) ---


        [System.Serializable]
        private class SerializableCamera
        {
            public string                   format;
            public string                   manufacturer;
            public string                   mount;
            public string                   name;
            public int[]                    native_resolution;
            public float                    sensor_height_mm;
            public SerializableSensorMode[] sensor_modes;
            public float                    sensor_width_mm;
            public int[]                    supported_fps;
            public int                      year;
        }


        [System.Serializable]
        private class SerializableDatabase
        {
            public SerializableCamera[] cameras;
            public SerializableLens[]   lenses;
        }


        [System.Serializable]
        private class SerializableLens
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
        private class SerializableSensorMode
        {
            public int     max_fps;
            public string  name;
            public int[]   resolution;
            public float[] sensor_area_mm;
        }
    }
}