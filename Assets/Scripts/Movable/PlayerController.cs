using EnhancedEditor;
using System.Collections;
using UnityEngine;

namespace Nowhere
{
    public class PlayerController : Movable
    {
        #region Fields / Properties

        #region Parameters
        /**********************************
         *********     FIELDS     *********
         *********************************/

        [HorizontalLine(1, order = 0), Section("Player Controller", 50, 0, order = 1), HorizontalLine(2, SuperColor.Chocolate, order = 2)]

        /// <summary>
        /// Player controller attributes, stored in a scriptable object.
        /// </summary>
        [SerializeField, Required]
        private PlayerControllerAttributes      attributes =            null;

        /// <summary>
        /// Player controller attributes, stored in a scriptable object.
        /// </summary>
        [SerializeField, Required]
        private Animator                        animator =              null;


        [HorizontalLine(2, SuperColor.Raspberry)]


        /// <summary>Backing field for <see cref="IsPlayable"/>.</summary>
        [SerializeField, ReadOnly]
        private bool                            isPlayable =            true;

        /// <summary>Backing field for <see cref="IsMoving"/>.</summary>
        [SerializeField, ReadOnly]
        private bool                            isMoving =              false;

        /// <summary>Backing field for <see cref="IsJumping"/>.</summary>
        [SerializeField, ReadOnly]
        private bool                            isJumping =             false;

        /// <summary>Backing field for <see cref="IsSliding"/>.</summary>
        [SerializeField, ReadOnly]
        private bool                            isSliding =             false;


        /// <summary>Backing field for <see cref="WallStuckState"/>.</summary>
        [SerializeField, ReadOnly]
        private WallStuck                       wallStuckState =        0;


        /**********************************
         *******     PROPERTIES     *******
         *********************************/

        /// <summary>
        /// Indicates if the player is actually playable.
        /// </summary>
        public bool IsPlayable
        {
            get { return isPlayable; }
            protected set
            {
                isPlayable = value;
            }
        }

        /// <summary>
        /// Indicates if the player is currently jumping or not.
        /// </summary>
        public bool IsJumping
        {
            get { return isJumping; }
            protected set
            {
                isJumping = value;
            }
        }

        /// <summary>
        /// Indicates if the player is currently moving on its own.
        /// </summary>
        public bool IsMoving
        {
            get { return isMoving; }
            protected set
            {
                isMoving = value;
                animator.SetBool("IsMoving", value);
            }
        }

        /// <summary>
        /// Indicates if the player is currently sliding or not.
        /// </summary>
        public bool IsSliding
        {
            get { return isSliding; }
            protected set
            {
                isSliding = value;
            }
        }


        /// <summary>
        /// Indicates if the player is currently stuck to a wall, and if so on which side.
        /// </summary>
        public WallStuck WallStuckState
        {
            get { return wallStuckState; }
            protected set
            {
                wallStuckState = value;
                animator.SetBool("IsWallStuck", value != 0);
            }
        }
        #endregion

        #region Coroutines & Memory
        /**********************************
         *******     COROUTINES     *******
         *********************************/

        /// <summary>Stored coroutine of the <see cref="Freeze"/> method.</summary>
        private Coroutine       freezeCoroutine =       null;

        /// <summary>Stored coroutine of the <see cref="DoJump"/> method.</summary>
        private Coroutine       jumpCoroutine =         null;

        /// <summary>Stored coroutine of the <see cref="DoSlide"/> method.</summary>
        private Coroutine       slideCoroutine =        null;


        /**********************************
         *********     MEMORY     *********
         *********************************/

        /// <summary>
        /// Player X movement at previous frame.
        /// Used to calculate new movement and lerps before applying it.
        /// </summary>
        [SerializeField, ReadOnly, HelpBox("Last movement of the player on the x axis", HelpBoxType.Info)]
        private float           lastMovement =          0;

        /// <summary>
        /// Time used to get current speed value according to <see cref="PlayerControllerAttributes.speedCurve"/>.
        /// </summary>
        private float           speedCurveVarTime =     0;
        #endregion

        #endregion

        #region Methods

        #region Original Methods

        #region Inputs
        /**********************************
         *********     INPUTS     *********
         *********************************/

        /// <summary>
        /// Check player inputs and execute movement or other actions.
        /// </summary>
        private void CheckInputs()
        {
            // If not playable, do nothing
            if (!isPlayable) return;

            // Executes player actions and then move them
            CheckMovement();
            CheckActions();
        }

        /// <summary>
        /// Executes actions related to player inputs.
        /// </summary>
        private void CheckActions()
        {
            // Jump if pressing associated key
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0)) Jump();

