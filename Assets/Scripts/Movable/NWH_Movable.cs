using System.Collections;
using UnityEngine;

public class NWH_Movable : MonoBehaviour
{
    #region Fields / Properties

    #region Parameters
    /**********************************
     *********     FIELDS     *********
     *********************************/


    /// <summary>Backing field for <see cref="IsFacingRightSide"/>.</summary>
    [SerializeField] private bool           isFacingRight =     true;

    /// <summary>Backing field for <see cref="IsMoving"/>.</summary>
    [SerializeField] private bool           isMoving =          false;

    /// <summary>Backing field for <see cref="IsOnGround"/>.</summary>
    [SerializeField] private bool           isOnGround =        false;

    /// <summary>Backing field for <see cref="UseGravity"/>.</summary>
    [SerializeField] private bool           useGravity =        true;


    /// <summary>
    /// Physics collider of the object.
    /// </summary>
    [SerializeField] private new Collider   collider =          null;


    /// <summary>
    /// Movement speed of the object.
    /// </summary>
    [SerializeField] private float          speed =             1;

    /// <summary>
    /// Weight of the object (Influence gravity force).
    /// </summary>
    [SerializeField] private float          weight =            1;


    /// <summary>
    /// Layers obstacles to this object.
    /// </summary>
    [SerializeField] private LayerMask      obstacleLayers =    new LayerMask();


    /// <summary>Backing field for <see cref="Velocity"/>.</summary>
    [SerializeField] private Vector2        velocity =          Vector2.zero;


    /**********************************
     *******     PROPERTIES     *******
     *********************************/


    /// <summary>
    /// Indicates if the object is facing the right side of the screen or not.
    /// </summary>
    public bool     IsFacingRightSide
    {
        get { return isFacingRight; }
    }

    /// <summary>
    /// Indicates if the object is currently moving to a destination.
    /// </summary>
    public bool     IsMoving
    {
        get { return isMoving; }
    }

    /// <summary>
    /// Indicates if the object is touching ground.
    /// </summary>
    public bool     IsOnGround
    {
        get { return isOnGround; }
    }

    /// <summary>
    /// If true, the object will be affected by physics gravity and ground attraction.
    /// </summary>
    public bool     UseGravity
    {
        get { return useGravity; }
    }


    /// <summary>
    /// Movement velocity of the object.
    /// </summary>
    public Vector2  Velocity
    {
        get { return velocity; }
        set
        {
            velocity = value;
            if (applyVelocityCoroutine != null) applyVelocityCoroutine = StartCoroutine(ApplyVelocity());
        }
    }
    #endregion

    #region Coroutines & Memory
    /// <summary>Stored coroutine of the <see cref="ApplyVelocity"/> method.</summary>
    private Coroutine   applyVelocityCoroutine =   null;

    /// <summary>Stored coroutine of the <see cref="DoMoveTo"/> method.</summary>
    private Coroutine   doMoveToCoroutine =     null;
    #endregion

    #endregion

    #region Methods

    #region Original Methods
    /**********************************
     ******     IENUMERATORS     ******
     *********************************/


    /// <summary>
    /// Apply velocity to the object movement.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ApplyVelocity()
    {
        applyVelocityCoroutine = null;
        yield break;
    }

    /// <summary>
    /// IEnumerator linked to the <see cref="MoveTo(Vector2)"/> method.
    /// </summary>
    /// <param name="_to">Aimed position.</param>
    /// <returns>IEnumerator, baby.</returns>
    private IEnumerator DoMoveTo(Vector2 _to)
    {
        doMoveToCoroutine = null;
        yield break;
    }


    /**********************************
     **********     FIXS     **********
     *********************************/


    /// <summary>
    /// Makes the object flip.
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Makes the object look at the indicated side.
    /// </summary>
    /// <param name="_doFaceRight">Should the object face the right side of screen or not.</param>
    /// <returns>Returns true if the object flipped, false if was already looking indicated side.</returns>
    public bool Flip(bool _doFaceRight)
    {
        if (isFacingRight == _doFaceRight) return false;

        Flip();
        return true;
    }

    /// <summary>
    /// Makes the object move in a given direction.
    /// </summary>
    /// <param name="_dir">Direction to move to.</param>
    /// <returns>Returns true if hit something during travel, false otherwise.</returns>
    public bool MoveInDirection(Vector2 _dir)
    {
        return false;
    }

    /// <summary>
    /// Makes the object move to a specified position.
    /// </summary>
    /// <param name="_to">Aimed position.</param>
    public void MoveTo(Vector2 _to)
    {
        if (doMoveToCoroutine != null) StopCoroutine(doMoveToCoroutine);
        doMoveToCoroutine = StartCoroutine(DoMoveTo(_to));
    }
    #endregion

    #region Unity Methods
    // Awake is called when the script instance is being loaded
    private void Awake()
	{
		
	}
	
    // Start is called before the first frame update
    private void Start()
    {
        
    }
	#endregion
	
	#endregion
}
