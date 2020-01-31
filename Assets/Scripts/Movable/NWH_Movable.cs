using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NWH_Movable : MonoBehaviour
{
    #region Fields / Properties

    #region Constants
    /// <summary>
    /// Minimum required movement distance to move the object.
    /// </summary>
    public const float          MIN_MOVEMENT_DISTANCE =     .001f;
    #endregion

    #region Parameters
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>Backing field for <see cref="IsFacingRight"/>.</summary>
    [SerializeField]
    protected bool                 isFacingRight =     true;

    /// <summary>Backing field for <see cref="IsMoving"/>.</summary>
    [SerializeField]
    protected bool                 isMoving =          false;

    /// <summary>Backing field for <see cref="IsGrounded"/>.</summary>
    [SerializeField]
    protected bool                 isGrounded =        false;

    /// <summary>Backing field for <see cref="UseGravity"/>.</summary>
    [SerializeField]
    protected bool                 useGravity =        true;


    /// <summary>Backing field for <see cref="Speed"/>.</summary>
    [SerializeField]
    protected float                speed =             1;

    /// <summary>Backing field for <see cref="SpeedCoef"/>.</summary>
    [SerializeField]
    protected float                speedCoef =         1;


    /// <summary>
    /// Physics collider of the object.
    /// </summary>
    [SerializeField]
    protected new Collider2D       collider =          null;

    /// <summary>
    /// Rigidbody of the object (should be Kinematic).
    /// This should only be used by moving its position to update the collider one.
    /// </summary>
    [SerializeField]
    protected new Rigidbody2D      rigidbody =         null;


    /// <summary>
    /// Obstacles layers used for this object collisions with <see cref="contactFilter"/>.
    /// </summary>
    [SerializeField]
    protected LayerMask            obstaclesMask =     new LayerMask();


    /// <summary>Backing field for <see cref="Velocity"/>.</summary>
    [SerializeField]
    protected Vector2              velocity =          Vector2.zero;


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
    public bool         IsGrounded
    {
        get { return isGrounded; }
        protected set
        {
            isGrounded = value;
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


    /**********************************
     *********     MEMORY     *********
     *********************************/

    /// <summary>
    /// Contact filter used to detect collisions.
    /// </summary>
    protected ContactFilter2D       contactFilter =     new ContactFilter2D();

    /// <summary>
    /// This object position at previous frame.
    /// Updated during late update.
    /// </summary>
    protected Vector2               lastPosition =      new Vector2();
    #endregion

    #endregion

    #region Methods

    #region Original Methods

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

            // Perform velocity movement
            DoPerformMovement(velocity * Time.deltaTime);

            // X velocity
            if (velocity.x != 0)
            {
                //velocity.x *= .99f;
                if (Mathf.Abs(velocity.x) < .01f) velocity.x = 0;
            }

            // Y velocity
            if (velocity.y != 0) velocity.y += Physics2D.gravity.y * Time.deltaTime;
            continue;
        }

        cApplyVelocity = null;
    }


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
        // Get movement distance and return in inferior to minimum required
        float _distance = _movement.magnitude;
        if (_distance < MIN_MOVEMENT_DISTANCE) return false;

        bool _isGrounded = false;
        RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];

        // Cast the collider in the movement direction to detect collisions
        int _hitAmount = collider.Cast(_movement, contactFilter, _hitBuffer, _distance + Physics2D.defaultContactOffset);

        for (int _i = 0; _i < _hitAmount; _i++)
        {
            // Set grounded status if needed
            //_isGrounded = _hitBuffer[_i].normal.x == 0;

            //if (velocity.x != 0) velocity.x *= 1 - Mathf.Abs(_hitBuffer[_i].normal.x);

            // Set distance to closest hit one
            float _hitDistance = _hitBuffer[_i].distance - Physics2D.defaultContactOffset;
            if (_hitDistance < _distance) _distance = _hitDistance;

            if (_movement.y != 0)
            {
                if (_hitBuffer[_i].normal.y == 1f)
                {
                    _isGrounded = true;
                    if (velocity.y < 0) velocity.y = 0;
                }
                else if (_hitBuffer[_i].normal.y == -1)
                {
                    velocity.y = 0;
                }
            }
        }

        if ((isGrounded != _isGrounded) && (_movement.y != 0))
        {
            Debug.Log("Ground => " + _isGrounded);
            IsGrounded = _isGrounded;
        }

        // Return in movement inferior to minimum distance required
        if (_distance < MIN_MOVEMENT_DISTANCE) return true;

        // Now move the object to its new position
        MoveObject((Vector2)transform.position + (_movement.normalized * _distance));

        return _hitAmount > 0;
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
    }

    // LateUpdate is called after all Update functions have been called
    protected virtual void LateUpdate()
    {
        UpdatePosition();
    }

    // This function is called when the object becomes enabled and active
    private void OnEnable()
    {
        if (!collider) collider = GetComponent<Collider2D>();
        if (!rigidbody) rigidbody = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Get initial position
        lastPosition = transform.position;

        // Set object contact filter
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = obstaclesMask;
    }
    #endregion

    #endregion
}
