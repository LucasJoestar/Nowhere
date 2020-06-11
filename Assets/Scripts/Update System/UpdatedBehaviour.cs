// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using UnityEngine;

namespace Nowhere
{
    public class UpdatedBehaviour : MonoBehaviour 
    {
        #region Methods
        /// <summary>
        /// Called when object is disabled but the application is not quitting.
        /// Use this to unregister methods from Update Manager.
        /// </summary>
        protected virtual void OnDisableCallback() { }

        protected virtual void OnDisable()
        {
            if (!GameManager.Instance.IsQuittingApplication)
                OnDisableCallback();
        }
        #endregion
    }
}
