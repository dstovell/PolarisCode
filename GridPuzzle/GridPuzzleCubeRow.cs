using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleCubeRow : DSTools.MessengerListener
{
	public GridPuzzleCube [] cubes;

	public BoxCollider box;

	private GridPuzzleVectorUIItem button = null;

	public GridPuzzleCamera.Angle angle = GridPuzzleCamera.Angle.Side2D;

	public int x;
	public int y;

	private int lastEditorIndex = 0;

	private GridPuzzle parentPuzzle;

	public Vector3 NavPosition
	{
		get
		{
			return this.gameObject.transform.position + 0.5f*Vector3.up;
		}
	}

	public bool IsTop
	{
		get
		{
			return (this.parentPuzzle != null) ? this.parentPuzzle.IsTopRow(this) : false;
		}
	}

	void Awake()
	{
		this.gameObject.layer = LayerMask.NameToLayer("CubeRow");
	}

	void Start()
	{
		if (GridPuzzleEditor.IsActive())
		{
			this.InitMessenger("GridPuzzleCubeRow");
		}
		this.OnCameraAngleChange(this.angle);

		this.parentPuzzle = this.gameObject.GetComponentInParent<GridPuzzle>();
	}

	public void UpdateCollider()
	{		
		if (this.box != null)
		{
			bool enabledForAngle = (this.angle != GridPuzzleCamera.Angle.Isometric);
			bool enabled = this.IsTop && enabledForAngle;
			if (this.box.enabled != enabled)
			{
				this.box.enabled = enabled;
			}
		}
	}

	void Update ()
	{
		UpdateCollider();

		/*if (GridPuzzleEditor.IsActive() && (this.angle == GridPuzzleCamera.Angle.Side2D))
		{
			if (this.button == null)
			{
				this.button = this.gameObject.AddComponent<GridPuzzleVectorUIItem>();
				this.button.editorAction = GridPuzzleEditorAction.AddCubes;
				this.button.text = "+";
				this.button.size = 0.1f;
			}

			this.button.position = Camera.main.WorldToViewportPoint(this.transform.position);
		}
		else if ((this.angle == GridPuzzleCamera.Angle.Side2D) && this.isTopRow)
		{
			if (this.button == null)
			{
				this.button = this.gameObject.AddComponent<GridPuzzleVectorUIItem>();
				this.button.gameplayAction = GridPuzzleGameplayAction.MoveToCubeRow;
				this.button.text = "*";
				this.button.size = 0.05f;
			}

			this.button.position = Camera.main.WorldToViewportPoint(this.transform.position);
		}
		else if (this.button != null)
		{
			this.button.CloseAndDestroy();
			GameObject.DestroyObject(this.button);
			this.button = null;
		}*/
	}

	public void Destroy()
	{
		if (this.button != null)
		{
			this.button.CloseAndDestroy();
			GameObject.DestroyObject(this.button);
			this.button = null;
		}
		RemoveMessenger();
		GameObject.Destroy(this.gameObject);
	}

	public GridPuzzleCube GetFrontCube()
	{
		if (this.cubes == null)
		{
			return null;
		}

		GridPuzzleCube frontCube = null;
		for (int j=0; j<this.cubes.Length; j++)
		{
			GridPuzzleCube cube = this.cubes[j];
			if (cube != null)
			{
				if ((frontCube == null) || (cube.z  > frontCube.z))
				{
					frontCube = cube;
				}
			}
		}

		return frontCube;
	}

	public int GetCubeCount()
	{
		int count = 0;
		if (this.cubes == null)
		{
			return count;
		}

		for (int j=0; j<this.cubes.Length; j++)
		{
			GridPuzzleCube cube = this.cubes[j];
			if (cube != null)
			{							
				count++;
			}
		}

		return count;
	}

	public void AddCubes(GameObject prefab, int maxCubeCount, Vector3 basePostion)
	{
		if ((maxCubeCount > 0) && (this.cubes == null))
		{
			int safeCubeCount =  Mathf.Max(maxCubeCount, 1);
			int offset = -1 * Mathf.FloorToInt((float)safeCubeCount/2f);
			this.cubes = new GridPuzzleCube[maxCubeCount];
			for (int j=0; j<maxCubeCount; j++)
			{
				int z = j+offset;
				Vector3 pos = basePostion + new Vector3(0, 0, z);
				GridPuzzleCube cube = GridPuzzleCube.GeneratePrefab(prefab, pos, this.x, this.y, j);
				cube.gameObject.transform.SetParent(this.transform);
				this.cubes[j] = cube;
			}
		}
	}

	public void DestoryCubes()
	{
		if (this.cubes != null)
		{
			for (int i=0; i<this.cubes.Length; i++)
			{
				if (this.cubes[i] != null)
				{
					GameObject.Destroy(this.cubes[i].gameObject);
				}
			}
			this.cubes = null;
		}
	}

	static public GridPuzzleCubeRow GeneratePrefab(GridPuzzle.Settings settings, int maxCubeCount, Vector3 basePostion, int gridX, int gridY, bool addCubes = true)
	{
		int safeCubeCount =  Mathf.Max(maxCubeCount, 1);
		GameObject rowObj = new GameObject("GridPuzzleCubeRow");
		rowObj.transform.position = basePostion;
		GridPuzzleCubeRow rowComp = rowObj.AddComponent<GridPuzzleCubeRow>();

		rowComp.box = rowObj.AddComponent<BoxCollider>();
		rowComp.box.size = new Vector3(1, 1, safeCubeCount);
		//rowComp.box.isTrigger = true;

		rowComp.x = gridX;
		rowComp.y = gridY;

		if (addCubes && (maxCubeCount > 0))
		{
			rowComp.AddCubes(settings.PickRandomPrefab(settings.cubePrefabs), maxCubeCount, basePostion);
		}
		return rowComp;
	}

	public void OnCameraAngleChange(GridPuzzleCamera.Angle _angle)
	{
		this.angle = _angle;
		if (this.angle == GridPuzzleCamera.Angle.Side2D)
		{
			if ((this.cubes != null) && (this.cubes.Length > 0))
			{
				this.box.enabled = true;
				this.box.isTrigger = false;
			}
		}
		else if (this.angle == GridPuzzleCamera.Angle.Isometric)
		{
			this.box.enabled = false;
		}

		if ((this.cubes != null) && (this.cubes.Length > 0))
		{
			for (int i=0; i<this.cubes.Length; i++)
			{
				if (this.cubes[i] != null)
				{
					this.cubes[i].OnCameraAngleChange(this.angle);
				}
			}
		}
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{
		if (id == "GridPuzzleEditorAction")
		{
			GridPuzzleEditorAction action = (GridPuzzleEditorAction)obj1;

			switch(action)
			{
			case GridPuzzleEditorAction.AddCubes:
				GameObject obj = obj2 as GameObject;
				if (obj == this.gameObject)
				{
					this.DestoryCubes();
					GameObject prefab = GridPuzzleEditor.Instance.cubePrefabs[this.lastEditorIndex];
					this.AddCubes(prefab, 5, this.transform.position);
					this.lastEditorIndex++;
					if (this.lastEditorIndex >= GridPuzzleEditor.Instance.cubePrefabs.Length)
					{
						this.lastEditorIndex = 0;
					}
				}
				break;
			default:
				break;
			}
		}
	}
}

