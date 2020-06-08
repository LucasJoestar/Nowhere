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
        #region Fields / Properties
        /**********************************
         *******     PARAMETERS     *******
         *********************************/

        [HorizontalLine(1, order = 0), Section("PLAYER CAMERA ATTRIBUTES", order = 1), Space(order = 2)]

        [SerializeField]
        [Tooltip("The camera will ensure that the followed target remains between these bounds, regarding the center of the screen")]
        private Bounds              targetBounds =      new Bounds();

        /// <summary>
        /// The camera will ensure that the followed target
        /// remains between these bounds, regarding the center of the screen.
        /// </summary>
        public Bounds               TargetBounds        { get { return targetBounds; } }
        #endregion
    }
}
