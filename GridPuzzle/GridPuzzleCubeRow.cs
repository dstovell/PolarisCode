﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleCubeRow : GridPuzzleNavigable
{
	public GridPuzzleCube [] cubes;

	public BoxCollider box;

	private GridPuzzleVectorUIItem button = null;

	public int x;
	public int y;

	private int lastEditorIndex = 0;

	public bool IsColliderRow
	{
		get
		{
			return (this.parentPuzzle != null) ? this.parentPuzzle.IsColliderRow(this) : false;
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
		CreateSafeZone();
		this.OnCameraAngleChange(this.angle);

		this.parentPuzzle = this.gameObject.GetComponentInParent<GridPuzzle>();

	}

	public void UpdateCollider()
	{		
		if (this.box != null)
		{
			if (this.safeZone != null)
			{
				this.box.isTrigger = this.safeZone.IsAnySafe();
			}
			else
			{
				this.box.isTrigger = false;
			}

			bool enabledForAngle = (this.angle != GridPuzzleCamera.Angle.Isometric);
			bool enabled = this.IsColliderRow && enabledForAngle;
			if (this.box.enabled != enabled)
			{
				this.box.enabled = enabled;
			}
		}
	}

	void Update ()
	{
		UpdateCollider();
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

	public void CreateNavPoint()
	{
		if ((this.NavPoint == null) && this.IsNavigable)
		{
			GridPuzzleCube frontCube = this.GetFrontCube();
			Vector3 nav = (frontCube != null) ? frontCube.NavPosition : this.NavPosition;
			
			this.NavPoint = new GameObject("NavPoint_Side2D");
			this.NavPoint.transform.position = this.NavPosition;
			this.NavPoint.transform.SetParent(this.transform);
			this.NavPoint.tag = "NavPoint_Side2D";
		}
	}

	public GridPuzzleSafeZone safeZone;
	public void CreateSafeZone()
	{
		if (this.safeZone != null)
		{
			return;
		}

		int cubeCount = 0;
		for (int j=0; j<this.cubes.Length; j++)
		{
			if (this.cubes[j] != null)
			{
				break;
			}
			cubeCount++;
		}

		if (cubeCount == 0)
		{
			return;
		}

		float cubeSize = 1f;
		float safeZoneLength = cubeCount*cubeSize;
		float remainingLength = this.cubes.Length*cubeSize - safeZoneLength;
		Vector3 rowPos = this.transform.position;
		Vector3 pos = new Vector3(rowPos.x, rowPos.y, rowPos.z - (0.5f*remainingLength));
		Vector3 size = new Vector3(cubeSize, cubeSize, safeZoneLength);

		GameObject obj = new GameObject("RowSafeZone");
		obj.transform.position = pos;
		this.safeZone = obj.AddComponent<GridPuzzleSafeZone>();
		this.safeZone.AddBox(size);
		obj.transform.SetParent(this.transform);
	}

	public bool IsInSafeZone(Collider other)
	{
		if (this.safeZone == null)
		{
			return false;
		}

		return this.safeZone.IsInSafeZone(other);
	}

	public void MakeSafe(Collider other)
	{
		if (this.safeZone == null)
		{
			return;
		}

		this.safeZone.MakeSafe(other);
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

	public GridPuzzleCube GetClosestCube(Vector3 pos)
	{
		GridPuzzleCube closestCube = null;
		float closestDistance = 999999;
		for (int j=0; j<this.cubes.Length; j++)
		{
			GridPuzzleCube cube = this.cubes[j];
			if (cube != null)
			{
				float thisDistance = Vector3.Distance(cube.NavPosition, pos);
				if (thisDistance < closestDistance)
				{
					closestCube = cube;
					closestDistance = thisDistance;
				}
			}
		}

		return closestCube;
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
			if (this.safeZone != null)
			{
				//this.safeZone.box.enabled = true;
			}
		}
		else if (this.angle == GridPuzzleCamera.Angle.Isometric)
		{
			if (this.safeZone != null)
			{
				//this.safeZone.box.enabled = false;
			}
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

	void OnCollisionEnter(Collision collisionInfo) 
	{
		GridPuzzlePlayerController controller = collisionInfo.collider.gameObject.GetComponent<GridPuzzlePlayerController>();
		if (controller != null)
		{
			//Figure out which what they are from us and register ourself with them!
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

