using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

public class NWH_GameManager : ScriptableObject
{
    #region Constants
    /*********************************
     *******     CONSTANTS     *******
     ********************************/

    private const string FILE_PATH = "GameManager";
    #endregion

    #region Fields / Properties
    /********************************
     ********     FIELDS     ********
     *******************************/

    /// <summary>Backing field for <see cref="Settings"/>.</summary>
    [SerializeField]
    private NWH_Settings        settings =      null;

    /// <summary>
    /// Game update mode order
    /// </summary>
    [SerializeField]
    private NWH_UpdateOrder     updateOrder =   new NWH_UpdateOrder();


    /********************************
     ******     PROPERTIES     ******
     *******************************/

    /// <summary>
    /// Settings scriptable object of the game.
    /// </summary>
    public NWH_Settings Settings { get { return settings; } }
    #endregion

    #region Singleton
    /*********************************
     *******     SINGLETON     *******
     ********************************/

    /// <summary>
    /// Singleton instance of this class.
    /// </summary>
    public static NWH_GameManager   I =     null;
    #endregion

    #region Methods
    /**********************************************
     *****     RUNTIME INITIALIZE ON LOAD     *****
     *********************************************/

    /// <summary>
    /// Loads the Game Manager before the first scene is being loaded.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadInstance()
    {
        I = Resources.Load<NWH_GameManager>(FILE_PATH);

        if (!I) Debug.LogError($"Error ! No Game Manager found on the project. Please place it in a Resources folder at path {FILE_PATH}.asset");
    }

    /// <summary>
    /// Initialize what needs to be after first scene loading.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeGame()
    {
        // Create game update system
        NWH_UpdateSystem.CreateInstance(I.updateOrder.UpdateModes);
    }


    /**********************************
     *********     EDITOR     *********
     *********************************/

    #if UNITY_EDITOR
    /// <summary>
    /// Creates an instance of a GameManager if none is found
    /// on the project database, or get the one existing.
    /// </summary>
    [MenuItem("Nowhere/Create Game Manager")]
    public static void CreateInstance()
    {
        Object _gameManager = null;
        string[] _gameManagers = AssetDatabase.FindAssets($"t:NWH_GameManager");

        // Get first GameManager if existing...
        if (_gameManagers.Length > 0)
        {
            _gameManager = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_gameManagers[0]), typeof(NWH_GameManager));
        }
        // ... Or create one if not
        else
        {
            string _path = string.Empty;
            Object[] _selection = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

            if (_selection.Length > 0)
            {
                _path = AssetDatabase.GetAssetPath(_selection[0]).Replace("Assets/", string.Empty);

                if (Path.GetExtension(_path) != string.Empty) _path = Path.GetDirectoryName(_path);
            }
            _path = Path.Combine(_path, "Resources", Path.GetDirectoryName(FILE_PATH));

            if (!Directory.Exists(Path.Combine(Application.dataPath, _path))) Directory.CreateDirectory(Path.Combine(Application.dataPath, _path));

            _path = $"{Path.Combine("Assets", _path, Path.GetFileName(FILE_PATH))}.asset";

            AssetDatabase.CreateAsset(CreateInstance<NWH_GameManager>(), _path);
            _gameManager = AssetDatabase.LoadAssetAtPath(_path, typeof(NWH_GameManager));
        }
        
        // Select the GameManager in the project window
        EditorGUIUtility.PingObject(_gameManager);
        Selection.objects = new Object[] { _gameManager };
    }
    #endif
    #endregion
}
