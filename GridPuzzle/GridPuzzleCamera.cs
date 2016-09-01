using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleCameraSettings
{
	public float orthographicSize;
	public float nearPlane;
	public float farPlane;

	public Vector3 cameraPosition;
	public Quaternion cameraRotation;

	public float verticalLensShift;
}

public class GridPuzzleCamera : DSTools.MessengerListener
{
	public enum Angle
	{
		None,
		Side2D,
		Isometric,
		Front2D
	}

	public float TransitionTimeSeconds = 1.0f;

	public Angle desiredAngle = Angle.Side2D;
	private Angle currentAngle = Angle.None;

	private bool isUpdating;
	private float updateTime = 0f;

	public CameraPerspectiveEditor editor; 

	public Camera cam;

	public GameObject Side2DCursor;
	public GameObject IsometricCursor;

	private Dictionary<GridPuzzleCamera.Angle,GridPuzzleCameraSettings> settings;

	void Awake()
	{
		this.settings = new Dictionary<GridPuzzleCamera.Angle,GridPuzzleCameraSettings>();

		this.settings[Angle.Side2D] = new GridPuzzleCameraSettings();
		this.settings[Angle.Side2D].orthographicSize = 3.35f;
		this.settings[Angle.Side2D].cameraPosition = new Vector3(-0.52f, -0.28f, -8f);
		this.settings[Angle.Side2D].cameraRotation = Quaternion.identity;
		this.settings[Angle.Side2D].nearPlane = 0.1f;
		this.settings[Angle.Side2D].farPlane = 500f;
		this.settings[Angle.Side2D].verticalLensShift = 0f;

		this.settings[Angle.Isometric] = new GridPuzzleCameraSettings();
		this.settings[Angle.Isometric].orthographicSize = 2.84f;
		this.settings[Angle.Isometric].cameraPosition = new Vector3(-4.84f, 1.23f, -4.96f);
		this.settings[Angle.Isometric].cameraRotation = Quaternion.Euler(new Vector3(0f, 45f, 0f));
		this.settings[Angle.Isometric].nearPlane = 0.1f;
		this.settings[Angle.Isometric].farPlane = 500f;
		this.settings[Angle.Isometric].verticalLensShift = -0.53f;
	}

	// Use this for initialization
	void Start ()
	{
		this.InitMessenger("GridPuzzleCamera");
		if (this.cam == null) 
		{
			this.cam = Camera.main;
		}

		GridPuzzleCameraSettings to = this.settings[this.desiredAngle];
		if ((this.cam != null) && (to != null))
		{
			this.cam.orthographicSize = to.orthographicSize;
			this.cam.transform.position = to.cameraPosition;
			this.cam.transform.rotation = to.cameraRotation;
			this.cam.nearClipPlane = to.nearPlane;
			this.cam.farClipPlane = to.farPlane;

			this.editor.lensShift = new Vector2(0f, to.verticalLensShift);

			this.currentAngle = this.desiredAngle;
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		if (this.desiredAngle != this.currentAngle) 
		{
			if (!this.isUpdating)
			{
				this.isUpdating = true;
				this.updateTime = 0;

				this.Side2DCursor.SetActive(false);
				this.IsometricCursor.SetActive(false);
			}

			float t = this.updateTime/this.TransitionTimeSeconds;

			UpdateCameraAngle(this.currentAngle, this.desiredAngle, t);

			if (t < 1.0f)
			{
				this.updateTime += Time.deltaTime;
				this.updateTime = Mathf.Min(this.updateTime, this.TransitionTimeSeconds);
			}
			else 
			{
				this.isUpdating = false;
				this.currentAngle = this.desiredAngle;

				if (this.currentAngle == Angle.Front2D)
				{
					//this.Side2DCursor.SetActive(true);
				}
				else if (this.currentAngle == Angle.Isometric)
				{
					
					//this.IsometricCursor.SetActive(true);
				}
			}
		}
	}

	public void UpdateCameraAngle(GridPuzzleCamera.Angle fromAngle, GridPuzzleCamera.Angle toAngle, float t)
	{
		GridPuzzleCameraSettings _from = this.settings[fromAngle];
		GridPuzzleCameraSettings _to = this.settings[toAngle];
		if ((_from == null) || (_to == null))
		{
			return;
		}

		if (this.cam != null)
		{
			this.cam.orthographicSize = Mathf.Lerp(_from.orthographicSize, _to.orthographicSize, t);

			this.cam.transform.position = Vector3.Lerp(_from.cameraPosition, _to.cameraPosition, t);
			this.cam.transform.rotation = Quaternion.Lerp(_from.cameraRotation, _to.cameraRotation, t);

			this.editor.lensShift = Vector3.Lerp(new Vector2(0f, _from.verticalLensShift), new Vector2(0f, _to.verticalLensShift), t);

			//this.cam.nearClipPlane = Mathf.Lerp(_from.nearPlane, _to.nearPlane, t);
			//this.cam.farClipPlane = Mathf.Lerp(_from.farPlane, _to.farPlane, t);
		}
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{
		if (id == "GridPuzzleAction")
		{
			GridPuzzleAction action = (GridPuzzleAction)obj1;

			switch(action)
			{
			case GridPuzzleAction.Camera_Side2D:
				this.desiredAngle = Angle.Side2D;
				break;
			case GridPuzzleAction.Camera_Isometric:
				this.desiredAngle = Angle.Isometric;
				break;
			case GridPuzzleAction.Camera_Front2D:
				this.desiredAngle = Angle.Front2D;
				break;
			default:
				break;
			}
		}
	}

	void OnGUI()
	{
		if (!this.isUpdating)
		{
			if (GUI.Button(new Rect(15, 15, 60, 60), (this.desiredAngle == Angle.Side2D) ? "2D" : "ISO"))
	        {
				this.desiredAngle = (this.desiredAngle == Angle.Side2D) ? Angle.Isometric : Angle.Side2D;
	        }
		}
	}
}

