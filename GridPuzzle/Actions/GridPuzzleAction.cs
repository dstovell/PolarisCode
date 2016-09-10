using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleAction
{
	public enum State
	{
		Pending,
		Started,
		Complete
	}
	public State state { get; protected set; }

	public bool immediate = false;

	protected GridPuzzleActor actor;

	public List<GridPuzzleCube> cubePath;
	public List<GridPuzzleCubeRow> cubeRowPath;

	public int turnCount;
	private int remainingTurnCount;

	public bool IsPlayer
	{
		get
		{
			return (this.actor != null) ? this.actor.IsPlayer : false;
		}
	}

	public void SetActor(GridPuzzleActor _actor)
	{
		this.actor = _actor;
	}

	public void Init(GridPuzzleCube start, GridPuzzleCube end = null)
	{
		this.cubePath = new List<GridPuzzleCube>();
		this.cubePath.Add(start);
		this.cubePath.Add(end);

		this.state = State.Pending;
		InitAction();
	}

	public void Init(List<GridPuzzleCube> path, int turns = -1)
	{
		//Debug.LogError("Init cube path=" + path.Count + " p0=" + path[0].NavPosition.ToString() + " p1=" + path[1].NavPosition.ToString());
		this.cubePath = path;
		this.turnCount = turns;
		this.remainingTurnCount = (turns >= 0) ? turns : path.Count;

		this.state = State.Pending;
		InitAction();
	}

	public void Init(List<GridPuzzleCubeRow> path, int turns = -1)
	{
		//Debug.LogError("Init row path=" + path.Count);
		this.cubeRowPath = new List<GridPuzzleCubeRow>(path);
		this.turnCount = turns;
		this.remainingTurnCount = (turns >= 0) ? turns : path.Count;

		this.state = State.Pending;
		InitAction();
	}


	public void Start()
	{
		//Debug.Log("Action Start state=" + this.state.ToString());
		if (this.state == State.Pending)
		{
			this.state = State.Started;
			StartAction();
			if (this.actor != null)
			{
				this.actor.OnActionStarted(this);
			}
		}
	}

	public void Turn()
	{
		this.remainingTurnCount = Mathf.Max(this.remainingTurnCount - 1, 0);
		if (state == State.Started)
		{
			this.TurnAction();
		}
	}

	public void Update()
	{
		if (state == State.Started)
		{
			this.UpdateAction();
		}
	}

	public void Complete()
	{
		if (this.state == State.Started)
		{
			this.state = State.Complete;
			CompleteAction();
			if (this.actor != null)
			{
				this.actor.OnActionCompleted(this);
			}
		}
	}

	public bool IsTurnsComplete()
	{
		return (this.remainingTurnCount == 0);
	}

	protected virtual void InitAction()
	{
	}

	protected virtual void StartAction()
	{
	}

	protected virtual void TurnAction()
	{
	}

	protected virtual void UpdateAction()
	{
	}

	protected virtual void CompleteAction()
	{
	}
}

