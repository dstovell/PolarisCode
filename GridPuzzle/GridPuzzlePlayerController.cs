using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DSTools;

public class GridPuzzlePlayerPathNode
{
	public GridPuzzlePlayerPathNode(Vector3 dest)
	{
		this.destination = dest;
	}

	public Vector3 destination;
}

public class GridPuzzlePlayerPath
{
	private List<GridPuzzlePlayerPathNode> nodes;
	private int currentTargetNode;
	private Vector3 postion;

	public GridPuzzleCube targetCube;
	public GridPuzzleCubeRow targetRow;

	public bool isDone;

	public enum MoveType
	{
		Run,
		Jump
	}
	public MoveType moveType = MoveType.Run;

	public bool jumpUp = true;

	public GridPuzzlePlayerPath(List<Vector3> points, MoveType mt = MoveType.Run)
	{
		this.moveType = mt;
		this.nodes = new List<GridPuzzlePlayerPathNode>();
		for (int i=0; i<points.Count; i++)
		{
			this.nodes.Add( new GridPuzzlePlayerPathNode(points[i]));
		}
		Init();
	}

	public GridPuzzlePlayerPath(List<GridPuzzlePlayerPathNode> points, MoveType mt = MoveType.Run)
	{
		this.moveType = mt;
		this.nodes = points;
		Init();
	}

	public void Init()
	{
		this.postion = (this.nodes.Count > 0) ? this.nodes[0].destination : Vector3.zero;
		this.currentTargetNode = (this.nodes.Count > 1) ? 1 : 0;
		this.isDone = false;
	}

	public Vector3 GetDirection()
	{
		if ((this.nodes.Count == 0) || this.isDone)
		{
			return Vector3.forward;
		}

		Vector3 dest = this.nodes[this.currentTargetNode].destination;
		return (dest - this.postion).normalized;
	}

	public Vector3 Move(float speed, float deltaT)
	{
		if ((this.nodes.Count == 0) || this.isDone)
		{
			return this.postion;
		}

		Vector3 dest = this.nodes[this.currentTargetNode].destination;

		if (this.moveType == MoveType.Run)
		{
			this.postion = Vector3.MoveTowards(this.postion, dest, speed*deltaT);
		}
		else if (this.moveType == MoveType.Jump)
		{
			if (jumpUp)
			{
				if (this.postion.y < (dest.y + 0.5f))
				{
					dest.y += 1.0f;
				}
			}
			else 
			{
				if (Mathf.Abs(this.postion.x - dest.x) > 0.25f)
				{
					speed *= 0.8f;
					dest.y = this.postion.y;
				}
				this.postion = Vector3.MoveTowards(this.postion, dest, speed*deltaT);
			}

		}

		if (Vector3.Distance(this.postion, dest) < 0.01)
		{
			if (this.currentTargetNode == (this.nodes.Count - 1))
			{
				this.isDone = true;
				this.postion = this.nodes[this.nodes.Count - 1].destination;
			}
			else
			{
				this.currentTargetNode++;
			}
		}

		return this.postion;
	}
}


public class GridPuzzlePlayerController : GridPuzzleNavigable
{
	public float MoveSpeed = 1f;
	public float RotateSpeed = 1f;

	public enum State
	{
		Idle,
		Walk,
		Jog,
		Run,
		Jump,
		Climb
	};
	public State currentState;
	private float timeInState = 0;

	public enum Surface
	{
		Floor,
		Ceiling,
	}
	public Surface currentSurface;

	public GridPuzzleCube currentCube = null;
	public GridPuzzleCubeRow currentCubeRow = null;

	public Transform [] Visors;
	[Range(0.0f, 1.0f)]
	public float VisorOpenT = 0f;
	Vector3 VisorOpenAngle = new Vector3(-90, 0, 0);

	public bool Helmet = true;

	private Rigidbody rb;

	public Animator anim;

	private GridPuzzlePlayerPath movePath;

