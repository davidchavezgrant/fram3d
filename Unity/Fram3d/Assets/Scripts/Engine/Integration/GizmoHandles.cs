using System.Collections.Generic;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Return type for <see cref="GizmoHandleFactory.Build"/>. Holds
    /// references to the constructed gizmo GameObjects and the per-handle
    /// axis color registry used by <see cref="GizmoHighlighter"/>.
    /// </summary>
    internal sealed class GizmoHandles
    {
        public GizmoHandles(Dictionary<Renderer, Color> axisColors,
                            GameObject                  root,
                            GameObject                  rotateGroup,
                            GameObject                  scaleGroup,
                            GameObject                  translateGroup)
        {
            this.AxisColors     = axisColors;
            this.Root           = root;
            this.RotateGroup    = rotateGroup;
            this.ScaleGroup     = scaleGroup;
            this.TranslateGroup = translateGroup;
        }

        public Dictionary<Renderer, Color> AxisColors  { get; }
        public GameObject                  Root        { get; }
        public GameObject                  RotateGroup { get; }
        public GameObject                  ScaleGroup  { get; }
        public GameObject                  TranslateGroup { get; }
    }
}