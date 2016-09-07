using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DSTools;
using Pathfinding;

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

	public int GridWidth;
	public int GridHeight;
	public int GridDepth;
	public float GridCubeSize = 1.0f;

	public GameObject spawnPoint;
	public GridPuzzlePortal exitPoint;
	public GridPuzzleCubeRow [] rows;

	private PointGraph navGraph;

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
		//Fix();
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

	public GridPuzzleCube GetCubeByNavPoint(Vector3 navPoint)
	{
		for (int x=0; x<this.cubeGrid.GetLength(0); x++)
		{
			for (int y=0; y<this.cubeGrid.GetLength(1); y++)
			{
				for (int z=0; z<this.cubeGrid.GetLength(2); z++)
				{
					GridPuzzleCube cube = this.GetCube(x,y,z);
					if (cube != null)
					{
						float dist = Vector3.Distance(navPoint, cube.NavPosition);
						if (dist < 0.1f)
						{
							return cube;
						}
					}
				}
			}
		}

		return null;
	}

	public List<GridPuzzleCubeRow> GetCubeRowsByNavPoints(List<Vector3> navPoints)
	{
		Dictionary<Vector3, GridPuzzleCubeRow> rowMap = new Dictionary<Vector3, GridPuzzleCubeRow>();
		for (int x=0; x<this.rows.Length; x++)
		{
			GridPuzzleCubeRow row = this.rows[x];
			if (row == null)
			{
				continue;
			}

			for (int i=0; i<navPoints.Count; i++)
			{
				float dist = Vector3.Distance(navPoints[i], row.NavPosition);
				if (dist < 0.1f)
				{
					rowMap[ navPoints[i] ] = row;
				}
			}
		}

		List<GridPuzzleCubeRow> rows = new List<GridPuzzleCubeRow>();
		for (int i=0; i<navPoints.Count; i++)
		{
			if (rowMap.ContainsKey(navPoints[i]))
			{
				rows.Add( rowMap[ navPoints[i] ] );
			}
		}

		return rows;
	}

	public List<GridPuzzleCube> GetCubesByNavPoints(List<Vector3> navPoints)
	{
		Dictionary<Vector3, GridPuzzleCube> cubeMap = new Dictionary<Vector3, GridPuzzleCube>();
		for (int x=0; x<this.cubeGrid.GetLength(0); x++)
		{
			for (int y=0; y<this.cubeGrid.GetLength(1); y++)
			{
				for (int z=0; z<this.cubeGrid.GetLength(2); z++)
				{
					GridPuzzleCube cube = this.GetCube(x,y,z);
					if (cube != null)
					{
						for (int i=0; i<navPoints.Count; i++)
						{
							float dist = Vector3.Distance(navPoints[i], cube.NavPosition);
							if (dist < 0.1f)
							{
								cubeMap[ navPoints[i] ] = cube;
							}
						}
					}
				}
			}
		}

		List<GridPuzzleCube> cubes = new List<GridPuzzleCube>();
		for (int i=0; i<navPoints.Count; i++)
		{
			if (cubeMap.ContainsKey(navPoints[i]))
			{
				cubes.Add( cubeMap[ navPoints[i] ] );
			}
		}

		return cubes;
	}

	public void SetupNavPoints(PointGraph graph)
	{
		this.navGraph = graph;
		Debug.Log("SetupNavPoints");
		if(this.cubeGrid == null)
		{
			return;
		}

		Debug.Log("SetupNavPoints started");
		for (int x=0; x<this.cubeGrid.GetLength(0); x++)
		{
			for (int y=0; y<this.cubeGrid.GetLength(1); y++)
			{
				for (int z=0; z<this.cubeGrid.GetLength(2); z++)
				{
					GridPuzzleCube cube = this.GetCube(x,y,z);
					if ((cube != null) && this.IsTopCube(cube))
					{
						cube.CreateNavPoint();
						BoxCollider	box = cube.gameObject.GetComponent<BoxCollider>();
						if (box != null)
						{
							box.enabled = true;
						}
					}
				}
			}
		}

		for (int i=0; i<this.rows.Length; i++)
		{
			GridPuzzleCubeRow row = this.rows[i];
			if ((row != null) && this.IsTopRow(row))
			{
				row.CreateNavPoint();
			}
		}
	}

	public List<GridPuzzleCube> GetPerspectiveAlignedCubes(GridPuzzleCube cube)
	{
		List<Vector3> cubeAlignmentVectors = new List<Vector3>();
		List<bool> vectorUsed = new List<bool>();
		cubeAlignmentVectors.Add(Vector3.right); vectorUsed.Add(false);
		cubeAlignmentVectors.Add(Vector3.forward); vectorUsed.Add(false);

		Vector3 deltaVector = new Vector3(1,-1,1);

		Vector3 n1EdgePos = cube.NavPosition + new Vector3(0.5f, 0, 0f);

		string [] maskStrings = new string[1]{"Cube"};
		LayerMask mask = LayerMask.GetMask(maskStrings);

		List<GridPuzzleCube> alignedCubes = new List<GridPuzzleCube>();
		for (int i=1; i<2; i++)
		{
			for (int j=0; j<cubeAlignmentVectors.Count; j++)
			{
				if (vectorUsed[j])
				{
					continue;
				}

				Vector3 dv = cubeAlignmentVectors[j] + i*deltaVector;
				GridPuzzleCube alignedCube = this.GetCube(cube.x + (int)dv.x, cube.y + (int)dv.y, cube.z + (int)dv.z);
				if ((alignedCube != null) && this.IsTopCube(alignedCube))
				{
					Vector3 n2EdgePos = alignedCube.NavPosition + new Vector3(-0.5f, 0, 0f);;
					Vector3 dir = (n2EdgePos - n1EdgePos).normalized;

					alignedCubes.Add(alignedCube);

					/*RaycastHit hitPoint = new RaycastHit();
        			Ray ray = new Ray(transform.position, transform.forward);
					//RayCast, then connect
					if (Physics.Raycast(ray, out hitPoint, 20, mask))
					{
						if (hitPoint.collider.gameObject == alignedCube.gameObject)
						{
							alignedCubes.Add(alignedCube);
						}
						else
						{
							GridPuzzleCube cubeHit = hitPoint.collider.gameObject.GetComponent<GridPuzzleCube>();
							Debug.LogError("expected " + alignedCube.x + ","+alignedCube.y +","+alignedCube.z + " got " + cubeHit.x + ","+cubeHit.y +","+cubeHit.z);
						}
					}
					else
					{
						Debug.LogError("Hit nothing at distance " + 1.5f*i);
					}*/
				}
			}
		}
		return alignedCubes;
	}

	public void LinkPerspectiveAlignedCubes(PointGraph graph)
	{
		for (int x=0; x<this.cubeGrid.GetLength(0); x++)
		{
			for (int y=0; y<this.cubeGrid.GetLength(1); y++)
			{
				for (int z=0; z<this.cubeGrid.GetLength(2); z++)
				{
					GridPuzzleCube cube = this.GetCube(x,y,z);
					if ((cube != null) && this.IsTopCube(cube))
					{
						NNInfo node1 = graph.GetNearest(cube.NavPosition);

						List<GridPuzzleCube> alignedCubes = this.GetPerspectiveAlignedCubes(cube);
						for (int j=0; j<alignedCubes.Count; j++)
						{
							GridPuzzleCube alignedCube = alignedCubes[j];
							NNInfo node2 = graph.GetNearest(alignedCube.NavPosition);

							if (!node1.node.ContainsConnection(node2.node))
							{
								node1.node.AddConnection(node2.node, 1);
							}
						}
					}
				}
			}
		}
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
					int numAdded = this.AddCubesFromRow(row);
					if (numAdded == 0)
					{
						GameObject.Destroy(row);
						this.rows[i] = null;
					}
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

	public bool IsColliderRow(GridPuzzleCubeRow row)
	{
		if ((row == null) || (row.GetCubeCount() == 0))
		{
			return false;
		}

		GridPuzzleCubeRow rowAbove = this.GetRow(row.x, row.y+1);
		if (rowAbove == null)
		{
			return true;
		}

		return (rowAbove.GetCubeCount() < row.cubes.Length);
	}

	public bool IsTopRow(GridPuzzleCubeRow row)
	{
		if ((row == null) || (row.GetCubeCount() == 0))
		{
			return false;
		}

		GridPuzzleCubeRow rowAbove = this.GetRow(row.x, row.y+1);
		if (rowAbove == null)
		{
			return true;
		}

		return false;
	}

	public bool IsTopCube(GridPuzzleCube cube)
	{
		if (cube != null)
		{
			return !this.IsCubeAt(cube.x, cube.y+1, cube.z);
		}
		return false;
	}

	public bool IsTopCube(int x, int y, int z)
	{
		GridPuzzleCube cube = this.GetCube(x, y, z);
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
		this.DestroyCube( this.GetCube(x, y, z) );
	}

	public void DestroyCube(GridPuzzleCube cube)
	{
		if (cube != null)
		{
			this.cubeGrid[cube.x,cube.y,cube.z] = null;
			GameObject.Destroy(cube);
		}
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
		puzzle.GridWidth = settings.GridWidth;
		puzzle.GridHeight = settings.GridHeight;
		puzzle.GridDepth = settings.GridDepth;

		int rowLength = puzzle.GridDepth;

		puzzle.rows = new GridPuzzleCubeRow[puzzle.GridWidth*puzzle.GridHeight];

		int widthOffset = -1 * Mathf.FloorToInt((float)settings.GridWidth/2f);
		int heightOffset = -1 * Mathf.FloorToInt((float)settings.GridHeight/2f);

		int rowIndex = 0;
		for (int j=0; j<puzzle.GridHeight; j++)
		{
			for (int i=0; i<puzzle.GridWidth; i++)
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


	public int AddCubesFromRow(GridPuzzleCubeRow newRow)
	{
		int numAdded = 0;
		if (this.cubeGrid == null)
		{
			if ((this.GridWidth == 0) || (this.GridHeight == 0) || (this.GridDepth == 0))
			{
				Debug.LogError("Attemping to add cubes to a puzzle with no cubeGrid or stored settings");
				return numAdded;
			}

			Debug.LogWarning("Attemping to add cubes to a puzzle with no cubeGrid, building from strored settings");
			this.cubeGrid = new GridPuzzleCube[this.GridWidth, this.GridHeight, this.GridDepth];
		}

		if (newRow.cubes != null)
		{
			
			for (int k=0; k<newRow.cubes.Length; k++)
			{
				GridPuzzleCube cube = newRow.cubes[k];
				if (cube != null)
				{
					this.cubeGrid[cube.x, cube.y, cube.z] = cube;
					numAdded++;
				}
			}
		}
		return numAdded;
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