	public SWS.PathManager currentPath;
	public bool moverStarted = false;

	public Quaternion desiredRotation;

	private GameObject lastTeleport;

	private SWS.splineMove mover;
	private SWS.PathManager pathManager;

	private CapsuleCollider capsule;

	// Use this for initialization
	void Start() 
	{
		this.InitMessenger("GridPuzzlePlayerController");
		this.currentState = State.Idle;
		this.currentSurface = Surface.Floor;
		this.rb = this.gameObject.GetComponent<Rigidbody>();
		this.capsule = this.gameObject.GetComponent<CapsuleCollider>();
		this.mover = this.gameObject.GetComponent<SWS.splineMove>();
		this.pathManager = this.gameObject.GetComponent<SWS.PathManager>();

		if (this.anim == null)
		{
			this.anim = this.gameObject.GetComponentInChildren<Animator>();
		}

		this.mover.lockRotation = DG.Tweening.AxisConstraint.X;
	}

	public bool IsPlayingAnimType(string type)
	{
		if (type == "Jog")
		{
			return this.IsPlaying("Jog_Fwd_Start") || this.IsPlaying("Jog_Fwd") || this.IsPlaying("Jog_Fwd_Stop");
		}
		else if (type == "Run")
		{
			return this.IsPlaying("Run_Fwd_Start") || this.IsPlaying("Run_Fwd") || this.IsPlaying("Run_Fwd_Stop");
		}
		else if (type == "Walk")
		{
			return this.IsPlaying("Walk_Fwd_Start") || this.IsPlaying("Walk_Fwd") || this.IsPlaying("Walk_Fwd_Stop");
		}
		else if (type == "Jump")
		{
			return this.IsPlaying("Jump") || this.IsPlaying("Jump_Land");
		}
		else if (type == "Climb")
		{
			return this.IsPlaying("Mount");
		}

		return false;
	}

	public bool IsPlayingMovingAnim()
	{
		return this.IsPlayingAnimType("Walk") || this.IsPlayingAnimType("Jog") || this.IsPlayingAnimType("Run");
	}

	public void SetAnimBool(string name, bool enabled)
	{
		//Debug.Log(name + " enabled=" + enabled);
		this.anim.SetBool(name, enabled);
	}

	public void SetAnimTrigger(string name)
	{
		//Debug.Log(name + " triggered");
		this.anim.SetTrigger(name);
	}

	public void SetMoveType(string type = "")
	{
		if (this.anim.GetBool(type))
		{
			return;
		}

		string [] states = new string[]{"Idle", "Walk", "Jog", "Run"};

		this.anim.CrossFade(type, 0.5f);

		this.SetAnimBool(type, true);

		for (int i=0; i<states.Length; i++)
		{
			if (states[i] != type)
			{
				this.SetAnimBool(states[i], false);
			}
		}
	}

	public void SetState(State newState)
	{
		//Debug.LogError("SetState newState=" + newState.ToString());
		if (newState == this.currentState)
		{
			return;
		}

		switch(newState)
		{
		case State.Idle:
			//this.capsule.enabled = true;
			this.SetMoveType("Idle");
			break;
		case State.Walk:
			//this.capsule.enabled = true;
			this.SetMoveType("Walk");
			this.mover.easeType = DG.Tweening.Ease.Linear;
			this.MoveSpeed = 2f;
			break;
		case State.Jog:
			//this.capsule.enabled = true;
			this.SetMoveType("Jog");
			this.mover.easeType = DG.Tweening.Ease.InOutSine;
			this.MoveSpeed = 5f;
			break;
		case State.Run:
			//this.capsule.enabled = true;
			this.SetMoveType("Run");
			this.mover.easeType = DG.Tweening.Ease.InOutSine;
			this.MoveSpeed = 5f;
			break;
		case State.Jump:
			//this.capsule.enabled = true;
			//this.SetAnimTrigger("Jump");
			this.SetAnimTrigger("Dismount");
			this.mover.easeType = DG.Tweening.Ease.OutSine;
			this.MoveSpeed = 5f;
			break;
		case State.Climb:
			//this.capsule.enabled = false;
			this.SetAnimTrigger("Mount");
			this.mover.easeType = DG.Tweening.Ease.Linear;
			this.MoveSpeed = 0.8f;
			break;
		default:
			break;
		}
		this.currentState = newState;
		this.timeInState = 0f;
	}

