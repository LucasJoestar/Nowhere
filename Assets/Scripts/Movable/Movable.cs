using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movable : MonoBehaviour
{
    #region Events
    /******************************
     *******     EVENTS     *******
     *****************************/

    public event Action<RaycastHit2D[]> OnMovementHitCallback = null;
    #endregion

    #region Fields / Properties

    #region Constants
    /*********************************
     *******     CONSTANTS     *******
     ********************************/

    /// <summary>
    /// Maximum distance when casting collider to detect collision from closest hit.
    /// </summary>
    protected const float           castMaxDistanceDetection =      .01f;
    #endregion

    #region Parameters
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>Backing field for <see cref="IsAwake"/>.</summary>
    [SerializeField]
    protected bool                  isAwake =                       true;

    /// <summary>Backing field for <see cref="IsFacingRight"/>.</summary>
    [SerializeField]
    protected bool                  isFacingRight =                 true;

    /// <summary>Backing field for <see cref="IsGrounded"/>.</summary>
    [SerializeField]
    protected bool                  isGrounded =                    false;

    /// <summary>Backing field for <see cref="UseGravity"/>.</summary>
    [SerializeField]
    protected bool                  useGravity =                    true;


    /// <summary>Backing field for <see cref="Speed"/>.</summary>
    [SerializeField]
    protected float                 speed =                         1;

    /// <summary>Backing field for <see cref="SpeedCoef"/>.</summary>
    [SerializeField]
    protected float                 speedCoef =                     1;


    /// <summary>
    /// Physics collider of the object.
    /// </summary>
    [SerializeField]
    protected new Collider2D        collider =                      null;

    /// <summary>
    /// Rigidbody of the object (should be Kinematic).
    /// This should only be used by moving its position to update the collider one.
    /// </summary>
    [SerializeField]
    protected new Rigidbody2D       rigidbody =                     null;


    /// <summary>Backing field for <see cref="Velocity"/>.</summary>
    [SerializeField]
    protected Vector2               velocity =                      Vector2.zero;


    /**********************************
     *******     PROPERTIES     *******
     *********************************/

    /// <summary>
    /// If awake, the object position will be updated according to velocity at the end of each frame.
    /// </summary>
    public bool     IsAwake
    {
        get { return isAwake; }
        set
        {
            if (value != isAwake)
            {
                if (value) UpdateManager.SubscribeToUpdate(UpdateMovable, UpdateModeTimeline.EndOfFrame);
                else UpdateManager.UnsubscribeToUpdate(UpdateMovable, UpdateModeTimeline.EndOfFrame);
            }

            isAwake = value;
        }
    }

    /// <summary>
    /// Indicates if the object is facing the right side of the screen or the left one.
    /// </summary>
    public bool     IsFacingRight
    {
        get { return isFacingRight; }
        protected set
        {
            isFacingRight = value;
        }
    }

    /// <summary>
    /// Indicates if the object is touching ground.
    /// </summary>
    public bool     IsGrounded
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
    public bool     UseGravity
    {
        get { return useGravity; }
        set
        {
            if (value != useGravity)
            {
                if (value) UpdateManager.SubscribeToUpdate(AddGravity, UpdateModeTimeline.Update);
                else UpdateManager.UnsubscribeToUpdate(AddGravity, UpdateModeTimeline.Update);
            }

            useGravity = value;
        }
    }


    /// <summary>
    /// Base movement speed of the object (per second).
    /// </summary>
    public float    Speed
    {
        get { return speed; }
    }

    /// <summary>
    /// Movement speed coefficient.
    /// </summary>
    public float    SpeedCoef
    {
        get { return speedCoef; }
        protected set
        {
            speedCoef = value;
        }
    }


    /// <summary>
    /// Movement of the object during this frame.
    /// </summary>
    public Vector2  Velocity
    {
        get { return velocity; }
        protected set
        {
            velocity = value;
        }
    }
    #endregion

    #region Coroutines & Memory
    /**********************************
     *******     COROUTINES     *******
     *********************************/

    // Nothing to see here...


    /**********************************
     *********     MEMORY     *********
     *********************************/

    /// <summary>
    /// Contact filter used to detect collisions.
    /// </summary>
    protected ContactFilter2D       contactFilter =     new ContactFilter2D();

    #if UNITY_EDITOR
    /// <summary>
    /// This object position at previous frame.
    /// Used to update position when moving object in editor.
    /// </summary>
    protected Vector2               lastPosition =      new Vector2();
    #endif
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
    /// <param name="_doFaceRight">Should the object face the right side of screen or the left one.</param>
    /// <returns>Returns true if the object flipped, false if was already looking indicated side.</returns>
    public virtual bool Flip(bool _doFaceRight)
    {
        if (isFacingRight == _doFaceRight) return false;

        Flip();
        return true;
    }
    #endregion

    #region Speed
    /// <summary>
    /// Add a coefficient to this object speed.
    /// </summary>
    /// <param name="_coef">Coefficient to add.</param>
    public void AddSpeedCoefficient(float _coef) { if (_coef != 0) SpeedCoef *= _coef; }

    /// <summary>
    /// Remove a coefficient from this object speed.
    /// </summary>
    /// <param name="_coef">Coefficient to remove.</param>
    public void RemoveSpeedCoefficient(float _coef) { if (_coef != 0) SpeedCoef /= _coef; }

    /// <summary>
    /// Get this object movement speed for this frame.
    /// </summary>
    /// <returns>Returns this frame speed.</returns>
    protected virtual float GetMovementSpeed() => speed * speedCoef * Time.deltaTime;
    #endregion

    #region Movements
    /**************************************
     *******     MOV. MEDIATORS     *******
     *************************************/

    /// <summary>
    /// Add a force to this object movement.
    /// </summary>
    /// <param name="_force">Movement force to add to the object.</param>
    public void AddForce(Vector2 _force) => Velocity += _force;

    /// <summary>
    /// Add gravity force to the object.
    /// </summary>
    protected void AddGravity() => AddForce(new Vector2(0, Physics2D.gravity.y * Time.deltaTime));


    /// <summary>
    /// Move the object in a given direction.
    /// </summary>
    /// <param name="_dir">Direction to move the object to.</param>
    public virtual void MoveInDirection(Vector2 _dir) => Move(_dir.normalized * GetMovementSpeed());

    /// <summary>
    /// Move the object in direction of a given position.
    /// </summary>
    /// <param name="_position">Position to move the object to.</param>
    public virtual void MoveTo(Vector2 _position)
    {
        float _speed = GetMovementSpeed();
        Vector2 _movement = _position - (Vector2)transform.position;
        if (_movement.magnitude > _speed) _movement = _movement.normalized * _speed;

        Move(_movement);
    }

    /// <summary>
    /// Makes the object travel until reaching a given position.
    /// </summary>
    /// <param name="_position">Position to travel to.</param>
    public void TravelTo(Vector2 _position)
    {
        // Implement travel here
    }

    /// <summary>
    /// Makes the object move on its own.
    /// This do some extra things in addition to the <see cref="AddForce(Vector2)"/>
    /// method like flipping the object and other things due to when the object
    /// move on its own and not pushed by an external force.
    /// 
    /// Override this method do add extra behaviours.
    /// </summary>
    /// <param name="_movement">Movement to perform.</param>
    protected virtual void Move(Vector2 _movement)
    {
        // Flip object if not facing movement direction
        if ((_movement.x != 0) && (Mathf.Sign(_movement.x) != isFacingRight.ToSign())) Flip();

        AddForce(_movement);
    }


    /**************************************
     ******     MOV. SYSTEM COGS     ******
     *************************************/

    /// <summary>
    /// Set this object position.
    /// Use this instead of setting <see cref="Transform.position"/>.
    /// </summary>
    /// <param name="_position">New position of the object.</param>
    public void SetPosition(Vector2 _position)
    {
        // Do nothing if position has not changed
        if (rigidbody.position == _position) return;

        // Set rigidbody and refresh position
        rigidbody.position = _position;
        RefreshPosition();
    }

    /// <summary>
    /// Apply stored velocity and move the object.
    /// </summary>
    /// <returns>Returns final movement.</returns>
    protected Vector2 ApplyVelocity()
    {
        // If no velocity, return
        if (velocity == Vector2.zero) return velocity;

        // Calcul movement collisions and refresh position if needed
        Vector2 _movement = CalculVelocityCollisions(out RaycastHit2D[] _hitBuffer);
        if (_movement != Vector2.zero)
        {
            rigidbody.position += _movement;
            RefreshPosition();
        }

        // Set grounded value
        if (useGravity)
        {
            bool _isGrounded = _hitBuffer.Any(h => h.normal.y == 1);
            if (_isGrounded != isGrounded) IsGrounded = _isGrounded;
        }

        // Call hit method if collision
        if (_hitBuffer.Length > 0) OnMovementHit(_hitBuffer);

        // Reset velocity and return movement
        Velocity = Vector2.zero;
        return _movement;
    }


    /// <summary>
    /// Calculs object collisions and final movement from velocity.
    /// 
    /// Override this method to add extra behaviours,
    /// like different cast method or obstacle consideration.
    /// </summary>
    /// <param name="_hitResults">All colliders hit during movement.</param>
    /// <returns>Returns final object movement.</returns>
    protected virtual Vector2 CalculVelocityCollisions(out RaycastHit2D[] _hitResults)
    {
        // Initialize hit buffer
        _hitResults = new RaycastHit2D[16];

        // Cast collider, and return modified velocity if hit something
        int _amount = CastCollider(velocity, _hitResults, out float _distance);
        if (_amount > 0)
        {
            Vector2 _extraVelocity = velocity;
            for (int _i = 0; _i < _amount; _i++)
            {
                // Do extra cast here
            }
        }

        Array.Resize(ref _hitResults, _amount);

        if (_amount > 0) return velocity.normalized * _distance;
        return velocity;
    }

    /// <summary>
    /// Cast collider in a given movement direction and get informations about hit colliders.
    /// </summary>
    /// <param name="_movement">Movement to perform.</param>
    /// <param name="_hitBuffer">Array of raycast hit to fill with detected collision informations.</param>
    /// <param name="_distance">Distance on which to perform cast.</param>
    /// <returns>Returns the amount of hit colliders.</returns>
    protected int CastCollider(Vector2 _movement, RaycastHit2D[] _hitBuffer, out float _distance)
    {
        // Get initial movement distance
        _distance = _movement.magnitude;

        // Cast the collider in the movement direction to detect collisions
        int _hitAmount = collider.Cast(_movement, contactFilter, _hitBuffer, _distance);
        if (_hitAmount > 0)
        {
            // Hits are already sorted by distance, so simply get closest one
            _distance = _hitBuffer[0].distance;

            // Retains only closest hits by ignoring
            // all hits with greater distance than shortest one
            for (int _i = 1; _i < _hitAmount; _i++)
            {
                if ((_hitBuffer[_i].distance + castMaxDistanceDetection) > _distance) return _i;
            }
        }

        // Return hit amount
        return _hitAmount;
    }

    /// <summary>
    /// Eject the object from physics collisions.
    /// </summary>
    protected void ExtractFromCollisions()
    {
        Collider2D[] _collisions = new Collider2D[16];
        int _count = collider.OverlapCollider(contactFilter, _collisions);

        for (int _i = 0; _i < _count; _i++)
        {
            ColliderDistance2D _distance = collider.Distance(_collisions[_i]);
            if (!_distance.isOverlapped) continue;

            rigidbody.position += _distance.normal * _distance.distance;
        }
    }

    /// <summary>
    /// Called when hitting colliders during movement.
    /// </summary>
    /// <param name="_hits">Raycast hits of touched colliders during movement.</param>
    protected virtual void OnMovementHit(RaycastHit2D[] _hits) => OnMovementHitCallback?.Invoke(_hits);

    /// <summary>
    /// Refresh object position based on rigidbody.
    /// </summary>
    protected virtual void RefreshPosition()
    {
        // First, extract the rigidbody from potential collisions
        ExtractFromCollisions();

        // Then, update position if needed
        if ((Vector2)transform.position != rigidbody.position) transform.position = rigidbody.position;
    }

    /// <summary>
    /// Update this object, from position based on velocity to related informations.
    /// Called at the end of the frame.
    /// 
    /// Override this method to add extra behaviours before or after update.
    /// </summary>
    protected virtual void UpdateMovable()
    {
        #if UNITY_EDITOR
        // Refresh position if moving object in editor
        if (lastPosition != rigidbody.position) RefreshPosition();
        #endif

        // Apply stored velocity
        ApplyVelocity();

        #if UNITY_EDITOR
        // Update last position
        lastPosition = rigidbody.position;
        #endif
    }
    #endregion

    #endregion

    #region Unity Methods
    /*********************************
     *****     MONOBEHAVIOUR     *****
     ********************************/

    // Awake is called when the script instance is being loaded
    protected virtual void Awake()
	{
        if (!collider) collider = GetComponent<Collider2D>();
        if (!rigidbody) rigidbody = GetComponent<Rigidbody2D>();
    }

    // Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy
    protected virtual void OnDestroy()
    {
        // Unsubscribe update methods
        if (isAwake) UpdateManager.UnsubscribeToUpdate(UpdateMovable, UpdateModeTimeline.EndOfFrame);
        if (useGravity) UpdateManager.UnsubscribeToUpdate(AddGravity, UpdateModeTimeline.Update);
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        #if UNITY_EDITOR
        // Get initial position
        lastPosition = transform.position;
        #endif

        // Set object contact filter
        contactFilter.layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
        contactFilter.useLayerMask = true;

        // Subscribe update methods
        if (isAwake) UpdateManager.SubscribeToUpdate(UpdateMovable, UpdateModeTimeline.EndOfFrame);
        if (useGravity) UpdateManager.SubscribeToUpdate(AddGravity, UpdateModeTimeline.Update);
    }
    #endregion

    #endregion
}
