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

	public bool GeneratePrefabNow = false;

	public void GeneratePrefab()
	{
		GridPuzzle.Settings settings = new GridPuzzle.Settings();
		settings.cubePrefabs = this.cubePrefabs;
		settings.teleporterPrefabs = this.teleporterPrefabs;
		settings.sideWallPrefabs = this.sideWallPrefabs;

		settings.GridHeight = this.GridHeight;
		settings.GridDepth = this.GridDepth;
		settings.GridWidth = this.GridWidth;

		settings.GridPlateauHeight = this.GridPlateauHeight;

		this.currentPuzzle = GridPuzzle.GeneratePrefab( settings );
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
}
