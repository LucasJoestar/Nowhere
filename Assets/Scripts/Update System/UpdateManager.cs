using System;
using UnityEngine;

namespace Nowhere
{
    public class UpdateManager : MonoBehaviour
    {
        #region Events
        /**********************************
         *********     EVENTS     *********
         *********************************/

        /// <summary>
        /// Called every frame for frames-related update modes.
        /// </summary>
        private event Action OnFrameUpdate;

        /// <summary>
        /// Called every frame with <see cref="Time.deltaTime"/> as parameter
        /// for seconds-related update modes.
        /// </summary>
        private event Action<float> OnTimeUpdate;
        #endregion

        #region Singleton
        /*********************************
         *******     SINGLETON     *******
         ********************************/

        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        public static UpdateManager Instance { get; private set; } = null;
        #endregion

        #region Memory
        /**********************************
         *********     MEMORY     *********
         *********************************/

        /// <summary>
        /// Cache reference for update modes in <see cref="GameManager.updateOrder"/>.
        /// </summary>
        private UpdateMode[] updateModes = null;
        #endregion

        #region Methods

        #region Original Methods
        /*********************************************
         *****     RUNTIME INSTANCE CREATION     *****
         ********************************************/

        /// <summary>
        /// Create an instance of this class and set it as singleton.
        /// Should be called on game load by GameManager only.
        /// </summary>
        public static void CreateInstance(UpdateMode[] _updateModes, GameObject _root)
        {
            // Add an UpdateSystem component to a root GameObject,
            // and set it as singleton
            if (Instance) return;

            Instance = (UpdateManager)_root.AddComponent(typeof(UpdateManager));

            // Set singleton cache reference for update modes
            Instance.updateModes = _updateModes;

            // In the original array order,
            // subscribe each update mode IncreaseTimer method to the correct event
            for (int _i = 0; _i < _updateModes.Length; _i++)
            {
                if (_updateModes[_i].IsFrameInterval) Instance.OnFrameUpdate += _updateModes[_i].IncreaseTimer;
                else Instance.OnTimeUpdate += _updateModes[_i].IncreaseTimer;
            }
        }


        /****************************************
         ******     UPDATE SUBSCRIBERS     ******
         ***************************************/

        /// <summary>
        /// Suscribes a delegate to an indicated timeline for update.
        /// </summary>
        /// <param name="_delegate">Delegate to subscribe.</param>
        /// <param name="_timeline">Timeline to subscribe to for update.</param>
        public static void SubscribeToUpdate(Action _delegate, UpdateModeTimeline _timeline)
        {
            UpdateMode _updateMode = FindUpdateMode(_timeline);
            if (_updateMode == null) return;

            _updateMode.OnUpdate += _delegate;

        }

        /// <summary>
        /// Unsuscribes a delegate from an indicated timeline for update.
        /// </summary>
        /// <param name="_delegate">Delegate to unsubscribe.</param>
        /// <param name="_timeline">Timeline to unsubscribe from.</param>
        public static void UnsubscribeToUpdate(Action _delegate, UpdateModeTimeline _timeline)
        {
            UpdateMode _updateMode = FindUpdateMode(_timeline);
            if (_updateMode == null) return;

            _updateMode.OnUpdate -= _delegate;
        }


        /// <summary>
        /// Retrieves the update mode associated with the indicated timeline.
        /// </summary>
        /// <param name="_timeline">Timeline to find update mode associated with.</param>
        /// <returns>Returns the update mode associated with the timeline, null if not found.</returns>
        private static UpdateMode FindUpdateMode(UpdateModeTimeline _timeline)
        {
            // If no instance, debug it return
            if (!Instance)
            {
#if UNITY_EDITOR
                //Debug.LogError("Subscription Exception ! No UpdateManager found in scene !");
#endif
                return null;
            }

            // Subscribe delegate to the indicated timeline
            for (int _i = 0; _i < Instance.updateModes.Length; _i++)
            {
                if (Instance.updateModes[_i].Timeline == _timeline) return Instance.updateModes[_i];
            }

            // If indicated timeline couldn't be found, debug it
            Debug.LogError($"Subscription Exception ! No Timeline for \"{_timeline}\" could be found !");
            return null;
        }
        #endregion

        #region Unity Methods
        /*********************************
         *****     MONOBEHAVIOUR     *****
         ********************************/

        // Start is called before the first frame update
        private void Start()
        {
            // Destroy object if not singleton
            if (Instance != this) Destroy(this);
        }

        // Update is called once per frame
        private void Update()
        {
            // Update all update modes by calling events
            OnFrameUpdate?.Invoke();
            OnTimeUpdate?.Invoke(Time.deltaTime);
        }
        #endregion

        #endregion
    }
}
