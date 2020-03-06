using System.Collections;
using UnityEngine;

public class PlayerController : Movable
{
    #region Fields / Properties

    #region Constants
    /*********************************
     *******     CONSTANTS     *******
     ********************************/

    /// <summary>
    /// Amount of time after which speed should be reset when not moving.
    /// </summary>
    public const float                      SpeedResetTime =    .2f;
    #endregion

    #region Parameters
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>Backing field for <see cref="IsJumping"/>.</summary>
    [SerializeField]
    protected bool                          isJumping =         false;

    /// <summary>Backing field for <see cref="IsSliding"/>.</summary>
    [SerializeField]
    protected bool                          isSliding =         false;


    /// <summary>
    /// Player controller attributes, stored in a scriptable object.
    /// </summary>
    [SerializeField]
    protected PlayerControllerAttributes    attributes =        null;


    /// <summary>
    /// Indicates if the player is currently stuck to a wall, and if so on which side.
    /// </summary>
    [SerializeField]
    protected WallStuckState                wallStuckState =    0;


    /**********************************
     *******     PROPERTIES     *******
     *********************************/

    /// <summary>
    /// Indicates if the player is currently jumping or not.
    /// </summary>
    public bool             IsJumping
    {
        get { return isJumping; }
        protected set
        {
            isJumping = value;
        }
    }

    /// <summary>
    /// Indicates if the player is currently sliding or not.
    /// </summary>
    public bool             IsSliding
    {
        get { return isSliding; }
        protected set
        {
            isSliding = value;
        }
    }
    #endregion

    #region Coroutines & Memory
    /**********************************
     *******     COROUTINES     *******
     *********************************/

    /// <summary>Stored coroutine of the <see cref="DoJump"/> method.</summary>
    protected Coroutine     doJumpCoroutine =           null;

    /// <summary>Stored coroutine of the <see cref="DoSlide"/> method.</summary>
    protected Coroutine     doSlideCoroutine =          null;


    /**********************************
     *********     MEMORY     *********
     *********************************/

    /// <summary>
    /// Time used to get current speed value according to <see cref="PlayerControllerAttributes.GroundSpeedCurve"/>.
    /// </summary>
    protected float         speedCurveTime =            0;
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
    protected void CheckInputs()
    {
        // Executes player actions and then move them
        CheckActions();
        CheckMovement();
    }

    /// <summary>
    /// Executes actions related to player inputs.
    /// </summary>
    protected void CheckActions()
    {
        // Jump if pressing associated key
        if (Input.GetKeyDown(KeyCode.Space)) Jump();
    }

    /// <summary>
    /// Move the player according to input movement.
    /// </summary>
    protected void CheckMovement()
    {
        // If no player movement, decelerate while speed is not null
        float _movement = Input.GetAxis("Horizontal");
        if (_movement == 0)
        {
            if (speed != 0) Decelerate();
            return;
        }
        else Accelerate();

        // Move the object according to movement input
        Move(new Vector2(_movement * GetMovementSpeed(), 0));
    }
    #endregion

    #region Speed
    /*********************************
     *********     SPEED     *********
     ********************************/

    /// <summary>
    /// Increase player speed if below max value.
    /// </summary>
    protected void Accelerate()
    {
        // Increase speed according to the associated curve.
        AnimationCurve _curve = isGrounded ? attributes.GroundSpeedCurve : attributes.AirSpeedCurve;

        if (speed < _curve[_curve.length - 1].value)
        {
            speedCurveTime = Mathf.Min(speedCurveTime + Time.deltaTime, _curve[_curve.length - 1].time);

            speed = _curve.Evaluate(speedCurveTime);
        }
    }

    /// <summary>
    /// Decrease player speed if greater than zero.
    /// </summary>
    protected void Decelerate()
    {
        // Decrease speed with a deceleration force
        speed = Mathf.MoveTowards(speed, 0, attributes.SpeedDecelerationForce * Time.deltaTime);

        // Reset speed curve time when reaching zero
        if ((speed == 0) && (speedCurveTime > 0)) speedCurveTime = 0;
    }

    /// <summary>
    /// Reset base movement speed.
    /// </summary>
    public void ResetSpeed()
    {
        speedCurveTime = 0;
        speed = 0;
    }
    #endregion

    #region Movements
    /*********************************
     *******     MOVEMENTS     *******
     ********************************/

