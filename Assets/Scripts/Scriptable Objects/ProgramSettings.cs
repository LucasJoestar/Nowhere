using EnhancedEditor;
using UnityEngine;

namespace Nowhere
{
    [CreateAssetMenu(fileName = "ProgramSettings", menuName = "Datas/Program Settings", order = 50)]
    public class ProgramSettings : ScriptableObject
    {
        #region Physics
        /**********************************
         *********     FIELDS     *********
         *********************************/

        [HorizontalLine(1, order = 0), Section("PROGRAM SETTINGS", order = 1), Space(order = 2)]

        [SerializeField, Range(0, 1)]
        [Tooltip("Multiplier applied to object horizontal force when they get grounded")]
        private float       onGroundedHorizontalForceMultiplier =       .55f;

        [SerializeField]
        [Tooltip("Minimum allowed gravity value")]
        private float       minGravity =                                -25;


        [SerializeField, Min(0), HorizontalLine(1)]
        [Tooltip("Deceleration applied to object force when on ground")]
        private float       groundDecelerationForce =                   12.5f;

        [SerializeField, Min(0)]
        [Tooltip("Deceleration applied to object force when in air")]
        private float       airDecelerationForce =                      5f;


        /**********************************
         *******     PROPERTIES     *******
         *********************************/

        /// <summary>
        /// Multiplier applied to object horizontal force when they get grounded.
        /// </summary>
        public float        OnGroundedHorizontalForceMultiplier                  { get { return onGroundedHorizontalForceMultiplier; } }

        /// <summary>
        /// Minimum allowed gravity value.
        /// </summary>
        public float        MinGravity                                  { get { return minGravity; } }


        /// <summary>
        /// Deceleration applied to object force when on ground.
        /// </summary>
        public float        GroundDecelerationForce                     { get { return groundDecelerationForce; } }

        /// <summary>
        /// Deceleration applied to object force when in air.
        /// </summary>
        public float        AirDecelerationForce                        { get { return airDecelerationForce; } }
        #endregion
    }
}
