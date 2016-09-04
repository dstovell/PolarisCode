using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DSTools;

public class GridPuzzle : MessengerListener 
{
	public class Settings
	{
		public GameObject [] teleporterPrefabs;
		public GameObject [] sideWallPrefabs;

		public GameObject [] cubePrefabs;

		public int GridWidth;
		public int GridHeight;
		public int GridDepth;

		public int GridPlateauHeight;

		public float GridCubeSize = 1.0f;

		public float PuzzleHeight 
		{
			get
			{
				return this.GridCubeSize * (float)this.GridHeight;
			}
		}

		public float PuzzleWidth 
		{
			get
			{
				return this.GridCubeSize * (float)this.GridWidth;
			}
		}

		public float PuzzleDepth
		{
			get
			{
				return this.GridCubeSize * (float)this.GridDepth;
			}
		}

		public GameObject PickRandomPrefab(GameObject [] array)
	    {
			int count = array.Length;
			if (count == 0)
	    	{
	    		return null;
	    	}

			int randomIndex = Random.Range(0, count);

			return array[randomIndex];
	    }
	}

	GridPuzzleCube [,,] cubeGrid;

	public Settings settings;

	public GameObject spawnPoint;
	public GridPuzzlePortal exitPoint;
	public GridPuzzleCubeRow [] rows;

	public GridPuzzleCamera.Angle currentAngle = GridPuzzleCamera.Angle.None;

	public GridPuzzleManager.PuzzlePosition postion = GridPuzzleManager.PuzzlePosition.None;
	public GridPuzzleManager.PuzzlePosition previousPosition = GridPuzzleManager.PuzzlePosition.None;

	public Vector3 [] navNeighbourDirs;

	private bool markedForDelete = false;

	void Awake() 
	{
	}

	void Start()
	{
		Fix();
	}

	public List<Vector3> GetGeoNeighbourDirs()
	{
		List<Vector3> geoNeighbourDirs = new List<Vector3>();
		geoNeighbourDirs.Add(Vector3.up);
		geoNeighbourDirs.Add(Vector3.down);
		geoNeighbourDirs.Add(Vector3.left);
		geoNeighbourDirs.Add(Vector3.right);
		geoNeighbourDirs.Add(Vector3.forward);
		geoNeighbourDirs.Add(Vector3.back);

		return geoNeighbourDirs;
	}

	public void Optimize()
	{
		Debug.Log("Optimize");
		if(this.cubeGrid == null)
		{
			return;
		}

		for (int x=0; x<this.cubeGrid.GetLength(0); x++)
		{
			for (int y=0; y<this.cubeGrid.GetLength(1); y++)
			{
				for (int z=0; z<this.cubeGrid.GetLength(2); z++)
				{
					GridPuzzleCube cube = this.GetCube(x,y,z);
					if (cube != null)
					{
						List<GridPuzzleCube> neighbours = this.GetNeighbours(cube);
						for (int i=0; i<neighbours.Count; i++)
						{
							bool allRemoved = cube.RemoveSharedSurfaces(neighbours[i]);
							if (allRemoved)
							{
								this.DestroyCube(cube);
								break;
							}
						}
					}
				}
			}
		}
	}

	public void Fix()
	{
		if (this.rows != null)
		{
			for (int i=0; i<this.rows.Length; i++)
			{
				GridPuzzleCubeRow row = this.rows[i];
				if (row != null)
				{
					this.AddCubesFromRow(row);
				}
			}
		}
	}

	public bool HasRelativeNeighbour(GridPuzzleCube cube, Vector3 dir, string name)
	{
		Vector3 posToCheck = cube.GridPositon + dir;
		Debug.Log(name  
					+ " " + cube.GridPositon.x + "," + cube.GridPositon.y + "," + cube.GridPositon.z 
					+ " + " + dir.x + "," + dir.y + "," + dir.z 
					+ " => " + posToCheck.x + "," + posToCheck.y + "," + posToCheck.z);
		return this.IsCubeAt(Mathf.FloorToInt(posToCheck.x), Mathf.FloorToInt(posToCheck.y), Mathf.FloorToInt(posToCheck.z));
	}

