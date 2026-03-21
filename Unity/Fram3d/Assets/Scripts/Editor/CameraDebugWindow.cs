using System.Linq;
using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using UnityEditor;
using UnityEngine;
namespace Fram3d.Editor
{
    /// <summary>
    /// Temporary debug window for switching camera body and lens set.
    /// Will be replaced by the Properties Panel (FRA-123).
    /// </summary>
    public sealed class CameraDebugWindow: EditorWindow
    {
        private string[]        _bodyNames;
        private CameraBehaviour _cameraBehaviour;
        private string[]        _lensSetNames;
        private int             _selectedBodyIndex;
        private int             _selectedLensSetIndex;

        [MenuItem("Fram3d/Camera Debug")]
        public static void ShowWindow() => GetWindow<CameraDebugWindow>("Camera Debug");

        private void BuildDropdowns(CameraDatabase db)
        {
            this._bodyNames    = db.Bodies.Select(b => $"{b.Manufacturer} — {b.Name}").ToArray();
            this._lensSetNames = db.LensSets.Select(ls => ls.Name).ToArray();
            var cam = this._cameraBehaviour.CameraElement;
            this._selectedBodyIndex    = cam.Body          != null? db.Bodies.ToList().IndexOf(cam.Body) : 0;
            this._selectedLensSetIndex = cam.ActiveLensSet != null? db.LensSets.ToList().IndexOf(cam.ActiveLensSet) : 0;
        }

        private void DrawBodySelector(CameraElement cam, CameraDatabase db)
        {
            EditorGUILayout.LabelField("Camera Body", EditorStyles.boldLabel);
            var newIndex = EditorGUILayout.Popup(this._selectedBodyIndex, this._bodyNames);

            if (newIndex == this._selectedBodyIndex)
                return;

            this._selectedBodyIndex = newIndex;
            cam.SetBody(db.Bodies[newIndex]);
            EditorGUILayout.Space();
        }

        private void DrawCurrentState(CameraElement cam)
        {
            EditorGUILayout.LabelField("Current State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Body",          cam.Body?.Name ?? "(none)");
            EditorGUILayout.LabelField("Sensor",        $"{cam.SensorWidth:F2} x {cam.SensorHeight:F2} mm");
            EditorGUILayout.LabelField("Lens Set",      cam.ActiveLensSet?.Name ?? "(none)");
            EditorGUILayout.LabelField("Focal Length",  $"{cam.FocalLength:F1} mm");
            EditorGUILayout.LabelField("Vertical FOV",  $"{cam.VerticalFov * Mathf.Rad2Deg:F1}°");
            var focusDist = cam.FocusAtInfinity ? "\u221E" : $"{cam.FocusDistance:F1}m";
            EditorGUILayout.LabelField("DOF",           cam.DofEnabled ? $"ON  f/{cam.Aperture:G}  @ {focusDist}" : "OFF");
            EditorGUILayout.LabelField("Shake",         cam.ShakeEnabled ? $"ON  amp:{cam.ShakeAmplitude:F2}  freq:{cam.ShakeFrequency:F1}" : "OFF");
            EditorGUILayout.Space();
        }

        private void DrawLensSetSelector(CameraElement cam, CameraDatabase db)
        {
            EditorGUILayout.LabelField("Lens Set", EditorStyles.boldLabel);
            var newIndex = EditorGUILayout.Popup(this._selectedLensSetIndex, this._lensSetNames);

            if (newIndex == this._selectedLensSetIndex)
                return;

            this._selectedLensSetIndex = newIndex;
            cam.SetLensSet(db.LensSets[newIndex]);
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                this.RefreshReferences();
        }

        private void RefreshReferences()
        {
            this._cameraBehaviour = FindObjectOfType<CameraBehaviour>();

            if (this._cameraBehaviour != null)
                this.BuildDropdowns(this._cameraBehaviour.Database);
        }

        private static void DrawLensSetDetails(CameraElement cam)
        {
            var lensSet = cam.ActiveLensSet;

            if (lensSet == null)
                return;

            EditorGUILayout.Space();

            if (lensSet.IsZoom)
            {
                EditorGUILayout.LabelField("Type",  "Zoom");
                EditorGUILayout.LabelField("Range", $"{lensSet.MinFocalLength}–{lensSet.MaxFocalLength} mm");
            }
            else
            {
                EditorGUILayout.LabelField("Type",          "Prime");
                EditorGUILayout.LabelField("Focal Lengths", string.Join(", ", lensSet.FocalLengths.Select(f => $"{f}mm")));
            }

            if (lensSet.IsAnamorphic)
                EditorGUILayout.LabelField("Squeeze", $"{lensSet.SqueezeFactor}x");
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += this.OnPlayModeChanged;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use camera debug controls.", MessageType.Info);
                return;
            }

            if (this._cameraBehaviour == null)
                this.RefreshReferences();

            if (this._cameraBehaviour == null)
            {
                EditorGUILayout.HelpBox("No CameraBehaviour found in scene.", MessageType.Warning);
                return;
            }

            var cam = this._cameraBehaviour.CameraElement;
            var db  = this._cameraBehaviour.Database;

            if (this._bodyNames == null)
                this.BuildDropdowns(db);

            this.DrawCurrentState(cam);
            this.DrawBodySelector(cam, db);
            this.DrawLensSetSelector(cam, db);
            DrawLensSetDetails(cam);
            this.Repaint();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayModeChanged;
        }
    }
}