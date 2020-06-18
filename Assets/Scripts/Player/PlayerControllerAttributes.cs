// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
//      All variables contained here are for accessible for design tests and iterations,
//      and the maximum of them should be remplaced by constants once fixed values have been assigned.
//
// ========================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace Nowhere
{
    [CreateAssetMenu(fileName = "PlayerControllerAttributes", menuName = "Datas/Player Controller Attributes", order = 50)]
    public class PlayerControllerAttributes : ScriptableObject
    {
        #region Attributes
        [HorizontalLine(1, order = 0), Section("PLAYER CONTROLLER ATTRIBUTES", order = 1), Space(order = 2)]

        public AnimationCurve SpeedCurve = null;
        [Range(0, 1)] public float AirSpeedAccelCoef = .65f;

        [HorizontalLine(1)]
        [Min(0)] public float GroundMovementDecel = 75f;
        [Min(0)] public float AirMovementDecel = 50f;

        [HorizontalLine(1)]

        [Min(0)] public float GroundAboutTurnAccel = 6f;
        [Min(0)] public float AirAboutTurnAccel = 5f;

        [HorizontalLine(2, SuperColor.Raspberry)]

        public AnimationCurve HighJumpCurve = null;
        [Min(0)] public float CoyoteTime = .25f;
        [Min(0)] public float JumpBufferTime = .15f;
        [Min(0)] public float JumpPeekDifference = 7f;

        [HorizontalLine(2, SuperColor.Sapphire)]

        public AnimationCurve WallJumpCurve = null;
        [Max(0)] public float WallJumpHorizontalForce = -6;
        [Min(0)] public float WallJumpGap = .25f;

        [Min(0)] public float WallStuckTakeOffInertia = 5f;
        [Range(0, 1)] public float WallStuckGravityCoef = .5f;
        [Range(0, 1)] public float WallStuckMinGravityCoef = .75f;

        [HorizontalLine(2, SuperColor.Green)]

        [Min(0)] public float SlideDuration =           .75f;
        [Min(0)] public float SlideMaxMomentumCoef =    1.25f;
        [Min(0)] public float SlideMinMomentumCoef =    .85f;
        [Min(1)] public float SlideOnLandCoef =         1.1f;
        [Min(0)] public float SlideForceCoef =          .5f;
        [Min(0)] public float SlideRequiredVelocity =   10f;
        [Min(0)] public float SlideBufferTime =         .15f;

        [HorizontalLine(1)]

        [Min(0)] public float CrouchSpeedCoef = .75f;

        [HorizontalLine(2, SuperColor.Chocolate)]

        public Bounds colliderBounds = new Bounds();
        #endregion
    }
}
