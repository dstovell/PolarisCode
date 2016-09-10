using UnityEngine;
using System.Collections;

public class GridPuzzleMoveToRow : GridPuzzleAction
{
	public GridPuzzleCubeRow startRow;
	public GridPuzzleCubeRow endRow;

	public void Init(GridPuzzleCubeRow start, GridPuzzleCubeRow end = null)
	{
		this.startRow = start;
		this.endRow = end;

		this.state = State.Pending;
		InitAction();
	}

	protected override void InitAction()
	{
		
	}

	protected override void StartAction()
	{
		if (this.actor != null)
		{
			this.actor.MovePath(this.cubeRowPath);
		}
	}

	protected override void UpdateAction()
	{
		if (!this.actor.IsMoving())
		{
			this.Complete();
		}
	}

	protected virtual void CompleteAction()
	{
	}
}

