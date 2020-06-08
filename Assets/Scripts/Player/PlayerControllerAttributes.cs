// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace Nowhere
{
    [CreateAssetMenu(fileName = "PlayerControllerAttributes", menuName = "Datas/Player Controller Attributes", order = 50)]
    public class PlayerControllerAttributes : ScriptableObject
    {
        /* Player Controller Attributes
         * 
         * All variables contained here are for accessible for design tests and iterations,
         * and the maximum of them should be remplaced by constants once fixed values have been assigned.
        */

        #region Fields / Properties
        /**********************************
         *******     PARAMETERS     *******
         *********************************/

        [HorizontalLine(1, order = 0), Section("PLAYER CONTROLLER ATTRIBUTES", order = 1), Space(order = 2)]

        [SerializeField]
        [Tooltip("Player horizontal speed acceleration over time")]
        private AnimationCurve      speedCurve =                    null;

        /// <summary>
        /// Player horizontal speed acceleration over time.
        /// </summary>
        public AnimationCurve       SpeedCurve                      { get { return speedCurve; } }


        [SerializeField, Range(0, 1)]
        [Tooltip("Coefficient applied to speed acceleration when in air")]
        private float               airSpeedAccelCoef =             .65f;

        /// <summary>
        /// Coefficient applied to speed acceleration when in air.
        /// </summary>
        public float                AirSpeedAccCoef                 { get { return airSpeedAccelCoef; } }



        [SerializeField, Min(0), HorizontalLine(1)]
        [Tooltip("Deceleration applied to movement when grounded")]
        private float               groundMovementDecel =           75f;

        /// <summary>
        /// Deceleration applied to movement when grounded.
        /// </summary>
        public float                GroundMovementDecel             { get { return groundMovementDecel; } }


        [SerializeField, Min(0)]
        [Tooltip("Deceleration applied to movement when in air")]
        private float               airMovementDecel =              50f;

        /// <summary>
        /// Deceleration applied to movement when in air.
        /// </summary>
        public float                AirMovementDecel                { get { return airMovementDecel; } }



        [SerializeField, Min(0), HorizontalLine(1)]
        [Tooltip("Movement transition acceleration when performing an about-turn on ground")]
        private float               groundAboutTurnAccel =          6f;

        /// <summary>
        /// Movement transition acceleration when performing an about-turn on ground.
        /// </summary>
        public float                GroundAboutTurnAccel            { get { return groundAboutTurnAccel; } }


        [SerializeField, Min(0)]
        [Tooltip("Movement transition acceleration when performing an about-turn in air")]
        private float               airAboutTurnAccel =             5f;

        /// <summary>
        /// Movement transition acceleration when performing an about-turn in air.
        /// </summary>
        public float                AirAboutTurnAccel               { get { return airAboutTurnAccel; } }



        [SerializeField, Range(0, 1), HorizontalLine(1)]
        [Tooltip("Maximum height at which player get snap when on ground and going down (used for slopes, for exemple)")]
        private float               groundSnapHeight =              .25f;

        /// <summary>
        /// Maximum height at which player get snap when on ground and going down (used for slopes, for exemple).
        /// </summary>
        public float                GroundSnapHeight                { get { return groundSnapHeight; } }


        [HorizontalLine(2, SuperColor.Raspberry)]


        [SerializeField]
        [Tooltip("Player \"Hight Jump\" vertical movement over time")]
        private AnimationCurve      highJumpCurve =                 null;

        /// <summary>
        /// Player "Hight Jump" vertical movement over time.
        /// </summary>
        public AnimationCurve       HighJumpCurve                   { get { return highJumpCurve; } }


        [SerializeField, Min(0)]
        [Tooltip("Time (in seconds) during which the player is still able to jump after leaving a platform")]
        private float               coyoteTime =                    .25f;

        /// <summary>
        /// Time (in seconds) during which the player is still able to jump after leaving a platform.
        /// </summary>
        public float                CoyoteTime                      { get { return coyoteTime; } }


        [SerializeField, Min(0)]
        [Tooltip("Maximum difference between player vertical force and movement to consider jump is at its peak")]
        private float               jumpPeekDifference =            7f;

        /// <summary>
        /// Maximum difference between player vertical force and movement to consider jump is at its peak.
        /// </summary>
        public float                JumpPeekDifference              { get { return jumpPeekDifference; } }


        [HorizontalLine(2, SuperColor.Sapphire)]


        [SerializeField]
        [Tooltip("Player \"Wall Jump\" vertical movement over time")]
        private AnimationCurve      wallJumpCurve =                 null;

        /// <summary>
        /// Player "Wall Jump" vertical movement over time.
        /// </summary>
        public AnimationCurve       WallJumpCurve                   { get { return wallJumpCurve; } }


        [SerializeField, Max(0)]
        [Tooltip("Horizontal force applied to the Player when performing a wall jump. " +
        "Must be a negative value")]
        private float               wallJumpHorizontalForce =       -6;

        /// <summary>
        /// Horizontal force applied to the Player when performing a wall jump.
        /// Must be a negative value.
        /// </summary>
        public float                WallJumpHorizontalForce         { get { return wallJumpHorizontalForce; } }


        [SerializeField, Min(0)]
        [Tooltip("Maximum distance between player and wall to perform wall jumps")]
        private float               wallJumpGap =                   .25f;

        /// <summary>
        /// Maximum distance between player and wall to perform wall jumps.
        /// </summary>
        public float                WallJumpGap                     { get { return wallJumpGap; } }


        [SerializeField, Range(0, 1)]
        [Tooltip("Coefficient applied to new gravity when stuck to a wall")]
        private float               wallStuckGravityCoef =          .5f;

        /// <summary>
        /// Coefficient applied to new gravity when stuck to a wall.
        /// </summary>
        public float                WallStuckGravityCoef            { get { return wallStuckGravityCoef; } }


        [SerializeField, Range(0, 1)]
        [Tooltip("Coefficient applied to minimum allowed gravity value" +
        "when stuck to a wall")]
        private float               wallStuckMinGravityCoef =       .75f;

        /// <summary>
        /// Coefficient applied to minimum allowed gravity value when stuck to a wall.
        /// </summary>
        public float                WallStuckMinGravityCoef         { get { return wallStuckMinGravityCoef; } }


        [HorizontalLine(2, SuperColor.Green)]


        [SerializeField]
        [Tooltip("Player horizontal slide movement over time")]
        private AnimationCurve  slideCurve =                        null;

        /// <summary>
        /// Player horizontal slide movement over time.
        /// </summary>
        public AnimationCurve SlideCurve { get { return slideCurve; } }
        #endregion
    }
}
