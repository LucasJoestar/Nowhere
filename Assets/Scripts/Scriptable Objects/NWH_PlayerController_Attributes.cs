using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControllerAttributes", menuName = "Datas/Player Controller Attributes", order = 50)]
public class NWH_PlayerController_Attributes : ScriptableObject
{
    #region Animation Curves
    /**********************************
     ******     ANIM. CURVES     ******
     *********************************/


    /// <summary>
    /// Jump Y axis movement over time.
    /// </summary>
    public AnimationCurve   JumpCurve =             null;

    /// <summary>
    /// Slide X axis movement over time.
    /// </summary>
    public AnimationCurve   SlideCurve =            null;

    /// <summary>
    /// X axis movement speed over time.
    /// </summary>
    public AnimationCurve   SpeedCurve =            null;

    /// <summary>
    /// Wall jump Y axis movement over time.
    /// </summary>
    public AnimationCurve   WallJumpCurve =         null;
    #endregion

    #region Parameters
    /**********************************
     *******     PARAMETERS     *******
     *********************************/


    /// <summary>
    /// Velocity Y minimum value when falling.
    /// </summary>
    public float            FallMinVelocity =       -10;

    /// <summary>
    /// X velocity applied to the player when performing a wall jump.
    /// Must be a negative value.
    /// </summary>
    public float            WallJumpXVelocity =     -5;

    /// <summary>
    /// Coefficient applied to the player Y velocity when falling and stuck to a wall.
    /// Should be between 0 and 1.
    /// </summary>
    public float            WallStuckFallCoef =     .33f;
    #endregion
}
