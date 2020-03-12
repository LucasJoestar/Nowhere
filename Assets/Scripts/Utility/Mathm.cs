using UnityEngine;

/// <summary>
/// Contains a bunch of useful math methods.
/// </summary>
public static class Mathm
{
    #region Methods
    /*********************************
     *********     SIGNS     *********
     ********************************/

    /// <summary>
    /// Get if two floats have a different sign.
    /// </summary>
    /// <param name="_a">Float a to compare.</param>
    /// <param name="_b">Float b to compare.</param>
    /// <returns>Returns false if floats have the same sign, true otherwise./returns>
    public static bool HaveDifferentSign(float _a, float _b) => Mathf.Sign(_a) != Mathf.Sign(_b);

    /// <summary>
    /// Get if two floats have the same sign and are not null.
    /// </summary>
    /// <param name="_a">Float a to compare.</param>
    /// <param name="_b">Float b to compare.</param>
    /// <returns>Returns true if both floats do not equal 0 and have a different sign, false otherwise.</returns>
    public static bool HaveDifferentSignAndNotNull(float _a, float _b) => ((_a == 0) || (_b == 0)) ? false : HaveDifferentSign(_a, _b);
    #endregion
}
