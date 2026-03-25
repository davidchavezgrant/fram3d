using System.Numerics;
using Fram3d.Core.Common;
namespace Fram3d.Core.Scene
{
    /// <summary>
    /// Encapsulates the state of an active gizmo drag. Created when a drag
    /// begins, queried each frame during drag, discarded when drag ends.
    /// All computation is pure — no Unity dependencies.
    /// </summary>
    public sealed class DragSession
    {
        private const float MIN_SCALE          = 0.01f;
        private const float ROTATE_SENSITIVITY = 0.5f;
        private const float SCALE_SENSITIVITY  = 0.005f;

        public DragSession(GizmoAxis axis,
                           Element   element,
                           float     mouseX,
                           float     mouseY,
                           Vector3   axisOffset)
        {
            this.Axis            = axis;
            this.Element         = element;
            this.StartPosition   = element.Position;
            this.StartRotation   = element.Rotation;
            this.StartScale      = element.Scale;
            this.StartMouseX     = mouseX;
            this.StartMouseY     = mouseY;
            this.StartAxisOffset = axisOffset;
        }

        public GizmoAxis  Axis            { get; }
        public Element    Element         { get; }
        public Vector3    StartAxisOffset { get; }
        public float      StartMouseX     { get; }
        public float      StartMouseY     { get; }
        public Vector3    StartPosition   { get; }
        public Quaternion StartRotation   { get; }
        public float      StartScale      { get; }

        public void UpdateRotation(float currentMouseX)
        {
            var deltaX = currentMouseX - this.StartMouseX;

            this.Element.Rotation = TransformOperations.ComputeRotation(this.StartRotation,
                                                                        this.Axis.Direction,
                                                                        deltaX,
                                                                        ROTATE_SENSITIVITY);
        }

        public void UpdateScale(float currentMouseY)
        {
            var deltaY = currentMouseY - this.StartMouseY;

            this.Element.Scale = TransformOperations.ComputeScale(this.StartScale,
                                                                  deltaY,
                                                                  SCALE_SENSITIVITY,
                                                                  MIN_SCALE);
        }

        public void UpdateTranslation(Vector3 rayOrigin, Vector3 rayDirection, Vector3 cameraForward)
        {
            var axisDir = this.Axis.Direction;

            var projected = TransformOperations.ProjectOntoAxis(this.StartPosition,
                                                                axisDir,
                                                                rayOrigin,
                                                                rayDirection,
                                                                cameraForward);

            this.Element.Position = TransformOperations.ComputeTranslation(this.StartPosition,
                                                                           axisDir,
                                                                           projected,
                                                                           this.StartPosition,
                                                                           this.StartAxisOffset);
        }
    }
}