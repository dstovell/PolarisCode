using UnityEngine;
using System.Collections;

public class GridPuzzleGuts : MonoBehaviour 
{
	public GridPuzzleActor actor;
	public Collider col;

	// Use this for initialization
	void Awake() 
	{
		if (this.actor == null)
		{
			this.actor = this.gameObject.GetComponentInParent<GridPuzzleActor>();
		}
		this.col = this.gameObject.GetComponent<Collider>();
	}

	private void OnTriggerEnter(Collider other)
    {
		if ((this.actor != null) && !this.actor.IsMoving())
		{
			//GridPuzzleCube cube = other.gameObject.GetComponent<GridPuzzleCube>();
			GridPuzzleCubeRow cubeRow = other.gameObject.GetComponent<GridPuzzleCubeRow>();
			if (cubeRow != null)
			{
				if (cubeRow.IsInSafeZone(other))
				{
					cubeRow.MakeSafe(this.col);
				}
				else
				{
					this.actor.RequestKill();
				}
			}
		}
    }
}
