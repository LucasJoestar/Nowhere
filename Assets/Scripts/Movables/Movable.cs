// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using System;
using UnityEngine;

namespace Nowhere
{
    /// <summary>
    /// Used to determine collision contacts 
    /// when calculating object movement.
    /// </summary>
    public enum CollisionSystem
    {
        Simple,
        Complex,
        Physics,
        Custom
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class Movable : MonoBehaviour, IMovableUpdate, IPhysicsUpdate
    {
        #region Fields / Properties

        #region Constants
        /*********************************
         *******     CONSTANTS     *******
         ********************************/

        protected const float   castMaxDistanceDetection =      .001f;
        protected const int     collisionSystemRecursionCeil =  3;
        #endregion

        #region Serialized Variables
        /************************************
         ***     SERIALIZED VARIABLES     ***
         ***********************************/

        [Section("MOVABLE", 50, 0, order = 0), HorizontalLine(2, SuperColor.Sapphire, order = 1)]

        [SerializeField, Required] private new Collider2D       collider =      null;
        [SerializeField, Required] private new Rigidbody2D      rigidbody =     null;

        // --------------------------------------------------

        [HorizontalLine(2, SuperColor.Crimson)]

        [SerializeField, PropertyField] protected bool  isAwake =       true;
        [SerializeField, PropertyField] protected bool  useGravity =    true;

        [SerializeField, PropertyField] protected CollisionSystem collisionSystem = CollisionSystem.Complex;

        public bool IsAwake
        {
            get { return isAwake; }
            protected set
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) return;
                isAwake = !value;
                #endif

                if (value != isAwake)
                {
                    isAwake = value;

                    // Manage movable update registration
                    if (value)
                        UpdateManager.Instance.Register((IMovableUpdate)this);

                    else
                        UpdateManager.Instance.Unregister((IMovableUpdate)this);
                }
            }
        }

        public bool UseGravity
        {
            get { return useGravity; }
            protected set
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) return;
                useGravity = !value;
                #endif

