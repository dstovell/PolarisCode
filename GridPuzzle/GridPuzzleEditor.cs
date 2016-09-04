using UnityEngine;
using System.Collections;

public class GridPuzzleEditor : DSTools.MessengerListener  
{
	static public GridPuzzleEditor Instance = null;
	static public bool IsActive() { return ((Instance != null) && Instance.enabled); }

	public GameObject [] cubePrefabs;
	public GameObject [] teleporterPrefabs;
	public GameObject [] sideWallPrefabs;

	public int GridHeight;
	public int GridWidth;
	public int GridDepth;

	public int GridPlateauHeight;

	private GridPuzzle currentPuzzle;
	private int index = 0;

	void Awake() 
	{
		GridPuzzleEditor.Instance = this;
	}

	void Start() 
	{
		this.InitMessenger("GridPuzzleEditor");
	}

	public bool GeneratePrefabNow = false;

	public GridPuzzle.Settings GetSettings()
	{
		GridPuzzle.Settings settings = new GridPuzzle.Settings();
		settings.cubePrefabs = this.cubePrefabs;
		settings.teleporterPrefabs = this.teleporterPrefabs;
		settings.sideWallPrefabs = this.sideWallPrefabs;

		settings.GridHeight = this.GridHeight;
		settings.GridDepth = this.GridDepth;
		settings.GridWidth = this.GridWidth;

		settings.GridPlateauHeight = this.GridPlateauHeight;
		return settings;
	}

	public void GeneratePrefab()
	{
		this.currentPuzzle = GridPuzzle.GeneratePrefab( GetSettings() );
		this.currentPuzzle.gameObject.name = "GridPuzzle" + this.index;
		this.index++;
	}

	public void OptimizePuzzle()
	{
		if (this.currentPuzzle != null)
		{
			this.currentPuzzle.Optimize();
		}
	}

	public void SetCameraAngle(GridPuzzleCamera.Angle angle)
	{
		if ((this.currentPuzzle != null) && (this.currentPuzzle.currentAngle != angle))
		{
			this.currentPuzzle.OnCameraAngleChange(angle);
		}
	}

	public GridPuzzle.Stats GetStats()
	{
		return (this.currentPuzzle != null) ? this.currentPuzzle.GetStats() : new GridPuzzle.Stats();
	}
	
	void Update() 
	{
		if (this.GeneratePrefabNow)
		{
			this.GeneratePrefab();
			this.GeneratePrefabNow = false;
		}
	}

	void OnGUI()
	{
		
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{
		switch(id)
		{
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