    /// <summary>
    /// Add an external force to this object movement.
    /// </summary>
    /// <param name="_force">Force to add to the object.</param>
    public override void AddForce(Vector2 _force)
    {
        base.AddForce(_force);
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
    protected override void Move(Vector2 _movement)
    {
        base.Move(_movement);
    }


    /// <summary>
    /// Apply stored velocity and move the object.
    /// </summary>
    /// <returns>Returns final movement.</returns>
    /*protected override Vector2 ApplyVelocity()
    {
        // Manipulate force and movement according to if grounded or not
        // Decelerate movement
        if (((lastMovement.x < 0) && (movement.x > lastMovement.x)) ||
            ((lastMovement.x > 0) && (movement.x < lastMovement.x)))
        {
            // About-Turn
            if ((movement.x != 0) && (Mathf.Sign(lastMovement.x) != Mathf.Sign(movement.x)))
            {
                if (isGrounded)
                {
                    movement.x = Mathf.MoveTowards(lastMovement.x, movement.x, GetMovementSpeed() * .1f);
                }
                else
                {
                    movement.x = Mathf.MoveTowards(lastMovement.x, movement.x, GetMovementSpeed() * .5f);
                }
            }
            // Deceleration
            else
            {
                movement.x = Mathf.MoveTowards(lastMovement.x, movement.x, Time.deltaTime * (isGrounded ? .4f : .3f));
            }
        }

        Velocity = force + movement;
        Vector2 _originalVelocity = velocity;
        Vector2 _velocity = base.ApplyVelocity();
        force.y = 0;
        if (_velocity.x == 0)
        {
            force.x = 0;
            movement.x = 0;
        }

        if (movement.x != 0)
        {
            if (force.x != 0)
            {
                if (Mathf.Sign(movement.x) != Mathf.Sign(force.x))
                {
                    force.x = Mathf.MoveTowards(force.x, 0, movement.x);
                }
            }
        }
        else if (force.x != 0)
        {
            if (!isGrounded)
            {
                force.x = Mathf.MoveTowards(force.x, 0, Time.deltaTime * 10);
            }
        }

        // Reset movement
        lastMovement = movement;
        movement = Vector2.zero;
        return _velocity;
    }*/
    #endregion

    #region Special Moves
    /***********************************
     ******     SPECIAL MOVES     ******
     **********************************/

    /// <summary>
    /// Makes the player start jumping.
    /// </summary>
    /// <returns>Return true if successfully started a jump, false otherwise.</returns>
    public bool Jump()
    {
        // Return false if cannot jump
        if (!isGrounded && (wallStuckState == WallStuckState.None)) return false;

        if (isJumping) StopCoroutine(doJumpCoroutine);
        StopSlide();

        doJumpCoroutine = StartCoroutine(DoJump());
        return true;
    }

    /// <summary>
    /// Makes the player start sliding.
    /// </summary>
    /// <returns>Return true if successfully started a slide, false otherwise.</returns>
    public bool Slide()
    {
        // Return false if cannot slide
        if (!isGrounded || isJumping || isSliding) return false;

        doSlideCoroutine = StartCoroutine(DoSlide());
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
        StopCoroutine(doJumpCoroutine);
        doJumpCoroutine = null;

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
        StopCoroutine(doSlideCoroutine);
        doSlideCoroutine = null;

        return true;
    }


    /**********************************
     ******     IENUMERATORS     ******
     *********************************/

    /// <summary>
    /// Makes the player perform a jump over time.
    /// </summary>
    /// <returns>IEnumerator, baby.</returns>
    protected IEnumerator DoJump()
    {
        AnimationCurve _curve = null;
        isJumping = true;

        // Executes actions depending on performing a normal or a wall jump.
        if (isGrounded)
        {
            _curve = attributes.JumpCurve;

            // If moving, add extra X velocity to the player
            //if (isMoving) Velocity = new Vector2(velocity.x + (speed * isFacingRight.ToSign() * .25f), velocity.y);
        }
        else
        {
            _curve = attributes.WallJumpCurve;

            // When performing a wall jump, add opposite side X velocity.
            //Velocity = new Vector2(velocity.x + (attributes.WallJumpXVelocity * (int)wallStuckState), velocity.y);
        }

        // Perform jump over time following the associated curve
        float _time = 0;
        float _limit = _curve[_curve.length - 1].time;

        // While holding the jump button and havn't reached jump maximum duration,
        // add more vertical velocity !
        while (Input.GetKey(KeyCode.Space))
        {
            // Move up
            Move(new Vector2(0, _curve.Evaluate(_time)));

            if (_time == _limit) break;
            
            yield return null;
            _time = Mathf.Min(_time + Time.deltaTime, _limit);
        }

        isJumping = false;
        doJumpCoroutine = null;

        yield break;
    }

    /// <summary>
    /// Makes the player perform a jump over time.
    /// </summary>
    /// <returns>IEnumerator, baby.</returns>
    protected IEnumerator DoSlide()
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
        doSlideCoroutine = null;

        yield break;
    }
    #endregion

    #endregion

    #region Unity Methods
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
