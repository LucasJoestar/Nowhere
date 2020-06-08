using UnityEngine;

namespace Nowhere
{
    [CreateAssetMenu(fileName = "Attack", menuName = "Datas/Attack", order = 50)]
    public class Attack : ScriptableObject 
    {
        #region Fields / Properties
        /**********************************
         *********     FIELDS     *********
         *********************************/

        [SerializeField, Min(0)] protected int damages = 3;

        /**********************************
         *******     PROPERTIES     *******
         *********************************/

        public int Damages { get { return damages; } }
        #endregion
    }
}
