namespace Fram3d.Core.Camera
{
    public sealed class MovementSpeeds
    {
        public float Dolly     { get; set; } = 0.01f;
        public float PanTilt   { get; set; } = 0.2f;
        public float Roll      { get; set; } = 0.03f;
        public float Crane     { get; set; } = 0.02f;
        public float Truck     { get; set; } = 0.02f;
        public float DollyZoom { get; set; } = 0.5f;
    }
}