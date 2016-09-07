using UnityEngine;
using System.Collections;

public class GridPuzzleGuts : MonoBehaviour 
{
	public GridPuzzleActor actor;

	// Use this for initialization
	void Awake() 
	{
		if (this.actor == null)
		{
			this.actor = this.gameObject.GetComponentInParent<GridPuzzleActor>();
		}
	}

	private void OnTriggerEnter(Collider other)
    {
		if ((this.actor != null) && !this.actor.IsMoving())
		{
			GridPuzzleCube cube = other.gameObject.GetComponent<GridPuzzleCube>();
			GridPuzzleCubeRow cubeRow = other.gameObject.GetComponent<GridPuzzleCubeRow>();
			if ((cube != null) || (cubeRow != null))
			{
				this.actor.RequestKill();
			}
		}
    }
}
