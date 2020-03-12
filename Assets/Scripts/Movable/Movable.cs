using EnhancedEditor;
using System;
using UnityEngine;

namespace Nowhere
{
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
        protected const float               castMaxDistanceDetection =      .001f;
        #endregion

        #region Parameters
        /**********************************
         *********     FIELDS     *********
         *********************************/

        [Section("Movable", 50, 0, order = 0), HorizontalLine(2, SuperColor.Sapphire, order = 1)]

        /// <summary>
        /// Physics collider of the object.
        /// </summary>
        [SerializeField, Required]
        protected new Collider2D            collider =                      null;

        /// <summary>
        /// Rigidbody of the object (should be Kinematic).
        /// This should only be used by moving its position to update the collider one.
        /// </summary>
        [SerializeField, Required]
        protected new Rigidbody2D           rigidbody =                     null;


        [HorizontalLine(2, SuperColor.Crimson)]


        /// <summary>Backing field for <see cref="IsAwake"/>.</summary>
        [SerializeField, PropertyField]
        protected bool                      isAwake =                       true;

        /// <summary>Backing field for <see cref="UseGravity"/>.</summary>
        [SerializeField, PropertyField]
        protected bool                      useGravity =                    true;


        [HorizontalLine(2, SuperColor.Green)]


        /// <summary>Backing field for <see cref="IsFacingRight"/>.</summary>
        [SerializeField, ReadOnly]
        protected bool                      isFacingRight =                 true;

        /// <summary>Backing field for <see cref="IsGrounded"/>.</summary>
        [SerializeField, ReadOnly]
        protected bool                      isGrounded =                    false;


        [HorizontalLine(2, SuperColor.Indigo)]


        /// <summary>Backing field for <see cref="Speed"/>.</summary>
        [SerializeField]
        protected float                     speed =                         1;

        /// <summary>Backing field for <see cref="SpeedCoef"/>.</summary>
        [SerializeField, PropertyField]
        protected float                     speedCoef =                     1;


        [HorizontalLine(2, SuperColor.HarvestGold)]


        /// <summary>
        /// Velocity system of the object.
        /// Used to separate forces from instant forces and object movement.
        /// </summary>
        [SerializeField]
        protected Velocity                  velocity =                      new Velocity();


        /**********************************
         *******     PROPERTIES     *******
         *********************************/

        /// <summary>
        /// If awake, the object position will be updated according to velocity at the end of each frame.
        /// </summary>
        public bool IsAwake
        {
            get { return isAwake; }
            set
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) return;
                isAwake = !value;
                #endif

                if (value != isAwake)
                {
                    if (value) UpdateManager.SubscribeToUpdate(UpdateObject, UpdateModeTimeline.EndOfFrame);
                    else UpdateManager.UnsubscribeToUpdate(UpdateObject, UpdateModeTimeline.EndOfFrame);
                }

