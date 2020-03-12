using System;
using UnityEngine;

namespace Nowhere
{
    [Serializable]
    public class Velocity
    {
        #region Fields / Properties
        /**********************************
         *********     FIELDS     *********
         *********************************/

        /// <summary>
        /// Velocity external forces.
        /// Force should be decreased over time to slowly reduce velocity,
        /// like wind or hit recoil.
        /// </summary>
        public Vector2      Force =             Vector2.zero;

        /// <summary>
        /// Velocity instant forces.
        /// Should be applied on a single frame and reset right after
        /// velocity has been applied, unlike <see cref="Force"/>.
        /// </summary>
        public Vector2      InstantForce =      Vector2.zero;

        /// <summary>
        /// Velocity owner movement.
        /// Movement applied by the object itself, like the walking of a character.
        /// </summary>
        public Vector2      Movement =          Vector2.zero;
        #endregion

        #region Constructors
        /********************************
         *****     CONSTRUCTORS     *****
         *******************************/

        /// <summary>
        /// Creates a new empty velocity object.
        /// </summary>
        public Velocity() { }

        /// <summary>
        /// Creates a new velocity object.
        /// </summary>
        /// <param name="_force">Force (applied over time).</param>
        /// <param name="_instantForce">Instant force (applied on single frame).</param>
        /// <param name="_movement">Movement (applied by the object itself).</param>
        public Velocity(Vector2 _force, Vector2 _instantForce, Vector2 _movement)
        {
            Force = _force;
            InstantForce = _instantForce;
            Movement = _movement;
        }
        #endregion

        #region Methods
        /*********************************
         ********     METHODS     ********
         ********************************/

        /// <summary>
        /// Get velocity from all forces and movements combined.
        /// </summary>
        /// <returns>Returns full class velocity.</returns>
        public Vector2 GetVelocity() => ((Movement + Force) * Time.deltaTime) + InstantForce;
        #endregion
    }

}
