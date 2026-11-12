using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LLG.EditorTools
{
    /// <summary>
    /// Adds a "LLG" menu to the top of the Unity window.
    /// LLG > Build Start Screen Scene (re)creates Assets/Scenes/StartScreen.unity.
    /// </summary>
    public static class SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/StartScreen.unity";

        [MenuItem("LLG/Build Start Screen Scene")]
        public static void BuildStartScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(96, 160, 232, 255);
            camGo.transform.position = new Vector3(0, 0, -10);
            camGo.AddComponent<AudioListener>();

            new GameObject("StartScreen").AddComponent<StartScreenController>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("LLG: start screen scene saved to " + ScenePath);
        }
    }
}