	void Update () 
	{
		UpdateGravity();
		CheckNavTriggers();

		switch(this.currentState)
		{
		case State.Idle:
			break;
		case State.Walk:
			break;
		case State.Jog:
			break;
		case State.Run:
			break;
		case State.Jump:
			break;
		case State.Climb:
			break;
		default:
			break;
		}

		if (this.IsMoving() || this.IsJumping() || this.IsClimbing())
		{
			
			bool isStateReady = (	(this.IsMoving() && this.IsPlayingMovingAnim())
								||	(this.IsJumping() && (this.timeInState > 0.5f))
								||	(this.IsClimbing() && (this.timeInState > 0.1f))	);

			if (isStateReady && (this.currentPath != null))
			{
				if(!this.moverStarted)
				{
					this.moverStarted = true;
					this.mover.speed = this.MoveSpeed;
					this.mover.SetPath(this.currentPath);
				}
				else 
				{
					//Debug.Log("currentPoint=" + this.mover.currentPoint + " / " + this.mover.waypoints.Length );
					bool atFinalNode = (this.mover.currentPoint == (this.mover.waypoints.Length-1));
					if (atFinalNode)
					{
						if (GridPuzzleCamera.Is2DAngle(this.angle))
						{
							GridPuzzleCube closestCube = (this.currentCubeRow != null) ? this.currentCubeRow.GetClosestCube(this.transform.position) : null;
							if (closestCube != null)
							{
								Vector3 nav = closestCube.NavPosition;
								if (this.IsJumping())
								{
									nav.y = this.transform.position.y;
								}
								this.transform.position = nav;
							}
						}

						this.Stop();
					}
				}
			}
		}

		Vector3 angles = this.transform.eulerAngles;
		angles.x = 0;
		this.transform.rotation = Quaternion.Euler(angles);

		this.timeInState += Time.deltaTime;
	}

	public void UpdateGravity()
	{
		if (!this.IsJumping() && !this.IsClimbing())
		{
			this.rb.AddForce(9.8f * this.rb.mass * Vector3.down);
		}
		return;

		if (this.currentCube != null)
		{
			Vector3 gravity = this.currentCube.EvaluateMagneticGravity(this) * 9.8f * this.rb.mass;
			this.rb.AddForce(gravity);

			if (this.currentSurface == Surface.Floor)
			{
				if (gravity.y > 0f)
				{
					this.currentSurface = Surface.Ceiling;
					this.Stop();
				}
			}
			else if (this.currentSurface == Surface.Ceiling)
			{
				if (gravity.y < 0f)
				{
					this.currentSurface = Surface.Floor;
					this.Stop();
				}
			}
		}
	}

	public void CheckNavTriggers()
	{
		if ((this.currentPath == null) || (this.anim == null) || (this.mover == null))
		{
			return;
		}

		int currentPoint = this.mover.currentPoint;
		if (this.currentPath.waypoints.Length <= currentPoint)
		{
			return;
		}

		GameObject obj = this.currentPath.waypoints[currentPoint].gameObject;
		if (obj.tag.StartsWith("AT_"))
		{
			string triggerName = obj.tag.Replace("AT_", "");
			this.anim.SetTrigger(triggerName);
			this.mover.speed *= 0.5f;
			obj.tag = "Done";
		}

		GameObject nextObj = ((currentPoint+1) < this.currentPath.waypoints.Length) ? this.currentPath.waypoints[currentPoint+1].gameObject : null;
		if ((nextObj != null) && (nextObj.tag == "Teleport"))
		{
			//Debug.LogError("GenerateFullPath Teleport from=" + this.transform.position.ToString() + " to="+ obj.transform.position.ToString());
			this.mover.GoToWaypoint(currentPoint+1);
			nextObj.tag = "Done";
		}
	}

