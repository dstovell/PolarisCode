using UnityEngine;
using System.Collections;

public class GridPuzzleActor : DSTools.MessengerListener 
{
	private GridPuzzlePlayerController player;
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
	}

	void Start () 
	{
	}
	
	void Update () 
	{
	}

	public bool RequestAction(GridPuzzleAction action)
	{
		return GridPuzzleActionManager.Instance.RequestAction(this, action);
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

	public void RequestMoveTo(GridPuzzleCube cube)
	{
		GridPuzzleMoveTo action = new GridPuzzleMoveTo();
		action.Init(this.player.currentCube, cube);
		this.RequestAction(action);
	}

	public void RequestMoveTo(GridPuzzleCubeRow row)
	{
		GridPuzzleCube cube = row.GetFrontCube();
		this.MoveTo(cube);
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
