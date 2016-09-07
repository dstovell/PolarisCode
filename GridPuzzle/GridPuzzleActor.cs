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

	public bool RequestKill()
	{
		if (this.player != null)
		{
			this.player.Respawn();
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
			return this.player.IsMoving();
		}
		return false;
	}

	public void MoveTo(GridPuzzleCube cube)
	{
		if (this.player != null)
		{
			this.player.MoveTo(cube);
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
	public void RequestMoveTo(GridPuzzleCube cube)
	{
		PointGraph graph = GridPuzzleManager.Instance.path.astarData.pointGraph;
		NNInfo nodeInfo = GridPuzzleManager.Instance.path.GetNearest(cube.NavPosition);

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

		GridPuzzle puzzle = GridPuzzleManager.Instance.GetCurrentPuzzle();

		GridPuzzleManager.Instance.SpawnPathFollower(p.vectorPath, 10f, 3f);

		//This is likely super slow...need to just make a lookup table I think....
		List<GridPuzzleCube> cubes = puzzle.GetCubesByNavPoints(p.vectorPath);

		GridPuzzleCube lastCube = this.player.currentCube;
		for (int i=0; i<cubes.Count; i++)
		{	
			GridPuzzleCube cube = cubes[i];

			Vector3 pos = cube.NavPosition;
			//Debug.Log("OnPathComplete name=" + cube.name + " pos=" + pos.x + "," + pos.y + "," + pos.z);

			GridPuzzleMoveTo action = new GridPuzzleMoveTo();
			action.Init(lastCube, cube);
			this.RequestAction(action);
			lastCube = cube;
		}
	}

	public void MoveTo(GridPuzzleCubeRow row)
	{
		if (this.player != null)
		{
			this.player.MoveTo(row);
		}
	}

	public void RequestMoveTo(GridPuzzleCubeRow row)
	{
		//this.targetCube = cube;

		int graphMask = this.GetGraphMask(GridPuzzleManager.Instance.cameraAngle);
		seeker.StartPath(transform.position, row.NavPosition, OnRowPathComplete, graphMask);

		//Debug.Log("RequestMoveTo graphMask=" + graphMask);
		//Debug.Log("RequestMoveTo " + row.NavPosition.x + "," + row.NavPosition.y + "," + row.NavPosition.z);

		//GridPuzzleMoveTo action = new GridPuzzleMoveTo();
		//action.Init(this.player.currentCube, cube);
		//this.RequestAction(action);
	}

	public void OnRowPathComplete (Path p) 
	{
		//Debug.Log("OnPathComplete length=" + p.GetTotalLength() + " graphMask=" + p.nnConstraint.graphMask);
		p.Claim(this);

		GridPuzzle puzzle = GridPuzzleManager.Instance.GetCurrentPuzzle();

		GridPuzzleManager.Instance.SpawnPathFollower(p.vectorPath, 10f, 3f);

		//This is likely super slow...need to just make a lookup table I think....
		List<GridPuzzleCubeRow> rows = puzzle.GetCubeRowsByNavPoints(p.vectorPath);

		GridPuzzleCubeRow lastCubeRow = null;
		for (int i=0; i<rows.Count; i++)
		{	
			GridPuzzleCubeRow row = rows[i];

			Vector3 pos = row.NavPosition;
			//Debug.Log("OnPathComplete name=" + row.name + " row=" + pos.x + "," + pos.y + "," + pos.z);

			GridPuzzleMoveToRow action = new GridPuzzleMoveToRow();
			action.Init(lastCubeRow, row);
			this.RequestAction(action);
			lastCubeRow = row;
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
