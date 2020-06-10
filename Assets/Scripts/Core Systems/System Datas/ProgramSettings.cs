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
        /// <summary>
        /// Short way to write <see cref="GameManager.Instance"/>.ProgramSettings.
        /// </summary>
        public static ProgramSettings I { get { return GameManager.Instance.ProgramSettings; } }

        #region Physics
        // -------------------------------------------
        // Physics
        // -------------------------------------------

        [HorizontalLine(1, order = 0), Section("PHYSICS SETTINGS", order = 1), Space(order = 2)]

        [Max(0)] public float MaxGravity =          -25;
        [Min(0)] public float GroundClimbHeight =   .7f;
        [Min(0)] public float GroundSnapHeight =    .2f;

        [Min(.01f)] public float GroundMinYNormal = .01f;
        [Range(0, 1)] public float OnGroundedHorizontalForceMultiplier = .55f;

        [HorizontalLine(1)]

        [Min(0)] public float GroundDecelerationForce = 12.5f;
        [Min(0)] public float AirDecelerationForce =    5f;
        #endregion
    }
}
