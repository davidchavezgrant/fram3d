using System.Collections.Generic;
using System.Linq;
namespace Fram3d.Core.Camera
{
    public sealed class CameraDatabase
    {
        private readonly List<CameraBody> _bodies   = new();
        private readonly List<LensSet>    _lensSets = new();

        public CameraDatabase()
        {
            this.AddGenericDefaults();
        }

        public IReadOnlyList<CameraBody> Bodies                      => this._bodies;
        public CameraBody                DefaultBody                 => this.FindBody("Generic 35mm");
        public LensSet                   DefaultLensSet              => this.FindLensSet("Generic Prime");
        public IReadOnlyList<LensSet>    LensSets                    => this._lensSets;
        public void                      AddBody(CameraBody body)    => this._bodies.Add(body);
        public void                      AddLensSet(LensSet lensSet) => this._lensSets.Add(lensSet);
        public CameraBody                FindBody(string    name)    => this._bodies.FirstOrDefault(b => b.Name     == name);
        public LensSet                   FindLensSet(string name)    => this._lensSets.FirstOrDefault(ls => ls.Name == name);

        private void AddGenericDefaults()
        {
            this._bodies.AddRange(new[]
            {
                new CameraBody("Generic 35mm",
                               "Generic",
                               36.0f,
                               24.0f,
                               "FF",
                               "",
                               new[] { 4096, 2160 },
                               new[] { 24, 25, 30, 48, 60 }),
                new CameraBody("Generic Super 35",
                               "Generic",
                               24.89f,
                               18.66f,
                               "S35",
                               "",
                               new[] { 4096, 2160 },
                               new[] { 24, 25, 30, 48, 60 }),
                new CameraBody("Generic 16mm",
                               "Generic",
                               10.26f,
                               7.49f,
                               "16mm",
                               "",
                               new[] { 1920, 1080 },
                               new[] { 24, 25, 30 }),
                new CameraBody("Generic Super 16",
                               "Generic",
                               12.52f,
                               7.41f,
                               "S16",
                               "",
                               new[] { 1920, 1080 },
                               new[] { 24, 25, 30 }),
                new CameraBody("Generic 8mm",
                               "Generic",
                               4.5f,
                               3.3f,
                               "8mm",
                               "",
                               new[] { 1920, 1080 },
                               new[] { 24, 25, 30 }),
            });

            this._lensSets.Add(new LensSet("Generic Prime",
                                           new float[] { 14, 18, 21, 24, 28, 35, 50, 65, 75, 85, 100, 135, 150, 200, 300, 400 },
                                           false,
                                           1.0f));
        }
    }
}