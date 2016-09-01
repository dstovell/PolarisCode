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
}

public class GridPuzzleCube : MessengerListener 
{
	public BoxCollider box;

	public GameObject [] surfaces;

	public GameObject NegitiveFX;
	public GameObject PositiveFX;

	public MagneticCharge charge = MagneticCharge.None;
	public CubeMaterial cubeMaterial = CubeMaterial.None;

	private MagneticCharge lastCharge = MagneticCharge.None;

	public Material mat;

	public Vector3 gridPosition;

	public Vector3 NavPosition
	{
		get
		{
			return this.gameObject.transform.position + 0.5f*Vector3.up;
		}
	}

	static public Vector3 GetSufaceNormal(GameObject surface)
	{
		return surface.transform.up;
	}

	void Awake()
	{
		this.box = this.gameObject.GetComponent<BoxCollider>();

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

		this.box.enabled = true;
		this.box.isTrigger = true;
	}

	public void SetCharge(MagneticCharge _charge)
	{
		this.charge = _charge;
	}

	public void UpdateChargeFX() 
	{
		if (this.charge != this.lastCharge)
		{
			if ((this.NegitiveFX != null) && (this.PositiveFX != null)) 
			{
				if (this.charge == MagneticCharge.Positive)
				{
					this.PositiveFX.SetActive(true);
					this.NegitiveFX.SetActive(false);
				}
				else if (this.charge == MagneticCharge.Negative)
				{
					this.PositiveFX.SetActive(false);
					this.NegitiveFX.SetActive(true);
				}
				else
				{
					this.PositiveFX.SetActive(false);
					this.NegitiveFX.SetActive(false);
				}
			}

			this.lastCharge = this.charge;
		}
	}

	// Use this for initialization
	void Start () 
	{
		UpdateChargeFX();
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateChargeFX();
	}

	public Vector3 EvaluateMagneticGravity(GridPuzzlePlayerController controller, bool defaultToGravity=true)
	{
		return Vector3.down;//DSTools.SurfaceComponent.EvaluateMagneticGravity(controller.currentCharge, this.floorCharge, this.ceilingCharge, defaultToGravity);
	}

	static public Vector3 GetBoxSize(GameObject obj)
	{
		BoxCollider thisBox = obj.GetComponent<BoxCollider>();
		return (thisBox != null) ? thisBox.size : Vector3.one;
	}

	static public GridPuzzleCube GeneratePrefab(GridPuzzle.Settings settings, Vector3 position)
	{
		GameObject cubeObj = GameObject.Instantiate(settings.PickRandomPrefab(settings.cubePrefabs), position, Quaternion.identity) as GameObject;
		GridPuzzleCube cube = cubeObj.GetComponent<GridPuzzleCube>();
		if (cube == null)
		{
			GameObject.Destroy(cubeObj);
			return null;
		}

		BoxCollider cubeBox = cubeObj.GetComponent<BoxCollider>(); 
		cubeBox.size = Vector3.one;

		return cube;
	}

	void OnMouseDown() 
	{
		Debug.Log("GridPuzzleCube.OnMouseDown CubeSelected");
		this.SendMessengerMsg("CubeSelected", this);
    }

	void OnTriggerEnter(Collider other)
    {
		//this means death...i would assume
    }


	public void OnCameraAngleChange(GridPuzzleCamera.Angle angle)
	{
		if ((angle == GridPuzzleCamera.Angle.Side2D) || (angle == GridPuzzleCamera.Angle.Front2D))
		{
			//this.box.enabled = false;
			this.box.isTrigger = true;
		}
		else if (angle == GridPuzzleCamera.Angle.Isometric)
		{
			//this.box.enabled = true;
			this.box.isTrigger = false;
		}
	}
}
