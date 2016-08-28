using UnityEngine;
using System.Collections;

/*
 * Example implementation of the SuperStateMachine and SuperCharacterController
 */
[RequireComponent(typeof(SuperCharacterController))]
[RequireComponent(typeof(GridPuzzleActor))]
public class GridPuzzlePlayerMachine : SuperStateMachine 
{
    public Transform AnimatedMesh;

	public Rigidbody rb;
	private Animator Animator;

    public float WalkSpeed = 4.0f;
    public float WalkAcceleration = 30.0f;
	public float RunSpeed = 4.0f;
	public float RunAcceleration = 8.0f;
    public float JumpAcceleration = 5.0f;
    public float JumpHeight = 0.5f;
    public float Gravity = 25.0f;

    // Add more states by comma separating them
	enum PlayerStates { Idle, Walk, Run, Dive, Jump, Fall, Float }

    public SuperCharacterController controller;

    // current velocity
    private Vector3 moveDirection;
    // current direction our character's art is facing
    public Vector3 lookDirection { get; private set; }

	private GridPuzzleActor actor;

	void Awake()
	{
		this.rb = this.GetComponent<Rigidbody>();
		this.Animator = this.GetComponent<Animator>();

		//this.rb.isKinematic = true;
		this.moveDirection = Vector3.zero;
		this.rb.velocity = Vector3.zero;
	}

	void Start() 
	{
	    // Put any code here you want to run ONCE, when the object is initialized

		actor = gameObject.GetComponent<GridPuzzleActor>();

        // Grab the controller object from our object
        controller = gameObject.GetComponent<SuperCharacterController>();
		
		// Our character's current facing direction, planar to the ground
        lookDirection = transform.forward;

        // Set our currentState to idle on startup
        currentState = PlayerStates.Idle;
	}

    protected override void EarlyGlobalSuperUpdate()
    {
		// Rotate out facing direction horizontally based on mouse input
        // Put any code in here you want to run BEFORE the state's update function.
        // This is run regardless of what state you're in

		//lookDirection = Quaternion.AngleAxis(input.Current.MouseInput.x, controller.up) * lookDirection;
    }

    protected override void LateGlobalSuperUpdate()
    {
        // Put any code in here you want to run AFTER the state's update function.
        // This is run regardless of what state you're in

        // Move the player by our velocity every frame
        transform.position += moveDirection * controller.deltaTime;

        // Rotate our mesh to face where we are "looking"
        AnimatedMesh.rotation = Quaternion.LookRotation(lookDirection, controller.up);
    }

    public bool AcquiringGround()
    {
        return controller.currentGround.IsGrounded(false, 0.01f);
    }

	public bool MaintainingGround()
    {
        return controller.currentGround.IsGrounded(true, 0.2f);
    }

    /// <summary>
    /// Constructs a vector representing our movement local to our lookDirection, which is
    /// controlled by the camera
    /// </summary>
    private Vector3 LocalMovement()
    {

        Vector3 right = Vector3.Cross(controller.up, lookDirection);

        Vector3 local = Vector3.zero;

		if (actor.IsMoving())
        {
            local += lookDirection * 1.0f;
        }

        return local.normalized;
    }

