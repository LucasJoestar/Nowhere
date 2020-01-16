using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Datas/Settings", order = 50)]
public class NWH_Settings : ScriptableObject
{
    #region Mechanics
    // Everything about game mechanics variables will be stocked here

    /// <summary>
    /// Degree of precision used when performing raycast.
    /// </summary>
    public float            RaycastPrecision =  .25f;
    #endregion

    #region Image & Video
    // Everything about image & video settings will be stocked here

    /// <summary>
    /// Full screen mode used to display the game.
    /// </summary>
    public FullScreenMode   FullScreenMode =    FullScreenMode.FullScreenWindow;

    /// <summary>
    /// Resolution used to display the game.
    /// </summary>
    public Resolution       Resolution;
    #endregion

    #region Sound
    // Everything about sound settings will be stocked here
    #endregion

    #region Mapping
    // Everything about keyboard & controller will be stocked here
    #endregion
}
