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
	public static bool RaycastBox(BoxCollider2D _collider, Vector3 _to, LayerMask _obstacles, out Vector3 _position)
    {


        _position = Vector3.zero;
        return false;
    }
	#endregion
}
