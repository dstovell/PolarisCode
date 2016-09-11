using UnityEngine;
using System.Collections;

public class GridPuzzleChangeVertical : GridPuzzleAction
{
	protected override void InitAction()
	{
		
	}

	protected override void StartAction()
	{
		Debug.LogError("GridPuzzleChangeVertical Count=" + this.cubeRowPath.Count + " actor=" + this.actor);
		if (this.actor != null)
		{
			this.actor.ChangeVertical(this.cubeRowPath);
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

