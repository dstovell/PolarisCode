using UnityEngine;
using System.Collections;
using DSTools;

public class GridPuzzle : MessengerListener 
{
	public class Settings
	{
		public GameObject blankNodePrefab;
		public GameObject metalFloorPrefab;
		public GameObject plasticFloorPrefab;
		public GameObject glassFloorPrefab;
		public GameObject metalCeilingPrefab;
		public GameObject plasticCeilingPrefab;
		public GameObject glassCeilingPrefab;
		public GameObject [] backWallPrefabs;

		public GameObject [] nodePrefabs;
		public GameObject [] teleporterPrefabs;
		public GameObject [] sideWallPrefabs;

		public GameObject [] cubePrefabs;

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

		public float GridNodeWidth;
		public float GridNodeHeight;
		public int GridWidth;
		public int GridHeight;
		public int GridDepth;

		public float GridFloorHeight;
		public float GridFloorDepth;

		public float PuzzleHeight 
		{
			get
			{
		 		return this.GridNodeHeight * (float)this.GridHeight;
			}
		}

		public float PuzzleWidth 
		{
			get
			{
				return this.GridNodeWidth * (float)this.GridWidth;
			}
		}
	}

	public GameObject spawnPoint;
	public GridPuzzlePortal exitPoint;
	public GridPuzzleCubeRow [] rows;

	public GridPuzzleCamera.Angle currentAngle = GridPuzzleCamera.Angle.None;

	public GridPuzzleManager.PuzzlePosition postion = GridPuzzleManager.PuzzlePosition.None;
	public GridPuzzleManager.PuzzlePosition previousPosition = GridPuzzleManager.PuzzlePosition.None;

	private bool markedForDelete = false;

	// Use this for initialization
	void Start () 
	{
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

	/*static public GameObject AddSurface(GridPuzzle.Settings settings, SurfacePosition surfacePos, SurfaceMaterial mat, MagneticCharge charge, Vector3 pos, Vector3 size, GameObject parent)
	{
		GameObject obj = GameObject.Instantiate(settings.surfacePrefab, pos, Quaternion.identity) as GameObject;
		SurfaceComponent surface = obj.GetComponent<SurfaceComponent>();
		BoxCollider collider = obj.GetComponent<BoxCollider>();
		if (surface == null)
		{
			GameObject.Destroy(obj);
			return null;
		}

		surface.SurfacePos = surfacePos;
		surface.SetMaterial(mat);
		surface.SetCharge(charge);

		obj.transform.localScale = size;

		if (parent != null)
		{
			obj.transform.SetParent(parent.transform);
		}
		return obj;
	}*/

	static public GridPuzzle GeneratePrefab(GridPuzzle.Settings settings)
	{
		GameObject puzzleObj = new GameObject("GridPuzzle");
		puzzleObj.transform.position = Vector3.zero;
		GridPuzzle puzzle = puzzleObj.AddComponent<GridPuzzle>();
		int rowLength = settings.GridDepth;

		puzzle.rows = new GridPuzzleCubeRow[settings.GridWidth*settings.GridHeight];

		int widthOffset = -1 * Mathf.FloorToInt((float)settings.GridWidth/2f);
		int heightOffset = -1 * Mathf.FloorToInt((float)settings.GridHeight/2f);

		int rowIndex = 0;
		for (int j=0; j<settings.GridHeight; j++)
		{
			for (int i=0; i<settings.GridWidth; i++)
			{
				Vector3 pos = new Vector3(i+widthOffset, j+heightOffset, 0f);

				bool addCubes = (j == 0) || (j == (settings.GridHeight-1));

				GridPuzzleCubeRow newRow = GridPuzzleCubeRow.GeneratePrefab(settings, rowLength, pos, addCubes);
				newRow.gameObject.transform.SetParent(puzzleObj.transform);
				puzzle.rows[rowIndex] = newRow;
				rowIndex++;
			}
		}
		float portalHeight = 0.5f + (float)heightOffset;
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

	static public GridPuzzle GeneratePrefabOld(GridPuzzle.Settings settings)
	{
		GameObject puzzleObj = new GameObject("GridPuzzle");
		GridPuzzle puzzle = puzzleObj.AddComponent<GridPuzzle>();
		int groupHeight = 1;
		int groupWidth = settings.GridWidth;

		Vector3 basePosition = new Vector3(-0.5f*settings.PuzzleWidth, -1f*settings.GridHeight, 1f);
		GridPuzzlePortal lastPortal2 = null;
		for (int i=0; i<settings.GridHeight; i++)
		{
			Vector3 pos = basePosition + new Vector3(0f, (float)i*settings.GridNodeHeight, 0f);
			GridPuzzleNodeGroup newGroup = GridPuzzleNodeGroup.GeneratePrefab(settings, groupHeight, groupWidth, pos);
			newGroup.gameObject.transform.parent = puzzleObj.transform;

			Vector3 portal1Pos = new Vector3(-0.5f*settings.PuzzleWidth, pos.y-1.6f, 1f);
			GridPuzzlePortal portal1 = AddRandomPortal(settings, portal1Pos, newGroup.gameObject);
			Vector3 portal2Pos = new Vector3(0.5f*settings.PuzzleWidth-settings.GridNodeWidth, pos.y-1.6f, 1f);
			GridPuzzlePortal portal2 = AddRandomPortal(settings, portal2Pos, newGroup.gameObject);

			if (lastPortal2 == null)
			{
				puzzle.spawnPoint = portal1.gameObject;
				portal1.gameObject.name = "portalSpawn";
			}
			else
			{
				lastPortal2.target = portal2;
				portal1.target = puzzle.spawnPoint.GetComponent<GridPuzzlePortal>();
				puzzle.exitPoint = portal1;
				portal1.gameObject.name = "portalExit";
			}

			portal1.transform.rotation = Quaternion.LookRotation(Vector3.right, portal1.transform.up);
			portal2.transform.rotation = Quaternion.LookRotation(Vector3.left, portal1.transform.up);

			lastPortal2 = portal2;
		}

		Vector3 wall1Pos = new Vector3(-0.5f*settings.PuzzleWidth-1.7f, -0.5f*settings.PuzzleHeight, 0f);
		AddRandomPrefab(settings, settings.sideWallPrefabs, wall1Pos, puzzleObj);
		Vector3 wall2Pos = new Vector3(0.5f*settings.PuzzleWidth+0.2f, -0.5f*settings.PuzzleHeight, 0f);
		AddRandomPrefab(settings, settings.sideWallPrefabs, wall2Pos, puzzleObj);

		//AddSurface(settings, SurfacePosition.Floor, SurfaceMaterial.Metal, MagneticCharge.None, Vector3.zero, new Vector3(settings.GridNodeWidth, settings.GridFloorHeight, settings.GridFloorDepth), puzzleObj);

		return puzzle;
	}
}
