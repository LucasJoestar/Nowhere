using System.Collections;
using UnityEngine;

public class NWH_PlayerController : NWH_Movable
{
    #region Fields / Properties

    #region Constants
    /// <summary>
    /// Amount of time after which speed should be reset when not moving.
    /// </summary>
    public const float  RESET_SPEED_TIME =  .2f;
    #endregion

    #region Parameters
    /**********************************
     *********     FIELDS     *********
     *********************************/


    /// <summary>Backing field for <see cref="IsJumping"/>.</summary>
    [SerializeField] protected bool                             isJumping =         false;

    /// <summary>Backing field for <see cref="IsSliding"/>.</summary>
    [SerializeField] protected bool                             isSliding =         false;


    /// <summary>
    /// Player controller attributes, stored in a scriptable object.
    /// </summary>
    [SerializeField] protected NWH_PlayerController_Attributes  attributes =        null;


    /// <summary>
    /// Indicates if the player is currently stuck to a wall, and if so on which side.
    /// </summary>
    [SerializeField] protected WallStuckState                   wallStuckState =    0;


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
    protected Coroutine     cDoJump =           null;

    /// <summary>Stored coroutine of the <see cref="DoSlide"/> method.</summary>
    protected Coroutine     cDoSlide =          null;


    /**********************************
     *********     MEMORY     *********
     *********************************/


    /// <summary>
    /// Time since player has not moved.
    /// </summary>
    protected float         notMovingTime =     0;

    /// <summary>
    /// Time used to get current speed value according to <see cref="NWH_PlayerController_Attributes.SpeedCurve"/>.
    /// </summary>
    protected float         speedCurveTime =    0;
    #endregion

    #endregion

    #region Methods

    #region Original Methods

    #region Inputs
    /**********************************
     *********     INPUTS     *********
     *********************************/


    protected void CheckActions()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Jump();
    }

    protected void CheckMovement()
    {
        // If no movement, skip this frame
        float _movement = _movement = Input.GetAxis("Horizontal");
        if (_movement == 0)
        {
            // Reset speed after not moving during a certain amount of time
            if (notMovingTime < RESET_SPEED_TIME)
            {
                notMovingTime += Time.deltaTime;
                if (notMovingTime >= RESET_SPEED_TIME) ResetSpeed();
            }

            return;
        }
        if (notMovingTime > 0) notMovingTime = 0;

        // Increase speed according to the associated curve.
        if (speedCurveTime < attributes.SpeedCurve[attributes.SpeedCurve.length - 1].time)
        {
            speedCurveTime = Mathf.Min(speedCurveTime + Time.deltaTime, attributes.SpeedCurve[attributes.SpeedCurve.length - 1].time);

            speed = attributes.SpeedCurve.Evaluate(speedCurveTime);
        }

        if (!isOnGround)
        {
            _movement /= Time.deltaTime * speed;
            if (_movement > 0)
            {
                if (velocity.x >= _movement) return;
            }
            else if (velocity.x <= _movement) return;

            Velocity = new Vector2(velocity.x + ((_movement - velocity.x) * (speed * Time.deltaTime)), velocity.y);
            return;
        }

        MoveInDirection(new Vector2(_movement, 0));
    }
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
        if (!isOnGround && (wallStuckState == WallStuckState.None)) return false;

        if (isJumping) StopCoroutine(cDoJump);
        StopSlide();

        cDoJump = StartCoroutine(DoJump());
        return true;
    }

    /// <summary>
    /// Makes the player start sliding.
    /// </summary>
    /// <returns>Return true if successfully started a slide, false otherwise.</returns>
    public bool Slide()
    {
        // Return false if cannot slide
        if (!isOnGround || isJumping || isSliding) return false;

        cDoSlide = StartCoroutine(DoSlide());
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
        StopCoroutine(cDoJump);
        cDoJump = null;

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
        StopCoroutine(cDoSlide);
        cDoSlide = null;

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
        if (isOnGround)
        {
            _curve = attributes.JumpCurve;

            // If moving, add extra X velocity to the player
            if (isMoving) Velocity = new Vector2(velocity.x + (speed * isFacingRight.ToSign() * .25f), velocity.y);
        }
        else
        {
            _curve = attributes.WallJumpCurve;

            // When performing a wall jump, add opposite side X velocity.
            Velocity = new Vector2(velocity.x + (attributes.WallJumpXVelocity * (int)wallStuckState), velocity.y);
        }

        // Perform jump over time following the associated curve
        float _time = 0;
        float _limit = _curve[_curve.length - 1].time;

        // While holding the jump button and havn't reached jump maximum duration,
        // add more vertical velocity !
        while (Input.GetKey(KeyCode.Space))
        {
            // Move up
            Velocity = new Vector2(velocity.x, velocity.y + _curve.Evaluate(_time));

            if (_time == _limit) break;

            yield return null;
            _time = Mathf.Min(_time + Time.deltaTime, _limit);
        }

        isJumping = false;
        cDoJump = null;

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
            DoPerformMovement(new Vector2(attributes.SlideCurve.Evaluate(_time) * isFacingRight.ToSign(), 0));

            if (_time == _limit) break;

            yield return null;
            _time = Mathf.Min(_time + Time.deltaTime, _limit);
        }

        isSliding = false;
        cDoSlide = null;

        yield break;
    }
    #endregion

    #region Movements
    /*********************************
     *******     MOVEMENTS     *******
     ********************************/


    /// <summary>
    /// Resets base movement speed.
    /// </summary>
    public void ResetSpeed()
    {
        speedCurveTime = 0;
        speed = attributes.SpeedCurve[0].value;
    }
    #endregion

    #endregion

    #region Unity Methods
    // Awake is called when the script instance is being loaded
    protected override void Awake()
	{
        base.Awake();

        // Destroy script if no attribute is linked to it.
        if (!attributes)
        {
            Debug.LogError($"Alert ! No Attributes detected on the Player Controller \"{name}\" ! The script is being destroyed...");

            Destroy(this);
            return;
        }
	}

    private void FixedUpdate()
    {
        CheckMovement();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Get attributes values
        fallMinVelocity = attributes.FallMinVelocity;
        speed = attributes.SpeedCurve[0].value;
    }

    private void Update()
    {
        CheckActions();
    }
    #endregion

    #endregion
}