	public GridPuzzleCube GetNeighbour(GridPuzzleCube cube, Vector3 pos)
	{
		return this.GetNeighbour(cube, Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
	}

	public GridPuzzleCube GetNeighbour(GridPuzzleCube cube, int dx, int dy, int dz)
	{
		int x = cube.x + dx;
		int y = cube.y + dy;
		int z = cube.z + dz;
		Debug.Log(name  
					+ " " + cube.x + "," + cube.y + "," + cube.z 
					+ " + " + dx + "," + dy + "," + dz 
					+ " => " + x + "," + y + "," + z);
		return this.GetCube(x, y, z);
	}

	public List<GridPuzzleCube> GetNeighbours(GridPuzzleCube cube)
	{
		List<Vector3> dirs = this.GetGeoNeighbourDirs();

		List<GridPuzzleCube> list = new List<GridPuzzleCube>();
		for (int i=0; i<dirs.Count; i++)
		{
			Vector3 dir = dirs[i];
			GridPuzzleCube n = this.GetNeighbour(cube, dir);
			if (n != null)
			{
				list.Add(n);
			}
		}
		return list;
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void OnCameraAngleChange(GridPuzzleCamera.Angle angle)
	{
		if (this.currentAngle != angle)
		{
			this.currentAngle = angle;
			if (this.rows != null)
			{
				for (int i=0; i<this.rows.Length; i++)
				{
					if (this.rows[i] != null)
					{
						this.rows[i].OnCameraAngleChange(angle);
					}
				}
			}
		}
	}

	public GridPuzzleManager.PuzzlePosition GetPosition()
	{
		return this.postion;
	}

	public GridPuzzleManager.PuzzlePosition GetPreviousPosition()
	{
		return this.previousPosition;
	}

	public void SetPosition(GridPuzzleManager.PuzzlePosition newPos)
	{
		//Debug.Log("SetPosition " + this.postion.ToString() + " => " + newPos.ToString());
		if (this.postion != newPos)
		{
			this.previousPosition = this.postion;
			this.postion = newPos;
		}
	}

	public bool IsInPosition(Vector3 pos)
	{
		return (this.gameObject.transform.position == pos);
	}

	public void MoveTowards(Vector3 newPos, float speed)
	{
		this.gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, newPos, speed*Time.deltaTime);
	}

	public void MarkForDelete()
	{
		this.markedForDelete = true;
	}

	public bool IsMarkedForDelete()
	{
		return this.markedForDelete;
	}

	public Transform GetSpawn()
	{
		return (this.spawnPoint != null) ? this.spawnPoint.transform : this.gameObject.transform;
	}

	public bool IsTopRow(GridPuzzleCubeRow row)
	{
		if (row != null)
		{
			return !this.IsRowAt(row.x, row.y);
		}
		return false;
	}

	public bool IsTopCube(GridPuzzleCube cube)
	{
		if (cube != null)
		{
			return this.IsTopCube(cube.x, cube.y, cube.z);
		}
		return false;
	}

	public bool IsTopCube(int x, int y, int z)
	{
		GridPuzzleCube cube = this.GetCube(x, y,  z);
		if (cube != null)
		{
			return !this.IsCubeAt(x, y+1, z);
		}
		return false;
	}

	public bool IsCubeAt(int x, int y, int z)
	{
		return (this.GetCube(x, y, z) != null);
	}

	public GridPuzzleCube GetCube(int x, int y, int z)
	{
		if (this.cubeGrid == null)
		{
			return null;
		}

		if (	(x >= 0) && (x < this.cubeGrid.GetLength(0)) &&
				(y >= 0) && (y < this.cubeGrid.GetLength(1)) &&
				(z >= 0) && (z < this.cubeGrid.GetLength(2))    )
		{
			return this.cubeGrid[x,y,z];
		}
		else 
		{
			return null;
		}
	}

	public bool IsRowAt(int x, int y)
	{
		return (this.GetRow(x, y) != null);
	}

	public GridPuzzleCubeRow GetRow(int x, int y)
	{
		for (int i=0; i<this.rows.Length; i++)
		{
			GridPuzzleCubeRow row = this.rows[i];
			if ((row != null) && (row.x == x) && (row.y == y))
			{
				return row;
			}
		}
		return null;
	}

	public void DestroyCube(int x, int y, int z)
	{
		if (	(x >= 0) && (x < this.cubeGrid.GetLength(0)) &&
				(y >= 0) && (y < this.cubeGrid.GetLength(1)) &&
				(z >= 0) && (z < this.cubeGrid.GetLength(2))    )
		{
			GameObject.Destroy(this.cubeGrid[x,y,z]);
			this.cubeGrid[x,y,z] = null;
		}
	}

	public void DestroyCube(GridPuzzleCube cube)
	{
		DestroyCube(cube.x, cube.y, cube.z);
	}

	static public GridPuzzlePortal AddRandomPortal(GridPuzzle.Settings settings, Vector3 pos, GameObject parent)
	{
		GameObject obj = AddRandomPrefab(settings, settings.teleporterPrefabs, pos, parent);
		GridPuzzlePortal portal = obj.GetComponent<GridPuzzlePortal>();
		return portal;
	}

	static public GameObject AddRandomPrefab(GridPuzzle.Settings settings, GameObject [] prefabs, Vector3 pos, GameObject parent)
	{
		GameObject obj = GameObject.Instantiate(settings.PickRandomPrefab(prefabs), pos, Quaternion.identity) as GameObject;
		if (parent != null)
		{
			obj.transform.SetParent(parent.transform);
		}
		return obj;
	}

	static public GridPuzzle GeneratePrefab(GridPuzzle.Settings settings)
	{
		GameObject puzzleObj = new GameObject("GridPuzzle");
		puzzleObj.transform.position = Vector3.zero;
		GridPuzzle puzzle = puzzleObj.AddComponent<GridPuzzle>();
		int rowLength = settings.GridDepth;

		puzzle.rows = new GridPuzzleCubeRow[settings.GridWidth*settings.GridHeight];

		int widthOffset = -1 * Mathf.FloorToInt((float)settings.GridWidth/2f);
		int heightOffset = -1 * Mathf.FloorToInt((float)settings.GridHeight/2f);

		puzzle.cubeGrid = new GridPuzzleCube[settings.GridWidth,settings.GridHeight,settings.GridDepth];

		int rowIndex = 0;
		for (int j=0; j<settings.GridHeight; j++)
		{
			for (int i=0; i<settings.GridWidth; i++)
			{
				Vector3 pos = new Vector3(i+widthOffset, j+heightOffset, 0);

				bool addCubes = (j < settings.GridPlateauHeight) ? true : false;
				if (addCubes)
				{
					GridPuzzleCubeRow newRow = GridPuzzleCubeRow.GeneratePrefab(settings, rowLength, pos, i, j, addCubes);
					newRow.gameObject.transform.SetParent(puzzleObj.transform);

					puzzle.AddCubesFromRow(newRow);

					puzzle.rows[rowIndex] = newRow;
					rowIndex++;
				}
			}
		}

		float portalHeight = 0.5f + (float)heightOffset + (settings.GridPlateauHeight - 1);
		Vector3 portal1Pos = new Vector3(widthOffset, portalHeight, 0f);
		GridPuzzlePortal portal1 = AddRandomPortal(settings, portal1Pos, puzzleObj);
		portal1.gameObject.name = "portalSpawn";
		puzzle.spawnPoint = portal1.gameObject;

		Vector3 portal2Pos = new Vector3(-1*widthOffset-1, portalHeight, 0f);
		GridPuzzlePortal portal2 = AddRandomPortal(settings, portal2Pos, puzzleObj);
		portal2.gameObject.name = "portalExit";
		puzzle.exitPoint = portal2;

		puzzle.OnCameraAngleChange(GridPuzzleCamera.Angle.Side2D);

		return puzzle;
	}


	public void AddCubesFromRow(GridPuzzleCubeRow newRow)
	{
		if (this.cubeGrid == null)
		{
			Debug.LogWarning(this.gameObject.name + " has a null cubeGrid");
			return;
		}

		if (newRow.cubes != null)
		{
			for (int k=0; k<newRow.cubes.Length; k++)
			{
				GridPuzzleCube cube = newRow.cubes[k];
				if (cube != null)
				{
					this.cubeGrid[cube.x, cube.y, cube.z] = cube;
				}
			}
		}
	}

	public class Stats
	{
		public int cubeCount = 0;
		public int surfaceCount = 0;
		public int activeSurfaceCount = 0;
	}

	public GridPuzzle.Stats GetStats()
	{
		Stats s  = new Stats();
		if(this.cubeGrid == null)
		{
			return s;
		}

		for (int x=0; x<this.cubeGrid.GetLength(0); x++)
		{
			for (int y=0; y<this.cubeGrid.GetLength(1); y++)
			{
				for (int z=0; z<this.cubeGrid.GetLength(2); z++)
				{
					GridPuzzleCube cube = this.GetCube(x,y,z);
					if (cube != null)
					{
						s.cubeCount++;
						GameObject [] surfaces = cube.surfaces;
						if (surfaces != null)
						{
							for (int i=0; i<surfaces.Length; i++)
							{
								if (surfaces[i] != null)
								{
									s.surfaceCount++;
									if (surfaces[i].active)
									{
										s.activeSurfaceCount++;						
									}
								}
							}
						}
					}
				}
			}
		}

		return s;
	}
}
