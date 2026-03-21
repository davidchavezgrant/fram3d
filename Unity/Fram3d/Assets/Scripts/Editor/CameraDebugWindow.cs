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
        private CameraBehaviour _cameraBehaviour;
        private string[]        _bodyNames;
        private string[]        _lensSetNames;
        private int             _selectedBodyIndex;
        private int             _selectedLensSetIndex;

        [MenuItem("Fram3d/Camera Debug")]
        public static void ShowWindow() => GetWindow<CameraDebugWindow>("Camera Debug");

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += this.OnPlayModeChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                this.RefreshReferences();
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

            // Current state
            EditorGUILayout.LabelField("Current State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Body",          cam.Body?.Name ?? "(none)");
            EditorGUILayout.LabelField("Sensor",        $"{cam.SensorWidth:F2} x {cam.SensorHeight:F2} mm");
            EditorGUILayout.LabelField("Lens Set",      cam.ActiveLensSet?.Name ?? "(none)");
            EditorGUILayout.LabelField("Focal Length",  $"{cam.FocalLength:F1} mm");
            EditorGUILayout.LabelField("Vertical FOV",  $"{cam.ComputeVerticalFov() * Mathf.Rad2Deg:F1}°");
            EditorGUILayout.Space();

            // Body selector
            if (this._bodyNames == null)
                this.BuildDropdowns(db);

            EditorGUILayout.LabelField("Camera Body", EditorStyles.boldLabel);
            var newBodyIndex = EditorGUILayout.Popup(this._selectedBodyIndex, this._bodyNames);

            if (newBodyIndex != this._selectedBodyIndex)
            {
                this._selectedBodyIndex = newBodyIndex;
                cam.SetBody(db.Bodies[newBodyIndex]);
            }

            EditorGUILayout.Space();

            // Lens set selector
            EditorGUILayout.LabelField("Lens Set", EditorStyles.boldLabel);
            var newLensIndex = EditorGUILayout.Popup(this._selectedLensSetIndex, this._lensSetNames);

            if (newLensIndex != this._selectedLensSetIndex)
            {
                this._selectedLensSetIndex = newLensIndex;
                cam.SetLensSet(db.LensSets[newLensIndex]);
            }

            // Show available focal lengths
            var activeLensSet = cam.ActiveLensSet;

            if (activeLensSet != null)
            {
                EditorGUILayout.Space();

                if (activeLensSet.IsZoom)
                {
                    EditorGUILayout.LabelField("Type",  "Zoom");
                    EditorGUILayout.LabelField("Range", $"{activeLensSet.MinFocalLength}–{activeLensSet.MaxFocalLength} mm");
                }
                else
                {
                    EditorGUILayout.LabelField("Type",          "Prime");
                    EditorGUILayout.LabelField("Focal Lengths", string.Join(", ", activeLensSet.FocalLengths.Select(f => $"{f}mm")));
                }

                if (activeLensSet.IsAnamorphic)
                    EditorGUILayout.LabelField("Squeeze", $"{activeLensSet.SqueezeFactor}x");
            }

            this.Repaint();
        }

        private void RefreshReferences()
        {
            this._cameraBehaviour = FindObjectOfType<CameraBehaviour>();

            if (this._cameraBehaviour != null)
                this.BuildDropdowns(this._cameraBehaviour.Database);
        }

        private void BuildDropdowns(CameraDatabase db)
        {
            this._bodyNames    = db.Bodies.Select(b => $"{b.Manufacturer} — {b.Name}").ToArray();
            this._lensSetNames = db.LensSets.Select(ls => ls.Name).ToArray();

            // Find current selection indices
            var cam = this._cameraBehaviour.CameraElement;
            this._selectedBodyIndex    = cam.Body          != null? db.Bodies.ToList().IndexOf(cam.Body) : 0;
            this._selectedLensSetIndex = cam.ActiveLensSet != null? db.LensSets.ToList().IndexOf(cam.ActiveLensSet) : 0;
        }
    }
}