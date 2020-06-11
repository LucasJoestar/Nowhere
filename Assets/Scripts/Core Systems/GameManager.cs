﻿// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
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

            while (sleepTimer > 0)
            {
                yield return null;
                sleepTimer -= Time.unscaledDeltaTime;
            }

            Time.timeScale = 1;
            sleepCoroutine = null;
        }

        private IEnumerator Chronos()
        {
            while(true)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0))
                    Time.timeScale = 0;

                if (Input.GetKeyDown(KeyCode.Alpha9))
                    Time.timeScale = 1;

                if (Input.GetKeyDown(KeyCode.Alpha8))
                    Time.timeScale = .5f;

                if (Input.GetKeyDown(KeyCode.Alpha7))
                    Time.timeScale = .25f;

                yield return null;
            }
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

            StartCoroutine(Chronos());
        }

        private void OnApplicationQuit()
        {
            IsQuittingApplication = true;
        }
        #endregion

        #endregion
    }
}
