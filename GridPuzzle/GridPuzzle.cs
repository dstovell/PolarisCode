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

	private PointGraph IsometricGraph;
	private PointGraph Side2dGraph;

	public GridPuzzleCamera.Angle currentAngle = GridPuzzleCamera.Angle.None;

	public GridPuzzleManager.PuzzlePosition postion = GridPuzzleManager.PuzzlePosition.None;
	public GridPuzzleManager.PuzzlePosition previousPosition = GridPuzzleManager.PuzzlePosition.None;

	private bool markedForDelete = false;

	void Awake() 
	{
	}

	void Start()
	{
		//Fix();
	}

	private List<Vector3> geoNeighbourDirs;
	public List<Vector3> GetGeoNeighbourDirs()
	{
		if (this.geoNeighbourDirs != null)
		{
			return this.geoNeighbourDirs;
		}

		this.geoNeighbourDirs = new List<Vector3>();
		this.geoNeighbourDirs.Add(Vector3.up);
		this.geoNeighbourDirs.Add(Vector3.down);
		this.geoNeighbourDirs.Add(Vector3.left);
		this.geoNeighbourDirs.Add(Vector3.right);
		this.geoNeighbourDirs.Add(Vector3.forward);
		this.geoNeighbourDirs.Add(Vector3.back);

		return this.geoNeighbourDirs;
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
				if ((alignedCube != null) && alignedCube.IsNavigable)
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

	public void Scan(AstarPath path)
	{
		this.IsometricGraph = path.astarData.AddGraph(typeof(PointGraph)) as PointGraph;
		this.IsometricGraph.searchTag = "NavPoint_Iso";
		this.IsometricGraph.maxDistance = 1.01f;
		this.IsometricGraph.limits.x = 1.01f;
		this.IsometricGraph.limits.y = 1.01f;
		this.IsometricGraph.limits.z = 1.01f;
		this.IsometricGraph.raycast = false;

		this.Side2dGraph = path.astarData.AddGraph(typeof(PointGraph)) as PointGraph;
		this.Side2dGraph.searchTag = "NavPoint_Side2D";
		this.Side2dGraph.maxDistance = 3.0f;
		this.Side2dGraph.limits.x = 1.2f;
		this.Side2dGraph.limits.y = 3.01f;
		this.Side2dGraph.limits.z = 1.0f;
		this.IsometricGraph.raycast = false;

		path.Scan();

		if ((this.cubeGrid == null) || (this.IsometricGraph == null) || (this.Side2dGraph == null))
		{
			return;
		}

		this.ScanIsometric(this.IsometricGraph);
		this.ScanSide2d(this.Side2dGraph);
	}

	public void ScanIsometric(PointGraph isometricGraph)
	{
		this.IsometricGraph = isometricGraph;

		if ((this.cubeGrid == null) || (this.IsometricGraph == null))
		{
			return;
		}

		this.cubeNavLinks = new Dictionary<int, CubeNavLink>();

		this.SetupIsometricNavPoints(false);

		this.LinkNavigableCubes();
	}

	public void ScanSide2d(PointGraph side2dGraph)
	{
		this.Side2dGraph = side2dGraph;

		if ((this.cubeGrid == null) || (this.Side2dGraph == null))
		{
			return;
		}

		this.rowNavLinks = new Dictionary<int, CubeRowNavLink>();

		this.SetupSide2dNavPoints(false);
	}

	public class CubeNavLink
	{
		public CubeNavLink(PointNode n, GridPuzzleCube c)
		{
			this.node = n;
			this.cube = c;
		}

		public PointNode node;
		public GridPuzzleCube cube;
	}

	public class CubeRowNavLink
	{
		public CubeRowNavLink(PointNode n, GridPuzzleCubeRow r)
		{
			this.node = n;
			this.row = r;
		}

		public PointNode node;
		public GridPuzzleCubeRow row;
	}

	private Dictionary<int, CubeNavLink> cubeNavLinks;
	private Dictionary<int, CubeRowNavLink> rowNavLinks;

	private void SetupIsometricNavPoints(bool tagOnly = false)
	{
		int pointsAdded = 0;

		for (int x=0; x<this.cubeGrid.GetLength(0); x++)
		{
			for (int y=0; y<this.cubeGrid.GetLength(1); y++)
			{
				for (int z=0; z<this.cubeGrid.GetLength(2); z++)
				{
					GridPuzzleCube cube = this.GetCube(x,y,z);
					if ((cube != null) && this.IsTopCube(cube) && cube.IsNavigable)
					{
						if (tagOnly)
						{
							cube.CreateNavPoint();
						}
						else
						{
							PointNode node = null;
							Int3 pos = new Int3(cube.NavPosition);
							NNInfo info = this.IsometricGraph.GetNearest(cube.NavPosition);
							if ((info.node != null) && (info.node.position == pos))
							{
								node = info.node as PointNode;
							}
							else
							{
								this.IsometricGraph.AddNode(pos);
								node.Walkable = true;
								pointsAdded++;
							}
							cube.NavGridIndex = node.NodeIndex;
							cubeNavLinks.Add( node.NodeIndex, new CubeNavLink(node, cube) );
						}

						BoxCollider	box = cube.gameObject.GetComponent<BoxCollider>();
						if (box != null)
						{
							box.enabled = true;
						}
					}
					else if (cube != null)
					{
						cube.IsNavigable = false;
					}
				}
			}
		}

		//Debug.LogError("SetupIsometricNavPoints pointsAdded=" + pointsAdded);
	}

	private void SetupSide2dNavPoints(bool tagOnly = false)
	{
		int pointsAdded = 0;

		for (int i=0; i<this.rows.Length; i++)
		{
			GridPuzzleCubeRow row = this.rows[i];
			if ((row != null) && this.IsTopRow(row) && row.IsNavigable)
			{
				if (tagOnly)
				{
					row.CreateNavPoint();
				}
				else
				{
					PointNode node = null;
					Int3 pos = new Int3(row.NavPosition);
					NNInfo info = this.Side2dGraph.GetNearest(row.NavPosition);
					if ((info.node != null) && (info.node.position == pos))
					{
						node = info.node as PointNode;
					}
					else
					{
						node = this.Side2dGraph.AddNode(pos);
						node.Walkable = true;
						pointsAdded++;
					}
					row.NavGridIndex = node.NodeIndex;
					rowNavLinks.Add( node.NodeIndex, new CubeRowNavLink(node, row) );
				}
			}
			else if (row != null)
			{
				row.IsNavigable = false;
			}
		}

		//Debug.LogError("SetupSide2dNavPoints pointsAdded=" + pointsAdded);
	}

	private int LinkCubes(GridPuzzleCube cube, PointNode cubeNode, List<GridPuzzleCube> linkedCubes, float cost)
	{
		int cubesLinked = 0;
		for (int j=0; j<linkedCubes.Count; j++)
		{
			GridPuzzleCube linkedCube = linkedCubes[j];
			PointNode linkedNode = this.cubeNavLinks[linkedCube.NavGridIndex].node;

			if (cubeNode.connections != null)
			{
				if (cubeNode.ContainsConnection(linkedNode))
				{
					continue;
				}
			}
			else if (linkedNode == cubeNode)
			{
				continue;
			}

			uint wackyCost = (uint)Mathf.RoundToInt(cost*Int3.FloatPrecision);

			cubeNode.AddConnection(linkedNode, wackyCost);
			linkedNode.AddConnection(cubeNode, wackyCost);
			cubesLinked++;
			Vector3 p1 = (Vector3)cubeNode.position;
			Vector3 p2 = (Vector3)linkedNode.position;
			//Debug.Log("CubesLinked " + p1.x + "," + p1.y + "," + p1.z + " -> " + p2.x + "," + p2.y + "," + p2.z);
		}
		return cubesLinked;
	}

	private void LinkNavigableCubes()
	{
		int cubesSearched = 0;
		int cubesLinked = 0;
		int cubesLinkedPerspective = 0;

		for (int x=0; x<this.cubeGrid.GetLength(0); x++)
		{
			for (int y=0; y<this.cubeGrid.GetLength(1); y++)
			{
				for (int z=0; z<this.cubeGrid.GetLength(2); z++)
				{
					GridPuzzleCube cube = this.GetCube(x,y,z);
					if ((cube != null) && cube.IsNavigable && this.cubeNavLinks.ContainsKey(cube.NavGridIndex))
					{
						cubesSearched++;
						PointNode node = this.cubeNavLinks[cube.NavGridIndex].node;

						cubesLinked += this.LinkCubes(cube, node, this.GetNavNeighbours(cube), 1);

						cubesLinkedPerspective += this.LinkCubes(cube, node, this.GetPerspectiveAlignedCubes(cube), 3);
					}
				}
			}
		}

		Debug.LogError("LinkNavigableCubes cubesSearched=" + cubesSearched + " cubesLinked=" + cubesLinked + " cubesLinkedPerspective=" + cubesLinkedPerspective);
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
						List<GridPuzzleCube> neighbours = this.GetGeoNeighbours(cube);
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

	public bool HasRelativeNeighbour(GridPuzzleCube cube, Vector3 dir)
	{
		return (this.GetNeighbour(cube, dir) != null);
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
//		Debug.Log(name  
//					+ " " + cube.x + "," + cube.y + "," + cube.z 
//					+ " + " + dx + "," + dy + "," + dz 
//					+ " => " + x + "," + y + "," + z);
		return this.GetCube(x, y, z);
	}

	public List<GridPuzzleCube> GetGeoNeighbours(GridPuzzleCube cube)
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

	public List<GridPuzzleCube> GetNavNeighbours(GridPuzzleCube cube)
	{
		List<Vector3> dirs = new List<Vector3>();
		dirs.Add(Vector3.left);
		dirs.Add(Vector3.right);
		dirs.Add(Vector3.forward);
		dirs.Add(Vector3.back);

		List<GridPuzzleCube> navNeighbours = new List<GridPuzzleCube>();
		for (int i=0; i<dirs.Count; i++)
		{
			Vector3 dir = dirs[i];
			GridPuzzleCube cubeToAdd = null;

			cubeToAdd = this.GetNeighbour(cube, dir + Vector3.up);
			if ((cubeToAdd != null) && cubeToAdd.IsNavigable)
			{
				navNeighbours.Add(cubeToAdd);
				continue;
			}

			cubeToAdd = this.GetNeighbour(cube, dir);
			if ((cubeToAdd != null) && cubeToAdd.IsNavigable)
			{
				navNeighbours.Add(cubeToAdd);
				continue;
			}

			cubeToAdd = this.GetNeighbour(cube, dir + Vector3.down);
			if ((cubeToAdd != null) && cubeToAdd.IsNavigable)
			{
				navNeighbours.Add(cubeToAdd);
				continue;
			}
		}
		return navNeighbours;
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
