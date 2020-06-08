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
        /***************************************
         *****     FIELDS / PROPERTIES     *****
         **************************************/

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static GameManager   Instance            { get; private set; }

        // ------------------------------

        [SerializeField, Required]
        private ProgramSettings     programSettings =   null;

        /// <summary>
        /// Game global program settings.
        /// </summary>
        public ProgramSettings      ProgramSettings
        {
            get { return programSettings; }
        }
        #endregion

        #region Methods
        /*********************************
         *****     MONOBEHAVIOUR     *****
         ********************************/

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
