using UnityEngine;
using System.Collections;

public class GridPuzzle : DSTools.MessengerListener 
{
	public class Settings
	{
		public float GridNodeWidth;
		public float GridNodeHeight;
		public int GridWidth;
		public int GridHeight;

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

	static public GridPuzzle GeneratePrefab(GameObject [] nodePrefabs, GridPuzzle.Settings settings)
	{
		GameObject puzzleObj = new GameObject("GridPuzzle");
		GridPuzzle puzzle = puzzleObj.AddComponent<GridPuzzle>();
		int groupHeight = 1;
		int groupWidth = settings.GridWidth;

		Vector3 basePosition = new Vector3(-0.5f*settings.PuzzleWidth, -1f*settings.GridHeight, 1f);
		for (int i=0; i<settings.GridHeight; i++)
		{
			Vector3 pos = basePosition + new Vector3(0f, (float)i*settings.GridNodeHeight, 0f);
			GridPuzzleNodeGroup newGroup = GridPuzzleNodeGroup.GeneratePrefab(nodePrefabs, settings, groupHeight, groupWidth, pos);
			newGroup.gameObject.transform.parent = puzzleObj.transform;
		}

		return puzzle;
	}
}
