// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace Nowhere
{
    [CreateAssetMenu(fileName = "PlayerCameraAttributes", menuName = "Datas/Player Camera Attributes", order = 50)]
    public class PlayerCameraAttributes : ScriptableObject 
    {
        #region Attributes
        [HorizontalLine(1, order = 0), Section("PLAYER CAMERA ATTRIBUTES", order = 1), Space(order = 2)]

        public Bounds TargetBounds = new Bounds();

        [Range(0, 1)] public float Speed = .1f;

        [HorizontalLine(1)]

        [Min(0)] public float ShakeForce =      25;
        [Min(0)] public float ShakeSoftening =  2.25f;
        [Min(0)] public int ShakeTraumaPower =  2;

        public float ShakeMaxAngle = 5;
        public Vector2 ShakeMaxOffset = Vector2.one;
        #endregion
    }
}
