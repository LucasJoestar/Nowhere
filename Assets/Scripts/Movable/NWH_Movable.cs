using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NWH_Movable : MonoBehaviour
{
    #region Fields / Properties

    #region Parameters
    /**********************************
     *********     FIELDS     *********
     *********************************/


    /// <summary>Backing field for <see cref="IsFacingRight"/>.</summary>
    [SerializeField] protected bool                 isFacingRight =     true;

    /// <summary>Backing field for <see cref="IsMoving"/>.</summary>
    [SerializeField] protected bool                 isMoving =          false;

    /// <summary>Backing field for <see cref="IsOnGround"/>.</summary>
    [SerializeField] protected bool                 isOnGround =        false;

    /// <summary>Backing field for <see cref="UseGravity"/>.</summary>
    [SerializeField] protected bool                 useGravity =        true;


    /// <summary>
    /// Velocity Y minimum value when falling.
    /// </summary>
    [SerializeField] protected float                fallMinVelocity =   -25;

    /// <summary>Backing field for <see cref="Speed"/>.</summary>
    [SerializeField] protected float                speed =             1;

    /// <summary>Backing field for <see cref="SpeedCoef"/>.</summary>
    [SerializeField] protected float                speedCoef =         1;


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
    /// Obstacles layers to this object collisions.
    /// </summary>
    [SerializeField] protected LayerMask            obstaclesMask =     new LayerMask();


    /// <summary>Backing field for <see cref="Velocity"/>.</summary>
    [SerializeField] protected Vector2              velocity =          Vector2.zero;


    /**********************************
     *******     PROPERTIES     *******
     *********************************/


    /// <summary>
    /// Indicates if the object is facing the right side of the screen or th left one.
    /// </summary>
    public bool         IsFacingRight
    {
        get { return isFacingRight; }
        protected set
        {
            isFacingRight = value;
        }
    }

    /// <summary>
    /// Indicates if the object is currently moving to a destination.
    /// </summary>
    public bool         IsMoving
    {
        get { return isMoving; }
        protected set
        {
            isMoving = value;
        }
    }

    /// <summary>
    /// Indicates if the object is touching ground.
    /// </summary>
    public bool         IsOnGround
    {
        get { return isOnGround; }
        protected set
        {
            isOnGround = value;
            if (!value && (velocity.y == 0)) Velocity = new Vector2(velocity.x, -2.5f);
        }
    }

    /// <summary>
    /// If true, the object will be affected by physics gravity and ground attraction.
    /// </summary>
    public bool         UseGravity
    {
        get { return useGravity; }
        set
        {
            useGravity = value;
            if (value)
            {
                if (cCheckOnGround == null) cCheckOnGround = StartCoroutine(CheckOnGround());
            }
            else if (cCheckOnGround != null)
            {
                StopCoroutine(cCheckOnGround);
                cCheckOnGround = null;
            }
        }
    }


    /// <summary>
    /// Base movement speed of the object (per second).
    /// </summary>
    public float        Speed
    {
        get { return speed; }
    }

    /// <summary>
    /// Movement speed coefficient.
    /// </summary>
    public float        SpeedCoef
    {
        get { return speedCoef; }
        set
        {
            if (value < 0) value = 0;
            speedCoef = value;
        }
    }


    /// <summary>
    /// Movement velocity of the object (per second).
    /// </summary>
    public Vector2      Velocity
    {
        get { return velocity; }
        set
        {
            velocity = value;
            if ((value != Vector2.zero) && (cApplyVelocity == null)) cApplyVelocity = StartCoroutine(ApplyVelocity());
        }
    }
    #endregion

    #region Coroutines & Memory
    /**********************************
     *******     COROUTINES     *******
     *********************************/


    /// <summary>Stored coroutine of the <see cref="ApplyVelocity"/> method.</summary>
    protected Coroutine     cApplyVelocity =        null;

    /// <summary>Stored coroutine of the <see cref="CheckOnGround"/> method.</summary>
    protected Coroutine     cCheckOnGround =        null;

    /// <summary>Stored coroutine of the <see cref="DoMoveTo"/> method.</summary>
    protected Coroutine     cDoMoveTo =             null;


    /**********************************
     *********     MEMORY     *********
     *********************************/


    /// <summary>
    /// This object position at previous frame.
    /// Updated during late update.
    /// </summary>
    protected Vector2       lastPosition =          new Vector2();
    #endregion

    #endregion

    #region Methods

    #region Original Methods

    #region IEnumerators
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
            yield return null;

            if (DoPerformMovement(velocity * Time.deltaTime))
            {
                velocity = new Vector2(0, velocity.y > 0 ? -2 : 0);
                continue;
            }

            // X velocity
            if (velocity.x != 0)
            {
                velocity.x *= .975f;
                if (Mathf.Abs(velocity.x) < .01f) velocity.x = 0;
            }

            // Y velocity
            if (velocity.y > 0)
            {
                velocity.y *= .925f;
                if (velocity.y < .2f) velocity.y *= -1;
            }
            else if (velocity.y > fallMinVelocity)
            {
                velocity.y = Mathf.Max(fallMinVelocity, velocity.y * (velocity.y > -1.5f ? 1.1f : 1.05f));
            }
        }

        cApplyVelocity = null;
    }

    /// <summary>
    /// Checks if the object is touching ground or not.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator CheckOnGround()
    {
        if (!isOnGround) IsOnGround = true;

        while (true)
        {
            RaycastHit2D[] _hit = new RaycastHit2D[1];
            if ((collider.Cast(new Vector2(0, -.1f), _hit, .1f) > 0) != isOnGround) IsOnGround = !isOnGround;

            if (!isOnGround && velocity.y == 0) Velocity = new Vector2(velocity.x, -2.5f);

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
        while ((Mathf.Abs(transform.position.x - _to.x) > .01f) || (Mathf.Abs(transform.position.y - _to.y) > .01f))
        {
            yield return null;

            if (!PerformMovement(_to - (Vector2)transform.position)) break;
        }

        cDoMoveTo = null;
    }
    #endregion

    #region Flip
    /**********************************
     **********     FLIP     **********
     *********************************/


    /// <summary>
    /// Makes the object flip.
    /// </summary>
    public virtual void Flip()
    {
        IsFacingRight = !isFacingRight;
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
    #endregion

    #region Movements
    /*********************************
     *******     MEDIATORS     *******
     ********************************/


    /// <summary>
    /// Get this object movement speed for this frame.
    /// </summary>
    /// <returns>Returns this frame speed.</returns>
    protected virtual float GetMovementSpeed()
    {
        return speed * speedCoef * Time.deltaTime;
    }


    /// <summary>
    /// Makes the object move in a given direction.
    /// </summary>
    /// <param name="_dir">Direction to move to.</param>
    /// <returns>Returns true if hit something, false otherwise.</returns>
    public virtual bool MoveInDirection(Vector2 _dir)
    {
        return DoPerformMovement(_dir.normalized * GetMovementSpeed());
    }

    /// <summary>
    /// Makes the object move to a specific position.
    /// </summary>
    /// <param name="_to">Destination.</param>
    public void MoveTo(Vector2 _to)
    {
        if (cDoMoveTo != null) StopCoroutine(cDoMoveTo);
        cDoMoveTo = StartCoroutine(DoMoveTo(_to));
    }

    /// <summary>
    /// Makes the object perform a given movement, limited by its speed.
    /// </summary>
    /// <param name="_movement">Movement to perform.</param>
    /// <returns>Returns true if hit something, false otherwise.</returns>
    public virtual bool PerformMovement(Vector2 _movement)
    {
        if (_movement.magnitude > GetMovementSpeed()) _movement = _movement.normalized * GetMovementSpeed();
        return DoPerformMovement(_movement);
    }

    /// <summary>
    /// Stops the object from moving.
    /// </summary>
    public void StopMovingTo()
    {
        if (cDoMoveTo != null)
        {
            StopCoroutine(cDoMoveTo);
            cDoMoveTo = null;
        }
    }


    /*********************************
     ******     SYSTEM COGS     ******
     ********************************/


    /// <summary>
    /// Makes the object perform a given movement.
    /// </summary>
    /// <param name="_movement">Movement to perform.</param>
    /// <returns>Returns true if hit something, false otherwise.</returns>
    protected virtual bool DoPerformMovement(Vector2 _movement)
    {
        if (_movement == Vector2.zero) return false;

        bool _doHit = false;
        RaycastHit2D[] _hit = new RaycastHit2D[1];
        /*if (collider.Cast(new Vector2(_movement.x, 0), _hit, Mathf.Abs(_movement.x)) > 0)
        {
            _doHit = true;
            _movement.x = Mathf.Max(0, _hit[0].distance - Physics.defaultContactOffset) * Mathf.Sign(_movement.x);
        }
        if (collider.Cast(new Vector2(0, _movement.y), _hit, Mathf.Abs(_movement.y)) > 0)
        {
            _doHit = true;
            _movement.y = Mathf.Max(0, _hit[0].distance - Physics.defaultContactOffset) * Mathf.Sign(_movement.y);
        }*/

        if (collider.Cast(_movement, _hit, _movement.magnitude) > 0)
        {
            _movement = _movement.normalized * Mathf.Max(0, _hit[0].distance - Physics.defaultContactOffset);

            if (_movement == Vector2.zero) return true;

            _doHit = true;
        }

        MoveObject((Vector2)transform.position + _movement);

        return _doHit;
    }

    /// <summary>
    /// Makes the object perform a given movement.
    /// </summary>
    /// <param name="_movement">Movement to perform.</param>
    /// <param name="_hit">Cast result.</param>
    /// <returns>Returns true if hit something, false otherwise.</returns>
    protected virtual bool DoPerformMovement(Vector2 _movement, out RaycastHit2D _hit)
    {
        bool _doHit = false;
        RaycastHit2D[] _rayHit = new RaycastHit2D[1];

        if (collider.Cast(_movement, _rayHit, _movement.magnitude) > 0)
        {
            _doHit = true;
            _movement = _movement.normalized * Mathf.Max(0, _rayHit[0].distance - Physics.defaultContactOffset);
        }
        _hit = _rayHit[0];

        if (_movement == Vector2.zero) return _doHit;

        MoveObject((Vector2)transform.position + _movement);

        return _doHit;
    }

    /// <summary>
    /// Moves the object to a specified position.
    /// Use this instead of setting transform.position.
    /// </summary>
    /// <param name="_position">New position of the object.</param>
    public void MoveObject(Vector2 _position)
    {
        // Flip if not facing movement side.
        if (Mathf.Sign(_position.x - transform.position.x) != isFacingRight.ToSign()) Flip();

        transform.position = _position;
        rigidbody.position = _position;
    }

    /// <summary>
    /// Updates informations about position.
    /// Called during late update.
    /// </summary>
    /// <returns>Returns true if position changed, false otherwise.</returns>
    protected virtual bool UpdatePosition()
    {
        // Update isMoving value if needed
        if ((Vector2)transform.position == lastPosition)
        {
            if (isMoving) IsMoving = false;
            return false;
        }
        if (!isMoving) IsMoving = true;

        lastPosition = transform.position;
        return true;
    }
    #endregion

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

    // LateUpdate is called after all Update functions have been called
    protected virtual void LateUpdate()
    {
        UpdatePosition();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (useGravity) UseGravity = true;
        lastPosition = transform.position;
    }
    #endregion

    #endregion
}
