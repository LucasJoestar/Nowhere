using EnhancedEditor;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movable : MonoBehaviour
{
    #region Fields / Properties

    #region Constants
    /*********************************
     *******     CONSTANTS     *******
     ********************************/

    /// <summary>
    /// Maximum distance when casting collider to detect collision from closest hit.
    /// </summary>
    protected const float                   castMaxDistanceDetection =      .001f;
    #endregion

    #region Parameters
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>Backing field for <see cref="IsAwake"/>.</summary>
    [SerializeField, PropertyField]
    protected bool                          isAwake =                       true;

    /// <summary>Backing field for <see cref="IsFacingRight"/>.</summary>
    [SerializeField, PropertyField]
    protected bool                          isFacingRight =                 true;

    /// <summary>Backing field for <see cref="IsGrounded"/>.</summary>
    [SerializeField, PropertyField]
    protected bool                          isGrounded =                    false;

    /// <summary>Backing field for <see cref="UseGravity"/>.</summary>
    [SerializeField, PropertyField]
    protected bool                          useGravity =                    true;


    /// <summary>Backing field for <see cref="Speed"/>.</summary>
    [SerializeField]
    protected float                         speed =                         1;

    /// <summary>Backing field for <see cref="SpeedCoef"/>.</summary>
    [SerializeField, PropertyField]
    protected float                         speedCoef =                     1;


    /// <summary>
    /// Physics collider of the object.
    /// </summary>
    [SerializeField, Required]
    protected new Collider2D                collider =                      null;

    /// <summary>
    /// Rigidbody of the object (should be Kinematic).
    /// This should only be used by moving its position to update the collider one.
    /// </summary>
    [SerializeField, Required]
    protected new Rigidbody2D               rigidbody =                     null;


    /// <summary>
    /// Velocity system of the object.
    /// Used to separate forces from instant forces and object movement.
    /// </summary>
    [SerializeField]
    protected Velocity                      velocity =                      new Velocity();


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
            #if UNITY_EDITOR
            if (!Application.isPlaying) return;
            #endif

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
            #if UNITY_EDITOR
            if (!Application.isPlaying) return;
            #endif

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
    public Vector2  Velocity    { get { return velocity.GetVelocity(); } }
    #endregion

    #region Events
    /******************************
     *******     EVENTS     *******
     *****************************/

    /// <summary>
    /// Called when hitting colliders during movements,
    /// with an array of all raycast hits as parameter.
    /// </summary>
    public event Action<RaycastHit2D[]>     OnMovementHitCallback =     null;
    #endregion

    #region Memory
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
    /*********************************
     *********     SPEED     *********
     ********************************/

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
     *****     VELOCITY MEDIATORS     *****
     *************************************/

    /// <summary>
    /// Add an external force to this object movement
    /// that will be reduce over time.
    /// 
    /// Override this method to add extra behaviours.
    /// </summary>
    /// <param name="_force">Force to add to this object movement.</param>
    public virtual void AddForce(Vector2 _force) => velocity.Force += _force;

    /// <summary>
    /// Add an external force to this object movement
    /// for this frame only.
    /// 
    /// Override this method to add extra behaviours.
    /// </summary>
    /// <param name="_instantForce">Force to add to this object movement.</param>
    public virtual void AddInstantForce(Vector2 _instantForce) => velocity.InstantForce += _instantForce;

    /// <summary>
    /// Add gravity force to the object.
    /// </summary>
    protected void AddGravity()
    {
        // Only add gravity force if needed
        if (velocity.Force.y <= GameManager.ProgramSettings.GravityMinYForce) return;

        AddForce(new Vector2(0, Mathf.Max(Physics2D.gravity.y * Time.deltaTime, GameManager.ProgramSettings.GravityMinYForce - velocity.Force.y)));
    }


    /// <summary>
    /// Makes the object move on its own.
    /// This do some extra things in addition to the <see cref="AddVelocity(Vector2)"/>
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

        velocity.Movement += _movement;
    }


    /**********************************
     ******     CALCULATIONS     ******
     *********************************/

    /// <summary>
    /// Calculates new velocity value after a movement.
    /// </summary>
    /// <param name="_hits">All objects hit during travel.</param>
    protected virtual void CalculVelocityAfterMovement(RaycastHit2D[] _hits)
    {
        // Reset force if hit something on force movement
        foreach (RaycastHit2D _hit in _hits)
        {
            if (_hit.normal.x != 0) velocity.Force.x = 0;
            if (_hit.normal.y != 0) velocity.Force.y = 0;

            if (velocity.Force == Vector2.zero) break;
        }

        // Slowly decrease force if not null
        if (isGrounded && (velocity.Force.x != 0))
        {
            float _maxDelta = GameManager.ProgramSettings.GroundDecelerationForce * Time.deltaTime;
            if ((velocity.Movement.x != 0) && (Mathf.Sign(velocity.Movement.x) != Mathf.Sign(velocity.Force.x)))
            {
                _maxDelta = Mathf.Max(_maxDelta, Mathf.Abs(velocity.Movement.x));
            }

            velocity.Force.x = Mathf.MoveTowards(velocity.Force.x, 0, _maxDelta);
        }
        if ((velocity.Force.y != 0) && (velocity.Movement.y != 0) && (Mathf.Sign(velocity.Movement.y) != Mathf.Sign(velocity.Force.y)))
        {
            velocity.Force.y = Mathf.MoveTowards(velocity.Force.y, 0, Time.deltaTime * Mathf.Abs(velocity.Movement.y));
        }

        // Reset instant force and movement
        velocity.InstantForce = velocity.Movement = Vector2.zero;
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
        // Cache velocity & initialize hit buffer
        Vector2 _velocity = Velocity;
        _hitResults = new RaycastHit2D[16];

        // Cast collider and get hit informations
        int _amount = CastCollider(_velocity, _hitResults, out float _distance);

        // If nothing is hit, just return velocity
        if (_amount == 0)
        {
            _hitResults = new RaycastHit2D[] { };
            return _velocity;
        }

        // Get movement before collision
        Vector2 _movement = _velocity.normalized * _distance;
        Vector2 _extraVelocity = _velocity.normalized * (_velocity.magnitude - _distance);
        for (int _i = 0; _i < _amount; _i++)
        {
            // Reduce extra movement depending on impact normals
            if ((_extraVelocity.x != 0) && (_hitResults[_i].normal.x != 0) && (Math.Sign(_extraVelocity.x) != Mathf.Sign(_hitResults[_i].normal.x)))
            {
                _extraVelocity.x = 0;
                if (_extraVelocity.y == 0) break;
            }
            if ((_extraVelocity.y != 0) && (_hitResults[_i].normal.y != 0) && (Math.Sign(_extraVelocity.y) != Mathf.Sign(_hitResults[_i].normal.y)))
            {
                _extraVelocity.y = 0;
                if (_extraVelocity.x == 0) break;
            }
        }

        // If no extra movement is necessary, just resize hit array
        if (_extraVelocity == Vector2.zero) Array.Resize(ref _hitResults, _amount);
        // Otherwise perform extra cast, then add new movement and hit results
        else
        {
            RaycastHit2D[] _extraHitResults = new RaycastHit2D[16];
            int _extraAmount = CastCollider(_extraVelocity, _extraHitResults, out _distance);

            // If hit nothing, just add extra movement and resize initial hits array
            if (_extraAmount == 0)
            {
                _movement += _extraVelocity;
                Array.Resize(ref _hitResults, _amount);
            }
            else
            {
                // Add new closest hit movement
                _movement += _extraVelocity.normalized * _distance;

                // Then, add new cast hits
                RaycastHit2D[] _finalHitResults = new RaycastHit2D[_amount + _extraAmount];
                for (int _i = 0; _i < _amount; _i++) _finalHitResults[_i] = _hitResults[_i];
                for (int _i = 0; _i < _extraAmount; _i++) _finalHitResults[_i + _amount] = _extraHitResults[_i];

                _hitResults = _finalHitResults;
            }
        }

        // Return final movement
        return _movement;
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
        bool _isGrounded = false;
        Collider2D[] _collisions = new Collider2D[16];
        int _count = collider.OverlapCollider(contactFilter, _collisions);

        for (int _i = 0; _i < _count; _i++)
        {
            ColliderDistance2D _distance = collider.Distance(_collisions[_i]);
            if (!_distance.isOverlapped) continue;
            
            // Check grounded status if using gravity
            if (useGravity && _distance.normal.y < 0) _isGrounded = true;
            rigidbody.position += _distance.normal * _distance.distance;
        }

        // Set grounded value if different
        if (isGrounded != _isGrounded) SetGrounded(_isGrounded);
    }

    /// <summary>
    /// Set object grounded status.
    /// 
    /// Override this method to add extra verifications before set,
    /// like an additional cast for the player controller.
    /// </summary>
    /// <param name="_isGrounded">New grounded value.</param>
    protected virtual void SetGrounded(bool _isGrounded) => IsGrounded = _isGrounded;


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
    protected virtual Vector2 ApplyVelocity()
    {
        // If no velocity, return
        if (Velocity == Vector2.zero) return Vector2.zero;

        // Calcul movement collisions and refresh position
        Vector2 _movement = CalculVelocityCollisions(out RaycastHit2D[] _hitBuffer);
        Vector2 _lastPosition = rigidbody.position;

        rigidbody.position += _movement;
        RefreshPosition();

        // Call hit method if collision
        if (_hitBuffer.Length > 0) OnMovementHit(_hitBuffer);

        // Set velocity and return real movement after position refresh
        _movement = rigidbody.position - _lastPosition;

        CalculVelocityAfterMovement(_hitBuffer);
        return _movement;
    }

    /// <summary>
    /// Called when hitting colliders during movement.
    /// 
    /// Override this method to add extra behaviours and feedback.
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
        // Get missing components
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