	void LateUpdate()
    {
//        BlinkEyes();
//
//
//        for (int i = 0; i < m_headRenderers.Count; i++)
//        {
//            m_headRenderers[i].SetBlendShapeWeight((int)eBS.ANGRY, m_angry * 100f);
//            m_headRenderers[i].SetBlendShapeWeight((int)eBS.SMILE, m_smile * 100f);
//        }

        //visor
		if (this.Visors != null)
		{
			for (int i = 0; i < Visors.Length; i++)
	        {
				Visors[i].localRotation = Quaternion.Euler(Vector3.Lerp(Vector3.zero, VisorOpenAngle, VisorOpenT));
				Visors[i].parent.gameObject.SetActive(Helmet);
	        }
	    }

        //LookAt
        //LookAt();
    }

	public bool IsFacing(Vector3 dir)
	{
		return (Vector3.Angle(this.transform.forward, dir) < 0.1);
	}

	public bool IsMoving()
	{
		return ((this.currentState == State.Run) || (this.currentState == State.Walk) || (this.currentState == State.Jog));
	}

	public bool IsJumping()
	{
		return (this.currentState == State.Jump);
	}

	public bool IsClimbing()
	{
		return (this.currentState == State.Climb);
	}

	public bool IsPlaying(string animName)
	{
		return this.anim.GetCurrentAnimatorStateInfo(0).IsName(animName);
	}

	public void JumpTo(GridPuzzleCubeRow row)
	{
		Vector3 pos = this.gameObject.transform.position;
		Vector3 dest = row.NavPosition;
		dest.z = pos.z;

		List<Vector3> points = new List<Vector3>();
		points.Add(pos);
		points.Add(dest);

		bool jumpUp = (dest.y > pos.y);
		//Debug.LogError("jumpUp=" + jumpUp);

		this.movePath = new GridPuzzlePlayerPath(points, GridPuzzlePlayerPath.MoveType.Jump);
		this.movePath.targetRow = row;
		this.movePath.jumpUp = jumpUp;
		this.SetState(State.Jump);
	}

	public void ChangeVertical(List<GridPuzzleCubeRow> rows)
	{
		if ((rows == null) || (rows.Count < 2))
		{
			return;
		}

		if (this.currentPath != null)
		{
			return;
		}

		//Debug.LogError("ChangeVertical rows=" + rows.Count);
		//bool isJumpUp = (rows[0].NavPosition.y - rows[rows.Count-1].NavPosition.y);

		GameObject obj = new GameObject();
		obj.name = "PlayerVertRowPath";

		this.currentPath = obj.AddComponent<SWS.PathManager>();
		this.currentPath.waypoints = new Transform[rows.Count];

		//The rest of the path
		for (int i=0; i<rows.Count; i++)
		{
			GameObject cubeObj = new GameObject("PlayerRowNode");

			Vector3 nav = rows[i].NavPosition;
			if (i != 0)
			{
				nav.y += 0.3f;
			}
			cubeObj.transform.position = nav;

			cubeObj.transform.SetParent(obj.transform);
			this.currentPath.waypoints[i] = cubeObj.transform;
		}

		this.moverStarted = false;

		this.SetState(State.Jump);

		this.currentCubeRow = rows[rows.Count-1];
	}

	public void JumpTo(GridPuzzleCube cube)
	{
		Vector3 pos = this.gameObject.transform.position;
		Vector3 dest = cube.NavPosition;
		dest.z = pos.z;

		List<Vector3> points = new List<Vector3>();
		points.Add(pos);
		points.Add(dest);

		this.movePath = new GridPuzzlePlayerPath(points, GridPuzzlePlayerPath.MoveType.Jump);
		this.movePath.targetCube = cube;
		this.SetState(State.Jump);
	}

