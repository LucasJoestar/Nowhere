using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NWH_GameManager : ScriptableObject
{
    #region Fields / Properties
    /// <summary>
    /// Settings scriptable object of the game.
    /// </summary>
    public NWH_Settings     Settings =  null;
    #endregion

    #region Methods

    #region Original Methods
    /**********************************
     *********     EDITOR     *********
     *********************************/


    #if UNITY_EDITOR
    [MenuItem("Nowhere/Create Game Manager")]
    /// <summary>
    /// Creates an instance of a GameManager if none is found on the project database or get the one existing.
    /// </summary>
    public static void CreateInstance()
    {
        Object _gameManager = null;

        string[] _gameManagers = AssetDatabase.FindAssets($"t:NWH_GameManager");
        if (_gameManagers.Length > 0)
        {
            _gameManager = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_gameManagers[0]), typeof(NWH_GameManager));
        }
        else
        {
            string _path = "Assets";
            Object[] _selection = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            if (_selection.Length > 0) _path = AssetDatabase.GetAssetPath(_selection[0]);
            _path += "/GameManager.asset";

            AssetDatabase.CreateAsset(CreateInstance<NWH_GameManager>(), _path);
            _gameManager = AssetDatabase.LoadAssetAtPath(_path, typeof(NWH_GameManager));
        }
        
        EditorGUIUtility.PingObject(_gameManager);
        Selection.objects = new Object[] { _gameManager };
    }
    #endif
    #endregion

    #region Unity Methods

    #endregion

    #endregion
}