            // Force debug cheat code
            if (Input.GetKeyDown(KeyCode.Joystick1Button3)) AddForce(new Vector2(20, 0));
        }

        /// <summary>
        /// Move the player according to input movement.
        /// </summary>
        private void CheckMovement()
        {
            // Move the player according to movement input
            float _movement = Input.GetAxis("Horizontal");
            if (_movement != 0) Move(new Vector2(_movement, 0));
        }
        #endregion

        #region Flip
        /**********************************
         **********     FLIP     **********
         *********************************/

        /// <summary>
        /// Makes the object flip.
        /// </summary>
        public override void Flip()
        {
            // Don't flip while stuck to a wall
            if (wallStuckState != 0) return;

            base.Flip();
        }
        #endregion

        #region Speed
        /*********************************
         *********     SPEED     *********
         ********************************/

        /// <summary>
        /// Increase player speed if below max value.
        /// </summary>
        private void Accelerate()
        {
            // Cache speed curve
            AnimationCurve _curve = attributes.SpeedCurve;

            // Increase speed if needed
            if (speed < _curve[_curve.length - 1].value)
            {
                speedCurveVarTime = Mathf.Min(speedCurveVarTime + (Time.deltaTime * (isGrounded ? 1 : attributes.AirSpeedAccCoef)), _curve[_curve.length - 1].time);

                speed = _curve.Evaluate(speedCurveVarTime);
            }
        }

        /// <summary>
        /// Reset movement speed.
        /// </summary>
        public void ResetSpeed() => speed = speedCurveVarTime = 0;
        #endregion

        #region Velocity
        /**************************************
         ***     MOVEMENT MANIPULATIONS     ***
         *************************************/

        /// <summary>
        /// Reset player movement.
        /// </summary>
        private void ResetMovement()
        {
            ResetSpeed();
            velocity.Movement.x = lastMovement = 0;
        }


        /**************************************
         *****     VELOCITY MEDIATORS     *****
         *************************************/

        /// <summary>
        /// Makes the object move on its own.
        /// This do some extra things like flipping the object
        /// and more due to when it move on its own
        /// and not pushed by an external force.
        /// 
        /// Override this method do add extra behaviours.
        /// </summary>
        /// <param name="_movement">Movement to perform.</param>
        protected override void Move(Vector2 _movement)
        {
            // When moving on X, accelerate and multiply movement by speed
            if (_movement.x != 0)
            {
                Accelerate();
                _movement.x *= speed;
            }

            // Do base move
            base.Move(_movement);
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
        protected override void AddGravity()
        {
            // Apply a coefficient to gravity when stuck to a wall
            if ((wallStuckState != 0) && !isJumping) AddGravity(attributes.WallStuckGravityCoef, attributes.WallStuckMinGravityCoef);

            else base.AddGravity();
        }


        /**********************************
         ******     CALCULATIONS     ******
         *********************************/

        /// <summary>
        /// Calculates new velocity value after a movement.
        /// </summary>
        /// <param name="_hits">All objects hit during travel.</param>
        protected override void ComputeVelocityAfterMovement(RaycastHit2D[] _hits)
        {
            // Store last movement and execute base calculs
            lastMovement = velocity.Movement.x;
            base.ComputeVelocityAfterMovement(_hits);
        }

        /// <summary>
        /// Compute velocity value before movement.
        /// </summary>
        protected override void ComputeVelocityBeforeMovement()
        {
            // When going in opposite direction from previous movement, transit to it
            if ((lastMovement != 0) && !Mathm.HaveDifferentSign(lastMovement - velocity.Movement.x, lastMovement))
            {
                // About-Turn
                if ((velocity.Movement.x != 0) && Mathm.HaveDifferentSign(lastMovement, velocity.Movement.x))
                {
                    float _coef = isGrounded ? attributes.GroundAboutTurnAccel : attributes.AirAboutTurnAccel;

                    velocity.Movement.x = Mathf.MoveTowards(lastMovement, velocity.Movement.x, speed * Time.deltaTime * _coef);
                }
                // Deceleration
                else if (lastMovement != velocity.Movement.x)
                {
                    float _coef = isGrounded ? attributes.GroundMovementDecel : attributes.AirMovementDecel;

                    velocity.Movement.x = Mathf.MoveTowards(lastMovement, velocity.Movement.x, Time.deltaTime * _coef);
                }
            }

            // Execute base calculs
            base.ComputeVelocityBeforeMovement();
        }
        #endregion

        #region Walls
        /***************************************
         ******     WALL INTERACTIONS     ******
         **************************************/

        /// <summary>
        /// Check if the player is against a wall.
        /// </summary>
        private void CheckWallStatus()
        {
            // Create variables
            WallStuck _state = WallStuck.None;
            RaycastHit2D[] _hitBuffer = new RaycastHit2D[1];

            // Get first cast direction
            float _direction = (wallStuckState == 0) ? Mathf.Sign(Velocity.x) : (int)wallStuckState;
            Vector2 _movement = new Vector2(_direction * Physics2D.defaultContactOffset * 2.5f, 0);

            // Perform cast in given direction and opposite one if nothing is hit
            if (!CastForWall())
            {
                _direction *= -1;
                _movement.x *= -1;

                CastForWall();
            }

            // Set wall stuck state if different
            if (wallStuckState != _state)
            {
                // Face wall opposite side
                if (_state < 0) Flip(true);
                else if (_state > 0) Flip(false);

                WallStuckState = _state;
            } 

            // Set wall state when hitting a vertical collider
            bool CastForWall()
            {
                if (CastCollider(_movement, _hitBuffer, out _) > 0)
                {
                    if (_hitBuffer[0].normal.x == -_direction)
                        _state = (WallStuck)_direction;

                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Freeze
        /********************************
         ********     FREEZE     ********
         *******************************/

        /// <summary>
        /// Freeze the player for a certain duration.
        /// </summary>
        /// <param name="_duration">Freeze duration.</param>
        private void Freeze(float _duration)
        {
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
            freezeCoroutine = StartCoroutine(DoFreeze(_duration));
        }

        /// <summary>
        /// Freeze the player for a certain duration.
        /// Coroutine associated with the <see cref="Freeze(float)"/> method.
        /// </summary>
        /// <param name="_duration">Freeze duration.</param>
        /// <returns>IEnumerator, baby.</returns>
        private IEnumerator DoFreeze(float _duration)
        {
            IsPlayable = false;
            yield return new WaitForSeconds(_duration);
            IsPlayable = true;

            freezeCoroutine = null;
        }
        #endregion

        #region Movements
        /**************************************
         ******     MOV. SYSTEM COGS     ******
         *************************************/

        /// <summary>
        /// Update this object, from position based on velocity to related informations.
        /// Called at the end of the frame.
        /// 
        /// Override this method to add extra behaviours before or after update.
        /// </summary>
        protected override void UpdateObject()
        {
            // Check if the player is stuck to a wall and associated things
            if (!isGrounded) CheckWallStatus();

            // Do base update
            base.UpdateObject();
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
        protected override void OnAppliedVelocity(Vector2 _finalMovement)
        {
            // Execute base method
            base.OnAppliedVelocity(_finalMovement);

            // Set if player is moving if needed
            bool _isMoving = (velocity.Movement.x != 0) && (_finalMovement.x != 0);

            if (isMoving != _isMoving)
                IsMoving = _isMoving;

            // Reset speed if not moving
            if (!_isMoving && (speed > 0))
                ResetMovement();

            // Set animator ground state
            if (!isGrounded)
                animator.SetInteger("GroundState", (int)Mathf.Sign(_finalMovement.y));
        }

        /// <summary>
        /// Called when hitting colliders during movement.
        /// 
        /// Override this method to add extra behaviours and feedback.
        /// </summary>
        /// <param name="_hits">Raycast hits of touched colliders during movement.</param>
        protected override void OnMovementHit(RaycastHit2D[] _hits)
        {
            // Execute base method
            base.OnMovementHit(_hits);

            // Stop jump when hurting surface on Y axis
            if (isJumping)
            {
                foreach (RaycastHit2D _hit in _hits)
                {
                    if (_hit.normal.y != 0)
                    {
                        StopJump();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Called when grounded value has been set.
        /// 
        /// Override this method to add extra behaviours and feedback.
        /// </summary>
        protected override void OnSetGrounded()
        {
            // Execute base method
            base.OnSetGrounded();

            if (isGrounded)
            {
                // Set wall stuck state to none if needed
                if (wallStuckState != WallStuck.None)
                    WallStuckState = WallStuck.None;

                animator.SetInteger("GroundState", 0);
            }
        }
        #endregion

        #region Special Moves
        /******************************
         ********     JUMP     ********
         *****************************/

        /// <summary>
        /// Makes the player perform a jump over time.
        /// </summary>
        /// <returns>IEnumerator, baby.</returns>
        private IEnumerator DoJump()
        {
            AnimationCurve _curve = null;
            isJumping = true;

            animator.SetTrigger("Jump");

            // Executes actions depending on performing a normal or a wall jump.
            if (isGrounded)
            {
                _curve = attributes.HighJumpCurve;

                if (velocity.Force.y != 0) velocity.Force.y = 0;

                // If moving, add extra X velocity to the player
                if (velocity.Movement.x != 0) Move(new Vector2(isFacingRight.ToSign() * .25f, 0));

                AddForce(new Vector2(velocity.Movement.x * .9f, 0));
            }
            else
            {
                _curve = attributes.WallJumpCurve;

                if (velocity.Force.y < 0) velocity.Force.y *= .25f;

                // When performing a wall jump, add opposite side X velocity.
                ResetMovement();
                Freeze(.05f);
                AddForce(new Vector2(attributes.WallJumpHorizontalForce * (int)wallStuckState, 0));
            }

            // Perform jump over time following the associated curve
            float _time = 0;
            float _limit = _curve[_curve.length - 1].time;
            float _treshold = _limit * .7f;

            // While holding the jump button and havn't reached jump maximum duration,
            // add more vertical velocity !
            // 
            // If releasing jump button before max duration,
            // continue adding some intertia
            while (_time < _limit)
            {
                if ((_time < _treshold) && (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Joystick1Button0)))
                {
                    _time = _treshold;
                }

                // Move up
                Move(new Vector2(0, _curve.Evaluate(_time)));

                yield return null;
                _time = Mathf.Min(_time + Time.deltaTime, _limit);
            }

            isJumping = false;
            jumpCoroutine = null;

            yield break;
        }

        /// <summary>
        /// Makes the player start jumping.
        /// </summary>
        /// <returns>Return true if successfully started a jump, false otherwise.</returns>
        private bool Jump()
        {
            // Return false if cannot jump
            if (!isGrounded && (wallStuckState == WallStuck.None)) return false;

            if (isJumping) StopCoroutine(jumpCoroutine);
            StopSlide();

            jumpCoroutine = StartCoroutine(DoJump());
            return true;
        }

        /// <summary>
        /// Stops palyer's current jump.
        /// </summary>
        /// <returns>Return true if successfully stopped current jump, false otherwise.</returns>
        public bool StopJump()
        {
            if (!isJumping) return false;

            isJumping = false;
            StopCoroutine(jumpCoroutine);
            jumpCoroutine = null;

            return true;
        }


        /*******************************
         ********     SLIDE     ********
         ******************************/

        /// <summary>
        /// Makes the player perform a jump over time.
        /// </summary>
        /// <returns>IEnumerator, baby.</returns>
        private IEnumerator DoSlide()
        {
            isSliding = true;

            // Perform slide over time following the curve
            float _time = 0;
            float _limit = attributes.SlideCurve[attributes.SlideCurve.length - 1].time;

            while (true)
            {
                // Move in direction of the facing side
                //PerformMovement(new Vector2(attributes.SlideCurve.Evaluate(_time) * isFacingRight.ToSign(), 0));

                if (_time == _limit) break;

                yield return null;
                _time = Mathf.Min(_time + Time.deltaTime, _limit);
            }

            isSliding = false;
            slideCoroutine = null;

            yield break;
        }

        /// <summary>
        /// Makes the player start sliding.
        /// </summary>
        /// <returns>Return true if successfully started a slide, false otherwise.</returns>
        private bool Slide()
        {
            // Return false if cannot slide
            if (!isGrounded || isJumping || isSliding) return false;

            slideCoroutine = StartCoroutine(DoSlide());
            return true;
        }

        /// <summary>
        /// Stops player's current slide.
        /// </summary>
        /// <returns>Return true if successfully stopped current slide, false otherwise.</returns>
        public bool StopSlide()
        {
            if (!isSliding) return false;

            isSliding = false;
            StopCoroutine(slideCoroutine);
            slideCoroutine = null;

            return true;
        }
        #endregion

        #endregion

        #region Unity Methods
        /*********************************
         *****     MONOBEHAVIOUR     *****
         ********************************/

        // Awake is called when the script instance is being loaded
        protected override void Awake()
        {
            // Destroy script if no attribute is linked to it.
            if (!attributes)
            {
                Destroy(this);
                return;
            }

            // Execute base class awake
            base.Awake();

            // Get missing components
            if (!animator) animator = GetComponent<Animator>();
        }

        // Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy
        protected override void OnDestroy()
        {
            // Execute base class on destroy
            base.OnDestroy();

            // Unsubscribe update methods
            UpdateManager.UnsubscribeToUpdate(CheckInputs, UpdateModeTimeline.Update);
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            // Execute base class start
            base.Start();

            // Subscribe update methods
            UpdateManager.SubscribeToUpdate(CheckInputs, UpdateModeTimeline.Update);
        }
        #endregion

        #endregion
    }
}