                if (value != useGravity)
                {
                    useGravity = value;

                    // Manage physics update registration
                    if (value)
                        UpdateManager.Instance.Register((IPhysicsUpdate)this);

                    else
                        UpdateManager.Instance.Unregister((IPhysicsUpdate)this);
                }
                
            }
        }

        public CollisionSystem CollisionSystem
        {
            get { return collisionSystem; }
            protected set
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) return;
                #endif

                collisionSystem = value;

                // Set collision calcul method
                switch (value)
                {
                    case CollisionSystem.Simple:
                        CollisionSystemDelegate = SimpleCollisionsSystem;
                        break;

                    case CollisionSystem.Complex:
                        CollisionSystemDelegate = ComplexCollisionsSystem;
                        break;

                    case CollisionSystem.Physics:
                        CollisionSystemDelegate = PhysicsCollisionsSystem;
                        break;

                    // Set default as custom
                    default:
                        CollisionSystemDelegate = CustomCollisionsSystem;
                        break;
                }
            }
        }

        // --------------------------------------------------

        [HorizontalLine(2, SuperColor.Green)]

        [SerializeField, ReadOnly] protected int    facingSide =    1;
        [SerializeField, ReadOnly] protected bool   isGrounded =    false;

        [HorizontalLine(2, SuperColor.Indigo)]

        [SerializeField, Min(0)] protected float    speed =         1;
        [SerializeField, Min(0)] protected float    speedCoef =     1;

        // --------------------------------------------------
        //
        // Velocity variables
        //
        // Movable class velocity is composed of 3 Vector2 :
        //  • Force, which is related to external forces, having an impact in duration (like wind)
        //  • Instant Force, also external forces by applied for one frame only (like recoil)
        //  • Movement, the velocity applied by the object itself (like walking)
        //

        [HorizontalLine(2, SuperColor.Pumpkin)]

        [SerializeField] protected Vector2  force =         Vector2.zero;
        [SerializeField] protected Vector2  instantForce =  Vector2.zero;
        [SerializeField] protected Vector2  movement =      Vector2.zero;

        // ------------------------------------------

        #if UNITY_EDITOR

        [HorizontalLine(2, SuperColor.Purple, order = -1)]

        [HelpBox("Previous position is used for debug and refreshing object after moving it in scene editor", HelpBoxType.Warning)]
        [SerializeField, ReadOnly]
        protected Vector2 previousPosition = new Vector2();

        #endif

        #endregion

        #region Variables
        /*********************************
         *******     VARIABLES     *******
         ********************************/

        protected Action CollisionSystemDelegate = null;

        // ------------------------------------------

        protected ContactFilter2D   contactFilter =     new ContactFilter2D();
        protected Vector2           groundNormal =      new Vector2();
        #endregion

        #endregion

        #region Methods

        #region Movable

        #region Flip
        /**********************************
         **********     FLIP     **********
         *********************************/

        /// <summary>
        /// Flip the object.
        /// </summary>
        public virtual void Flip()
        {
            facingSide *= -1;

            Vector3 _scale = transform.localScale;
            transform.localScale = new Vector3(_scale.x * -1, _scale.y, _scale.z);
        }

        /// <summary>
        /// Make the object face a given side.
        /// </summary>
        /// <param name="_facingSide">Side to look (1 for right, -1 for left).</param>
        public void Flip(int _facingSide)
        {
            if (facingSide != _facingSide)
            {
                Flip();
            }
        }
        #endregion

        #region Speed
        /*********************************
         *********     SPEED     *********
         ********************************/

        /// <summary>
        /// Adds a speed coefficient to the object.
        /// </summary>
        public void AddSpeedCoef(float _coef)
        {
            if (_coef != 0)
            {
                speedCoef *= _coef;
            }
        }

        /// <summary>
        /// Remove a speed coefficient from the object.
        /// </summary>
        public void RemoveSpeedCoef(float _coef)
        {
            if (_coef != 0)
            {
                speedCoef /= _coef;
            }
        }
        #endregion

        #region Velocity
        /********************************
         *******     VELOCITY     *******
         *******************************/

        /// <summary>
        /// Adds a force to the object velocity.
        /// Force value is decreased over time.
        /// </summary>
        public virtual void AddForce(Vector2 _force) => force += _force;

        /// <summary>
        /// Adds a force to the object velocity for this frame only.
        /// </summary>
        public virtual void AddInstantForce(Vector2 _instantForce) => instantForce += _instantForce;

        /// <summary>
        /// Moves the object horizontally.
        /// </summary>
        protected virtual void MoveHorizontally(float _movement)
        {
            // Flip object if not facing movement direction
            if (_movement != 0)
            {
                if (Mathf.Sign(_movement) != facingSide)
                    Flip();

                movement.x += _movement;
            }
        }

        /// <summary>
        /// Moves the object vertically.
        /// </summary>
        protected virtual void MoveVertically(float _movement) => movement.y += _movement;

        // ----------------------------------------

        /// <summary>
        /// Get the object velocity movement during this frame.
        /// </summary>
        protected virtual Vector2 GetVelocity() => (((force + movement) * Time.deltaTime) + instantForce) * speedCoef;

        // ----------------------------------------

        float previousXForce =       0;
        float previousXVelocity =    0;

        /// <summary>
        /// Compute velocity value before movement calculs.
        /// </summary>
        protected virtual void ComputeVelocityBeforeMovement()
        {
            // Slowly decrease force over time.
            if (force.x != 0)
            {
                float _maxDelta = isGrounded ?
                                  ProgramSettings.GroundDecelerationForce :
                                  ProgramSettings.AirDecelerationForce;

                // Calculs when going to opposite force direction.
                if (movement.x != 0)
                {
                    if (Mathm.HaveDifferentSign(force.x, movement.x))
                    {
                        _maxDelta = Mathf.Max(_maxDelta, Mathf.Abs(movement.x) * 2);
                        movement.x = Mathf.MoveTowards(movement.x, 0, Mathf.Abs(force.x) * Time.deltaTime);
                    }
                    else
                        movement.x = Mathf.Max(0, Mathf.Abs(movement.x) - Mathf.Abs(force.x)) * Mathf.Sign(movement.x);
                }

                // ------------------------------

                // Calculs when going to opposite force direction,
                // compared to previous frame.
                float _previousXOtherVelocity = previousXVelocity - previousXForce;

                if (Mathm.HaveDifferentSignAndNotNull(_previousXOtherVelocity, previousXForce))
                {
                    float _difference = Mathf.Abs(_previousXOtherVelocity);

                    if (!Mathm.HaveDifferentSign(_previousXOtherVelocity, instantForce.x + movement.x))
                        _difference -= Mathf.Abs(instantForce.x + movement.x);

                    if (_difference > 0)
                        force.x = Mathf.MoveTowards(force.x, 0, _difference);
                }

                // ------------------------------

                // Reduce force
                if (_maxDelta != 0)
                    force.x = Mathf.MoveTowards(force.x, 0, _maxDelta * Time.deltaTime);
            }

            // ------------------------------

            previousXForce = force.x;
            previousXVelocity = force.x + instantForce.x + movement.x;

            // ------------------------------

            // If going to opposite force direction, accordingly reduce force and movement.
            if (Mathm.HaveDifferentSignAndNotNull(force.y, movement.y))
            {
                float _maxDelta = Mathf.Abs(movement.y);

                movement.y = Mathf.MoveTowards(movement.y, 0, Mathf.Abs(force.y));
                force.y = Mathf.MoveTowards(force.y, 0, _maxDelta * Time.deltaTime);
            }
        }
        #endregion

        #region Physics
        /*******************************
         *******     PHYSICS     *******
         ******************************/

        void IPhysicsUpdate.Update()
        {
            PhysicsUpdate();
        }

        /// <summary>
        /// Add gravity force to the object.
        /// Called every frame while the object is using gravity.
        /// </summary>
        public virtual void PhysicsUpdate()
        {
            AddGravity();
        }

        /// <summary>
        /// Add gravity force to the object.
        /// </summary>
        protected void AddGravity()
        {
            if (force.y > ProgramSettings.MaxGravity)
            {
                AddForce(new Vector2(0, Mathf.Max(Physics2D.gravity.y * Time.deltaTime, ProgramSettings.MaxGravity - force.y)));
            }
        }

        /// <summary>
        /// Add gravity force to the object.
        /// </summary>
        protected void AddGravity(float _gravityCoef, float _maxGravityCoef)
        {
            float _maxGravityValue = ProgramSettings.MaxGravity * _maxGravityCoef;
            if (force.y > _maxGravityValue)
            {
                AddForce(new Vector2(0, Mathf.Max(Physics2D.gravity.y * _gravityCoef * Time.deltaTime, _maxGravityValue - force.y)));
            }
        }
        #endregion

        #region Movements
        /*********************************
         *******     MOVEMENTS     *******
         ********************************/

        void IMovableUpdate.Update()
        {
            MovableUpdate();
        }

        /// <summary>
        /// Update this object position based on velocity and related informations.
        /// Called at the end of the frame.
        /// </summary>
        public virtual void MovableUpdate()
        {
            #if UNITY_EDITOR
            // Refresh position if object moved in editor
            if (previousPosition != rigidbody.position) RefreshPosition();
            #endif

            ApplyVelocity();

            #if UNITY_EDITOR
            previousPosition = rigidbody.position;
            #endif
        }

        /// <summary>
        /// Set this object position.
        /// Use this instead of setting <see cref="Transform.position"/>.
        /// </summary>
        public void SetPosition(Vector2 _position)
        {
            if (rigidbody.position != _position)
            {
                rigidbody.position = _position;
                RefreshPosition();
            }
        }

        /// <summary>
        /// Apply stored velocity and move the object.
        /// </summary>
        private void ApplyVelocity()
        {
            if ((force + instantForce + movement).IsNull())
            {
                OnAppliedVelocity(Vector2.zero);
                return;
            }

            Vector2 _lastPosition = rigidbody.position;

            ComputeVelocityBeforeMovement();
            CollisionSystemDelegate();
            RefreshPosition();

            Vector2 _finalMovement = rigidbody.position - _lastPosition;

            if (useGravity)
                RefreshGroundState(_finalMovement);

            OnAppliedVelocity(_finalMovement);
        }

        /// <summary>
        /// Refresh this object ground state (when using gravity).
        /// Called after position refresh.
        /// </summary>
        /// <param name="_movement">Final movement of the object during this frame.</param>
        private void RefreshGroundState(Vector2 _movement)
        {
            if (_movement == Vector2.zero)
                return;

            // Iterate over movement hits to find if one of these
            // can be considered as ground.
            bool _isGrounded = false;
            
            for (int _i = 0; _i < castBufferCount; _i++)
            {
                if (castBuffer[_i].normal.y >= ProgramSettings.GroundMinYNormal)
                {
                    _isGrounded = true;
                    groundNormal = castBuffer[_i].normal;
                    break;
                }
            }

            // If didn't hit ground during movement,
            // try to get it with last ground normal inverse direction cast.
            //
            // Necessary when movement magnitude is inferior to default contact offset.
            if (!_isGrounded)
            {
                _isGrounded =   CastCollider(groundNormal * Physics2D.defaultContactOffset * -2, out RaycastHit2D _groundHit) &&
                                (_groundHit.normal.y >= ProgramSettings.GroundMinYNormal);

                if (_isGrounded)
                    groundNormal = _groundHit.normal;

                else if (isGrounded)
                    groundNormal = Vector2.up;
            }

            // Set value
            if (isGrounded != _isGrounded)
            {
                isGrounded = _isGrounded;
                OnSetGrounded();
            }
        }

        /// <summary>
        /// Refresh object position based on rigidbody.
        /// </summary>
        private void RefreshPosition()
        {
            ExtractFromCollisions();

            if ((Vector2)transform.position != rigidbody.position)
                transform.position = rigidbody.position;
        }

        /***********************************
         ********     CALLBACKS     ********
         **********************************/

        /// <summary>
        /// Called after velocity has been applied.
        /// </summary>
        protected virtual void OnAppliedVelocity(Vector2 _movement) { }

        /// <summary>
        /// Called when grounded value has been set.
        /// </summary>
        protected virtual void OnSetGrounded()
        {
            // Reduce horizontal force if not null when get grounded.
            if (isGrounded && (force.x != 0))
            {
                force.x *= ProgramSettings.OnGroundedHorizontalForceMultiplier;
            }
        }
        #endregion

        #region Collision Calculs
        /*************************************
         *****     COLLISION SYSTEMS     *****
         ************************************/

        protected static int            castBufferCount =   0;
        protected static RaycastHit2D[] castBuffer =        new RaycastHit2D[4];
        protected static RaycastHit2D[] extraCastBuffer =   new RaycastHit2D[4];

        // ---------------------------------------------

        /// <summary>
        /// Move rigidbody according to a simple collision system.
        /// When hitting something, immediatly stop object movement.
        /// </summary>
        private void SimpleCollisionsSystem()
        {
            Vector2 _velocity = GetVelocity();
            castBufferCount = CastCollider(_velocity, castBuffer, out float _distance);

            if (castBufferCount != 0)
            {
                force.x = force.y = 0;
                rigidbody.position += _velocity.normalized * _distance;
            }
            else
                rigidbody.position += _velocity;

            // Reset instant force and movement
            instantForce = movement = Vector2.zero;
        }

        /// <summary>
        /// Move rigidbody according to a complex collision system.
        /// When hitting something, continue movement all along hit surface.
        /// Perform movement according to ground surface angle.
        /// </summary>
        private void ComplexCollisionsSystem()
        {
            // If grounded, adjust velocity according to ground normal
            Vector2 _velocity = GetVelocity();
            if (isGrounded)
            {
                Vector2 _x = Vector2.Perpendicular(groundNormal);
                if (_x.x < 0) _x *= -1;
                _x *= _velocity.x;

                Vector2 _y = (_velocity.y < 0 ? groundNormal : Vector2.up) * _velocity.y;

                _velocity = _x + _y;
            }

            castBufferCount = 0;
            RecursiveComplexCollisions(_velocity, groundNormal);

            // --------------------------------------------------

            // Modify force according to hit surfaces
            if (!force.IsNull())
            {
                for (int _i = 0; _i < castBufferCount; _i++)
                {
                    if (Mathm.HaveDifferentSignAndNotNull(force.x, castBuffer[_i].normal.x) && (Mathf.Abs(castBuffer[_i].normal.x) == 1))
                    {
                        force.x = 0;
                        if (force.y == 0) break;
                    }

                    if ((force.y < 0) && (castBuffer[_i].normal.y > ProgramSettings.GroundMinYNormal))
                    {
                        force.y = 0;
                        if (force.x == 0) break;
                    }
                }
            }

            // Reset instant force and movement
            instantForce = movement = Vector2.zero;
        }

        /// <summary>
        /// Move rigidbody according to a physics approaching collision system.
        /// When hitting something, continue movement according to physical approach.
        /// Perform movement according to world space.
        /// </summary>
        private void PhysicsCollisionsSystem()
        {
            castBufferCount = 0;
            RecursivePhysicsCollisions(GetVelocity());

            // --------------------------------------------------

            // Reduce force according to hit surfaces
            for (int _i = 0; _i < castBufferCount; _i++)
            {
                if (force.IsNull())
                    break;

                force -= castBuffer[_i].normal * Vector2.Dot(force, castBuffer[_i].normal);
            }

            // Reset instant force and movement
            instantForce = movement = Vector2.zero;
        }

        /// <summary>
        /// Custom collision system.
        /// 
        /// Override this method to implement a new and specific behaviour
        /// for collision calculs.
        /// </summary>
        protected virtual void CustomCollisionsSystem() { }

        /*************************************
         *****     RECURSIVE CALCULS     *****
         ************************************/

        /// <summary>
        /// Calculates complex collisions recursively.
        /// </summary>
        private void RecursiveComplexCollisions(Vector2 _velocity, Vector2 _normal, int _recursiveCount = 0)
        {
            int _amount = CastCollider(_velocity, extraCastBuffer, out float _distance);

            // No movement mean object is stuck into something, so return
            if (_distance == 0)
                return;

            if (_amount == 0)
            {
                rigidbody.position += _velocity;
                GroundSnap(_velocity, _normal);
                return;
            }


            // Move rigidbody and get extra cast velocity
            if ((_distance -= Physics2D.defaultContactOffset) > 0)
            {
                Vector2 _normalizedVelocity = _velocity.normalized;

                rigidbody.position += _normalizedVelocity * _distance;
                _velocity = _normalizedVelocity * (_velocity.magnitude - _distance);
            }

            // If reached recursion limit, stop
            if (_recursiveCount > collisionSystemRecursionCeil)
            {
                InsertCastInfos(extraCastBuffer, _amount);
                GroundSnap(_velocity, _normal);
                return;
            }

            // Get velocity outside normal surface, as pure value
            float _angle = Vector2.SignedAngle(_normal, _velocity);
            _normal.Set(0, 1);
            _velocity = _normal.Rotate(_angle) * _velocity.magnitude;

            Vector2 _hitNormal = extraCastBuffer[0].normal;
            _velocity = ClimbStep(_velocity, extraCastBuffer[0]);

            if ((Mathf.Abs(extraCastBuffer[0].normal.x) == 1) && (_velocity.x != 0))
            {
                for (int _i = 1; _i < _amount; _i++)
                {
                    InsertCastInfo(extraCastBuffer[_i]);
                }
            }
            else
                InsertCastInfos(extraCastBuffer, _amount);

            if (!_velocity.IsNull())
            {
                // Reduce extra movement according to main impact normals
                _velocity -= _hitNormal * Vector2.Dot(_velocity, _hitNormal);
                if (!_velocity.IsNull())
                {
                    RecursiveComplexCollisions(_velocity, _normal, _recursiveCount + 1);
                }
            }
        }

        /// <summary>
        /// Calculates physics collisions recursively.
        /// </summary>
        private void RecursivePhysicsCollisions(Vector2 _velocity, int _recursiveCount = 0)
        {
            int _amount = CastCollider(_velocity, extraCastBuffer, out float _distance);

            // No movement mean object is stuck into something, so return
            if (_distance == 0)
                return;

            if (_amount == 0)
            {
                rigidbody.position += _velocity;
                return;
            }

            InsertCastInfos(extraCastBuffer, _amount);

            // Move rigidbody and get extra cast velocity
            if ((_distance -= Physics2D.defaultContactOffset) > 0)
            {
                Vector2 _normalizedVelocity = _velocity.normalized;

                rigidbody.position += _normalizedVelocity * _distance;
                _velocity = _normalizedVelocity * (_velocity.magnitude - _distance);
            }

            // If reached recursion limit, stop
            if (_recursiveCount > collisionSystemRecursionCeil)
                return;

            // Reduce extra movement according to main impact normals
            _velocity -= extraCastBuffer[0].normal * Vector2.Dot(_velocity, extraCastBuffer[0].normal);
            if (!_velocity.IsNull())
            {
                RecursivePhysicsCollisions(_velocity, _recursiveCount + 1);
            }
        }

        /************************************
         *****     BUFFER UTILITIES     *****
         ***********************************/

        /// <summary>
        /// Inserts a RaycastHit information into the <see cref="castBuffer"/> buffer.
        /// </summary>
        protected void InsertCastInfo(RaycastHit2D _hit)
        {
            // Add new hit if there is enough space, or replace the last one.
            if (castBufferCount < castBuffer.Length)
            {
                castBuffer[castBufferCount] = _hit;
                castBufferCount++;
            }
            else
                castBuffer[castBufferCount - 1] = _hit;
        }

        /// <summary>
        /// Inserts an array of RaycastHit informations into the <see cref="castBuffer"/> buffer.
        /// </summary>
        protected void InsertCastInfos(RaycastHit2D[] _hits, int _amount)
        {
            // Add as many hits as possible while there is enough space,
            // or replace the last one if the buffer is already full.
            if (castBufferCount < castBuffer.Length)
            {
                for (int _i = 0; _i < _amount; _i++)
                {
                    castBuffer[castBufferCount + _i] = _hits[_i];
                    castBufferCount++;

                    if (castBufferCount == castBuffer.Length)
                        return;
                }
            }
            else
                castBuffer[castBufferCount - 1] = _hits[0];
        }

        /*********************************
         *****     SPECIAL MOVES     *****
         ********************************/

        /// <summary>
        /// Make the object climb a surface, if possible.
        /// Climb cast infos are automatically added to the <see cref="castBuffer"/> buffer.
        /// </summary>
        protected Vector2 ClimbStep(Vector2 _velocity, RaycastHit2D _stepHit)
        {
            // If climbing is not necessary, return.
            if (!((_stepHit.normal.y == 0) && (_velocity.y <= 0) && (_velocity.x != 0)))
                return _velocity;


            // Heighten the rigidbody position and add opposite velocity,
            // then cast collider and get hit informations.
            rigidbody.position += new Vector2(0, ProgramSettings.GroundClimbHeight);
            _velocity.y -= ProgramSettings.GroundClimbHeight;

            int _amount = CastCollider(_velocity, extraCastBuffer, out float _climbDistance);

            if (_amount == 0)
            {
                rigidbody.position += _velocity;
                _velocity.Set(0, 0);
            }
            // If hit something, check if it's a different surface than the one trying to climb
            // and that the rigidbody is not stuck in ; when so, distance is equal to zero.
            else if ((_climbDistance != 0) && ((_stepHit.collider != extraCastBuffer[0].collider) || (_stepHit.normal != extraCastBuffer[0].normal)))
            {
                Vector2 _normalized = _velocity.normalized;
                rigidbody.position += _normalized * _climbDistance;
                _velocity = _normalized * (_velocity.magnitude - _climbDistance);

                InsertCastInfos(extraCastBuffer, _amount);
            }
            // If not, climb is failed so just reset position and velocity.
            else
            {
                rigidbody.position -= new Vector2(0, ProgramSettings.GroundClimbHeight);
                _velocity.y += ProgramSettings.GroundClimbHeight;
                _velocity.x = 0;
            }

            return _velocity;
        }

        // -----------------------------------------------

        /// <summary>
        /// Snap the object to the ground only if already grounded
        /// and movement is going down.
        /// Used for slopes & steps movements.
        /// Ground cast info is automatically added to the <see cref="castBuffer"/> buffer.
        /// </summary>
        protected bool GroundSnap(Vector2 _velocity, Vector2 _normal)
        {
            // Get going down velocity.
            _velocity = _normal * Vector2.Dot(_velocity, _normal);

            // If object was grounded and going down, try to snap to ground (slope & steps).
            if (isGrounded && (_velocity.y < 0))
                return GroundSnap(_normal);

            return false;
        }

        /// <summary>
        /// Snap the object to the ground.
        /// Used for slopes & steps movements.
        /// Ground cast info is automatically added to the <see cref="castBuffer"/> buffer.
        /// </summary>
        protected bool GroundSnap(Vector2 _normal)
        {
            if (CastCollider(_normal * -ProgramSettings.GroundSnapHeight, out RaycastHit2D _snapHit))
            {
                rigidbody.position += _normal * -_snapHit.distance;
                InsertCastInfo(_snapHit);
                return true;
            }

            return false;
        }
        #endregion

        #region Collider Operations
        /***************************************
         *****     COLLIDER OPERATIONS     *****
         **************************************/

        private static RaycastHit2D[] singleCastBuffer = new RaycastHit2D[1];

        /// <summary>
        /// Cast collider in a given movement direction and get informations about hit collider.
        /// </summary>
        protected bool CastCollider(Vector2 _movement, out RaycastHit2D _hit)
        {
            bool _result = collider.Cast(_movement, contactFilter, singleCastBuffer, _movement.magnitude) > 0;

            _hit = singleCastBuffer[0];
            return _result;
        }

        /// <summary>
        /// Cast collider in a given movement direction and get informations about hit colliders.
        /// </summary>
        protected int CastCollider(Vector2 _movement, RaycastHit2D[] _hitBuffer, out float _distance)
        {
            _distance = _movement.magnitude;

            int _hitAmount = collider.Cast(_movement, contactFilter, _hitBuffer, _distance);
            if (_hitAmount > 0)
            {
                // Hits are already sorted by distance, so simply get closest one.
                _distance = Mathf.Max(0, _hitBuffer[0].distance - Physics2D.defaultContactOffset);

                // Retains only closest hits by ignoring those with too distants.
                for (int _i = 1; _i < _hitAmount; _i++)
                {
                    if ((_hitBuffer[_i].distance + castMaxDistanceDetection) > _hitBuffer[0].distance) return _i;
                }
            }
            return _hitAmount;
        }

        // -------------------------------------------------------

        private static Collider2D[]     extractionBuffer =  new Collider2D[6];

        /// <summary>
        /// Extract the object from physics collisions.
        /// </summary>
        private void ExtractFromCollisions()
        {
            int _count = collider.OverlapCollider(contactFilter, extractionBuffer);
            ColliderDistance2D _distance;

            for (int _i = 0; _i < _count; _i++)
            {
                // If overlap, extract from collision.
                _distance = collider.Distance(extractionBuffer[_i]);

                if (_distance.isOverlapped)
                    rigidbody.position += _distance.normal * _distance.distance;
            }
        }
        #endregion

        #endregion

        #region Monobehaviour
        /*********************************
         *****     MONOBEHAVIOUR     *****
         ********************************/

        protected virtual void OnDisable()
        {
            // Updates unregistration
            if (isAwake)
                UpdateManager.Instance.Unregister((IMovableUpdate)this);
            if (useGravity)
                UpdateManager.Instance.Unregister((IPhysicsUpdate)this);
        }

        protected virtual void OnEnable()
        {
            // Updates registration
            if (isAwake)
                UpdateManager.Instance.Register((IMovableUpdate)this);
            if (useGravity)
                UpdateManager.Instance.Register((IPhysicsUpdate)this);
        }

        protected virtual void Start()
        {
            #if UNITY_EDITOR
            // Get initial position.
            previousPosition = transform.position;
            #endif

            // Set object contact filter.
            contactFilter.layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
            contactFilter.useLayerMask = true;

            // Initialize ground normal & movement collision system.
            groundNormal = Vector2.up;
            CollisionSystem = collisionSystem;
        }
        #endregion

        #endregion
    }
}
