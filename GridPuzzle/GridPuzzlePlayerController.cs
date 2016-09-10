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
		Run,
		Jump
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

	private Rigidbody rb;

	private Animator anim;

	private GridPuzzlePlayerPath movePath;

	public SWS.PathManager currentPath;
	public bool moverStarted = false;

	public Quaternion desiredRotation;

	private GameObject lastTeleport;

	private SWS.splineMove mover;
	private SWS.PathManager pathManager;

	// Use this for initialization
	void Start() 
	{
		this.InitMessenger("GridPuzzlePlayerController");
		this.currentState = State.Idle;
		this.currentSurface = Surface.Floor;
		this.anim = this.gameObject.GetComponent<Animator>();
		this.rb = this.gameObject.GetComponent<Rigidbody>();
		this.mover = this.gameObject.GetComponent<SWS.splineMove>();
		this.pathManager = this.gameObject.GetComponent<SWS.PathManager>();
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
			break;
		case State.Run:
			this.anim.SetBool("Jump", false);
			this.anim.SetBool("Run", true);
			break;
		case State.Jump:
			this.anim.SetBool("Jump", true);
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

		switch(this.currentState)
		{
		case State.Idle:
			if (this.anim.GetBool("Run") && !GridPuzzleActionManager.Instance.PlayerHasActions())
			{
				this.anim.SetBool("Run", false);
			}
			break;
		case State.Run:
			break;
		case State.Jump:
			if (this.timeInState > 0.5f)
			{
				this.anim.SetBool("Jump", false);
			}
			this.anim.SetBool("Run", false);
			break;
		default:
			break;
		}

		Vector3 desiredLookDirection = (this.movePath != null) ? this.movePath.GetDirection() : this.transform.forward;
		desiredLookDirection.y = 0;
		desiredLookDirection = desiredLookDirection.normalized;
		//Debug.LogError(desiredLookDirection.x + "," + desiredLookDirection.y + "," + desiredLookDirection.z);
		Vector3 upDirection = Vector3.up; //(this.currentSurface == Surface.Ceiling) ? Vector3.down : Vector3.up;
		if (desiredLookDirection != Vector3.zero)
		{
			this.desiredRotation = Quaternion.LookRotation(desiredLookDirection, upDirection);
		}


		//TODO: Make turnaround more graceful??
		this.rb.rotation = this.desiredRotation; //Quaternion.RotateTowards(this.transform.rotation, this.desiredRotation, this.RotateSpeed*Time.deltaTime);

		if (this.IsMoving() || this.IsJumping())
		{
			bool isStateReady = this.IsJumping() ? (this.timeInState > 1.0f) : this.IsPlaying("Run");

			if (isStateReady && (this.movePath != null))
			{
				Vector3 movePos = this.movePath.Move(this.MoveSpeed, Time.deltaTime);
				//movePos.y = this.transform.position.y;
				if (this.movePath.targetRow != null)
				{
					movePos.z = this.transform.position.z;
				}
				this.transform.position = movePos;
				if (this.movePath.isDone)
				{
					if (this.movePath.targetRow != null)
					{
						GridPuzzleCube closestCube = this.movePath.targetRow.GetClosestCube(this.transform.position);
						if (closestCube != null)
						{
							this.transform.position = closestCube.NavPosition;//new Vector3(this.transform.position.x, this.transform.position.y, closestCube.NavPosition.z);
						}
					}
					Stop();
				}
			}

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
						if (this.angle != GridPuzzleCamera.Angle.Isometric)
						{
							GridPuzzleCube closestCube = (this.currentCubeRow != null) ? this.currentCubeRow.GetClosestCube(this.transform.position) : null;
							if (closestCube != null)
							{
								this.transform.position = closestCube.NavPosition;
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
		if (!this.IsJumping())
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

	public bool IsFacing(Vector3 dir)
	{
		return (Vector3.Angle(this.transform.forward, dir) < 0.1);
	}

	public bool IsMoving()
	{
		return (this.currentState == State.Run);
	}

	public bool IsJumping()
	{
		return (this.currentState == State.Jump);
	}

	public bool IsPlaying(string animName)
	{
		return this.anim.GetCurrentAnimatorStateInfo(0).IsName(animName);
	}

	public void MoveTo(GridPuzzleCubeRow row)
	{
		Vector3 pos = this.gameObject.transform.position;
		Vector3 dest = row.NavPosition;
		dest.z = pos.z;

		List<Vector3> points = new List<Vector3>();
		points.Add(pos);
		points.Add(dest);

		this.movePath = new GridPuzzlePlayerPath(points);
		this.movePath.targetRow = row;
		this.SetState(State.Run);
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

		//Debug.LogError("MovePath cubes=" + cubes.Count);
//		List<Vector3> points = new List<Vector3>();
//		for (int i=0; i<cubes.Count; i++)
//		{
//			points.Add( cubes[i].NavPosition );
//		}
//
//		GameObject obj = new GameObject();
//		obj.name = "PlayerCubePath";
//
//		this.currentPath = obj.AddComponent<SWS.PathManager>();
//		this.currentPath.waypoints = new Transform[cubes.Count];
//		for (int i=0; i<cubes.Count; i++)
//		{
//			GameObject cubeObj = new GameObject("PlayerCubeNode");
//			cubeObj.transform.position = cubes[i].NavPosition;
//			//cubeObj.transform.rotation =;
//			cubeObj.transform.SetParent(obj.transform);
//			this.currentPath.waypoints[i] = cubeObj.transform;
//		}
//
//		this.moverStarted = false;
//
//		this.SetState(State.Run);
	}

	public void MoveTo(GridPuzzleCube cube)
	{
		//Debug.LogError("MoveTo pos=" + cube.NavPosition.x + "," + cube.NavPosition.y + "," + cube.NavPosition.z);
		Vector3 pos = this.gameObject.transform.position;
		Vector3 dest = cube.NavPosition;
		//dest.y = pos.y;

		List<Vector3> points = new List<Vector3>();
		points.Add(pos);
		points.Add(dest);

		this.movePath = new GridPuzzlePlayerPath(points);
		this.movePath.targetCube = cube;
		this.SetState(State.Run);
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

	public void MovePath(List<GridPuzzleCubeRow> rows)
	{
		if (this.currentPath != null)
		{
			return;
		}

		//Debug.LogError("MovePath rows=" + rows.Count);

		GameObject obj = new GameObject();
		obj.name = "PlayerRowPath";

		this.currentPath = obj.AddComponent<SWS.PathManager>();
		this.currentPath.waypoints = new Transform[rows.Count];

		//The rest of the path
		for (int i=0; i<rows.Count; i++)
		{
			GameObject cubeObj = new GameObject("PlayerRowNode");

			cubeObj.transform.position = rows[i].NavPosition;
			//cubeObj.transform.rotation =;

			cubeObj.transform.SetParent(obj.transform);
			this.currentPath.waypoints[i] = cubeObj.transform;
		}

		this.moverStarted = false;

		this.SetState(State.Run);
	}

	public void MovePath(List<GridPuzzleCube> cubes)
	{
		if (this.currentPath != null)
		{
			return;
		}

		//Debug.LogError("MovePath cubes=" + cubes.Count);
		List<Vector3> points = new List<Vector3>();
		for (int i=0; i<cubes.Count; i++)
		{
			points.Add( cubes[i].NavPosition );
		}

		GameObject obj = new GameObject();
		obj.name = "PlayerCubePath";

		this.currentPath = obj.AddComponent<SWS.PathManager>();
		this.currentPath.waypoints = new Transform[cubes.Count];
		for (int i=0; i<cubes.Count; i++)
		{
			GameObject cubeObj = new GameObject("PlayerCubeNode");
			cubeObj.transform.position = cubes[i].NavPosition;
			//cubeObj.transform.rotation =;
			cubeObj.transform.SetParent(obj.transform);
			this.currentPath.waypoints[i] = cubeObj.transform;
		}

		this.moverStarted = false;

		this.SetState(State.Run);
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
			return;
		}

		GridPuzzleCubeRow row = obj.GetComponent<GridPuzzleCubeRow>();
		if (row != null)
		{
			this.currentCubeRow = row;
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
}