	public enum Transition
	{
		Unknown,
		Linear,
		JumpDown,
		JumpUp,
		ClimbDown,
		ClimbUp,
	}

	public bool IsUnitDirXZ(Vector3 dir)
	{
		float absX = Mathf.RoundToInt(Mathf.Abs(dir.x));
		float absZ = Mathf.RoundToInt(Mathf.Abs(dir.z));
		bool isOneZero = (absX == 0) || (absZ == 0);
		bool isOneOne = (absX == 1) || (absZ == 1);
		return 	( isOneZero && isOneOne );
	}

	public Transition GetTransitionType(GridPuzzleNavigable n1, GridPuzzleNavigable n2, ref Vector3 alignmentVector)
	{
		alignmentVector = Vector3.zero;
		Vector3 deltaDir = n2.NavPosition - n1.NavPosition;
		Vector3 clamped = new Vector3(Mathf.RoundToInt(deltaDir.x), Mathf.RoundToInt(deltaDir.y), Mathf.RoundToInt(deltaDir.z));

		Transition tt = Transition.Unknown;
		if (clamped.y == 0)
		{
			if (this.IsUnitDirXZ(clamped))
			{
				tt = Transition.Linear;
			}
		}
		else if ((clamped.y == -1) || (clamped.y == -2))
		{
			if (this.IsUnitDirXZ(clamped))
			{
				tt = Transition.JumpDown;
			}
		}
		else if ((clamped.y == 1) || (clamped.y == 2))
		{
			if (this.IsUnitDirXZ(clamped))
			{
				tt = Transition.ClimbUp;
			}
		}

		if (tt == Transition.Unknown)
		{
			//Perspective Transitions
			alignmentVector = this.parentPuzzle.GetPerspectiveAlignedCubeVector(n1 as GridPuzzleCube, n2 as GridPuzzleCube);
			//Debug.LogError("GetTransitionType alignmentVector=" + alignmentVector.ToString());
			if (alignmentVector.y > 0)
			{
				tt = Transition.ClimbUp;
			}
			else if (alignmentVector.y < 0)
			{
				tt = Transition.JumpDown;
			}
			else
			{
				tt = Transition.Linear;
			}
		}

		//Debug.LogError("GetTransitionPath " + n1.NavPosition.y + " -> " + n2.NavPosition.y + " clamped="+ clamped.ToString() + " IsUnitDirXZ=" + this.IsUnitDirXZ(clamped) +" => " + tt.ToString());

		return tt;
	}

	public List<Vector3> GetTransitionPath(GridPuzzleNavigable n1, GridPuzzleNavigable n2, ref bool teleportToFirstPos)
	{
		List<Vector3> pointsToAdd = new List<Vector3>();

		Vector3 alignmentVector = Vector3.zero;
		Transition tt = this.GetTransitionType(n1, n2, ref alignmentVector);
		bool isPerspectiveTransition = (alignmentVector != Vector3.zero);
		Vector3 startPos = isPerspectiveTransition ? (n2.NavPosition - alignmentVector) : n1.NavPosition;
		if (isPerspectiveTransition && (tt != Transition.Unknown))
		{
			teleportToFirstPos = true;
			pointsToAdd.Add(startPos);
		}
		else
		{
			teleportToFirstPos = false;
		}

		if (tt == Transition.Unknown)
		{
			return pointsToAdd;
		}
		else if (tt == Transition.Linear)
		{
			pointsToAdd.Add(n2.NavPosition);
		}
		else if (tt == Transition.JumpDown)
		{
			Vector3 p1 = startPos;
			p1.x = Mathf.Lerp(startPos.x, n2.NavPosition.x, 0.6f);

			Vector3 p2 = Vector3.Lerp(startPos, n2.NavPosition, 0.5f);
			p2.x = Mathf.Lerp(startPos.x, n2.NavPosition.x, 0.9f);

			pointsToAdd.Add(p1);
			pointsToAdd.Add(p2);
			pointsToAdd.Add(n2.NavPosition);
		}
		else if (tt == Transition.ClimbUp)
		{
			pointsToAdd.Add(n2.NavPosition);
		}

		return pointsToAdd;
	}

