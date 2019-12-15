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
    [SerializeField] private new Collider2D collider =          null;


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
        while ((Mathf.Abs(transform.position.x - _to.x) > .01f) && (Mathf.Abs(transform.position.y - _to.y) > .01f))
        {
            yield return new WaitForFixedUpdate();
            MoveInDirection(_to - (Vector2)transform.position);
        }

        Debug.Log("Reached Destination");
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
        _dir = _dir.normalized * Mathf.Min(_dir.magnitude, speed);

        bool _doHit = NWH_Physics.RaycastBox((BoxCollider2D)collider, ref _dir, obstacleLayers);
        transform.position += (Vector3)_dir;

        return _doHit;
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

    private void OnDrawGizmos()
    {
        if (!NWH_GameManager.I || NWH_GameManager.I.Settings.RaycastPrecision < .1f) return;

        Gizmos.color = Color.blue;

        Vector2 _firstPos = new Vector2(collider.bounds.center.x + collider.bounds.extents.x, collider.bounds.center.y - collider.bounds.extents.y);
        float _precision = collider.bounds.size.y / ((int)(collider.bounds.size.y / NWH_GameManager.I.Settings.RaycastPrecision));

        for (float _i = collider.bounds.size.y; _i > -.001f; _i -= _precision)
        {
            Gizmos.DrawSphere(new Vector2(_firstPos.x, _firstPos.y + _i), NWH_GameManager.I.Settings.RaycastPrecision / 2f);
        }

        _firstPos = new Vector2(collider.bounds.center.x - collider.bounds.extents.x, collider.bounds.center.y + collider.bounds.extents.y);
        _precision = collider.bounds.size.x / ((int)(collider.bounds.size.x / NWH_GameManager.I.Settings.RaycastPrecision));

        for (float _i = collider.bounds.size.x; _i > -.001f; _i -= _precision)
        {
            Gizmos.DrawSphere(new Vector2(_firstPos.x + _i, _firstPos.y), NWH_GameManager.I.Settings.RaycastPrecision / 2f);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        //MoveTo(transform.position + Vector3.one * 5);
    }

    private void FixedUpdate()
    {
        //MoveInDirection(new Vector2(Input.GetAxis("Horizontal"), 0));
    }
    #endregion

    #endregion
}
