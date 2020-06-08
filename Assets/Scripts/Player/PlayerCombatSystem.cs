// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using System.Collections;
using UnityEngine;

namespace Nowhere
{
    public class PlayerCombatSystem : Striker, IInputUpdate 
    {
        #region Fields / Properties
        /**********************************
         *********     FIELDS     *********
         *********************************/

        [HorizontalLine(1, order = 0), Section("PLAYER COMBAT SYSTEM", 50, 0, order = 1), HorizontalLine(2, SuperColor.Maroon, order = 2)]

        [SerializeField, Required] private Animator animator = null;
        #endregion

        #region Methods

        #region Original Methods

        #region Inputs
        void IInputUpdate.Update()
        {
            if (isAttacking)
                return;

            if (Input.GetKeyDown(KeyCode.Mouse0))
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
        }
        #endregion

        #region Striker
        protected override void Strike(Damageable _victim)
        {
            base.Strike(_victim);

            StartCoroutine(StrikeFeedback());
        }

        private IEnumerator StrikeFeedback()
        {
            float _timer = .1f;
            Time.timeScale = 0;

            while (_timer > 0)
            {
                yield return null;
                _timer -= Time.unscaledDeltaTime;
            } 

            Time.timeScale = 1;
        }
        #endregion

        #endregion

        #region Monobehaviour
        /*********************************
         *****     MONOBEHAVIOUR     *****
         ********************************/

        private void OnEnable()
        {
            UpdateManager.Instance.Register((IInputUpdate)this);
        }

        private void OnDisable()
        {
            UpdateManager.Instance.Unregister((IInputUpdate)this);
        }
        #endregion

        #endregion
    }
}
