using EnhancedEditor;
using UnityEngine;

namespace Nowhere
{
    [CreateAssetMenu(fileName = "PlayerControllerAttributes", menuName = "Datas/Player Controller Attributes", order = 50)]
    public class PlayerControllerAttributes : ScriptableObject
    {
        #region Parameters
        /******************************
         *******     FIELDS     *******
         *****************************/

        [HorizontalLine(1, order = 0), Section("PLAYER CONTROLLER ATTRIBUTES", order = 1), Space(order = 2)]

        [SerializeField]
        [Tooltip("Player horizontal speed acceleration over time")]
        private AnimationCurve      speedCurve =                    null;

        [SerializeField, Range(0, 1)]
        [Tooltip("Coefficient applied to speed acceleration when in air")]
        private float               airSpeedAccelCoef =             .65f;


        [SerializeField, Min(0), HorizontalLine(1)]
        [Tooltip("Deceleration applied to movement when grounded")]
        private float               groundMovementDecel =           75f;

        [SerializeField, Min(0)]
        [Tooltip("Deceleration applied to movement when in air")]
        private float               airMovementDecel =              50f;


        [SerializeField, Min(0), HorizontalLine(1)]
        [Tooltip("Movement transition acceleration when performing an about-turn on ground")]
        private float               groundAboutTurnAccel =          6f;

        [SerializeField, Min(0)]
        [Tooltip("Movement transition acceleration when performing an about-turn in air")]
        private float               airAboutTurnAccel =             5f;


        [HorizontalLine(2, SuperColor.Raspberry)]


        [SerializeField]
        [Tooltip("Player \"Hight Jump\" vertical movement over time")]
        private AnimationCurve      highJumpCurve =                 null;


        [HorizontalLine(2, SuperColor.Sapphire)]


        [SerializeField]
        [Tooltip("Player \"Wall Jump\" vertical movement over time")]
        private AnimationCurve      wallJumpCurve =                 null;

        [SerializeField]
        [Tooltip("Horizontal force applied to the Player when performing a wall jump. " +
        "Must be a negative value")]
        private float               wallJumpHorizontalForce =       -6;

        [SerializeField, Range(0, 1)]
        [Tooltip("Coefficient applied to new gravity when stuck to a wall")]
        private float               wallStuckGravityCoef =          .5f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Coefficient applied to minimum allowed gravity value" +
        "when stuck to a wall")]
        private float               wallStuckMinGravityCoef =       .75f;


        [HorizontalLine(2, SuperColor.Green)]


        [SerializeField]
        [Tooltip("Player horizontal slide movement over time")]
        private AnimationCurve  slideCurve =                        null;


        /******************************
         *****     PROPERTIES     *****
         *****************************/

        /// <summary>
        /// Player horizontal speed acceleration over time.
        /// </summary>
        public AnimationCurve   SpeedCurve                  { get { return speedCurve; } }

        /// <summary>
        /// Player "Hight Jump" vertical movement over time.
        /// </summary>
        public AnimationCurve   HighJumpCurve               { get { return highJumpCurve; } }

        /// <summary>
        /// Player "Wall Jump" vertical movement over time.
        /// </summary>
        public AnimationCurve   WallJumpCurve               { get { return wallJumpCurve; } }

        /// <summary>
        /// Player horizontal slide movement over time.
        /// </summary>
        public AnimationCurve   SlideCurve                  { get { return slideCurve; } }


        /// <summary>
        /// Coefficient applied to speed acceleration when in air.
        /// </summary>
        public float            AirSpeedAccCoef             { get { return airSpeedAccelCoef; } }


        /// <summary>
        /// Deceleration applied to movement when grounded.
        /// </summary>
        public float            GroundMovementDecel         { get { return groundMovementDecel; } }

        /// <summary>
        /// Deceleration applied to movement when in air.
        /// </summary>
        public float            AirMovementDecel            { get { return airMovementDecel; } }


        /// <summary>
        /// Movement transition acceleration when performing an about-turn on ground.
        /// </summary>
        public float            GroundAboutTurnAccel        { get { return groundAboutTurnAccel; } }

        /// <summary>
        /// Movement transition acceleration when performing an about-turn in air.
        /// </summary>
        public float            AirAboutTurnAccel           { get { return airAboutTurnAccel; } }


        /// <summary>
        /// Horizontal force applied to the Player when performing a wall jump.
        /// Must be a negative value.
        /// </summary>
        public float            WallJumpHorizontalForce     { get { return wallJumpHorizontalForce; } }

        /// <summary>
        /// Coefficient applied to new gravity when stuck to a wall.
        /// </summary>
        public float            WallStuckGravityCoef        { get { return wallStuckGravityCoef; } }

        /// <summary>
        /// Coefficient applied to minimum allowed gravity value when stuck to a wall.
        /// </summary>
        public float            WallStuckMinGravityCoef     { get { return wallStuckMinGravityCoef; } }
        #endregion
    }
}
