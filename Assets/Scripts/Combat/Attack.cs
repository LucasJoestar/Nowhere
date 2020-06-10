// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using UnityEngine;

namespace Nowhere
{
    [CreateAssetMenu(fileName = "Attack", menuName = "Datas/Attack", order = 50)]
    public class Attack : ScriptableObject 
    {
        #region Fields
        [Min(0)] public int Damages = 3;
        #endregion
    }
}
