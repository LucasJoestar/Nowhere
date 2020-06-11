// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using System.Collections.Generic;
using UnityEngine;

namespace Nowhere
{
    public class Striker : UpdatedBehaviour, IUpdate
    {
        #region Fields
        [HorizontalLine(1, order = 0), Section("STRIKER", 50, 0, order = 1), HorizontalLine(2, SuperColor.HarvestGold, order = 2)]

        [SerializeField, Required] private Collider2D strikeBox = null;
        [SerializeField, Required] private Collider2D hitBox = null;

        [HorizontalLine(2, SuperColor.SalmonPink)]

        [SerializeField] protected Attack[] attacks = new Attack[] { };

        [HorizontalLine(2, SuperColor.Lavender)]

        [SerializeField, ReadOnly] protected bool isAttacking = false;
        [SerializeField, ReadOnly] protected Attack activeAttack = null;

        // --------------

        #if UNITY_EDITOR
        [HorizontalLine(2, SuperColor.Brown)]
        [SerializeField] private bool doDrawStrikeBox = false;
        #endif

        // --------------

        protected ContactFilter2D contactFilter = new ContactFilter2D();
        #endregion

        #region Methods

        #region Attack System
        /// <summary>
        /// Activates the attack from <see cref="attacks"/> at the specified index.
        /// </summary>
        protected virtual void ActivateAttack(int _attackIndex)
        {
            if (!isAttacking)
                UpdateManager.Instance.Register(this);

            isAttacking = true;
            activeAttack = attacks[_attackIndex];

            attackVictims.Clear();
        }

        public virtual void StopAttack()
        {
            if (isAttacking)
                UpdateManager.Instance.Unregister(this);

            isAttacking = false;
            activeAttack = null;
        }

        // -----------------

        private List<Collider2D> attackVictims = new List<Collider2D>();

        private static Collider2D[] overlapBuffer = new Collider2D[6];
        private static int overlapBufferAmount = 0;

        /// <summary>
        /// While active, strike all overlapping objects once.
        /// </summary>
        void IUpdate.Update()
        {
            overlapBufferAmount = strikeBox.OverlapCollider(contactFilter, overlapBuffer);
            for (int _i = 0; _i < overlapBufferAmount; _i++)
            {
                if ((overlapBuffer[_i] != hitBox) && !attackVictims.Contains(overlapBuffer[_i]))
                {
                    attackVictims.Add(overlapBuffer[_i]);
                    Strike(overlapBuffer[_i].GetComponentInParent<Damageable>());
                }
            }
        }

        /// <summary>
        /// Strike an opponent and deal damages.
        /// </summary>
        protected virtual void Strike(Damageable _victim)
        {
            _victim.TakeDamage(activeAttack.Damages);
        }
        #endregion

        #region Monobehaviour
        protected override void OnDisableCallback()
        {
            StopAttack();
        }

        private void Start()
        {
            // Set up contact filter.
            contactFilter.layerMask = Physics2D.GetLayerCollisionMask(strikeBox.gameObject.layer);
            contactFilter.useLayerMask = true;
            contactFilter.useTriggers = true;
        }

        #if UNITY_EDITOR
        private static readonly Color gizmosColor = new Color(1, 0, 0, .5f);
        private void OnDrawGizmos()
        {
            if ((doDrawStrikeBox || isAttacking) && strikeBox)
            {
                Color _originalColor = Gizmos.color;
                Gizmos.color = gizmosColor;

                Gizmos.DrawCube(strikeBox.bounds.center, strikeBox.bounds.size);

                Gizmos.color = _originalColor;
            }
        }
        #endif

        #endregion

        #endregion
    }
}