                isAwake = value;
            }
        }

        /// <summary>
        /// Indicates if the object is facing the right side of the screen or the left one.
        /// </summary>
        public bool IsFacingRight
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
        public bool IsGrounded
        {
            get { return isGrounded; }
            protected set
            {
                isGrounded = value;
                OnSetGrounded();
            }
        }

        /// <summary>
        /// If true, the object will be affected by physics gravity and ground attraction.
        /// </summary>
        public bool UseGravity
        {
            get { return useGravity; }
            set
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) return;
                useGravity = !value;
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
        public float Speed
        {
            get { return speed; }
        }

        /// <summary>
        /// Movement speed coefficient.
        /// </summary>
        public float SpeedCoef
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
        public Vector2 Velocity { get { return velocity.GetVelocity() * speedCoef; } }
        #endregion

        #region Events
        /******************************
         *******     EVENTS     *******
         *****************************/

        /// <summary>
        /// Called when hitting colliders during movements,
        /// with an array of all raycast hits as parameter.
        /// </summary>
        public event Action<RaycastHit2D[]> OnMovementHitCallback = null;
        #endregion

        #region Memory
        /**********************************
         *********     MEMORY     *********
         *********************************/

        /// <summary>
        /// Contact filter used to detect collisions.
        /// </summary>
        protected ContactFilter2D   contactFilter =     new ContactFilter2D();

        #if UNITY_EDITOR
        /// <summary>
        /// This object position at previous frame.
        /// Used to update position when moving object in editor.
        /// </summary>
        [SerializeField, HelpBox("Last position of the object : used for debug and update object after moving it in scene editor", HelpBoxType.Warning)]
        protected Vector2           lastPosition =      new Vector2();
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
        public bool Flip(bool _doFaceRight)
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
        #endregion

        #region Velocity
        /**************************************
         *****     VELOCITY MEDIATORS     *****
         *************************************/

        /// <summary>
        /// Add an external force to this object movement
        /// that will be decreased over time.
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
        /// Makes the object move on its own.
        /// This do some extra things like flipping the object
        /// and more due to when it move on its own
        /// and not pushed by an external force.
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


        /*******************************
         *******     GRAVITY     *******
         ******************************/

        /// <summary>
        /// Add gravity force to the object.
        /// Called every frame while the object is awake and using gravity.
        /// 
        /// Override this method to add extra behaviours,
        /// like applying a coefficient to gravity.
        /// </summary>
        protected virtual void AddGravity() => AddGravity(1, 1);

        /// <summary>
        /// Add gravity force to the object.
        /// </summary>
        /// <param name="_gravityCoef">Coefficient applied to gravity.</param>
        /// <param name="_gravityMinValueCoef">Coefficient applied to minimum allowed gravity value.</param>
        protected void AddGravity(float _gravityCoef, float _gravityMinValueCoef)
        {
            // Get minimum allowed gravity value
            float _minGravityValue = GameManager.ProgramSettings.MinGravity * _gravityMinValueCoef;

            // Only add gravity force if needed
            if (velocity.Force.y <= _minGravityValue) return;

            AddForce(new Vector2(0, Mathf.Max(Physics2D.gravity.y * _gravityCoef * Time.deltaTime, _minGravityValue - velocity.Force.y)));
        }


        /**********************************
         ******     CALCULATIONS     ******
         *********************************/

        /// <summary>
        /// Calculates new velocity value after a movement.
        /// </summary>
        /// <param name="_hits">All objects hit during travel.</param>
        protected virtual void ComputeVelocityAfterMovement(RaycastHit2D[] _hits)
        {
            // Reset force according to hit surfaces
            if (velocity.Force != Vector2.zero)
            {
                foreach (RaycastHit2D _hit in _hits)
                {
                    if (_hit.normal.x != 0)
                    {
                        velocity.Force.x = 0;
                        if (velocity.Force.y == 0) break;
                    }
                    if (_hit.normal.y != 0)
                    {
                        velocity.Force.y = 0;
                        if (velocity.Force.x == 0) break;
                    }
                }
            }

            // Reset instant force and movement
            velocity.InstantForce = velocity.Movement = Vector2.zero;
        }

        /// <summary>
        /// Compute velocity value before movement.
        /// </summary>
        protected virtual void ComputeVelocityBeforeMovement()
        {
            // Slowly decrease force if not null
            if (velocity.Force.x != 0)
            {
                // Reduce force according to ground or air friction
                float _maxDelta = isGrounded ?
                                  GameManager.ProgramSettings.GroundDecelerationForce :
                                  GameManager.ProgramSettings.AirDecelerationForce;

                // If going to opposite force direction, accordingly reduce force
                if (velocity.Movement.x != 0)
                {
                    if (Mathm.HaveDifferentSign(velocity.Force.x, velocity.Movement.x))
                    {
                        _maxDelta = Mathf.Max(_maxDelta, Mathf.Abs(velocity.Movement.x) * 2);

                        // Reduce force depending on movement
                        if (Mathf.Abs(velocity.Force.x) < _maxDelta)
                            _maxDelta *= 1.5f;

                        else if (Mathf.Abs(velocity.Force.x) < (_maxDelta * 2))
                            _maxDelta *= 1.25f;

                        else _maxDelta *= 1.1f;
                    }
                }

                // Reduce movement according to velocity
                velocity.Movement.x = Mathf.MoveTowards(velocity.Movement.x, 0, Mathf.Abs(velocity.Force.x));

                // Reduce velocity force
                if (_maxDelta != 0) velocity.Force.x = Mathf.MoveTowards(velocity.Force.x, 0, _maxDelta * Time.deltaTime);
            }

            // If going to opposite force direction, accordingly reduce force
            if (Mathm.HaveDifferentSignAndNotNull(velocity.Force.y, velocity.Movement.y))
            {
                float _maxDelta = Mathf.Abs(velocity.Movement.y);

                // Reduce movement
                velocity.Movement.y = Mathf.MoveTowards(velocity.Movement.y, 0, Mathf.Abs(velocity.Force.y));

                // Reduce force
                velocity.Force.y = Mathf.MoveTowards(velocity.Force.y, 0, _maxDelta * Time.deltaTime);
            }
        }
        #endregion

        #region Movements
        /*********************************
         ********     PHYSICS     ********
         ********************************/

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
                if (Mathm.HaveDifferentSignAndNotNull(_extraVelocity.x, _hitResults[_i].normal.x))
                {
                    _extraVelocity.x = 0;
                    if (_extraVelocity.y == 0) break;
                }
                if (Mathm.HaveDifferentSignAndNotNull(_extraVelocity.y, _hitResults[_i].normal.y))
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

                // Resize array
                Array.Resize(ref _hitResults, _amount + _extraAmount);

                // If hit nothing, just add extra movement and resize initial hits array
                if (_extraAmount == 0)
                    _movement += _extraVelocity;

                // Otherwise, add extra hits and closest hit movement
                else
                {
                    Array.Copy(_extraHitResults, 0, _hitResults, _amount, _extraAmount);
                    _movement += _extraVelocity.normalized * _distance;
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
            if (isGrounded != _isGrounded)
                IsGrounded = _isGrounded;
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
        protected void ApplyVelocity()
        {
            // If no velocity, return
            if (Velocity == Vector2.zero)
            {
                OnAppliedVelocity(Vector2.zero);
                return;
            }

            // Compute velocity before performing movement
            ComputeVelocityBeforeMovement();

            // Calcul movement collisions and refresh position
            Vector2 _movement = CalculVelocityCollisions(out RaycastHit2D[] _hitBuffer);
            Vector2 _lastPosition = rigidbody.position;

            rigidbody.position += _movement;
            RefreshPosition();

            // Call hit method if collision
            if (_hitBuffer.Length > 0) OnMovementHit(_hitBuffer);

            // Call callback and compute velocity after movement
            OnAppliedVelocity(rigidbody.position - _lastPosition);
            ComputeVelocityAfterMovement(_hitBuffer);
        }

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
        protected virtual void UpdateObject()
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


        /***********************************
         ********     CALLBACKS     ********
         **********************************/

        /// <summary>
        /// Called after velocity has been applied and before new velocity calculs,
        /// with final object movement as parameter.
        /// 
        /// Override this method to add extra behaviours and feedback.
        /// </summary>
        /// <param name="_finalMovement">This object final movement during this frame.</param>
        protected virtual void OnAppliedVelocity(Vector2 _finalMovement) { }

        /// <summary>
        /// Called when hitting colliders during movement.
        /// 
        /// Override this method to add extra behaviours and feedback.
        /// </summary>
        /// <param name="_hits">Raycast hits of touched colliders during movement.</param>
        protected virtual void OnMovementHit(RaycastHit2D[] _hits) => OnMovementHitCallback?.Invoke(_hits);

        /// <summary>
        /// Called when grounded value has been set.
        /// 
        /// Override this method to add extra behaviours and feedback.
        /// </summary>
        protected virtual void OnSetGrounded()
        {
            // Reduce x force if not null when get grounded
            if (isGrounded && (velocity.Force.x != 0))
                velocity.Force.x *= GameManager.ProgramSettings.OnGroundedHorizontalForceMultiplier;
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
            if (isAwake) UpdateManager.UnsubscribeToUpdate(UpdateObject, UpdateModeTimeline.EndOfFrame);
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
            if (isAwake) UpdateManager.SubscribeToUpdate(UpdateObject, UpdateModeTimeline.EndOfFrame);
            if (useGravity) UpdateManager.SubscribeToUpdate(AddGravity, UpdateModeTimeline.Update);
        }
        #endregion

        #endregion
    }
}
