// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using System.Collections.Generic;
using UnityEngine;

namespace Nowhere
{
    #region Update Interfaces
    // -------------------------------------------
    // Update Interfaces
    // -------------------------------------------

    public interface IUpdate        { void Update(); }
    public interface ICameraUpdate  { void Update(); }
    public interface IInputUpdate   { void Update(); }
    public interface IMovableUpdate { void Update(); }
    public interface IPhysicsUpdate { void Update(); }
    #endregion

    public class UpdateManager : MonoBehaviour
    {
        #region Fields
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static UpdateManager Instance { get; private set; }

        // -----------------------

        private List<IUpdate> updates = new List<IUpdate>();
        private List<ICameraUpdate> cameraUpdates = new List<ICameraUpdate>();
        private List<IInputUpdate> inputUpdates = new List<IInputUpdate>();
        private List<IMovableUpdate> movableUpdates = new List<IMovableUpdate>();
        private List<IPhysicsUpdate> physicsUpdates = new List<IPhysicsUpdate>();
        #endregion

        #region Methods

        #region Registrations
        /// <summary>
        /// Registers an object on global update.
        /// </summary>
        public void Register(IUpdate _update) => updates.Add(_update);

        /// <summary>
        /// Unregisters an object from global update.
        /// </summary>
        public void Unregister(IUpdate _update) => updates.Remove(_update);

        // -----------------------

        /// <summary>
        /// Registers an object on Camera update.
        /// </summary>
        public void Register(ICameraUpdate _update) => cameraUpdates.Add(_update);

        /// <summary>
        /// Unregisters an object from Camera update.
        /// </summary>
        public void Unregister(ICameraUpdate _update) => cameraUpdates.Remove(_update);

        // -----------------------

        /// <summary>
        /// Registers an object on Input update.
        /// </summary>
        public void Register(IInputUpdate _update) => inputUpdates.Add(_update);

        /// <summary>
        /// Unregisters an object from Input update.
        /// </summary>
        public void Unregister(IInputUpdate _update) => inputUpdates.Remove(_update);

        // -----------------------

        /// <summary>
        /// Register an object on Movable update.
        /// </summary>
        public void Register(IMovableUpdate _update) => movableUpdates.Add(_update);

        /// <summary>
        /// Unregister an object from Movable update.
        /// </summary>
        public void Unregister(IMovableUpdate _update) => movableUpdates.Remove(_update);

        // -----------------------

        /// <summary>
        /// Register an object on Physics update.
        /// </summary>
        public void Register(IPhysicsUpdate _update) => physicsUpdates.Add(_update);

        /// <summary>
        /// Unregister an object from Physics update.
        /// </summary>
        public void Unregister(IPhysicsUpdate _update) => physicsUpdates.Remove(_update);
        #endregion

        #region Monobehaviour
        private void Awake()
        {
            // Singleton set.
            if (!Instance)
                Instance = this;
            else
                Destroy(this);
        }

        private void Update()
        {
            // Call all registered interface updates.
            int _i;
            for (_i = 0; _i < inputUpdates.Count; _i++)
                inputUpdates[_i].Update();

            for (_i = 0; _i < updates.Count; _i++)
                updates[_i].Update();

            for (_i = 0; _i < physicsUpdates.Count; _i++)
                physicsUpdates[_i].Update();

            for (_i = 0; _i < movableUpdates.Count; _i++)
                movableUpdates[_i].Update();

            for (_i = 0; _i < cameraUpdates.Count; _i++)
                cameraUpdates[_i].Update();
        }
        #endregion

        #endregion
    }
}
