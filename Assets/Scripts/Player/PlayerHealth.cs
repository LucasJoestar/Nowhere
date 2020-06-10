// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace Nowhere
{
    public class PlayerHealth : Damageable 
    {
        #region Fields
        [HorizontalLine(1, order = 0), Section("PLAYER HEALTH", 50, 0, order = 1), HorizontalLine(2, SuperColor.DarkOrange, order = 2)]

        [SerializeField, Required] private PlayerController controller = null;
        [SerializeField, Required] private PlayerCombatSystem combatSystem = null;
        #endregion

        #region Methods
        protected override void Die()
        {
            base.Die();

            // Deactive player systems.
            controller.enabled = combatSystem.enabled = enabled = false;
        }
        #endregion
    }
}
