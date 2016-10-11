using UnityEngine;
using System.Collections;

public class GridPuzzleNavigable : GridPuzzleMagnetic
{
	public GridPuzzle parentPuzzle;

	public GridPuzzleCamera.Angle angle = GridPuzzleCamera.Angle.Side2D;

	public bool IsNavigable = true;

	public int NavGridIndex;

	public Vector3 NavPosition
	{
		get
		{
			if (this.NavPoint != null)
			{
				return this.NavPoint.transform.position;
			}
			else
			{
				return this.gameObject.transform.position + 0.5f*Vector3.up;
			}
		}
	}

	public GameObject NavPoint = null;
}

