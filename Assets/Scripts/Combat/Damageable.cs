// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace Nowhere
{
    public class Damageable : MonoBehaviour 
    {
        #region Fields / Properties
        /**********************************
         *********     FIELDS     *********
         *********************************/

        [HorizontalLine(1, order = 0), Section("DAMAGEABLE", 50, 0, order = 1), HorizontalLine(2, SuperColor.SmokyBlack, order = 2)]

        [SerializeField, Required] private Collider2D   hitBox =    null;
        [SerializeField, Required] private Animator     animator =  null;

        [HorizontalLine(2, SuperColor.Indigo)]

        [ProgressBar("maxHealth", SuperColor.Crimson)]
        [SerializeField] private int health =       100;
        [SerializeField] private int maxHealth =    100;

        [HorizontalLine(2, SuperColor.Navy)]

        [SerializeField, ReadOnly] private bool isDead = false;

        // ---------------------------

        private static readonly int anim_HitID =    Animator.StringToHash("Hit");
        private static readonly int anim_DeathID =  Animator.StringToHash("Death");
        #endregion

        #region Methods

        #region Original Methods
        /// <summary>
        /// Make the object take a certain amount of damages.
        /// Returns false if the object is no longer alive, true otherwise.
        /// </summary>
        public virtual bool TakeDamage(int _damages)
        {
            health -= _damages;

            animator.SetTrigger(anim_HitID);

            if (health > 0)
                return true;

            Die();
            return false;
        }

        protected virtual void Die()
        {
            health = 0;
            isDead = true;
            animator.SetTrigger(anim_DeathID);

            // Deactivate object.
            hitBox.enabled = enabled = false;
        }
        #endregion

        #region Monobehaviour
        /*********************************
         *****     MONOBEHAVIOUR     *****
         ********************************/

        private void Start()
        {
            health = maxHealth;
        }
        #endregion

        #endregion
    }
}
