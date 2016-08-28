using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleManager : DSTools.MessengerListener 
{
	static public GridPuzzleManager Instance;

	public enum PuzzlePosition
	{
		None,

		Current,
		Top,
		Bottom,
		Left,
		Right
	}

	public bool GeneratePrefabNow = false;

	public float GridNodeWidth;
	public float GridNodeHeight;
	public int GridWidth;
	public int GridHeight;

	public float PuzzleMoveSpeed = 1.0f;

	private float PuzzleHeight;
	private float PuzzleWidth;

	public GameObject playerPrefab;
	public GameObject positonPrefab;
	public GameObject [] puzzlePrefabs;
	public GameObject [] nodePrefabs;
	public GameObject [] nodeGroupPrefabs;
	public GameObject [] teleporterPrefabs;
	public GameObject [] sideWallPrefabs;

	private GridPuzzleActor player;
	private GridPuzzlePortal spawnPortal;
	private List<GridPuzzleActor> loadedActors = new List<GridPuzzleActor>();

	private Dictionary<PuzzlePosition, GridPuzzle> puzzlePositions = new Dictionary<PuzzlePosition, GridPuzzle>();
	private Dictionary<PuzzlePosition, GameObject> puzzlePositionObjects = new Dictionary<PuzzlePosition, GameObject>();
	private List<GridPuzzle> loadedPuzzles = new List<GridPuzzle>();

	void Awake() 
	{
		Instance = this;

		this.PuzzleHeight = this.GridNodeHeight * (float)this.GridHeight;
		this.PuzzleWidth = this.GridNodeWidth * (float)this.GridWidth;
	}

	// Use this for initialization
	void Start () 
	{
		this.puzzlePositionObjects[PuzzlePosition.Current] = GameObject.Instantiate(this.positonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		this.puzzlePositionObjects[PuzzlePosition.Top] = GameObject.Instantiate(this.positonPrefab, new Vector3(0f, this.PuzzleHeight, 0f), Quaternion.identity) as GameObject;
		this.puzzlePositionObjects[PuzzlePosition.Bottom] = GameObject.Instantiate(this.positonPrefab, new Vector3(0f, -1f*this.PuzzleHeight, 0f), Quaternion.identity) as GameObject;
		this.puzzlePositionObjects[PuzzlePosition.Right] = GameObject.Instantiate(this.positonPrefab, new Vector3(this.PuzzleWidth, 0f, 0f), Quaternion.identity) as GameObject;
		this.puzzlePositionObjects[PuzzlePosition.Left] = GameObject.Instantiate(this.positonPrefab, new Vector3(-1f*this.PuzzleWidth, 0f, 0f), Quaternion.identity) as GameObject;

		this.LoadRequiredPuzzles();

		GridPuzzle current = this.GetPuzzle(PuzzlePosition.Current);
		if (current != null)
		{
			Transform currentSpawn = current.GetSpawn();
			this.spawnPortal = currentSpawn.gameObject.GetComponent<GridPuzzlePortal>();
			this.player = this.LoadActor(this.playerPrefab, currentSpawn);
		}
	}

	public GridPuzzleActor LoadActor(GameObject prefab, Transform spawn)
	{
		//Debug.Log("LoadActor " + prefab.name + " pos=" + spawn.position.x + "," + spawn.position.y + "," + spawn.position.z);
		GameObject instance = GameObject.Instantiate(prefab, spawn.position, spawn.rotation) as GameObject;
		GridPuzzleActor actor = instance.GetComponent<GridPuzzleActor>();
		if (actor == null)
		{
			GameObject.Destroy(instance);
			return null;
		}

		this.loadedActors.Add(actor);

		this.spawnPortal.TriggerInFX();

		return actor;
	}

	private GridPuzzle GetPuzzle(PuzzlePosition pos)
	{
		return this.puzzlePositions.ContainsKey(pos) ? this.puzzlePositions[pos] : null;
	}

	private Vector3 GetDesiredPosition(PuzzlePosition pos)
	{
		return this.puzzlePositionObjects.ContainsKey(pos) ? this.puzzlePositionObjects[pos].transform.position : Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () 
	{
		bool allInPosition = true;

		for (int i=0; i<loadedPuzzles.Count; i++)
		{
			GridPuzzle puzzle = loadedPuzzles[i];
			Vector3 desiredPos = Vector3.zero;
			if (puzzle.GetPosition() != PuzzlePosition.None)
			{
				desiredPos =  this.GetDesiredPosition( puzzle.GetPosition() );
			}

			if (!puzzle.IsInPosition(desiredPos))
			{
				allInPosition = false;
				puzzle.MoveTowards(desiredPos, this.PuzzleMoveSpeed);
			}
			else if (puzzle.IsMarkedForDelete())
			{
				this.UnloadPuzzle(puzzle);
			}
		}

		if (allInPosition)
		{
			this.LoadRequiredPuzzles();
		}

		if (this.GeneratePrefabNow)
		{
			this.GeneratePrefab();
			this.GeneratePrefabNow = false;
		}
	}

	public void GeneratePrefab()
	{
		GridPuzzle.Settings settings = new GridPuzzle.Settings();
		settings.nodePrefabs = this.nodePrefabs;
		settings.teleporterPrefabs = this.teleporterPrefabs;
		settings.sideWallPrefabs = this.sideWallPrefabs;

		settings.GridHeight = this.GridHeight;
		settings.GridWidth = this.GridWidth;
		settings.GridNodeHeight = this.GridNodeHeight;
		settings.GridNodeWidth = this.GridNodeWidth;

		GridPuzzle.GeneratePrefab( settings );
	}

	public GridPuzzle LoadPuzzle(GameObject prefab, PuzzlePosition positionOverride)
	{
		//Debug.Log("LoadPuzzel " + prefab.name + " in position " + positionOverride.ToString());

		GameObject instance = GameObject.Instantiate(prefab, this.GetDesiredPosition(positionOverride), Quaternion.identity) as GameObject;
		GridPuzzle puzzle = instance.GetComponent<GridPuzzle>();
		if (puzzle == null)
		{
			Debug.LogError("LoadPuzzel " + prefab.name + " has no GridPuzzle");
			GameObject.Destroy(instance);
			return null;
		}

		this.loadedPuzzles.Add(puzzle);

		this.MakePosition(puzzle, positionOverride);

		return puzzle;
	}

	private void MarkPuzzleForUnload(GridPuzzle puzzle)
	{
		puzzle.MarkForDelete();
	}

	private void UnloadPuzzle(GridPuzzle puzzle)
	{
		this.loadedPuzzles.Remove(puzzle);
		GameObject.Destroy(puzzle.gameObject);
	}

	private void LoadRequiredPuzzles()
	{
		/*foreach(PuzzlePosition pos in System.Enum.GetValues(typeof(PuzzlePosition)))
		{
			if (pos > PuzzlePosition.None)
			{
				if (!this.puzzlePositions.ContainsKey(pos))
				{
					this.LoadPuzzle( this.PickRandomPrefab(this.puzzlePrefabs), pos );
				}
			}
		}*/

		if (!this.puzzlePositions.ContainsKey(PuzzlePosition.Current))
		{
			this.LoadPuzzle( this.PickRandomPrefab(this.puzzlePrefabs), PuzzlePosition.Current );
		}
	}

	private void MakePosition(GridPuzzle puzzle, PuzzlePosition pos)
	{
		if (pos != PuzzlePosition.None)
		{
			this.puzzlePositions[pos] = puzzle;
			puzzle.SetPosition(pos);
		}
	}

	private void ChangePosition(PuzzlePosition oldPos, PuzzlePosition newPos)
	{
		if (newPos == PuzzlePosition.None)
		{
			this.MarkPuzzleForUnload( this.puzzlePositions[oldPos] );
			this.puzzlePositions.Remove(oldPos);
		}
		else
		{
			this.MakePosition(this.puzzlePositions[oldPos], newPos);
		}
	}

	private void ChangePuzzle(PuzzlePosition newPuzzlePosition)
	{
		switch(newPuzzlePosition)
		{
		case PuzzlePosition.Top:
			this.ChangePosition(PuzzlePosition.Current, PuzzlePosition.Bottom);
			this.ChangePosition(PuzzlePosition.Top, PuzzlePosition.Current);
			this.ChangePosition(PuzzlePosition.Left, PuzzlePosition.None);
			this.ChangePosition(PuzzlePosition.Right, PuzzlePosition.None);
			break;
		case PuzzlePosition.Bottom:
			this.ChangePosition(PuzzlePosition.Current, PuzzlePosition.Top);
			this.ChangePosition(PuzzlePosition.Bottom, PuzzlePosition.Current);
			this.ChangePosition(PuzzlePosition.Left, PuzzlePosition.None);
			this.ChangePosition(PuzzlePosition.Right, PuzzlePosition.None);
			break;
		case PuzzlePosition.Left:
			this.ChangePosition(PuzzlePosition.Current, PuzzlePosition.Right);
			this.ChangePosition(PuzzlePosition.Left, PuzzlePosition.Current);
			this.ChangePosition(PuzzlePosition.Top, PuzzlePosition.None);
			this.ChangePosition(PuzzlePosition.Bottom, PuzzlePosition.None);
			break;
		case PuzzlePosition.Right:
			this.ChangePosition(PuzzlePosition.Current, PuzzlePosition.Left);
			this.ChangePosition(PuzzlePosition.Right, PuzzlePosition.Current);
			this.ChangePosition(PuzzlePosition.Top, PuzzlePosition.None);
			this.ChangePosition(PuzzlePosition.Bottom, PuzzlePosition.None);
			break;
		default:
			break;
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
