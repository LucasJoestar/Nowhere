using UnityEngine;

public static class NWH_Physics
{
    #region Fields / Properties
    
	#endregion
	
	#region Methods
    /// <summary>
    /// Raycast a box movement from its position to a given destination.
    /// </summary>
    /// <param name="_collider">Box Collider to use.</param>
    /// <param name="_to">Destination of the collider.</param>
    /// <param name="_obstacles">What is an obstacle to the collider.</param>
    /// <param name="_position">Position of the collider after movement (Different from destination if hit something).</param>
    /// <returns>Returns true if something is encountered during the movement, false otherwise.</returns>
	public static bool RaycastBox(BoxCollider2D _collider, Vector2 _to, LayerMask _obstacles, out Vector2 _position)
    {
        bool _isObstacle = false;
        Vector2 _movement = _to - (Vector2)_collider.bounds.center;
        _position = _collider.bounds.center;

        if (_movement.x != 0)
        {
            Vector2 _firstPos = new Vector2(_collider.bounds.center.x + (_collider.bounds.extents.x * Mathf.Sign(_movement.x)), _collider.bounds.center.y - _collider.bounds.extents.y);

            for (float _i = _collider.bounds.size.y; _i > 0; _i-= NWH_GameManager.I.Settings.RaycastPrecision)
            {
                // Raycast to direction
                // From : _firstPos + new Vector2(_i, 0)
            }
        }
        if (_movement.y != 0)
        {
            Vector2 _firstPos = new Vector2(_collider.bounds.center.x - _collider.bounds.extents.x, _collider.bounds.center.y + (_collider.bounds.extents.y * Mathf.Sign(_movement.y)));

            for (float _i = _collider.bounds.size.x; _i > 0; _i -= NWH_GameManager.I.Settings.RaycastPrecision)
            {
                // Raycast to direction
                // From : _firstPos + new Vector2(0, _i)
            }
        }

        return _isObstacle;
    }
	#endregion
}
