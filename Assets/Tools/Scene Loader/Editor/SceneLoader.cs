// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneLoader : EditorWindow
{
    #region Methods
    [MenuItem("Tools/Scene Loader")]
    public static void Get() => GetWindow(typeof(SceneLoader)).Show();

    // -----------------------

    private GUIContent loadButtonGUI = new GUIContent("Load", "Load this scene.");
    private string[] scenesPath = new string[] { };
    private string[] scenesName = new string[] { };

    private void OnEnable()
    {
        scenesPath = Array.ConvertAll(AssetDatabase.FindAssets("t:Scene"), AssetDatabase.GUIDToAssetPath);
        Array.Sort(scenesPath);

        scenesName = new string[scenesPath.Length];
        for (int _i = 0; _i < scenesName.Length; _i++)
        {
            scenesName[_i] = Path.GetFileNameWithoutExtension(scenesPath[_i]);
        }
    }

    private void OnGUI()
    {
        for (int _i = 0; _i < scenesPath.Length; _i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(scenesName[_i]);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(loadButtonGUI))
            {
                EditorSceneManager.OpenScene(scenesPath[_i]);
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    #endregion
}
