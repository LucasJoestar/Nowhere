using System.Collections;
using UnityEngine;

public class NWH_Movable : MonoBehaviour
{
    #region Fields / Properties

    #region Constants
    /// <summary>
    /// Velocity Y minimum value.
    /// </summary>
    private const float     FALLING_MAX_VELOCITY =  -25;
    #endregion

    #region Parameters
    /**********************************
     *********     FIELDS     *********
     *********************************/


    /// <summary>Backing field for <see cref="IsFacingRightSide"/>.</summary>
    [SerializeField] protected bool                 isFacingRight =     true;

    /// <summary>Backing field for <see cref="IsMoving"/>.</summary>
    [SerializeField] protected bool                 isMoving =          false;

    /// <summary>Backing field for <see cref="IsOnGround"/>.</summary>
    [SerializeField] protected bool                 isOnGround =        false;

    /// <summary>Backing field for <see cref="UseGravity"/>.</summary>
    [SerializeField] protected bool                 useGravity =        true;


    /// <summary>
    /// Movement speed of the object.
    /// </summary>
    [SerializeField] protected float                speed =             1;


    /// <summary>
    /// Physics collider of the object.
    /// </summary>
    [SerializeField] protected new Collider2D       collider =          null;

    /// <summary>
    /// Rigidbody of the object (should be Kinematic).
    /// This should only be used by moving its position to update the collider one.
    /// </summary>
    [SerializeField] protected new Rigidbody2D      rigidbody =         null;


    /// <summary>
    /// Layers obstacles to this object.
    /// </summary>
    [SerializeField] protected LayerMask            obstacleLayers =    new LayerMask();


