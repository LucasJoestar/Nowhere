// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using System.Collections;
using UnityEngine;

namespace Nowhere
{
    public class GameManager : MonoBehaviour
    {
        #region Fields / Properties
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static GameManager Instance { get; private set; }

        // -----------------------

        [SerializeField, Required] private ProgramSettings programSettings = null;

        /// <summary>
        /// Global Program Settings (can be accessed through <see cref="ProgramSettings.I"/>).
        /// </summary>
        public ProgramSettings ProgramSettings { get { return programSettings; } }

        // -----------------------

        [SerializeField, ReadOnly] private bool isGamePaused = false;
        public bool IsGamePaused { get { return isGamePaused; } }

        // -----------------------

        public bool IsQuittingApplication { get; private set; } = false;
        #endregion

        #region Methods

        #region Time
        private Coroutine sleepCoroutine = null;
        private float sleepTimer = 0;

        // -----------------------

        /// <summary>
        /// Make the application "sleep" (set time scale to zero) for a certain duration (cumulative).
        /// </summary>
        public void Sleep(float _duration)
        {
            sleepTimer += _duration;

            if (sleepCoroutine == null)
                sleepCoroutine = StartCoroutine(DoSleep());
        }

        private IEnumerator DoSleep()
        {
            Time.timeScale = 0;
            isGamePaused = true;

            while (sleepTimer > 0)
            {
                yield return null;
                sleepTimer -= Time.unscaledDeltaTime;
            }

            isGamePaused = false;
            Time.timeScale = 1;
            sleepCoroutine = null;
        }
        #endregion

        #region Monobehaviour
        private void Awake()
        {
            // Singleton set.
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            #if !UNITY_EDITOR
            UnityEngine.SceneManagement.SceneManager.LoadScene(1, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            #endif
        }

        private void OnApplicationQuit()
        {
            IsQuittingApplication = true;
        }
        #endregion

        #endregion
    }
}
