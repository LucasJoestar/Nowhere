﻿// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
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
            private set
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) return;
                isPlayable = !value;
                #endif

                if (value != isPlayable)
                {
                    isPlayable = value;

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
        [SerializeField, ReadOnly] private bool isSliding =     false;
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

        private float coyoteTimeVar = 0;
        #endregion

        #region Methods

        #region Inputs
        private float jumpBufferDown = 0;

        /// <summary>
        /// Check player inputs and execute movement or other actions.
        /// </summary>
        void IInputUpdate.Update()
        {
            float _movement = Input.GetAxis("Horizontal");
            if (_movement != 0) MoveHorizontally(_movement);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
            {
                if (!Jump())
                {
                    jumpBufferDown = Time.time;
                }
            }

            // CHEAT CODES
            if (Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.F)) AddForce(new Vector2(facingSide * 20, 0));
        }

        private void ConsumeJumpBuffer()
        {
            if ((Time.time - jumpBufferDown < attributes.JumpBufferTime) &&
                (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Joystick1Button0)))
            {
                Jump();
            }
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

            else
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
        }

        // -----------------------

        /// <summary>
        /// Called after velocity has been applied.
        /// </summary>
        protected override void OnAppliedVelocity(Vector2 _movement)
        {
            base.OnAppliedVelocity(_movement);

            // Player moving state.
            bool _isMoving = (lastMovement != 0) && (Mathf.Abs(_movement.x) > .0001f);

            if (isMoving != _isMoving)
            {
                isMoving = _isMoving;
                animator.SetBool(anim_IsMovingID, _isMoving);
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
                        return;
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
                StopJump();

                if (wallStuckState != WallStuck.None)
                    WallStuckState = WallStuck.None;

                transform.rotation = Quaternion.identity;
                animator.SetInteger(anim_GroundStateID, 0);

                ConsumeJumpBuffer();
            }

            // Set last time the player left a platform.
            else
                coyoteTimeVar = Time.time;
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
                // Move up
                MoveVertically(attributes.HighJumpCurve.Evaluate(_time));

                // If releasing jump button before max duration,
                // continue adding some intertia.
                if (_isHolding)
                {
                    if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Joystick1Button0))
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
                    if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Joystick1Button0))
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

        #region Slide
        private Coroutine slideCoroutine = null;

        private bool Slide()
        {
            if (isGrounded && !isJumping && !isSliding)
            {
                slideCoroutine = StartCoroutine(DoSlide());
                return true;
            }

            return false;
        }

        private IEnumerator DoSlide()
        {
            isSliding = true;

            // Perform slide over time following the curve.
            float _time = 0;
            float _limit = attributes.SlideCurve[attributes.SlideCurve.length - 1].time;

            while (true)
            {
                if (_time == _limit)
                    break;

                yield return null;
                _time = Mathf.Min(_time + Time.deltaTime, _limit);
            }

            isSliding = false;
            slideCoroutine = null;

            yield break;
        }
        
        public void StopSlide()
        {
            if (isSliding)
            {
                isSliding = false;
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }
        }
        #endregion

        #region Monobehaviour
        protected override void OnDisable()
        {
            base.OnDisable();

            if (isPlayable)
                UpdateManager.Instance.Unregister((IInputUpdate)this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (isPlayable)
                UpdateManager.Instance.Register((IInputUpdate)this);
        }
        #endregion

        #endregion
    }
}