    // Calculate the initial velocity of a jump based off gravity and desired maximum height attained
    private float CalculateJumpSpeed(float jumpHeight, float gravity)
    {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

	private IEnumerator EnableBool( string key, float time, System.Action act=null )
    {
        yield return new WaitForSeconds(time);        
		this.Animator.SetBool(key, true);
        if (act != null)
            act();
    }

	private IEnumerator DisableBool( string key, float time, System.Action act=null )
    {
        yield return new WaitForSeconds(time);        
       	this.Animator.SetBool(key, false);
        if (act != null)
            act();
    }

	/*void Update () {
	 * Update is normally run once on every frame update. We won't be using it
     * in this case, since the SuperCharacterController component sends a callback Update 
     * called SuperUpdate. SuperUpdate is recieved by the SuperStateMachine, and then fires
     * further callbacks depending on the state
	}*/

    // Below are the three state functions. Each one is called based on the name of the state,
    // so when currentState = Idle, we call Idle_EnterState. If currentState = Jump, we call
    // Jump_SuperUpdate()
    void Idle_EnterState()
    {
		Debug.Log("Idle_EnterState");
        controller.EnableSlopeLimit();
        controller.EnableClamping();
    }

    void Idle_SuperUpdate()
    {
        // Run every frame we are in the idle state

        if (false)
        {
            currentState = PlayerStates.Jump;
            return;
        }

		if (false)
        {
			currentState = PlayerStates.Dive;
            return;
        }

        if (!MaintainingGround())
        {
            //currentState = PlayerStates.Fall;
            return;
        }

		if (false)
        {
            currentState = PlayerStates.Walk;
            return;
        }

		if (actor.IsMoving())
        {
            currentState = PlayerStates.Run;
            return;
        }

        // Apply friction to slow us to a halt
        //moveDirection = Vector3.MoveTowards(moveDirection, Vector3.zero, 20.0f * controller.deltaTime);
    }

    void Idle_ExitState()
    {
		Debug.Log("Idle_ExitState");
        // Run once when we exit the idle state
    }

	void Walk_EnterState()
    {
		Debug.Log("Walk_EnterState");
		this.Animator.SetBool("Walk", true);
    }

	void Walk_ExitState()
    {
		this.Animator.SetBool("Walk", false);
    }

    void Walk_SuperUpdate()
    {
		if (false)
        {
            currentState = PlayerStates.Jump;
            return;
        }

		if (false)
        {
			currentState = PlayerStates.Dive;
            return;
        }

        if (!MaintainingGround())
        {
			//currentState = PlayerStates.Fall;
            return;
        }

		if (actor.IsMoving())
        {
            moveDirection = Vector3.MoveTowards(moveDirection, LocalMovement() * WalkSpeed, WalkAcceleration * controller.deltaTime);
        }
        else
        {
            currentState = PlayerStates.Idle;
            return;
        }
    }

	void Run_EnterState()
    {
		Debug.Log("Run_EnterState");
		this.Animator.SetBool("Run", true);
    }

	void Run_ExitState()
    {
		this.Animator.SetBool("Run", false);
    }

	void Run_SuperUpdate()
    {
        if (false)
        {
            currentState = PlayerStates.Jump;
            return;
        }

		if (false)
        {
			currentState = PlayerStates.Dive;
            return;
        }

        if (!MaintainingGround())
        {
            //currentState = PlayerStates.Fall;
            return;
        }

		if (actor.IsMoving())
        {
			if (this.Animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
			{
            	moveDirection = Vector3.MoveTowards(moveDirection, LocalMovement() * RunSpeed, RunAcceleration * controller.deltaTime);
            }
        }
        else
        {
            currentState = PlayerStates.Idle;
            return;
        }
    }

    void Jump_EnterState()
    {
		Debug.Log("Jump_EnterState");
        controller.DisableClamping();
        controller.DisableSlopeLimit();

        this.Animator.SetBool("Jump", true);
		this.StartCoroutine(DisableBool("Jump", 0.1f));
    }

    void Jump_SuperUpdate()
    {
		if (!this.Animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
		//if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !Animator.IsInTransition(0))
		{
			currentState = PlayerStates.Idle;
		}

		// Apply friction to slow us to a halt
        moveDirection = Vector3.MoveTowards(moveDirection, Vector3.zero, 5.0f * controller.deltaTime);
    }

	void Dive_EnterState()
    {
		Debug.Log("Dive_EnterState");
        controller.DisableClamping();
        controller.DisableSlopeLimit();

        this.Animator.SetBool("Dive", true);
		this.StartCoroutine(DisableBool("Dive", 0.1f));
    }

	void Dive_SuperUpdate()
    {
		if (!this.Animator.GetCurrentAnimatorStateInfo(0).IsName("Dive"))
		//if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !Animator.IsInTransition(0))
		{
			currentState = PlayerStates.Idle;
		}

		// Apply friction to slow us to a halt
        moveDirection = Vector3.MoveTowards(moveDirection, Vector3.zero, 5.0f * controller.deltaTime);
    }

	void Float_EnterState()
    {
		Debug.Log("Float_EnterState");
        controller.DisableClamping();
        controller.DisableSlopeLimit();
    }

	void Float_SuperUpdate()
    {
        if (AcquiringGround())
        {
            moveDirection = Math3d.ProjectVectorOnPlane(controller.up, moveDirection);
            currentState = PlayerStates.Idle;
            return;
        }
    }

    void Fall_EnterState()
    {
		Debug.Log("Fall_EnterState");
        controller.DisableClamping();
        controller.DisableSlopeLimit();

        // moveDirection = trueVelocity;
    }

    void Fall_SuperUpdate()
    {
        if (AcquiringGround())
        {
            moveDirection = Math3d.ProjectVectorOnPlane(controller.up, moveDirection);
            currentState = PlayerStates.Idle;
            return;
        }

        moveDirection -= controller.up * Gravity * controller.deltaTime;
    }
}
