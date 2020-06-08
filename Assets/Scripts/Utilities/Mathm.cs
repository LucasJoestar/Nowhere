using UnityEngine;

namespace Nowhere
{
    /// <summary>
    /// Contains a bunch of useful math methods.
    /// </summary>
    public static class Mathm
    {
        #region Methods
        /************************************
         *******     BOOL UTILITY     *******
         ***********************************/

        /// <summary>
        /// Get a boolean as a sign.
        /// 1 if true, -1 otherwise.
        /// </summary>
        /// <param name="_bool">Boolean to get sign from.</param>
        /// <returns>Returns this boolean sign as 1 or -1.</returns>
        public static int Sign(bool _bool)
        {
            return _bool ? 1 : -1;
        }


        /************************************
         *****     SIGNS COMPARISON     *****
         ***********************************/

        /// <summary>
        /// Get if two floats have a different sign.
        /// </summary>
        /// <param name="_a">Float a to compare.</param>
        /// <param name="_b">Float b to compare.</param>
        /// <returns>Returns false if floats have the same sign, true otherwise./returns>
        public static bool HaveDifferentSign(float _a, float _b)
        {
            return Mathf.Sign(_a) != Mathf.Sign(_b);
        }

        /// <summary>
        /// Get if two floats have the same sign and are not null.
        /// </summary>
        /// <param name="_a">Float a to compare.</param>
        /// <param name="_b">Float b to compare.</param>
        /// <returns>Returns true if both floats do not equal 0 and have a different sign, false otherwise.</returns>
        public static bool HaveDifferentSignAndNotNull(float _a, float _b)
        {
            return ((_a != 0) && (_b != 0)) ? HaveDifferentSign(_a, _b) : false;
        }


        /***********************************
         *****     VECTORS UTILITY     *****
         **********************************/

        /// <summary>
        /// Get if a Vector2 is null, that is
        /// if its x & y values are equal to zero.
        /// </summary>
        /// <param name="_vector">Vector to check.</param>
        /// <returns>Returns true if both x & y Vector2 values
        /// are equal to zero, false otherwise.</returns>
        public static bool IsVectorNull(Vector2 _vector)
        {
            return (_vector.x == 0) && (_vector.y == 0);
        }

        /// <summary>
        /// Rotates a vector of a certain angle.
        /// </summary>
        /// <param name="_vector">Vector to rotate.</param>
        /// <param name="_angle">Angle to rotate vector by.</param>
        /// <returns>Returns new rotated vector.</returns>
        public static Vector2 RotateVector(Vector2 _vector, float _angle)
        {
            // Equivalent :
            // Quaternion.AngleAxis(_angle, Vector3.forward) * _vector;

            float _sin = Mathf.Sin(_angle * Mathf.Deg2Rad);
            float _cos = Mathf.Cos(_angle * Mathf.Deg2Rad);

            return new Vector2()
            {
                x = (_cos * _vector.x) - (_sin * _vector.y),
                y = (_sin * _vector.x) + (_cos * _vector.y)
            };
        }
        #endregion
    }
}
