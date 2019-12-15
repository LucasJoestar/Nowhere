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
    /// <param name="_movement">Movement to perform. Updated when obstacles are encountered.</param>
    /// <param name="_obstacles">What is an obstacle to the collider.</param>
    /// <returns>Returns true if something is encountered during the movement, false otherwise.</returns>
	public static bool RaycastBox(BoxCollider2D _collider, ref Vector2 _movement, LayerMask _obstacles)
    {
        bool _isObstacle = false;

        if (_movement.x != 0)
        {
            int _movementSign = (int)Mathf.Sign(_movement.x);
            float _precision = _collider.bounds.size.y / (int)(_collider.bounds.size.y / NWH_GameManager.I.Settings.RaycastPrecision);
            Vector2 _rayPos = new Vector2(_collider.bounds.center.x + (_collider.bounds.extents.x * _movementSign), _collider.bounds.center.y - _collider.bounds.extents.y);

            for (float _i = _collider.bounds.size.y; _i > -.001f; _i-= _precision)
            {
                RaycastHit2D _hit = Physics2D.Raycast(new Vector2(_rayPos.x, _rayPos.y + _i), new Vector2(_movement.x, 0), _movement.x * _movementSign, _obstacles);

                if (_hit.transform)
                {
                    _movement.x = _movementSign * _hit.distance;
                    if (!_isObstacle) _isObstacle = true;
                }
            }
        }
        if (_movement.y != 0)
        {
            int _movementSign = (int)Mathf.Sign(_movement.y);
            float _precision = _collider.bounds.size.x / (int)(_collider.bounds.size.x / NWH_GameManager.I.Settings.RaycastPrecision);
            Vector2 _rayPos = new Vector2(_collider.bounds.center.x - _collider.bounds.extents.x, _collider.bounds.center.y + (_collider.bounds.extents.y * _movementSign));

            for (float _i = _collider.bounds.size.x; _i > -.001f; _i -= _precision)
            {
                RaycastHit2D _hit = Physics2D.Raycast(new Vector2(_rayPos.x + _i, _rayPos.y), new Vector2(0, _movement.y), _movement.y * _movementSign, _obstacles);

                if (_hit.transform)
                {
                    _movement.y = _movementSign * _hit.distance;
                    if (!_isObstacle) _isObstacle = true;
                }
            }
        }

        return _isObstacle;
    }

    public static bool RaycastCircle(CircleCollider2D _collider, ref Vector2 _movement, LayerMask _obstacles)
    {
        return false;
    }
    #endregion
}