    /// <summary>Backing field for <see cref="Velocity"/>.</summary>
    [SerializeField] protected Vector2              velocity =          Vector2.zero;


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
        private set
        {
            isOnGround = value;
            if (!value && (velocity.y == 0)) Velocity = new Vector2(velocity.x, -2.5f);
        }
    }

    /// <summary>
    /// If true, the object will be affected by physics gravity and ground attraction.
    /// </summary>
    public bool     UseGravity
    {
        get { return useGravity; }
        set
        {
            useGravity = value;
            if (value)
            {
                if (checkOnGroundCoroutine == null) checkOnGroundCoroutine = StartCoroutine(CheckOnGround());
            }
            else if (checkOnGroundCoroutine != null)
            {
                StopCoroutine(checkOnGroundCoroutine);
                checkOnGroundCoroutine = null;
            }
        }
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
            if ((value != Vector2.zero) && (applyVelocityCoroutine == null)) applyVelocityCoroutine = StartCoroutine(ApplyVelocity());
        }
    }
    #endregion

    #region Coroutines & Memory
    /// <summary>Stored coroutine of the <see cref="ApplyVelocity"/> method.</summary>
    protected Coroutine     applyVelocityCoroutine =      null;

    /// <summary>Stored coroutine of the <see cref="CheckOnGround"/> method.</summary>
    protected Coroutine     checkOnGroundCoroutine =    null;

    /// <summary>Stored coroutine of the <see cref="DoMoveTo"/> method.</summary>
    protected Coroutine     doMoveToCoroutine =           null;
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
    protected virtual IEnumerator ApplyVelocity()
    {
        while (velocity != Vector2.zero)
        {
            Vector2 _movement = velocity * Time.deltaTime;
            DoPerformMovement(ref _movement);

            if (velocity.x != 0)
            {
                if (_movement.x == 0) velocity.x = 0;
                else
                {
                    velocity.x *= .975f;
                    if (Mathf.Abs(velocity.x) < .01f) velocity.x = 0;
                }
            }

            if (velocity.y != 0)
            {
                if (_movement.y == 0)
                {
                    if (velocity.y > 0) velocity.y *= -.75f;
                    else velocity.y = 0;
                }
                else
                {
                    if (velocity.y > 0)
                    {
                        velocity.y *= .925f;

                        if (velocity.y < .2f) velocity.y *= -1;
                    }
                    else if (velocity.y > FALLING_MAX_VELOCITY)
                    {
                        velocity.y = Mathf.Max(FALLING_MAX_VELOCITY, velocity.y * (velocity.y > -1.5f ? 1.1f : 1.05f));
                    }
                }
            }

            yield return null;
        }

        applyVelocityCoroutine = null;
    }

    /// <summary>
    /// Checks if the object is touching ground or not.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator CheckOnGround()
    {
        while (true)
        {
            RaycastHit2D[] _hit = new RaycastHit2D[1];
            if ((collider.Cast(new Vector2(0, -.1f), _hit, .1f) > 0) != isOnGround) IsOnGround = !isOnGround;

            yield return null;
        }
    }

    /// <summary>
    /// IEnumerator linked to the <see cref="MoveTo(Vector2)"/> method.
    /// </summary>
    /// <param name="_to">Aimed position.</param>
    /// <returns>IEnumerator, baby.</returns>
    protected virtual IEnumerator DoMoveTo(Vector2 _to)
    {
        isMoving = true;

        while ((Mathf.Abs(transform.position.x - _to.x) > .01f) || (Mathf.Abs(transform.position.y - _to.y) > .01f))
        {
            yield return null;

            if (!PerformMovement(_to - (Vector2)transform.position)) break;
        }

        doMoveToCoroutine = null;
        isMoving = false;
    }


    /**********************************
     **********     FIXS     **********
     *********************************/


    /// <summary>
    /// Makes the object flip.
    /// </summary>
    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Makes the object look at the indicated side.
    /// </summary>
    /// <param name="_doFaceRight">Should the object face the right side of screen or not.</param>
    /// <returns>Returns true if the object flipped, false if was already looking indicated side.</returns>
    public virtual bool Flip(bool _doFaceRight)
    {
        if (isFacingRight == _doFaceRight) return false;

        Flip();
        return true;
    }


    /// <summary>
    /// Makes the object move in a given direction.
    /// </summary>
    /// <param name="_dir">Direction to move to.</param>
    /// <returns>Returns true if position changed, false otherwise.</returns>
    public virtual bool MoveInDirection(Vector2 _dir)
    {
        return DoPerformMovement(_dir.normalized * speed);
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

    /// <summary>
    /// Makes the object perform a given movement, limited by its speed.
    /// </summary>
    /// <param name="_movement">Movement to perform.</param>
    /// <returns>Returns true if position changed, false otherwise.</returns>
    public virtual bool PerformMovement(Vector2 _movement)
    {
        _movement = _movement.normalized * Mathf.Min(_movement.magnitude, speed);
        return DoPerformMovement(_movement);
    }


    /// <summary>
    /// Makes the object perform a given movement.
    /// </summary>
    /// <param name="_movement">Movement to perform.</param>
    /// <returns>Returns true if position changed, false otherwise.</returns>
    protected virtual bool DoPerformMovement(Vector2 _movement)
    {
        return DoPerformMovement(ref _movement);
    }

    /// <summary>
    /// Makes the object perform a given movement.
    /// </summary>
    /// <param name="_movement">Movement to perform.</param>
    /// <returns>Returns true if position changed, false otherwise.</returns>
    protected virtual bool DoPerformMovement(ref Vector2 _movement)
    {
        RaycastHit2D[] _hit = new RaycastHit2D[1];
        if (collider.Cast(new Vector2(_movement.x, 0), _hit, Mathf.Abs(_movement.x)) > 0)
        {
            //Debug.Log(_hit[0].collider.name + " | " + _hit[0].distance);
            _movement.x = Mathf.Max(0, _hit[0].distance - Physics.defaultContactOffset) * Mathf.Sign(_movement.x);
        }
        if (collider.Cast(new Vector2(0, _movement.y), _hit, Mathf.Abs(_movement.y)) > 0)
        {
            _movement.y = Mathf.Max(0, _hit[0].distance - Physics.defaultContactOffset) * Mathf.Sign(_movement.y);
        }

        //bool _doHit = NWH_Physics.RaycastBox((BoxCollider2D)collider, ref _movement, obstacleLayers);

        if (_movement == Vector2.zero) return false;

        transform.position = (Vector2)transform.position + _movement;
        rigidbody.position += _movement;

        return true;
    }


    /// <summary>
    /// Stops the object from moving.
    /// </summary>
    public void StopMoving()
    {
        if (doMoveToCoroutine != null)
        {
            StopCoroutine(doMoveToCoroutine);
            doMoveToCoroutine = null;

            isMoving = false;
        }
    }
    #endregion

    #region Unity Methods
    // Awake is called when the script instance is being loaded
    protected virtual void Awake()
	{
        #if UNITY_EDITOR
        if (!collider) Debug.LogError($"Missing Collider on \"{name}\" !");
        if (!rigidbody) Debug.LogError($"Missing Rigidbody on \"{name}\" !");
        #endif
    }

    private void OnDrawGizmos()
    {
        if (!NWH_GameManager.I || NWH_GameManager.I.Settings.RaycastPrecision < .1f) return;

        Gizmos.color = Color.cyan;

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
    protected virtual void Start()
    {
        isOnGround = true;
        if (useGravity) UseGravity = true;
    }

    private void Update()
    {
        MoveInDirection(new Vector2(Input.GetAxis("Horizontal"), 0));
    }
    #endregion

    #endregion
}