	public List<Transform> GenerateFullPath(List<GridPuzzleNavigable> navs)
	{
		List<Transform> points = new List<Transform>();
		for (int i=0; i<navs.Count; i++)
		{
			if (i == 0)
			{
				GameObject pointObj = new GameObject("Linear");
				pointObj.transform.position = navs[i].NavPosition;
				points.Add(pointObj.transform);
			}
			else
			{
				Vector3 alignmentVector = Vector3.zero;
				Transition tt = this.GetTransitionType(navs[i-1], navs[i], ref alignmentVector);
				bool isClimb = (tt == Transition.ClimbUp);
				bool isJump = ((tt == Transition.JumpDown) || (tt == Transition.JumpUp));
				bool teleportToFirstPos = false;
				List<Vector3> tp = this.GetTransitionPath(navs[i-1], navs[i], ref teleportToFirstPos);
				if (i > 1)
				{
					if (isJump)
					{
						points[points.Count-1].gameObject.tag = "AT_Jump";
					}
					else if (isClimb)
					{
						points[points.Count-1].gameObject.tag = "AT_Mount";
					}
				}

				for (int j=0; j<tp.Count; j++)
				{
					GameObject pointObj = new GameObject(tt.ToString());
					pointObj.transform.position = tp[j];
					points.Add(pointObj.transform);

					if ((j == 0) && teleportToFirstPos)
					{
						pointObj.tag = "Teleport";
					}
				}

				if (isJump || isClimb)
				{
					break;
				}
			}
		}
		return points;
	}

	public Transition GetTransitionTypeOfPoint(GameObject obj)
	{
		Transition tt = Transition.Linear;
		string name = obj.name;
		if (name == "Linear")
		{
			tt = Transition.Linear;
		}
		else if (name == "JumpUp")
		{
			tt = Transition.JumpUp;
		}
		else if (name == "JumpDown")
		{
			tt = Transition.JumpDown;
		}
		else if (name == "ClimbUp")
		{
			tt = Transition.ClimbUp;
		}
		else if (name == "ClimbDown")
		{
			tt = Transition.ClimbDown;
		}

		return tt;
	}

	public SWS.PathManager GeneratePathManager(string name, List<GridPuzzleNavigable> navs)
	{
		List<Transform> points = this.GenerateFullPath(navs);
		//Debug.LogError("GeneratePathManager navs=" + navs.Count + " points=" + points.Count);

		GridPuzzleManager.Instance.SpawnPathFollower(points, 10f, 3f);

		GameObject obj = new GameObject();
		obj.name = name + "Path";

		SWS.PathManager path = obj.AddComponent<SWS.PathManager>();
		path.waypoints = new Transform[points.Count];

		for (int i=0; i<points.Count; i++)
		{
			points[i].SetParent(obj.transform);
			path.waypoints[i] = points[i];
		}

		return path;
	}

	public void MovePath(List<GridPuzzleCubeRow> rows, State s = State.Jog)
	{
		if (this.currentPath != null)
		{
			return;
		}

		//Debug.LogError("MovePath rows=" + rows.Count);

		List<GridPuzzleNavigable> navs = new List<GridPuzzleNavigable>();
		for (int i=0; i<rows.Count; i++)
		{
			navs.Add(rows[i]);
		}
		this.currentPath = this.GeneratePathManager("PlayerRow", navs);

		this.moverStarted = false;

		if (this.currentPath.waypoints.Length > 1)
		{
			GameObject secondNavPoint = this.currentPath.waypoints[1].gameObject;
			Transition firstTransition = this.GetTransitionTypeOfPoint(secondNavPoint);
			//Debug.LogError("secondNavPoint name=" + secondNavPoint.name + " firstTransition=" + firstTransition.ToString());
			if ((firstTransition == Transition.ClimbUp) || (firstTransition == Transition.ClimbDown))
			{
				s = State.Climb;
			}
			else if ((firstTransition == Transition.JumpUp) || (firstTransition == Transition.JumpDown))
			{
				s = State.Jump;
			}
		}

		this.SetState(s);
	}

