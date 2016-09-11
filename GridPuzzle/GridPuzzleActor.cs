using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class GridPuzzleActor : DSTools.MessengerListener 
{
	public Seeker seeker;
	public GridPuzzlePlayerController player;
	public bool IsPlayer
	{
		get
		{
			return (this.player != null);
		}
	}

	private Animator anim;

	public GridPuzzleAction currentAction;

	void Awake()
	{
		this.anim = this.gameObject.GetComponent<Animator>();
		this.player = this.gameObject.GetComponent<GridPuzzlePlayerController>();
		this.seeker = this.gameObject.GetComponent<Seeker>();
	}

	void Start () 
	{
	}
	
	void Update () 
	{
		//We have fallen!!!
		if (this.transform.position.y < -10)
		{
			this.RequestKill();
		}
	}

	public bool RequestAction(GridPuzzleAction action)
	{
		return GridPuzzleActionManager.Instance.RequestAction(this, action);
	}

	public void OnCameraAngleChange(GridPuzzleCamera.Angle angle)
	{
		if (this.player != null)
		{
			this.player.OnCameraAngleChange(angle);
		}
	}

	public bool RequestKill()
	{
		if (this.player != null)
		{
			this.player.Respawn();
			this.SendMessengerMsg("ActorKilled", this);
			return true;
		}
		return false;
	}

	public bool IsActing()
	{
		return (this.currentAction != null);
	}

	public bool IsMoving()
	{
		if (this.player != null)
		{
			return this.player.IsMoving() || this.player.IsJumping();
		}
		return false;
	}

	public void ChangeVertical(List<GridPuzzleCubeRow> rows)
	{
		if (this.player != null)
		{
			this.player.ChangeVertical(rows);
		}
	}

	public void MovePath(List<GridPuzzleCube> cubes)
	{
		if (this.player != null)
		{
			GridPuzzlePlayerController.State state = (cubes.Count <= 4) ? GridPuzzlePlayerController.State.Walk : GridPuzzlePlayerController.State.Jog;
			this.player.MovePath(cubes, state);
		}
	}

	public void MovePath(List<GridPuzzleCubeRow> rows)
	{
		if (this.player != null)
		{
			GridPuzzlePlayerController.State state = (rows.Count <= 4) ? GridPuzzlePlayerController.State.Walk : GridPuzzlePlayerController.State.Jog;
			this.player.MovePath(rows, state);
		}
	}

	public void TeleportTo(GameObject obj)
	{
		if (this.player != null)
		{
			this.player.TeleportTo(obj);
		}
	}

	public int GetGraphMask(GridPuzzleCamera.Angle angle)
	{
		int graphMask = -1;
		if (GridPuzzleManager.Instance.cameraAngle == GridPuzzleCamera.Angle.Side2D)
		{
			graphMask = (1 << 1);
		}
		else if (GridPuzzleManager.Instance.cameraAngle == GridPuzzleCamera.Angle.Isometric)
		{
			graphMask = (1 << 0);
		}
		return graphMask;
	}

	private GridPuzzleCube targetCube = null;
	private GridPuzzleCubeRow targetCubeRow = null;
	public void RequestMoveTo(GridPuzzleCube cube)
	{
		if (this.targetCube != null)
		{
			return;
		}

		if (this.IsMoving())
		{
			return;
		}

		PointGraph graph = AstarPath.active.astarData.pointGraph;
		NNInfo nodeInfo = AstarPath.active.GetNearest(cube.NavPosition);
		NNInfo nodeInfoCurrent = AstarPath.active.GetNearest(this.transform.position);
		if (nodeInfoCurrent.node.ContainsConnection(nodeInfo.node))
		{
			//Directly connected, not need to pathfind!

			List<GridPuzzleCube> cubes = new List<GridPuzzleCube>();
			List<Vector3> vectorPath = new List<Vector3>();
			cubes.Add(this.player.currentCube); vectorPath.Add(this.player.currentCube.NavPosition);
			cubes.Add(cube); vectorPath.Add(cube.NavPosition);

			GridPuzzleManager.Instance.SpawnPathFollower(vectorPath, 10f, 3f);

			GridPuzzleMoveTo action = new GridPuzzleMoveTo();
			action.Init(cubes);
			this.RequestAction(action);

			return;
		}

		//ABPath path = this.seeker.GetNewPath(this.player.transform.position, cube.NavPosition);
		//float patheLength = path.GetTotalLength();
		this.targetCube = cube;

		int graphMask = this.GetGraphMask(GridPuzzleManager.Instance.cameraAngle);
		seeker.StartPath(transform.position, cube.NavPosition, OnPathComplete, graphMask);

		//Debug.Log("RequestMoveTo graphMask=" + graphMask);
		Debug.Log("RequestMove From " + transform.position.x + "," + transform.position.y + "," + transform.position.z);
		Debug.Log("RequestMove To " + cube.NavPosition.x + "," + cube.NavPosition.y + "," + cube.NavPosition.z);
		//Debug.Log("Node pos=" + nodeInfo.clampedPosition.x + "," + nodeInfo.clampedPosition.y + "," + nodeInfo.clampedPosition.z);

		//GridPuzzleMoveTo action = new GridPuzzleMoveTo();
		//action.Init(this.player.currentCube, cube);
		//this.RequestAction(action);
	}

	public void OnPathComplete (Path p) 
	{
		Debug.Log("OnPathComplete length=" + p.GetTotalLength() + " graphMask=" + p.nnConstraint.graphMask);
		p.Claim(this);

		Debug.Log("OnPathComplete current=" + this.transform.position.ToString() + " p0=" + p.vectorPath[0].ToString() + " p1=" + ((p.vectorPath.Count > 1) ? p.vectorPath[1].ToString() : "none") );

		GridPuzzle puzzle = GridPuzzleManager.Instance.GetCurrentPuzzle();

		GridPuzzleManager.Instance.SpawnPathFollower(p.vectorPath, 10f, 3f);

		//This is likely super slow...need to just make a lookup table I think....
		List<GridPuzzleCube> cubes = puzzle.GetCubesByNavPoints(p.vectorPath);

		GridPuzzleMoveTo action = new GridPuzzleMoveTo();
		action.Init(cubes);
		this.RequestAction(action);

		targetCube = null;
	}

	public void JumpTo(GridPuzzleCubeRow row)
	{
		if (this.player != null)
		{
			this.player.JumpTo(row);
		}
	}

	public void RequestMoveTo(GridPuzzleCubeRow row)
	{
		if (this.targetCubeRow != null)
		{
			return;
		}

		if (this.IsMoving())
		{
			return;
		}

		int graphMask = this.GetGraphMask(GridPuzzleManager.Instance.cameraAngle);
		seeker.StartPath(transform.position, row.NavPosition, OnRowPathComplete, graphMask);
		this.targetCubeRow = row;
	}

	public void OnRowPathComplete (Path p) 
	{
		//Debug.Log("OnPathComplete length=" + p.GetTotalLength() + " graphMask=" + p.nnConstraint.graphMask);
		p.Claim(this);

		GridPuzzle puzzle = GridPuzzleManager.Instance.GetCurrentPuzzle();

		GridPuzzleManager.Instance.SpawnPathFollower(p.vectorPath, 10f, 3f);

		//This is likely super slow...need to just make a lookup table I think....
		List<GridPuzzleCubeRow> rows = puzzle.GetCubeRowsByNavPoints(p.vectorPath);

		List<GridPuzzleCubeRow> currentMove = new List<GridPuzzleCubeRow>();
		for (int i=0; i<rows.Count; i++)
		{
			GridPuzzleCubeRow thisRow = rows[i];
			GridPuzzleCubeRow lastRow = (i != 0) ? rows[i-1] : this.player.currentCubeRow;

			float verticalDelta = (lastRow == null) ? 0 : Mathf.Abs((thisRow.NavPosition.y - lastRow.NavPosition.y));
			if (verticalDelta > 0.5f)
			{
				if (currentMove.Count > 1)
				{
					GridPuzzleMoveToRow moveAction = new GridPuzzleMoveToRow();
					moveAction.Init(currentMove);
					this.RequestAction(moveAction);
					currentMove = new List<GridPuzzleCubeRow>();
				}

				GridPuzzleJumpToRow jumpAction = new GridPuzzleJumpToRow();
				jumpAction.Init(lastRow, thisRow);
				this.RequestAction(jumpAction);
			}

			currentMove.Add(thisRow);
		}

		if (currentMove.Count > 1)
		{
			GridPuzzleMoveToRow moveAction = new GridPuzzleMoveToRow();
			moveAction.Init(currentMove);
			this.RequestAction(moveAction);
			currentMove.Clear();
		}


		this.targetCubeRow = null;
	}

	public void RequestJumpUp()
	{
		if (this.player == null)
		{
			return;
		}

		Debug.LogError("RequestJumpUp");
		if (this.IsActing())
		{
			Debug.LogError("   IsActing=" + this.IsActing());
			return;
		}

		GridPuzzleCubeRow current = this.player.currentCubeRow;
		if ((current == null) || (this.player.parentPuzzle == null))
		{
			Debug.LogError("   current=" + current + " puzzle=" + this.player.parentPuzzle);
			return;
		}

		GridPuzzleCubeRow oneUp = this.player.parentPuzzle.GetRow(current.x, current.y+1);
		if ((oneUp == null) || !oneUp.IsColliderRow)
		{
			Debug.LogError("   oneUp=" + oneUp);
			return;
		}

		List<GridPuzzleCubeRow> rows = new List<GridPuzzleCubeRow>();
		rows.Add(current);
		rows.Add(oneUp);
		GridPuzzleChangeVertical jumpUpAction = new GridPuzzleChangeVertical();
		jumpUpAction.Init(rows);
		this.RequestAction(jumpUpAction);
	}

	public void RequestJumpDown()
	{
		if (this.IsActing())
		{
			return;
		}
	}

	public void Stop()
	{
		if (this.player != null)
		{
			this.player.Stop();
		}
	}

	public void OnActionStarted(GridPuzzleAction action)
	{
		this.currentAction = action;
	}

	public void OnActionCompleted(GridPuzzleAction action)
	{
		this.currentAction = null;
	}
}
