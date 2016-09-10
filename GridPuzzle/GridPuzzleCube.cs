using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DSTools;

public enum CubeMaterial
{
	None,
	Metal,
	Plastic,
	Glass,
	Ice,
	Water,
	Organic,
	Dirt,
	Stone,
	Effect,
	Hazzard
}

public class GridPuzzleCube : GridPuzzleNavigable 
{
	public BoxCollider box;

	public GameObject [] surfaces;

	public CubeMaterial cubeMaterial = CubeMaterial.None;

	public Material mat;

	private GridPuzzleVectorUIItem button = null;

	public bool IsTop
	{
		get
		{
			return (this.parentPuzzle != null) ? this.parentPuzzle.IsTopCube(this) : false;
		}
	}

	public int x;
	public int y;
	public int z;

	public Vector3 GridPositon;

	static public Vector3 GetSufaceNormal(GameObject surface)
	{
		return surface.transform.up;
	}

	void Awake()
	{
		this.box = this.gameObject.GetComponent<BoxCollider>();
		this.gameObject.layer = LayerMask.NameToLayer("Cube");
		this.parentPuzzle = this.gameObject.GetComponentInParent<GridPuzzle>();

		if (this.mat != null)
		{
			for (int i=0; i<this.surfaces.Length; i++) {
				Renderer rend = this.surfaces[i].GetComponent<Renderer>();
				if (rend != null)
				{
					rend.material = this.mat;
				}
			}
		}
	}

	void Start()
	{
		if (GridPuzzleEditor.IsActive())
		{
			this.InitMessenger("GridPuzzleCubeRow");
		}
		this.OnCameraAngleChange(this.angle);
		this.UpdateMagnetic();
	}

	public void CreateNavPoint()
	{
		Debug.LogError("Cube.CreateNavPoint");
		if ((this.NavPoint == null) && this.IsNavigable)
		{
			this.NavPoint = new GameObject("NavPoint_Iso");
			this.NavPoint.transform.position = this.NavPosition;
			this.NavPoint.transform.SetParent(this.transform);
			this.NavPoint.tag = "NavPoint_Iso";
		}
	}

	public void UpdateCollider()
	{		
		if (this.box != null)
		{
			bool enabledForAngle = (angle == GridPuzzleCamera.Angle.Isometric);
			bool enabled = this.IsTop && enabledForAngle;
			if (this.box.enabled != enabled)
			{
				this.box.enabled = enabled;
			}
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateMagnetic();
		UpdateCollider();

		/*if (GridPuzzleEditor.IsActive() && (this.angle == GridPuzzleCamera.Angle.Isometric))
		{
			if (this.button == null)
			{
				this.button = this.gameObject.AddComponent<GridPuzzleVectorUIItem>();
				this.button.editorAction = GridPuzzleEditorAction.RemoveCube;
				this.button.text = "-";
				this.button.size = 0.1f;
			}

			this.button.position = Camera.main.WorldToViewportPoint(this.NavPosition);
		}
		else if ((this.angle == GridPuzzleCamera.Angle.Isometric))
		{
			if (this.button == null)
			{
				this.button = this.gameObject.AddComponent<GridPuzzleVectorUIItem>();
				this.button.gameplayAction = GridPuzzleGameplayAction.MoveToCube;
				this.button.text = "*";
				this.button.size = 0.05f;
			}

			this.button.position = Camera.main.WorldToViewportPoint(this.NavPosition);
		}
		else if (this.button != null)
		{
			this.button.CloseAndDestroy();
			GameObject.DestroyObject(this.button);
			this.button = null;
		}*/
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

	public Vector3 EvaluateMagneticGravity(GridPuzzlePlayerController controller, bool defaultToGravity=true)
	{
		return Vector3.down;//DSTools.SurfaceComponent.EvaluateMagneticGravity(controller.currentCharge, this.floorCharge, this.ceilingCharge, defaultToGravity);
	}

	public bool RemoveSharedSurfaces(GridPuzzleCube otherCube)
	{
		if (this.surfaces == null)
		{
			return false;
		}

		int remainingCount = 0;
		for (int i=0; i<this.surfaces.Length; i++)
		{
			GameObject s = this.surfaces[i];
			if (s != null)
			{
				Vector3 dirToSurface = (s.transform.position - this.transform.position).normalized;
				Vector3 dirToOtherCube = (otherCube.transform.position - this.transform.position).normalized;
				bool isShared = (dirToSurface == dirToOtherCube);

				if (isShared)
				{
					this.surfaces[i] = null;
					GameObject.Destroy(s);
				}
				else
				{
					remainingCount++;
				}
			}
		}

		return (remainingCount == 0);
	}

	static public Vector3 GetBoxSize(GameObject obj)
	{
		BoxCollider thisBox = obj.GetComponent<BoxCollider>();
		return (thisBox != null) ? thisBox.size : Vector3.one;
	}

	static public GridPuzzleCube GeneratePrefab(GameObject prefab, Vector3 position, int gridX, int gridY, int gridZ)
	{
		GameObject cubeObj = GameObject.Instantiate(prefab, position, Quaternion.identity) as GameObject;
		GridPuzzleCube cube = cubeObj.GetComponent<GridPuzzleCube>();
		if (cube == null)
		{
			GameObject.Destroy(cubeObj);
			return null;
		}

		BoxCollider cubeBox = cubeObj.GetComponent<BoxCollider>(); 
		cubeBox.size = Vector3.one;

		cube.x = gridX;
		cube.y = gridY;
		cube.z = gridZ;
		cube.GridPositon = new Vector3(gridX, gridY, gridZ);

		return cube;
	}

	static public GridPuzzleCube GeneratePrefab(GridPuzzle.Settings settings, Vector3 position, int gridX, int gridY, int gridZ)
	{
		return GeneratePrefab(settings.PickRandomPrefab(settings.cubePrefabs), position, gridX, gridY, gridZ);
	}

	void OnTriggerEnter(Collider other)
    {
		//this means death...i would assume
    }


	public void OnCameraAngleChange(GridPuzzleCamera.Angle angle)
	{
		this.angle = angle;
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{
		if (id == "GridPuzzleEditorAction")
		{
			GridPuzzleEditorAction action = (GridPuzzleEditorAction)obj1;

			switch(action)
			{
			case GridPuzzleEditorAction.RemoveCube:
				GameObject obj = obj2 as GameObject;
				if (obj == this.gameObject)
				{
					this.Destroy();
				}
				break;
			default:
				break;
			}
		}
	}
}
