﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

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

	public float GridNodeWidth;
	public float GridNodeHeight;

	public float GridFloorHeight;
	public float GridFloorDepth;

	public int GridHeight;
	public int GridWidth;
	public int GridDepth;

	public Vector3 PuzzleBasePosition = Vector3.zero;

	public float PuzzleMoveSpeed = 1.0f;

	private float PuzzleHeight;
	private float PuzzleWidth;

	public GameObject playerPrefab;
	public GameObject positonPrefab;

	public GameObject [] puzzlePrefabs;

	public GameObject pathFollowerPrefab;

	private GridPuzzleActor player;
	private GridPuzzlePortal spawnPortal;
	private List<GridPuzzleActor> loadedActors = new List<GridPuzzleActor>();

	private Dictionary<PuzzlePosition, GridPuzzle> puzzlePositions = new Dictionary<PuzzlePosition, GridPuzzle>();
	private Dictionary<PuzzlePosition, GameObject> puzzlePositionObjects = new Dictionary<PuzzlePosition, GameObject>();
	private List<GridPuzzle> loadedPuzzles = new List<GridPuzzle>();

	public GridPuzzleCamera cam;
	public GridPuzzleCamera.Angle cameraAngle;
	public GameObject Side2dCamera;
	public GameObject IsometricCamera;

	public void SetCameraAngle(GridPuzzleCamera.Angle angle)
	{
		this.cameraAngle = angle;
		for (int i=0; i<this.loadedPuzzles.Count; i++)
		{
			this.loadedPuzzles[i].OnCameraAngleChange(angle);
		}

		for (int i=0; i<this.loadedActors.Count; i++)
		{
			this.loadedActors[i].OnCameraAngleChange(angle);
		}
	}

	void Awake() 
	{
		Instance = this;

		this.cameraAngle = GridPuzzleCamera.Angle.Isometric;

		this.PuzzleHeight = 1f * (float)this.GridHeight;
		this.PuzzleWidth = 1f * (float)this.GridWidth;
	}

	// Use this for initialization
	void Start () 
	{
		this.InitMessenger("GridPuzzleManager");

		this.puzzlePositionObjects[PuzzlePosition.Current] = 	GameObject.Instantiate(this.positonPrefab, Vector3.zero+this.PuzzleBasePosition, Quaternion.identity) as GameObject;
		this.puzzlePositionObjects[PuzzlePosition.Top] = 		GameObject.Instantiate(this.positonPrefab, new Vector3(0f, this.PuzzleHeight, 0f)+this.PuzzleBasePosition, Quaternion.identity) as GameObject;
		this.puzzlePositionObjects[PuzzlePosition.Bottom] = 	GameObject.Instantiate(this.positonPrefab, new Vector3(0f, -1f*this.PuzzleHeight, 0f)+this.PuzzleBasePosition, Quaternion.identity) as GameObject;
		this.puzzlePositionObjects[PuzzlePosition.Right] = 		GameObject.Instantiate(this.positonPrefab, new Vector3(this.PuzzleWidth, 0f, 0f)+this.PuzzleBasePosition, Quaternion.identity) as GameObject;
		this.puzzlePositionObjects[PuzzlePosition.Left] = 		GameObject.Instantiate(this.positonPrefab, new Vector3(-1f*this.PuzzleWidth, 0f, 0f)+this.PuzzleBasePosition, Quaternion.identity) as GameObject;

		this.LoadRequiredPuzzles();

		StartCoroutine(LateStart(0.1f));
	}

	IEnumerator LateStart(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		GridPuzzle current = this.GetPuzzle(PuzzlePosition.Current);
		if (current != null)
		{
			Transform currentSpawn = current.GetSpawn();
			this.spawnPortal = currentSpawn.gameObject.GetComponent<GridPuzzlePortal>();
			this.player = this.LoadActor(this.playerPrefab, current, currentSpawn);
			this.player.TeleportTo(currentSpawn.gameObject);

			this.SendMessengerMsg("PlayerSpawned", this.player);
		}
	}

	public GridPuzzleActor LoadActor(GameObject prefab, GridPuzzle current, Transform spawn)
	{
		//Debug.Log("LoadActor " + prefab.name + " pos=" + spawn.position.x + "," + spawn.position.y + "," + spawn.position.z);
		GameObject instance = GameObject.Instantiate(prefab, spawn.position, spawn.rotation) as GameObject;
		GridPuzzleActor actor = instance.GetComponent<GridPuzzleActor>();
		if (actor == null)
		{
			GameObject.Destroy(instance);
			return null;
		}

		GridPuzzlePlayerController player = instance.GetComponent<GridPuzzlePlayerController>();
		if (player != null)
		{
			player.parentPuzzle = current;
			player.transform.SetParent(current.transform);
		}

		this.loadedActors.Add(actor);

		this.spawnPortal.TriggerInFX();

		return actor;
	}

	public GridPuzzle GetCurrentPuzzle()
	{
		return this.GetPuzzle(PuzzlePosition.Current);
	}

	private GridPuzzle GetPuzzle(PuzzlePosition pos)
	{
		return this.puzzlePositions.ContainsKey(pos) ? this.puzzlePositions[pos] : null;
	}

	private Vector3 GetDesiredPosition(PuzzlePosition pos)
	{
		return this.puzzlePositionObjects.ContainsKey(pos) ? this.puzzlePositionObjects[pos].transform.position : Vector3.zero;
	}

	void Update() 
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

		puzzle.DiscoverCubes();

		if ((puzzle.spawnPoint != null) && (puzzle.exitPoint != null))
		{
			puzzle.exitPoint = puzzle.spawnPoint.GetComponent<GridPuzzlePortal>();
		}

		puzzle.OnCameraAngleChange(this.cameraAngle);

		this.loadedPuzzles.Add(puzzle);

		this.MakePosition(puzzle, positionOverride);

		return puzzle;
	}

	public void SpawnPathFollower(List<Transform> path, float speed = 1f, float verticalAdjustment=0f, GameObject toEnable=null)
	{
		List<Vector3> pathV = new List<Vector3>();
		for (int i=0; i<path.Count; i++)
		{
			pathV.Add(path[i].position);
		}

		this.SpawnPathFollower(pathV, speed, verticalAdjustment, toEnable);
	}

	public void SpawnPathFollower(List<Vector3> path, float speed = 1f, float verticalAdjustment=0f, GameObject toEnable=null)
	{
		//Debug.LogError("SpawnPathFollower prefab=" + (this.pathFollowerPrefab != null) + " path.Count=" + path.Count);
		if ((this.pathFollowerPrefab != null) && (path != null) && (path.Count > 0))
		{
			List<Vector3> adjustedPath = new List<Vector3>(path);
			for (int i=0; i<adjustedPath.Count; i++)
			{
				Vector3 pos = adjustedPath[i];
				adjustedPath[i].Set( pos.x, pos.y+verticalAdjustment, pos.z);
			}

			GameObject obj = GameObject.Instantiate(this.pathFollowerPrefab, adjustedPath[0], Quaternion.identity) as GameObject;
			GridPuzzlePathFollower follower = obj.GetComponent<GridPuzzlePathFollower>();
			if (follower == null)
			{
				GameObject.Destroy(obj);
				return;
			}

			follower.FollowPath(adjustedPath, speed, toEnable);
		}
	}

	public void ConnectPuzzle(GridPuzzle source, GridPuzzle destination)
	{
		source.exitPoint.target = destination.spawnPoint.GetComponent<GridPuzzlePortal>();
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
		if (GridPuzzleEditor.IsActive())
		{
			return;
		}

		if (!this.puzzlePositions.ContainsKey(PuzzlePosition.Current))
		{
			this.LoadPuzzle( this.PickRandomPrefab(this.puzzlePrefabs), PuzzlePosition.Current );
			this.SetupPathfinding( this.GetPuzzle(PuzzlePosition.Current) );
			this.SendMessengerMsg("CurrentPuzzleUpdated", this.GetPuzzle(PuzzlePosition.Current));
		}

//		if (!this.puzzlePositions.ContainsKey(PuzzlePosition.Top))
//		{
//			this.LoadPuzzle( this.PickRandomPrefab(this.puzzlePrefabs), PuzzlePosition.Top );
//			//this.SetupPathfinding( this.GetPuzzle(PuzzlePosition.Top) );
//			this.ConnectPuzzle(this.GetPuzzle(PuzzlePosition.Current), this.GetPuzzle(PuzzlePosition.Top));
//		}
	}

	private void SetupPathfinding(GridPuzzle puzzle)
	{
		if (puzzle == null)
		{
			return;
		}

		puzzle.Scan(AstarPath.active);

//		AstarPath.RegisterSafeUpdate (() => {
//			
//		});		
		//puzzle.SetupNavPoints(this.path.astarData.graphs[0], this.path.astarData.graphs[1]);
		//this.path.Scan();
		//puzzle.LinkPerspectiveAlignedCubes(this.path.astarData.pointGraph);
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
		if (!this.puzzlePositions.ContainsKey(oldPos))
		{
			return;
		}

		if (newPos == PuzzlePosition.None)
		{
			this.MarkPuzzleForUnload( this.puzzlePositions[oldPos] );
		}
		else
		{
			this.MakePosition(this.puzzlePositions[oldPos], newPos);
		}
		this.puzzlePositions.Remove(oldPos);
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

	public override void OnMessage(string id, object obj1, object obj2)
	{
		switch(id)
		{
		case "OnTeleportedTo":
			GridPuzzle puzzle = obj1 as GridPuzzle;
			GameObject obj = obj2 as GameObject;
			if (obj.GetComponent<GridPuzzlePlayerController>() != null)
			{
				if (puzzle.GetPosition() != PuzzlePosition.Current)
				{
					this.ChangePuzzle(puzzle.GetPosition());
					this.SendMessengerMsg("OnUpdatedPuzzlePositions");
				}
			}
			break;
		case "CameraPositionUpdate":
			{
				GridPuzzleCamera.Angle newAngle = (GridPuzzleCamera.Angle)obj1;
				this.SetCameraAngle(newAngle);
			}
			break;

		default:
				break;
		}
	}
}
