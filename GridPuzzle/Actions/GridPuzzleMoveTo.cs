using UnityEngine;
using System.Collections;

public class GridPuzzleMoveTo : GridPuzzleAction
{
	protected override void InitAction()
	{
	}

	protected override void StartAction()
	{
		if (this.actor != null)
		{
			this.actor.MovePath(this.cubePath);
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