	public void MovePath(List<GridPuzzleCube> cubes, State s = State.Jog)
	{
		if (this.currentPath != null)
		{
			return;
		}

		List<GridPuzzleNavigable> navs = new List<GridPuzzleNavigable>();
		for (int i=0; i<cubes.Count; i++)
		{
			navs.Add(cubes[i]);
		}

		this.currentPath = this.GeneratePathManager("PlayerCube", navs);

		this.moverStarted = false;

		GameObject secondNavPoint = this.currentPath.waypoints[1].gameObject;
		Transition firstTransition = this.GetTransitionTypeOfPoint(secondNavPoint);
		//Debug.LogError("secondNavPoint name=" + secondNavPoint.name + " firstTransition=" + firstTransition.ToString());
		if ((firstTransition == Transition.ClimbUp) || (firstTransition == Transition.ClimbDown))
		{
			s = State.Climb;
		}
		else if ((firstTransition == Transition.JumpUp) || (firstTransition == Transition.JumpDown))
		{
			s = State.Jump;
		}

		this.SetState(s);
	}

	public void TeleportTo(GameObject location)
	{
		if (this.lastTeleport != null)
		{
			Vector3 pos = this.transform.position;
			List<Vector3> path = new List<Vector3>();
			path.Add(pos);
			pos.y = this.lastTeleport.transform.position.y + 2f;
			path.Add(pos);
			path.Add(location.transform.position);
			GridPuzzleManager.Instance.SpawnPathFollower(path, 10f, 0f, this.gameObject);
			this.gameObject.SetActive(false);
		}

		this.lastTeleport = location;
		this.transform.position = location.transform.position;
		this.transform.rotation = location.transform.rotation;
		this.Stop();
	}

	public void Respawn()
	{
		this.TeleportTo(this.lastTeleport);
	}

	public void Stop()
	{
		if (this.mover != null)
		{
			this.mover.Stop();
		}
		if (this.currentPath != null)
		{
			GameObject.Destroy(this.currentPath.gameObject);
			this.currentPath = null;
		}

		this.movePath = null;

		this.SetState(State.Idle);
	}

	void OnCollisionEnter(Collision info)
	{
		GameObject obj = info.collider.gameObject;
		GridPuzzleCube cube = obj.GetComponent<GridPuzzleCube>();
		if (cube != null)
		{
			this.currentCube = cube;
			if (cube.parentPuzzle != null)
			{
				this.parentPuzzle = cube.parentPuzzle;
			}
			return;
		}

		GridPuzzleCubeRow row = obj.GetComponent<GridPuzzleCubeRow>();
		if (row != null)
		{
			this.currentCubeRow = row;
			if (row.parentPuzzle != null)
			{
				this.parentPuzzle = row.parentPuzzle;
			}
			return;
		}
	}

	public void OnCameraAngleChange(GridPuzzleCamera.Angle newAngle)
	{
		if (this.angle != newAngle)
		{
			this.angle = newAngle;
		}
	}

	public void OnCameraLayerChange(GridPuzzleCamera.Angle newAngle)
	{
		if (this.anim != null)
		{
			if (GridPuzzleCamera.IsIsometricAngle(newAngle))
			{
				int layer = LayerMask.NameToLayer("PuzzleFront");
				if (this.anim.gameObject.layer != layer)
				{
					this.anim.gameObject.layer = layer;	
				}
			}
			else
			{
				int layer = LayerMask.NameToLayer("PuzzleMain");
				if (this.anim.gameObject.layer != layer)
				{
					this.anim.gameObject.layer = layer;	
				}
			}
		}
	}
}
