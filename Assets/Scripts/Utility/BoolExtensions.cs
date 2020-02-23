public static class BoolExtensions
{
    #region Methods
    /// <summary>
    /// Get the sign of this boolean.
    /// 1 if true, -1 otherwise.
    /// </summary>
    /// <param name="_bool">Boolean to get sign from.</param>
    /// <returns>Returns this boolean sign as 1 or -1.</returns>
    public static int ToSign(this bool _bool) => _bool ? 1 : -1;
	#endregion
}
