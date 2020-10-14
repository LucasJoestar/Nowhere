// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
//  Inputs management should be replaced by using
//  the new Input System, at some point.
//
// ========================================================================== //

using EnhancedEditor;
using System;
using System.Collections;
using UnityEngine;

namespace Nowhere
{
    /// <summary>
    /// Used to know if the player is stuck to a wall.
    /// </summary>
    public enum WallStuck
    {
        Left = -1,
        None,
        Right
    }

    public class PlayerController : Movable, IInputUpdate
    {
        #region Fields / Properties
        [HorizontalLine(1, order = 0), Section("PLAYER CONTROLLER", 50, 0, order = 1), HorizontalLine(2, SuperColor.Chocolate, order = 2)]

        [SerializeField, Required] private PlayerControllerAttributes attributes = null;
        [SerializeField, Required] private Animator animator = null;

        [HorizontalLine(2, SuperColor.Raspberry)]

        [SerializeField, PropertyField] private bool isPlayable = true;
        public bool IsPlayable
        {
            get { return isPlayable; }
            set
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) return;
                #endif

                if (value != isPlayable)
                {
                    isPlayable = value;
                    StopSlide();

                    // Manage input update registration.
                    if (value)
                        UpdateManager.Instance.Register((IInputUpdate)this);

                    else
                        UpdateManager.Instance.Unregister((IInputUpdate)this);
                }
            }
        }

        // -----------------------

        [SerializeField, ReadOnly] private bool isMoving =      false;
        [SerializeField, ReadOnly] private bool isJumping =     false;
        [SerializeField, ReadOnly] private bool isDashing =     false;
        [SerializeField, ReadOnly] private bool isSliding =     false;
        [SerializeField, ReadOnly] private bool isCrouched =    false;
        [SerializeField, ReadOnly] private bool isAtJumpPeek =  false;

        [SerializeField, ReadOnly] private WallStuck wallStuckState = WallStuck.None;
        public WallStuck WallStuckState
        {
            get { return wallStuckState; }
            private set
            {
                wallStuckState = value;
                animator.SetBool(anim_IsWallStuckID, value != 0);
            }
        }

        [SerializeField, ReadOnly, HelpBox("Last movement of the player on the X axis", HelpBoxType.Info)]
        private float lastMovement = 0;

        // -----------------------

        private readonly int anim_GroundStateID =   Animator.StringToHash("GroundState");
        private readonly int anim_IsMovingID =      Animator.StringToHash("IsMoving");
        private readonly int anim_IsWallStuckID =   Animator.StringToHash("IsWallStuck");
        private readonly int anim_JumpID =          Animator.StringToHash("Jump");
        private readonly int anim_SlideID =         Animator.StringToHash("IsSliding");
        private readonly int anim_CrouchID =        Animator.StringToHash("IsCrouched");

        private float coyoteTimeVar = 0;
        private float jumpBufferVar = 0;
        private float SlideBufferVar = 0;
        #endregion

        #region Methods

        #region Inputs
        /// <summary>
        /// Check player inputs and execute movement or other actions.
        /// </summary>
        void IInputUpdate.Update()
        {
            float _movement = Input.GetAxis("Horizontal");
            if (_movement != 0 && !isSliding)
            {
                MoveHorizontally(_movement);
                PlayerCamera.Instance.SetFacingSide(Mathf.Sign(_movement));
            }

            // Jump.
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
            {
                if (!Jump())
                    jumpBufferVar = Time.time;
            }

            // Dash.
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Joystick1Button2))
            {
                Dash();
                return;
            }

            // Slide & Crouch.
            if ((Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Joystick1Button1)) &&
                (Mathf.Abs(lastMovement + force.x) > attributes.SlideRequiredVelocity))
            {
                if (!Slide())
                    SlideBufferVar = Time.time;
            }
            if (isGrounded && !isSliding)
            {
                if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.Joystick1Button1))
                {
                    if (!isCrouched)
                        Crouch();
                }
                else if (isCrouched)
                {
                    GetUp();
                }
            }

            // ---------- Cheat Codes ---------- //

            if (Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.F))
                AddForce(new Vector2(facingSide * 20, 0));

            if (Input.GetKeyDown(KeyCode.R))
                UnityEngine.SceneManagement.SceneManager.LoadScene(0, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private bool ConsumeJumpBuffer()
        {
            if ((Time.time - jumpBufferVar < attributes.JumpBufferTime) &&
                (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Joystick1Button0)))
            {
                return Jump();
            }

            return false;
        }
        #endregion

        #region Flip
        /// <summary>
        /// Flip the object (face opposite side).
        /// </summary>
        public override void Flip()
        {
            // Don't flip while stuck to a wall.
            if (wallStuckState == 0)
                base.Flip();
        }
        #endregion

        #region Velocity
        private float speedCurveVarTime = 0;

        protected override void MoveHorizontally(float _movement)
        {
            // Increase speed according to curve.
            if (_movement != 0)
            {
                speed = AnimationCurveUtility.IncreaseValue(attributes.SpeedCurve, speed, ref speedCurveVarTime, isGrounded ? 1 : attributes.AirSpeedAccelCoef);
                _movement *= speed;

                if (isCrouched)
                    _movement *= attributes.CrouchSpeedCoef;

                base.MoveHorizontally(_movement);
            }
        }

        private void ResetMovement()
        {
            movement.x = lastMovement = speed = speedCurveVarTime = 0;
        }

        // -----------------------

        /// <summary>
        /// Get the object velocity movement for this frame.
        /// </summary>
        protected override Vector2 GetVelocity()
        {
            // Apply half gravity when at jump peek.
            if (isAtJumpPeek && (force.y < 0))
            {
                return (new Vector2(force.x, force.y / 2f) + instantForce + movement) * speedCoef * Time.deltaTime;
            }

            return base.GetVelocity();
        }

        // -----------------------

        bool isGoingOppositeSide = false;

        protected override void ComputeVelocityBeforeMovement()
        {
            // When going in opposite direction from previous movement, transit to it.
            if (isGoingOppositeSide || ((lastMovement != 0) && !Mathm.HaveDifferentSign(lastMovement - movement.x, lastMovement)))
            {
                isGoingOppositeSide = true;

                // About-Turn.
                if ((movement.x != 0) && Mathm.HaveDifferentSign(lastMovement, movement.x))
                {
                    float _coef = isGrounded ? attributes.GroundAboutTurnAccel : attributes.AirAboutTurnAccel;
                   
                    movement.x = Mathf.MoveTowards(lastMovement, movement.x, speed * Time.deltaTime * _coef);
                }
                // Deceleration.
                else if (lastMovement != movement.x)
                {
                    float _coef = isGrounded ? attributes.GroundMovementDecel : attributes.AirMovementDecel;
                    
                    movement.x = Mathf.MoveTowards(lastMovement, movement.x, Time.deltaTime * _coef);
                }
                else
                {
                    isGoingOppositeSide = false;
                }
            }

            lastMovement = movement.x;
            base.ComputeVelocityBeforeMovement();
        }
        #endregion

        #region Physics
        protected override void PhysicsUpdate()
        {
            // Apply a coefficient to gravity when :
            //  • Stuck to a wall
            //  • At jump peek.
            if ((wallStuckState != 0) && !isJumping)
                AddGravity(attributes.WallStuckGravityCoef, attributes.WallStuckMinGravityCoef);

            else if (isAtJumpPeek)
                AddGravity(.5f, 1);

            else if (!isDashing)
                base.PhysicsUpdate();
        }
        #endregion

        #region Walls
        private void CheckWallStatus()
        {
            WallStuck _state = WallStuck.None;

            // Get first cast direction.
            float _direction = (wallStuckState == 0) ? Mathf.Sign(movement.x + force.x) : (int)wallStuckState;
            Vector2 _movement = new Vector2(_direction * Physics2D.defaultContactOffset * 2.5f, 0);

            // Perform cast in given direction, and opposite one if nothing is hit.
            if (!CastForWall())
            {
                _direction *= -1;
                _movement.x *= -1;

                CastForWall();
            }

            if (wallStuckState != _state)
            {
                // Face wall opposite side.
                if (_state != 0)
                {
                    Flip(-(int)_state);
                    transform.rotation = Quaternion.identity;
                    ResetMovement();

                    ConsumeJumpBuffer();
                }

                WallStuckState = _state;
            } 

            // Set wall state when hitting a vertical collider.
            bool CastForWall()
            {
                if (CastCollider(_movement, out RaycastHit2D _hit))
                {
                    if (_hit.normal.x == -_direction)
                        _state = (WallStuck)_direction;

                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Freeze
        private Coroutine freezeCoroutine = null;

        /// <summary>
        /// Freeze the player for a certain duration.
        /// </summary>
        private void Freeze(float _duration)
        {
            if (freezeCoroutine != null)
                StopCoroutine(freezeCoroutine);

            freezeCoroutine = StartCoroutine(DoFreeze(_duration));
        }

        private IEnumerator DoFreeze(float _duration)
        {
            WaitForSeconds _wait = new WaitForSeconds(_duration);
            IsPlayable = false;

            yield return _wait;

            IsPlayable = true;
            freezeCoroutine = null;
        }
        #endregion

        #region Movements
        protected override void MovableUpdate()
        {
            if (!isGrounded)
                CheckWallStatus();

            base.MovableUpdate();

            if (isJumping)
            {
                // Code
            }
            if (isSliding)
            {
                // Code
            }
        }

        // -----------------------

        private bool hasJustLanded = false;

        /// <summary>
        /// Called after velocity has been applied.
        /// </summary>
        protected override void OnAppliedVelocity(Vector2 _movement)
        {
            base.OnAppliedVelocity(_movement);

            hasJustLanded = false;

            // Player moving state.
            bool _isMoving = (lastMovement != 0) && (Mathf.Abs(_movement.x) > .0001f);

            if (isMoving != _isMoving)
            {
                isMoving = _isMoving;
                animator.SetBool(anim_IsMovingID, _isMoving);

                if (!_isMoving)
                    StopSlide();
            }

            // Reset movement if not moving.
            if (!_isMoving && (speed > 0))
                ResetMovement();

            // Set animator ground state.
            if (!isGrounded)
                animator.SetInteger(anim_GroundStateID, _movement.y < 0 ? -1 : 1);

            // Stop jumping when hurting horizontal surfaces.
            if (isJumping)
            {
                for (int _i = 0; _i < castBufferCount; _i++)
                {
                    if (castBuffer[_i].normal.y == -1)
                    {
                        StopJump();
                        break;
                    }
                }
            }

            // Stop dash if needed.
            if (isDashing)
            {
                for (int _i = 0; _i < castBufferCount; _i++)
                {
                    if (Mathf.Abs(castBuffer[_i].normal.x) == 1)
                    {
                        StopDash();
                        break;
                    }
                }
            }

            // Air rotation.
            if (!isGrounded && wallStuckState == 0)
            {
                Quaternion _rotation;

                if (_movement.x == 0)
                    _rotation = Quaternion.identity;
                else
                    _rotation = Quaternion.Euler(0, 0, (Math.Sign(_movement.y) > 0 ? -7.5f : 12.5f) * Math.Sign(_movement.x));

                transform.rotation = Quaternion.RotateTowards(transform.rotation, _rotation, Time.deltaTime * 200);
            }
        }

        /// <summary>
        /// Called when grounded value has been set.
        /// </summary>
        protected override void OnSetGrounded()
        {
            base.OnSetGrounded();

            if (isGrounded)
            {
                hasJustLanded = true;

                StopJump();

                if (wallStuckState != WallStuck.None)
                    WallStuckState = WallStuck.None;

                transform.rotation = Quaternion.identity;
                animator.SetInteger(anim_GroundStateID, 0);

                if (!ConsumeJumpBuffer() && (Time.time - SlideBufferVar < attributes.SlideBufferTime) &&
                (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.Joystick1Button1)) &&
                (Mathf.Abs(lastMovement + force.x) > attributes.SlideRequiredVelocity))
                {
                    Slide();
                }
            }

            // Set last time the player left a platform.
            else
            {
                GetUp();
                coyoteTimeVar = Time.time;
            }
        }
        #endregion

        #region Jump
        private Coroutine   jumpCoroutine =     null;

        private bool Jump()
        {
            // Perform wall jump if against one.
            if (wallStuckState != WallStuck.None)
            {
                StopSlide();
                if (isJumping)
                    StopCoroutine(jumpCoroutine);

                jumpCoroutine = StartCoroutine(WallJump());
                return true;
            }

            // Perform a normal jump if on ground or was recently.
            if (isGrounded || (((Time.time - coyoteTimeVar) < attributes.CoyoteTime)) && !isJumping)
            {
                StopSlide();
                jumpCoroutine = StartCoroutine(HighJump());
                return true;
            }

            // Try to perform wall jump, if near to a wall.
            float _direction = facingSide * ((Physics2D.defaultContactOffset * 2) + attributes.WallJumpGap);
            if (CastCollider(new Vector2(_direction, 0), out RaycastHit2D _hit) || CastCollider(new Vector2(_direction *= -1, 0), out _hit))
            {
                // Set corresponding wall stuck state.
                WallStuckState = (WallStuck)Mathf.Sign(_direction);

                StopSlide();
                if (isJumping)
                    StopCoroutine(jumpCoroutine);
                
                jumpCoroutine = StartCoroutine(WallJump());
                return true;
            }

            return false;
        }

        private IEnumerator HighJump()
        {
            isJumping = true;
            animator.SetTrigger(anim_JumpID);

            // If moving, add extra X velocity to the player.
            if (movement.x != 0)
                MoveHorizontally(facingSide * .5f);

            AddForce(new Vector2(movement.x * .5f, 0));

            // Perform jump over time following the curve.
            float _time = 0;
            float _limit = attributes.HighJumpCurve[attributes.HighJumpCurve.length - 1].time;
            float _treshold = _limit * .7f;

            bool _isHolding = true;

            // While holding the jump button and havn't reached jump maximum duration,
            // add more vertical velocity!
            while (_time < _limit)
            {
                // Move up.
                MoveVertically(attributes.HighJumpCurve.Evaluate(_time));

                // If releasing jump button before max duration,
                // continue adding some intertia.
                if (_isHolding)
                {
                    if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.Joystick1Button0))
                    {
                        _isHolding = false;

                        if (_time < _treshold)
                            _time = _treshold;

                        if (isAtJumpPeek)
                            isAtJumpPeek = false;
                    }

                    // Set if at jump peek when holding button and force is negative.
                    else if ((force.y + instantForce.y) < 0)
                    {
                        // When holding jump button at the top of the jump, apply half gravity.
                        float _difference = movement.y + force.y + instantForce.y;

                        if (_difference > 0 && _difference < attributes.JumpPeekDifference)
                            isAtJumpPeek = true;

                        else
                            isAtJumpPeek = false;
                    }
                    else if (isAtJumpPeek)
                        isAtJumpPeek = false;
                }

                yield return null;
                _time = Mathf.Min(_time + Time.deltaTime, _limit);
            }

            isJumping = false;
            isAtJumpPeek = false;
            jumpCoroutine = null;
        }

        private IEnumerator WallJump()
        {
            isJumping = true;
            int _wallState = (int)wallStuckState;

            // Trigger animation.
            animator.SetTrigger(anim_JumpID);

            // When performing a wall jump, add opposite side X velocity.
            ResetMovement();
            Freeze(.065f + Time.deltaTime);
            force.x = 0;

            // Wait one frame to let all horizontal velocity being reset.
            yield return null;

            if (force.y < 0) force.y *= .25f;
            AddForce(new Vector2(attributes.WallJumpHorizontalForce * _wallState, 0));

            // Perform jump over time following the curve.
            float _time = 0;
            float _limit = attributes.WallJumpCurve[attributes.WallJumpCurve.length - 1].time;
            float _treshold = _limit * .7f;

            bool _isHolding = true;

            // While holding the jump button and havn't reached jump maximum duration,
            // add more vertical velocity !
            while (_time < _limit)
            {
                // Move up
                MoveVertically(attributes.WallJumpCurve.Evaluate(_time));

                // If releasing jump button before max duration,
                // continue adding some intertia.
                if (_isHolding)
                {
                    if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.Joystick1Button0))
                    {
                        _isHolding = false;

                        if (_time < _treshold)
                            _time = _treshold;

                        if (isAtJumpPeek)
                            isAtJumpPeek = false;
                    }

                    // Set if at jump peek when holding button and force is negative.
                    else if ((force.y + instantForce.y) < 0)
                    {
                        // When holding jump button at the top of the jump, apply half gravity.
                        float _difference = movement.y + force.y + instantForce.y;

                        if (_difference > 0 && _difference < attributes.JumpPeekDifference)
                            isAtJumpPeek = true;

                        else
                            isAtJumpPeek = false;
                    }
                    else if (isAtJumpPeek)
                        isAtJumpPeek = false;
                }

                yield return null;
                _time = Mathf.Min(_time + Time.deltaTime, _limit);
            }

            isJumping = false;
            isAtJumpPeek = false;
            jumpCoroutine = null;
        }

        public void StopJump()
        {
            if (isJumping)
            {
                isJumping = false;
                isAtJumpPeek = false;
                StopCoroutine(jumpCoroutine);
                jumpCoroutine = null;
            }
        }
        #endregion

        #region Dash
        private Coroutine dashCoroutine = null;

        private void Dash()
        {
            if (wallStuckState == 0)
            {
                StopJump();
                StopSlide();

                dashCoroutine = StartCoroutine(DoDash());
            }
        }

        private IEnumerator DoDash()
        {
            isDashing = true;
            IsPlayable = false;

            force.y = 0;
            float _side = movement.x != 0 ? Mathf.Sign(movement.x) : facingSide;

            float _boostDuration = attributes.DashDuration * (attributes.DashBoostPercent / 100f);
            float _boostTransitDuration = attributes.DashDuration * (attributes.DashBoostTransitPercent / 100f);
            float _breakDuration = attributes.DashDuration * (attributes.DashBreakPercent / 100f);
            float _breakTransitDuration = attributes.DashDuration * (attributes.DashBreakTransitPercent / 100f);
            float _normalDuration = attributes.DashDuration - (_boostDuration + _boostTransitDuration + _breakDuration + _breakTransitDuration);

            int _step = 0;
            float _time = 0;
            float _speed = attributes.DashBoostSpeed;

            yield return null;

            bool _loop = true;
            while (_loop)
            {
                switch (_step)
                {
                    case 0:
                        if (_time > _boostDuration)
                        {
                            _time -= _boostDuration;
                            AddInstantForce(new Vector2(_side * _speed * -(_time - Time.deltaTime), 0));

                            _step = 1;
                            continue;
                        }
                        break;

                    case 1:
                        _speed = Mathf.SmoothStep(attributes.DashBoostSpeed, attributes.DashNormalSpeed, _time / _boostTransitDuration);

                        if (_time > _boostTransitDuration)
                        {
                            _time -= _boostTransitDuration;
                            AddInstantForce(new Vector2(_side * _speed * -(_time - Time.deltaTime), 0));

                            _step = 2;
                            continue;
                        }
                        break;

                    case 2:
                        if (_time > _normalDuration)
                        {
                            _time -= _normalDuration;
                            AddInstantForce(new Vector2(_side * _speed * -(_time - Time.deltaTime), 0));

                            _step = 3;
                            continue;
                        }
                        break;

                    case 3:
                        _speed = Mathf.SmoothStep(attributes.DashNormalSpeed, attributes.DashBreakSpeed, _time / _breakTransitDuration);

                        if (_time > _breakTransitDuration)
                        {
                            _time -= _breakTransitDuration;
                            AddInstantForce(new Vector2(_side * _speed * -(_time - Time.deltaTime), 0));

                            _step = 4;
                            continue;
                        }
                        break;

                    case 4:
                        if (_time > _breakDuration)
                        {
                            _time -= _breakDuration;
                            AddInstantForce(new Vector2(_side * _speed * -(_time - Time.deltaTime), 0));

                            _loop = false;
                            continue;
                        }
                        break;
                }

                // Dash
                AddInstantForce(new Vector2(_side * _speed * Time.deltaTime, 0));

                yield return null;
                _time += Time.deltaTime;
            }

            isDashing = false;
            IsPlayable = true;
        }

        private void StopDash()
        {
            if (isDashing)
            {
                IsPlayable = true;
                isDashing = false;
                StopCoroutine(dashCoroutine);
                dashCoroutine = null;
            }
        }
        #endregion

        #region Slide
        private Coroutine slideCoroutine = null;

        private bool Slide()
        {
            if (isGrounded && !isJumping && !isSliding && !isCrouched)
            {
                slideCoroutine = StartCoroutine(DoSlide());
                return true;
            }

            return false;
        }

        private IEnumerator DoSlide()
        {
            isSliding = true;
            animator.SetBool(anim_SlideID, true);

            float _momentum = Mathf.Clamp((lastMovement + force.x) / attributes.SpeedCurve[attributes.SpeedCurve.length - 1].value, -1, 1);
            float _maxMomentum = _momentum * attributes.SlideMaxMomentumCoef;
            float _minMomentum = _momentum * attributes.SlideMinMomentumCoef;
            float _movement = _maxMomentum;

            float _forceMomentum = lastMovement * attributes.SlideForceCoef;

            if (hasJustLanded)
            {
                _maxMomentum *= attributes.SlideOnLandCoef;
                _forceMomentum *= attributes.SlideOnLandCoef;
            }

            // If moving, add extra X velocity to the player.
            AddForce(new Vector2(_forceMomentum, 0));

            // Perform slide over time following the curve.
            float _time = 0;
            float _limit = attributes.SlideDuration;
            float _treshold = _limit * .85f;

            bool _isHolding = true;

            // While holding the slide button and havn't reached slide maximum duration,
            // add more movement.
            while (_time < _limit)
            {
                // Move on.
                MoveHorizontally(_movement);
                _movement = Mathf.SmoothStep(_maxMomentum, _minMomentum, _time / _limit);

                // If releasing slide button before max duration,
                // continue adding some intertia.
                if ((_time > .25f) && _isHolding && !Input.GetKey(KeyCode.C) && !Input.GetKey(KeyCode.Joystick1Button1))
                {
                    _isHolding = false;

                    if (_time < _treshold)
                        _time = _treshold;
                }

                yield return null;
                _time = Mathf.Min(_time + Time.deltaTime, _limit);
            }

            isSliding = false;
            if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.Joystick1Button1) || !CanGetUp())
                Crouch();

            animator.SetBool(anim_SlideID, false);
            slideCoroutine = null;

            yield break;
        }
        
        public void StopSlide()
        {
            if (isSliding)
            {
                isSliding = false;
                animator.SetBool(anim_SlideID, false);
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }
        }
        #endregion

        #region Crouch
        private void Crouch()
        {
            if (!isCrouched)
            {
                isCrouched = true;
                animator.SetBool(anim_CrouchID, true);
            }
        }

        private bool CanGetUp()
        {
            return Physics2D.OverlapBox(transform.position + attributes.colliderBounds.center, attributes.colliderBounds.extents - (Vector3.one * .001f), 0, contactFilter, overlapBuffer) == 0;
        }

        private void GetUp()
        {
            if (isCrouched && CanGetUp())
            {
                isCrouched = false;
                animator.SetBool(anim_CrouchID, false);
            }
        }
        #endregion

        #region Monobehaviour
        protected override void OnEnable()
        {
            base.OnEnable();

            if (isPlayable)
                UpdateManager.Instance.Register((IInputUpdate)this);
        }

        protected override void OnDisableCallback()
        {
            base.OnDisableCallback();

            if (isPlayable)
                UpdateManager.Instance.Unregister((IInputUpdate)this);
        }

        private void OnDestroy()
        {
            PlayerCamera.Instance?.RemovePlayer();
        }

        protected override void Start()
        {
            base.Start();
            PlayerCamera.Instance.SetPlayer(collider);
        }
        #endregion

        #endregion
    }
}
