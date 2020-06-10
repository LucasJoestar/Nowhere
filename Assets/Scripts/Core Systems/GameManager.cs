// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
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
        #endregion

        #region Methods
        private void Awake()
        {
            // Singleton set.
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }
        #endregion
    }
}
