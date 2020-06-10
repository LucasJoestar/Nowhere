// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using UnityEngine;

namespace Nowhere
{
    public static class Vector2Extensions 
    {
        #region Methods
        /// <summary>
        /// Get if a Vector2 is null, that is
        /// if its x & y values are equal to zero.
        /// </summary>
        /// <param name="_vector">Vector to check.</param>
        /// <returns>Returns true if both x & y Vector2 values
        /// are equal to zero, false otherwise.</returns>
        public static bool IsNull(this Vector2 _vector) => Mathm.IsVectorNull(_vector);

        /// <summary>
        /// Rotates this vector of a certain angle.
        /// </summary>
        /// <param name="_vector">Vector to rotate.</param>
        /// <param name="_angle">Angle to rotate vector by.</param>
        /// <returns>Returns new rotated vector.</returns>
        public static Vector2 Rotate(this Vector2 _vector, float _angle) => Mathm.RotateVector(_vector, _angle);
        #endregion
    }
}
