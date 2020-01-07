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
    public static bool RaycastBox(BoxCollider2D _collider, Vector2 _movement, LayerMask _obstacles)
    {
        return RaycastBox(_collider, ref _movement, _obstacles);
    }

    /// <summary>
    /// Raycast a box movement from its position to a given destination.
    /// </summary>
    /// <param name="_collider">Box Collider to use.</param>
    /// <param name="_movement">Movement to perform. Updated when obstacles are encountered.</param>
    /// <param name="_obstacles">What is an obstacle to the collider.</param>
    /// <returns>Returns true if something is encountered during the movement, false otherwise.</returns>
    public static bool RaycastBox(BoxCollider2D _collider, ref Vector2 _movement, LayerMask _obstacles)
    {
        bool _result = RaycastBox(_collider.bounds.center, new Vector2(_collider.bounds.extents.x - .001f, _collider.bounds.extents.y - .001f), ref _movement, _obstacles);

        if (_movement.x != 0) _movement.x -= .001f * Mathf.Sign(_movement.x);
        if (_movement.y != 0) _movement.y -= .001f * Mathf.Sign(_movement.y);

        return _result;
    }

    /// <summary>
    /// Raycast a box movement from its position to a given destination.
    /// </summary>
    /// <param name="_center">Center of the box to raycast from.</param>
    /// <param name="_extents">Extents of the box.</param>
    /// <param name="_movement">Movement to perform. Updated when obstacles are encountered.</param>
    /// <param name="_obstacles">What is an obstacle to the collider.</param>
    /// <returns>Returns true if something is encountered during the movement, false otherwise.</returns>
    public static bool RaycastBox(Vector2 _center, Vector2 _extents, Vector2 _movement, LayerMask _obstacles)
    {
        return RaycastBox(_center, _extents, ref _movement, _obstacles);
    }

    /// <summary>
    /// Raycast a box movement from its position to a given destination.
    /// </summary>
    /// <param name="_center">Center of the box to raycast from.</param>
    /// <param name="_extents">Extents of the box.</param>
    /// <param name="_movement">Movement to perform. Updated when obstacles are encountered.</param>
    /// <param name="_obstacles">What is an obstacle to the collider.</param>
    /// <returns>Returns true if something is encountered during the movement, false otherwise.</returns>
	public static bool RaycastBox(Vector2 _center, Vector2 _extents, ref Vector2 _movement, LayerMask _obstacles)
    {
        bool _isObstacle = false;

        if (_movement.x != 0)
        {
            int _movementSign = (int)Mathf.Sign(_movement.x);
            float _precision = (_extents.y * 2) / (int)((_extents.y * 2) / NWH_GameManager.I.Settings.RaycastPrecision);
            Vector2 _rayPos = new Vector2(_center.x + (_extents.x * _movementSign), _center.y - _extents.y);

            for (float _i = (_extents.y * 2); _i > -.001f; _i-= _precision)
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
            float _precision = (_extents.x * 2) / (int)((_extents.x * 2) / NWH_GameManager.I.Settings.RaycastPrecision);
            Vector2 _rayPos = new Vector2(_center.x - _extents.x, _center.y + (_extents.y * _movementSign));

            for (float _i = (_extents.x * 2); _i > -.001f; _i -= _precision)
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
    #endregion
}
