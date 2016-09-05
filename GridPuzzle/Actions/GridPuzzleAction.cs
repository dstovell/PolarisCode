using UnityEngine;
using System.Collections;

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

	public GridPuzzleCube startCube;
	public GridPuzzleCube endCube;

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
		this.startCube = start;
		this.endCube = end;

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

	public void Update()
	{
		if (state == State.Started)
		{
			UpdateAction();
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

	protected virtual void InitAction()
	{
	}

	protected virtual void StartAction()
	{
	}

	protected virtual void UpdateAction()
	{
	}

	protected virtual void CompleteAction()
	{
	}
}

