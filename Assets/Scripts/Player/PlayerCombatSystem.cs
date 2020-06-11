// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace Nowhere
{
    public class PlayerCombatSystem : Striker, IInputUpdate 
    {
        #region Fields
        [HorizontalLine(1, order = 0), Section("PLAYER COMBAT SYSTEM", 50, 0, order = 1), HorizontalLine(2, SuperColor.Maroon, order = 2)]

        [SerializeField, Required] private Animator animator = null;
        #endregion

        #region Methods

        #region Inputs
        /// <summary>
        /// Check combat related input.
        /// </summary>
        void IInputUpdate.Update()
        {
            if (isAttacking)
                return;

            if (Input.GetKeyDown(KeyCode.Z))
            {
                animator.SetTrigger("Attack 1");
                return;
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                animator.SetTrigger("Attack 2");
                return;
            }

            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                animator.SetTrigger("Attack 3");
                return;
            }

            // ---------- Cheat Codes ---------- //

            if (Input.GetKeyDown(KeyCode.Alpha1))
                PlayerCamera.Instance.Shake(.1f);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                PlayerCamera.Instance.Shake(.25f);

            if (Input.GetKeyDown(KeyCode.Alpha3))
                PlayerCamera.Instance.Shake(.5f);

            if (Input.GetKeyDown(KeyCode.Alpha4))
                PlayerCamera.Instance.Shake(1f);
        }
        #endregion

        #region Striker
        /// <summary>
        /// Strike an opponent and deal damages.
        /// </summary>
        protected override void Strike(Damageable _victim)
        {
            base.Strike(_victim);

            GameManager.Instance.Sleep(.04f);
            PlayerCamera.Instance.Shake(.25f);
        }
        #endregion

        #region Monobehaviour
        private void OnEnable()
        {
            UpdateManager.Instance.Register((IInputUpdate)this);
        }

        protected override void OnDisableCallback()
        {
            base.OnDisableCallback();

            UpdateManager.Instance.Unregister((IInputUpdate)this);
        }
        #endregion

        #endregion
    }
}
