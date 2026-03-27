using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Fram3d.Tests
{
    /// <summary>
    /// Validates that all [SerializeField] references in SampleScene are wired
    /// correctly. Catches renames that update the C# field name but not the
    /// scene file — Unity silently deserializes those as null.
    /// </summary>
    public sealed class SceneWiringTests
    {
        private const string SCENE_PATH = "Assets/Scenes/SampleScene.unity";

        /// <summary>
        /// Fields that are intentionally unset in the scene (fileID: 0).
        /// Key: "TypeName.fieldName"
        /// </summary>
        private static readonly HashSet<string> _optionalFields = new()
        {
            "CameraInputHandler.viewLayoutView",
        };

        [UnityTest]
        public IEnumerator SerializedReferences__AreNotNull__When__SceneLoaded()
        {
            EditorSceneManager.LoadSceneAsyncInPlayMode(
                SCENE_PATH,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            var errors = new List<string>();
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsSortMode.None);

            foreach (var behaviour in behaviours)
            {
                var type = behaviour.GetType();

                if (!type.FullName.StartsWith("Fram3d."))
                {
                    continue;
                }

                var fields = type.GetFields(
                    BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    if (field.GetCustomAttribute<SerializeField>() == null)
                    {
                        continue;
                    }

                    if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    {
                        continue;
                    }

                    var key = $"{type.Name}.{field.Name}";

                    if (_optionalFields.Contains(key))
                    {
                        continue;
                    }

                    var value = (UnityEngine.Object)field.GetValue(behaviour);

                    if (value == null)
                    {
                        errors.Add($"{type.Name}.{field.Name} ({field.FieldType.Name})");
                    }
                }
            }

            Assert.IsEmpty(
                errors,
                $"Null [SerializeField] references in scene — likely a rename " +
                $"that wasn't updated in {SCENE_PATH}:\n  " +
                string.Join("\n  ", errors));
        }
    }
}
