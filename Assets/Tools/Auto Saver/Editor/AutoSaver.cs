// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AutoSaver : EditorWindow
{
    #region Fields
    [SerializeField] private bool   isAutoSave =    false;
    [SerializeField] private int    saveInterval =  300;
    private double nextSaveTime = 0;
    #endregion

    #region Methods
    [MenuItem("Tools/Auto Saver")]
    public static void Get() => GetWindow(typeof(AutoSaver)).Show();

    /// <summary>
    /// Save the current open edited scene.
    /// </summary>
    private void SaveScene()
    {
        // If the application is playing, do not save
        if (EditorApplication.isPlaying)
            return;

        if (EditorSceneManager.SaveOpenScenes())
        {
            nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        }
        else
        {
            nextSaveTime = EditorApplication.timeSinceStartup + 30;
            Debug.LogWarning("Scene not saved correctly ! Another try in 30 seconds.");
        }
    }

    // -----------------------

    private const string nextSaveGUI = "Next Save in :";

    private readonly GUIContent autoSaveGUI = new GUIContent("Auto Save :", "Activate or deactive auto save.");
    private readonly GUIContent saveIntervalGUI = new GUIContent("Save Interval (in seconds) : ", "Interval in seconds between each auto save.");

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        isAutoSave = EditorGUILayout.Toggle(autoSaveGUI, isAutoSave);

        // Save the scene when auto save is set enabled.
        if (EditorGUI.EndChangeCheck() && isAutoSave)
            SaveScene();

        // ---------- Show Time Values ---------- //

        EditorGUILayout.Space();

        if (isAutoSave)
        {
            EditorGUI.BeginChangeCheck();
            saveInterval = EditorGUILayout.IntField(saveIntervalGUI, saveInterval);

            if (EditorGUI.EndChangeCheck())
                nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;

            EditorGUILayout.LabelField(nextSaveGUI, ((int)(nextSaveTime - EditorApplication.timeSinceStartup)).ToString());

            // Used to display accurate remaining time before next save.
            Repaint();
        }
        else
        {
            saveInterval = EditorGUILayout.IntField(saveIntervalGUI, saveInterval);
        }
    }

    void Update()
    {
        if (isAutoSave && !EditorApplication.isPlaying && (EditorApplication.timeSinceStartup >= nextSaveTime))
            SaveScene();
	}
    #endregion
}
