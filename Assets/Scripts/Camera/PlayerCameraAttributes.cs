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
        #endregion
    }
}
