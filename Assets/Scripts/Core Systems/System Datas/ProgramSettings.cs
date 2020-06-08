// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
//  All variables contained here are accessible for design tests and iterations,
//  and should be remplaced by constants once fixed values have been assigned.
//
// ========================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace Nowhere
{
    [CreateAssetMenu(fileName = "ProgramSettings", menuName = "Datas/Program Settings", order = 50)]
    public class ProgramSettings : ScriptableObject
    {
        #region Physics
        /***********************************
         *********     PHYSICS     *********
         **********************************/

        [HorizontalLine(1, order = 0), Section("PHYSICS SETTINGS", order = 1), Space(order = 2)]

        [SerializeField, Max(0)]
        [Tooltip("Maximum allowed gravity value")]
        private float           maxGravity =                                -25;

        /// <summary>
        /// Maximum allowed gravity value.
        /// </summary>
        public static float     MaxGravity                                  { get { return GameManager.Instance.ProgramSettings.maxGravity; } }


        [SerializeField, Min(.01f)]
        [Tooltip("Minimum Y normal value of a surface to be considered as ground")]
        private float           groundMinYNormal =                          .01f;

        /// <summary>
        /// Minimum Y normal value of a surface to be considered as ground.
        /// </summary>
        public static float     GroundMinYNormal                            { get { return GameManager.Instance.ProgramSettings.groundMinYNormal; } }


        [SerializeField, Min(0)]
        [Tooltip("Maximum height of vertical surfaces objects can climb")]
        private float           groundClimbHeight =                         .7f;

        /// <summary>
        /// Maximum height of vertical surfaces objects can climb.
        /// </summary>
        public static float     GroundClimbHeight                           { get { return GameManager.Instance.ProgramSettings.groundClimbHeight; } }


        [SerializeField, Min(0)]
        [Tooltip("Maximum height at which objects can be snapped to ground")]
        private float           groundSnapHeight =                          .2f;

        /// <summary>
        /// Maximum height at which objects can be snapped to ground.
        /// </summary>
        public static float     GroundSnapHeight                            { get { return GameManager.Instance.ProgramSettings.groundSnapHeight; } }


        [SerializeField, Range(0, 1)]
        [Tooltip("Multiplier applied to object horizontal force when they get grounded")]
        private float           onGroundedHorizontalForceMultiplier =       .55f;

        /// <summary>
        /// Multiplier applied to object horizontal force when they get grounded.
        /// </summary>
        public static float     OnGroundedHorizontalForceMultiplier         { get { return GameManager.Instance.ProgramSettings.onGroundedHorizontalForceMultiplier; } }



        [SerializeField, Min(0), HorizontalLine(1)]
        [Tooltip("Deceleration applied to object force when on ground")]
        private float           groundDecelerationForce =                   12.5f;

        /// <summary>
        /// Deceleration applied to object force when on ground.
        /// </summary>
        public static float     GroundDecelerationForce                     { get { return GameManager.Instance.ProgramSettings.groundDecelerationForce; } }


        [SerializeField, Min(0)]
        [Tooltip("Deceleration applied to object force when in air")]
        private float           airDecelerationForce =                      5f;

        /// <summary>
        /// Deceleration applied to object force when in air.
        /// </summary>
        public static float     AirDecelerationForce                        { get { return GameManager.Instance.ProgramSettings.airDecelerationForce; } }



        [SerializeField, Min(1), HorizontalLine(1)]
        [Tooltip("Multiplier applied to horizontal force of an object when its movement is opposite and deceleration superior")]
        private float           counterMovementSuperiorMultiplier =         1.5f;

        /// <summary>
        /// Multiplier applied to horizontal force of an object when its movement is opposite and deceleration superior.
        /// </summary>
        public static float     CounterMovementSuperiorMultiplier           { get { return GameManager.Instance.ProgramSettings.counterMovementSuperiorMultiplier; } }


        [SerializeField, Min(1)]
        [Tooltip("Multiplier applied to horizontal force of an object when its movement is opposite and deceleration half superior")]
        private float           counterMovementHalfMultiplier =             1.25f;

        /// <summary>
        /// Multiplier applied to horizontal force of an object when its movement is opposite and deceleration half superior.
        /// </summary>
        public static float     CounterMovementHalfMultiplier               { get { return GameManager.Instance.ProgramSettings.counterMovementHalfMultiplier; } }


        [SerializeField, Min(1)]
        [Tooltip("Default multiplier applied to horizontal force of an object when its movement is opposite")]
        private float           counterMovementDefaultMultiplier =          1.1f;

        /// <summary>
        /// Default multiplier applied to horizontal force of an object when its movement is opposite.
        /// </summary>
        public static float     CounterMovementDefaultMultiplier            { get { return GameManager.Instance.ProgramSettings.counterMovementDefaultMultiplier; } }
        #endregion
    }
}
