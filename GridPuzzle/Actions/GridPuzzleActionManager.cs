using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleActionQueue
{
	public void AddAction(GridPuzzleAction action)
	{
		this.actions.Add(action);
	}

	public GridPuzzleAction NextAction()
	{
		return (this.actions.Count > 0) ? this.actions[0] : null;
	}

	public GridPuzzleAction StartAction()
	{
		GridPuzzleAction action = NextAction();
		if ((action != null) && (action.state == GridPuzzleAction.State.Pending))
		{
			action.Start();
		}
		return action;
	}

	public GridPuzzleAction UpdateAction()
	{
		GridPuzzleAction action = NextAction();
		if ((action != null) && (action.state == GridPuzzleAction.State.Started))
		{
			action.Update();
		}
		return action;
	}

	public GridPuzzleAction PopCompleteAction()
	{
		GridPuzzleAction action = NextAction();
		if ((action != null) && (action.state == GridPuzzleAction.State.Complete))
		{
			this.actions.Remove(action);
			return action;
		}
		return null;
	}

	public bool IsReady()
	{
		GridPuzzleAction action = NextAction();
		return ((action != null) && (action.state == GridPuzzleAction.State.Pending));
	}

	public bool IsActing()
	{
		GridPuzzleAction action = NextAction();
		return ((action != null) && (action.state == GridPuzzleAction.State.Started));
	}

	public bool IsComplete()
	{
		GridPuzzleAction action = NextAction();
		return ((action != null) && (action.state == GridPuzzleAction.State.Complete));
	}

	public int Count()
	{
		return this.actions.Count;
	}

	private List<GridPuzzleAction> actions = new List<GridPuzzleAction>();
}

public class GridPuzzleActionManager : DSTools.MessengerListener
{
	public static GridPuzzleActionManager Instance;

	private Dictionary<GridPuzzleActor, GridPuzzleActionQueue> queues;

	private GridPuzzleActor player;

	public int DebugActorCount;
	public int DebugActionCount;
	public int DebugActingCount;
	public int DebugReadyCount;

	void Awake()
	{
		GridPuzzleActionManager.Instance = this;
		this.queues = new Dictionary<GridPuzzleActor, GridPuzzleActionQueue>();
	}

	// Use this for initialization
	void Start ()
	{
		this.InitMessenger("GridPuzzleActionManager");
	}

	public bool RequestAction(GridPuzzleActor actor, GridPuzzleAction action)
	{
		if ((actor == null) || (this.queues == null))
		{
			return false;
		}

		if (!this.queues.ContainsKey(actor))
		{
			this.queues[actor] = new GridPuzzleActionQueue();
			if (actor.IsPlayer)
			{
				this.player = actor;
			}
		}

		action.SetActor(actor);
		this.queues[actor].AddAction(action);
		return true;
	}

	public bool IsReady(GridPuzzleActor actor)
	{
		if ((actor == null) || (this.queues == null))
		{
			return true;
		}

		GridPuzzleActionQueue queue;
		if (this.queues.TryGetValue(actor, out queue))
		{
			return queue.IsReady();
		}
		return false;
	}

	public bool IsActing(GridPuzzleActor actor)
	{
		if ((actor == null) || (this.queues == null))
		{
			return true;
		}

		GridPuzzleActionQueue queue;
		if (this.queues.TryGetValue(actor, out queue))
		{
			return queue.IsActing();
		}
		return false;
	}

	public bool IsComplete(GridPuzzleActor actor)
	{
		if ((actor == null) || (this.queues == null))
		{
			return true;
		}

		GridPuzzleActionQueue queue;
		if (this.queues.TryGetValue(actor, out queue))
		{
			return queue.IsComplete();
		}
		return false;
	}

	public bool IsPlayerReady()
	{
		return (this.player != null) ? this.IsReady(this.player) : false;
	}

	public bool IsPlayerActing()
	{
		return (this.player != null) ? this.IsActing(this.player) : false;
	}

	public bool IsPlayerComplete()
	{
		return (this.player != null) ? this.IsComplete(this.player) : false;
	}

	public bool IsAnyoneActing()
	{
		foreach(KeyValuePair<GridPuzzleActor, GridPuzzleActionQueue> entry in this.queues)
		{
			if (entry.Value.IsActing())
			{
				return true;
			}
		}
		return false;
	}

	private void RemoveCompletedActions()
	{
		foreach(KeyValuePair<GridPuzzleActor, GridPuzzleActionQueue> entry in this.queues)
		{
			if (entry.Value.IsComplete())
			{
				entry.Value.PopCompleteAction();
			}
		}
	}

	private void StartPendingActions()
	{
		foreach(KeyValuePair<GridPuzzleActor, GridPuzzleActionQueue> entry in this.queues)
		{
			if (entry.Value.IsReady())
			{
				entry.Value.StartAction();
			}
		}
	}

	private void UpdatedStartedActions()
	{
		foreach(KeyValuePair<GridPuzzleActor, GridPuzzleActionQueue> entry in this.queues)
		{
			entry.Value.UpdateAction();
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		RemoveCompletedActions();

		if (!this.IsAnyoneActing() && this.IsPlayerReady())
		{
			this.StartPendingActions();
		}

		UpdatedStartedActions();

		this.DebugActorCount = this.queues.Count;
		this.DebugActionCount = 0;
		this.DebugActingCount = 0;
		this.DebugReadyCount = 0;
		foreach(KeyValuePair<GridPuzzleActor, GridPuzzleActionQueue> entry in this.queues)
		{
			if (entry.Value.IsActing())
			{
				this.DebugActingCount++;
			}
			else if (entry.Value.IsReady())
			{
				this.DebugReadyCount++;
			}

			this.DebugActionCount += entry.Value.Count();
		}
	}
}

