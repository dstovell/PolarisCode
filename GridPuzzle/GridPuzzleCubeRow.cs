using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleCubeRow : MonoBehaviour
{
	public GridPuzzleCube [] cubes;

	public BoxCollider box;

	void Awake()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	static public GridPuzzleCubeRow GeneratePrefab(GridPuzzle.Settings settings, int maxCubeCount, Vector3 basePostion, bool addCubes = true)
	{
		int safeCubeCount =  Mathf.Max(maxCubeCount, 1);
		int offset = -1 * Mathf.FloorToInt((float)safeCubeCount/2f);
		Debug.Log("GridPuzzleCubeRow.GeneratePrefab safeCubeCount=" + safeCubeCount + " offset=" + offset);
		GameObject rowObj = new GameObject("GridPuzzleCubeRow");
		rowObj.transform.position = basePostion;
		GridPuzzleCubeRow rowComp = rowObj.AddComponent<GridPuzzleCubeRow>();

		rowComp.box = rowObj.AddComponent<BoxCollider>();
		rowComp.box.size = new Vector3(1, 1, safeCubeCount);
		rowComp.box.isTrigger = true;

		if (addCubes && (maxCubeCount > 0))
		{
			rowComp.cubes = new GridPuzzleCube[maxCubeCount];
			for (int j=0; j<maxCubeCount; j++)
			{
				Vector3 pos = basePostion + new Vector3(0, 0, j+offset);
				GridPuzzleCube cube = GridPuzzleCube.GeneratePrefab(settings, pos);
				cube.gameObject.transform.SetParent(rowObj.transform);
				rowComp.cubes[j] = cube;
			}
		}
		return rowComp;
	}

	public void OnCameraAngleChange(GridPuzzleCamera.Angle angle)
	{
		if (angle == GridPuzzleCamera.Angle.Side2D)
		{
			if ((this.cubes != null) && (this.cubes.Length > 0))
			{
				this.box.isTrigger = false;
			}
		}
		else if (angle == GridPuzzleCamera.Angle.Isometric)
		{
			this.box.isTrigger = true;
		}

		if ((this.cubes != null) && (this.cubes.Length > 0))
		{
			for (int i=0; i<this.cubes.Length; i++)
			{
				if (this.cubes[i] != null)
				{
					this.cubes[i].OnCameraAngleChange(angle);
				}
			}
		}
	}
}

