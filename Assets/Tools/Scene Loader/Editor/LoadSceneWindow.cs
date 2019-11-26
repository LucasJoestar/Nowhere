using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LoadSceneWindow : EditorWindow
{
    #region Fields / Properties
    /// <summary>
    /// All project loaded scenes path.
    /// </summary>
    private string[] allScenesPath = new string[] { };
    #endregion

    #region Methods

    #region Original Methods
    /// <summary>
    /// Call this window from the Unity menu toolbar.
    /// </summary>
    [MenuItem("Tools/Scenes Loader")]
    public static void CallWindow()
    {
        GetWindow(typeof(LoadSceneWindow), true, "Load Scene Window").Show();
    }
    #endregion

    #region Unity Methods
    // This function is called when the object is loaded
    private void OnEnable()
    {
        allScenesPath = Array.ConvertAll<string, string>(AssetDatabase.FindAssets("t:Scene"), AssetDatabase.GUIDToAssetPath);
        Array.Sort(allScenesPath);

        maxSize = new Vector2(275, (allScenesPath.Length * 20) + 5);
        minSize = maxSize;
    }

    // Implement your own editor GUI here
    private void OnGUI()
    {
        foreach (string _scene in allScenesPath)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(Path.GetFileNameWithoutExtension(_scene));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load"))
            {
                EditorSceneManager.OpenScene(_scene);
                Close();
                return;
            }

            EditorGUILayout.EndHorizontal();
        }
    }
    #endregion

    #endregion
}
