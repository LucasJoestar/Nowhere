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

        [SerializeField, Required] private PlayerController controller = null;
        [SerializeField, Required] private Animator animator = null;

        [HorizontalLine(2, SuperColor.Red)]

        [SerializeField, Min(0)] private float attackBufferTimer = .15f;

        [HorizontalLine(2, SuperColor.Green)]

        [SerializeField, ReadOnly] private bool isInPosture = false;
        [SerializeField, ReadOnly] private bool isInAttack = false;

        // -----------------------

        private readonly int anim_ComboBrakID = Animator.StringToHash("ComboBreak");
        private readonly int anim_AttackID =    Animator.StringToHash("Attack");
        private readonly int anim_PostureID =   Animator.StringToHash("Posture");
        #endregion

        #region Methods

        #region Inputs
        private float attackBufferTime = 0;

        /// <summary>
        /// Check combat related input.
        /// </summary>
        void IInputUpdate.Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (isInAttack)
                {
                    attackBufferTime = Time.time;
                    return;
                } 

                Attack();
                return;
            }

            if (isInPosture)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    isInPosture = false;
                    animator.SetInteger(anim_PostureID, -1);

                    controller.IsPlayable = true;
                    return;
                }

                // Posture set ;
                // All attack system is designed in the animator,
                // and its posture value is set depending on the player orientation.
                Vector2 _orientation = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
                if (_orientation.x == controller.FacingSide)
                {
                    animator.SetInteger(anim_PostureID, 1);
                }
                else if (_orientation.x == -controller.FacingSide)
                {
                    animator.SetInteger(anim_PostureID, 2);
                }
                else if (_orientation.y == 1)
                {
                    animator.SetInteger(anim_PostureID, 3);
                }
                else if (_orientation.y == -1)
                {
                    animator.SetInteger(anim_PostureID, 4);
                }
                else
                {
                    animator.SetInteger(anim_PostureID, 0);
                }
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                isInPosture = true;
                controller.IsPlayable = false;
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

        public void ComboBreaker()
        {
            isInPosture = false;
            animator.SetTrigger(anim_ComboBrakID);
            animator.SetInteger(anim_PostureID, -1);
        }
        #endregion

        #region Striker
        private void Attack()
        {
            isInAttack = true;
            animator.SetTrigger(anim_AttackID);
        }

        /// <summary>
        /// Strike an opponent and deal damages.
        /// </summary>
        protected override void Strike(Damageable _victim)
        {
            base.Strike(_victim);

            if (attackVictims.Count == 1)
            {
                GameManager.Instance.Sleep(.05f);
                PlayerCamera.Instance.Shake(.25f);
            }
            else
            {
                GameManager.Instance.Sleep(.015f);
                PlayerCamera.Instance.Shake(.1f);
            }
        }

        public override void StopAttack()
        {
            base.StopAttack();

            if (Time.time - attackBufferTime < attackBufferTimer)
                Attack();
            else
                isInAttack = false;
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
