using EnhancedEditor;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Nowhere
{
    public class GameManager : ScriptableObject
    {
        #region Constants
        /*********************************
         *******     CONSTANTS     *******
         ********************************/

        private const string            FILE_PATH =             "GameManager";
        #endregion

        #region Singleton
        /*********************************
         *******     SINGLETON     *******
         ********************************/

        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        public static GameManager       Instance                { get; private set; } = null;
        #endregion

        #region Fields / Properties
        /********************************
         ********     FIELDS     ********
         *******************************/

        /// <summary>Backing field for <see cref="ProgramSettings"/>.</summary>
        [SerializeField, Required]
        private ProgramSettings         programSettings =       null;

        /// <summary>
        /// Update system used for the game.
        /// </summary>
        [SerializeField, Required]
        private UpdateSystem            updateOrder =           null;


        /********************************
         ******     PROPERTIES     ******
         *******************************/

        /// <summary>
        /// Program settings scriptable object of the game.
        /// </summary>
        public static ProgramSettings   ProgramSettings         { get { return Instance?.programSettings; } }
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
            Instance = Resources.Load<GameManager>(FILE_PATH);
            if (!Instance) Debug.LogError($"Error ! No Game Manager found on the project. Please place it in a Resources folder at path \"{FILE_PATH}.asset\"");
        }

        /// <summary>
        /// Initialize what needs to be after first scene loading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeGame()
        {
            // Creates a new game object for game logic
            GameObject _gameLogic = new GameObject("[GAME LOGIC]");
            DontDestroyOnLoad(_gameLogic);

            // Create game update system
            UpdateManager.CreateInstance(Instance.updateOrder.UpdateModes, _gameLogic);
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
                _gameManager = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_gameManagers[0]), typeof(GameManager));
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

                AssetDatabase.CreateAsset(CreateInstance<GameManager>(), _path);
                _gameManager = AssetDatabase.LoadAssetAtPath(_path, typeof(GameManager));
            }

            // Select the GameManager in the project window
            EditorGUIUtility.PingObject(_gameManager);
            Selection.objects = new Object[] { _gameManager };
        }
#endif
        #endregion
    }

}
