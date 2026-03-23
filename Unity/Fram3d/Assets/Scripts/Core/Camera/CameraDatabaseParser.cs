using System.Linq;
namespace Fram3d.Core.Camera
{
    /// <summary>
    /// Parses raw deserialized camera/lens data into Core domain types.
    /// Pure logic — no Unity, no I/O. The Engine layer handles file reading
    /// and JSON deserialization, then delegates to this class.
    /// </summary>
    public static class CameraDatabaseParser
    {
        public static void LoadBodies(CameraDatabase db, RawCamera[] cameras)
        {
            if (cameras == null)
            {
                return;
            }

            foreach (var raw in cameras)
            {
                var sensorModes = ParseSensorModes(raw.SensorModes);

                db.AddBody(new CameraBody(raw.Name,
                                          raw.Manufacturer,
                                          raw.Year,
                                          raw.SensorWidthMm,
                                          raw.SensorHeightMm,
                                          raw.Format,
                                          raw.Mount,
                                          raw.NativeResolution,
                                          raw.SupportedFps,
                                          sensorModes));
            }
        }

        public static void LoadLensSets(CameraDatabase db, RawLens[] lenses)
        {
            if (lenses == null)
            {
                return;
            }

            var groupedBySet = lenses.GroupBy(l => l.Set).OrderBy(g => g.Key);

            foreach (var group in groupedBySet)
            {
                var first = group.First();

                if (first.Type == "Zoom")
                {
                    LoadZoomLensSets(db, group);
                }
                else
                {
                    LoadPrimeLensSet(db,
                                     group.Key,
                                     group,
                                     first);
                }
            }
        }

        public static SensorMode[] ParseSensorModes(RawSensorMode[] rawModes)
        {
            if (rawModes == null || rawModes.Length == 0)
            {
                return null;
            }

            var modes = new SensorMode[rawModes.Length];

            for (var i = 0; i < rawModes.Length; i++)
            {
                var raw        = rawModes[i];
                var res        = raw.Resolution;
                var sensorArea = raw.SensorAreaMm;

                modes[i] = new SensorMode(raw.Name,
                                          res        != null && res.Length        >= 2? res[0] : 0,
                                          res        != null && res.Length        >= 2? res[1] : 0,
                                          sensorArea != null && sensorArea.Length >= 2? sensorArea[0] : 0f,
                                          sensorArea != null && sensorArea.Length >= 2? sensorArea[1] : 0f,
                                          raw.MaxFps);
            }

            return modes;
        }

        private static void LoadPrimeLensSet(CameraDatabase             db,
                                             string                     setName,
                                             IGrouping<string, RawLens> group,
                                             RawLens                    representative)
        {
            var specs = group.Where(l => l.FocalLengthMm > 0)
                             .OrderBy(l => l.FocalLengthMm)
                             .Select(l => new LensSpec(l.FocalLengthMm, l.MaxApertureTstop, l.CloseFocusM))
                             .ToArray();

            if (specs.Length == 0)
            {
                return;
            }

            db.AddLensSet(new LensSet(setName,
                                      specs,
                                      representative.IsAnamorphic,
                                      representative.SqueezeFactor));
        }

        private static void LoadZoomLensSets(CameraDatabase db, IGrouping<string, RawLens> group)
        {
            foreach (var lens in group)
            {
                var range = lens.FocalRangeMm;

                if (range == null || range.Length != 2)
                {
                    continue;
                }

                db.AddLensSet(new LensSet(lens.Name,
                                          range[0],
                                          range[1],
                                          lens.IsAnamorphic,
                                          lens.SqueezeFactor,
                                          lens.MaxApertureTstop > 0? lens.MaxApertureTstop : 0f,
                                          lens.CloseFocusM      > 0? lens.CloseFocusM : 0f));
            }
        }
    }


    /// <summary>
    /// Raw deserialized camera body from JSON. Field names use PascalCase
    /// for Core; the Engine deserializer maps from snake_case JSON fields.
    /// </summary>
    public sealed class RawCamera
    {
        public string          Format;
        public string          Manufacturer;
        public string          Mount;
        public string          Name;
        public int[]           NativeResolution;
        public float           SensorHeightMm;
        public RawSensorMode[] SensorModes;
        public float           SensorWidthMm;
        public int[]           SupportedFps;
        public int             Year;
    }


    public sealed class RawLens
    {
        public float   CloseFocusM;
        public float   FocalLengthMm;
        public float[] FocalRangeMm;
        public bool    IsAnamorphic;
        public float   MaxApertureTstop;
        public string  Name;
        public string  Set;
        public float   SqueezeFactor;
        public string  Type;
    }


    public sealed class RawSensorMode
    {
        public int    MaxFps;
        public string Name;
        public int[]  Resolution;
        public float[] SensorAreaMm;
    }
}