﻿namespace Nowhere
{
    /// <summary>
    /// Used to know if a character is stuck to a wall.
    /// • -1 when stuck to a wall on the left side
    /// • 0 if stuck to nothing
    /// • 1 for a wall on the right side.
    /// </summary>
    public enum WallStuck
    {
        Left = -1,
        None,
        Right
    }
}
