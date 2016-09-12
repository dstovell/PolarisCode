﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleCameraSettings
{
	public float orthographicSize;
	public float orthographicSizeMax;
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

	private bool isUpdating = false;
	private float updateTime = 0f;
	private bool isUpdatingManually = false;
	private float manualAngleT = 0f;

	public CameraPerspectiveEditor editor; 

	public Camera cam;

	public GridPuzzlePlayerController player;
	private float playerStartX;
	private float playerStartY;

	private Dictionary<GridPuzzleCamera.Angle,GridPuzzleCameraSettings> settings;

	void Awake()
	{
		this.settings = new Dictionary<GridPuzzleCamera.Angle,GridPuzzleCameraSettings>();

		this.settings[Angle.Side2D] = new GridPuzzleCameraSettings();
		this.settings[Angle.Side2D].orthographicSize = 3.35f;
		this.settings[Angle.Side2D].orthographicSizeMax = 8.35f;
		this.settings[Angle.Side2D].cameraPosition = new Vector3(-0.52f, -0.28f, -8f);
		this.settings[Angle.Side2D].cameraRotation = Quaternion.identity;
		this.settings[Angle.Side2D].nearPlane = 0.1f;
		this.settings[Angle.Side2D].farPlane = 50f;
		this.settings[Angle.Side2D].verticalLensShift = 0f;

		this.settings[Angle.Isometric] = new GridPuzzleCameraSettings();
		this.settings[Angle.Isometric].orthographicSize = 3.8f;
		this.settings[Angle.Isometric].orthographicSizeMax = 8.8f;
		this.settings[Angle.Isometric].cameraPosition = new Vector3(-4.84f, 3f, -4.96f);
		this.settings[Angle.Isometric].cameraRotation = Quaternion.Euler(new Vector3(0f, 45f, 0f));
		this.settings[Angle.Isometric].nearPlane = 0.1f;
		this.settings[Angle.Isometric].farPlane = 50f;
		this.settings[Angle.Isometric].verticalLensShift = -0.7f;
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

	public Vector3 GetCharacterScreenPos()
	{
		if (this.player == null)
		{
			return Vector3.zero;
		}
		return editor.WorldToViewportPoint(this.player.transform.position);
	}

	public Ray ScreenPointToRay(Vector2 screenPoint)
	{
		return editor.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y));
	}

	private void AssignPlayer(GridPuzzlePlayerController _player)
	{
		this.player = _player;
		this.playerStartX = this.player.transform.position.x;
		this.playerStartY = this.player.transform.position.y;
	}
	
	// Update is called once per frame
	void Update()
	{
		if (this.desiredAngle != this.currentAngle) 
		{
			if (!this.isUpdating)
			{
				this.isUpdating = true;
				this.updateTime = 0.001f;
			}

			float t = this.IsManualCamera() ? Mathf.Abs(this.manualAngleT) : this.updateTime/this.TransitionTimeSeconds;
			Debug.Log("this.updateTime=" + this.updateTime + " t=" + t);

			UpdateCameraAngle(this.currentAngle, this.desiredAngle, t);
			UpdateCameraZoom(this.currentAngle, this.desiredAngle, t);
			UpdateCameraPosition(this.currentAngle, this.desiredAngle, t);

			if (t >= 1.0f)
			{
				this.isUpdating = false;
				this.isUpdatingManually = false;
				this.currentAngle = this.desiredAngle;

				this.SendMessengerMsg("CameraPositionUpdate", this.currentAngle);
			}
			else if (t <= 0.0f)
			{
				this.isUpdating = false;
				this.isUpdatingManually = false;
				this.desiredAngle = this.currentAngle;

				this.SendMessengerMsg("CameraPositionUpdate", this.currentAngle);
			}
			else 
			{
				if (!this.IsManualCamera())
				{
					bool isMovingBack = ((Mathf.Abs(this.manualAngleT) < 0.5f) && !GridPuzzleEditor.IsActive());

					this.updateTime += isMovingBack ? (-1f*Time.deltaTime) : Time.deltaTime;
					this.updateTime = Mathf.Min(this.updateTime, this.TransitionTimeSeconds);
					this.updateTime = Mathf.Max(this.updateTime, 0f);
				}
			}

		}
		else 
		{
			if (!this.IsManualCamera())
			{
				UpdateCameraZoom(this.currentAngle);
			}
			bool isPlayerMoving = ((this.player != null) && this.player.IsMoving());
			if (!isPlayerMoving)
			{
				UpdateCameraPosition(this.currentAngle);
			}
		}
	}

	public void OnManualZoom(float deltaScale)
	{
		if (GridPuzzleEditor.IsActive())
		{
			return;
		}

		if (this.cam != null)
		{
			float orthographicSizeCurrent = this.cam.orthographicSize;
			float orthographicSizeMax = this.settings[this.currentAngle].orthographicSizeMax;

			float orthographicSizeTarget = orthographicSizeCurrent + (orthographicSizeCurrent * (1.0f - deltaScale));

			this.cam.orthographicSize = Mathf.Min(orthographicSizeTarget, orthographicSizeMax);

			this.isUpdatingManually = true;
		}
	}

	public void OnManualRotate(float deltaT)
	{
		if (GridPuzzleEditor.IsActive())
		{
			return;
		}

		if (this.desiredAngle != this.currentAngle)
		{
			this.manualAngleT += 2f*deltaT;

			if (this.manualAngleT > 0.98f) 
			{
				this.manualAngleT = 1f;
			}
			else if (this.manualAngleT < -0.98f) 
			{
				this.manualAngleT = -1f;
			}
			else if (Mathf.Abs(this.manualAngleT) < 0.02)
			{
				this.manualAngleT = 0f;
			}
		}
		else if ((this.currentAngle == Angle.Side2D) && (deltaT > 0.0f))
		{
			//Debug.LogError("Moving To Isometric");
			this.desiredAngle = Angle.Isometric;
			this.manualAngleT = 0.05f;
			this.isUpdatingManually = true;
		}
		else if ((this.currentAngle == Angle.Isometric) && (deltaT < 0.0f))
		{
			//Debug.LogError("Moving To Side2D");
			this.desiredAngle = Angle.Side2D;
			this.manualAngleT = -0.05f;
			this.isUpdatingManually = true;
		}
	}

	public void OnEndManualInput()
	{
		this.isUpdatingManually = false;
		float absAngleT = Mathf.Abs(this.manualAngleT);
		this.updateTime = absAngleT*this.TransitionTimeSeconds;
	}

	public bool IsManualCamera()
	{
		return this.isUpdatingManually;
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

			this.cam.transform.rotation = Quaternion.Lerp(_from.cameraRotation, _to.cameraRotation, t);

			this.editor.lensShift = Vector3.Lerp(new Vector2(0f, _from.verticalLensShift), new Vector2(0f, _to.verticalLensShift), t);

			//this.cam.nearClipPlane = Mathf.Lerp(_from.nearPlane, _to.nearPlane, t);
			//this.cam.farClipPlane = Mathf.Lerp(_from.farPlane, _to.farPlane, t);
		}
	}

	public void UpdateCameraZoom(GridPuzzleCamera.Angle fromAngle, GridPuzzleCamera.Angle toAngle = GridPuzzleCamera.Angle.None, float t = 0f)
	{
		GridPuzzleCameraSettings _from = this.settings.ContainsKey(fromAngle) ? this.settings[fromAngle] : null;
		GridPuzzleCameraSettings _to = this.settings.ContainsKey(toAngle) ? this.settings[toAngle] : null;
		if (_from == null)
		{
			return;
		}

		if (this.cam != null)
		{
			if (_to != null)
			{
				this.cam.orthographicSize = Mathf.Lerp(_from.orthographicSize, _to.orthographicSize, t);
			}
			else if (this.cam.orthographicSize != _from.orthographicSize)
			{
				this.cam.orthographicSize = Mathf.MoveTowards(this.cam.orthographicSize, _from.orthographicSize, 5.0f*Time.deltaTime);
			}
		}
	}

	public float editorX = 0.0f;
	public float editorY = 0.0f;

	public void UpdateCameraPosition(GridPuzzleCamera.Angle fromAngle, GridPuzzleCamera.Angle toAngle = GridPuzzleCamera.Angle.None, float t = 0f)
	{
		GridPuzzleCameraSettings _from = this.settings.ContainsKey(fromAngle) ? this.settings[fromAngle] : null;
		GridPuzzleCameraSettings _to = this.settings.ContainsKey(toAngle) ? this.settings[toAngle] : null;
		if (_from == null)
		{
			return;
		}

		if (this.cam != null)
		{
			float deltaX = ((this.player != null) && this.player.gameObject.activeInHierarchy) ? (this.player.transform.position.x - this.playerStartX) : 0f;
			float deltaY = ((this.player != null) && this.player.gameObject.activeInHierarchy) ? (this.player.transform.position.y - this.playerStartY) : 0f;
			if (GridPuzzleEditor.IsActive())
			{
				deltaY = editorX;
				deltaY = editorY;
			}
			Vector3 fromFinal = _from.cameraPosition;
			fromFinal.y += deltaY;
			if (this.currentAngle == Angle.Isometric)
			{
				fromFinal.y += 0.25f*deltaX;
			}

			if (_to != null)
			{	
				Vector3 toFinal = _to.cameraPosition;
				toFinal.y += deltaY;

				this.cam.transform.position = Vector3.Lerp(fromFinal, toFinal, t);
			}
			else if (this.cam.transform.position != fromFinal)
			{
				this.cam.transform.position = Vector3.MoveTowards(this.cam.transform.position, fromFinal, 4.0f*Time.deltaTime);
			}
		}
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{
		if (id == "GridPuzzleUIAction")
		{
			GridPuzzleUIAction.Type action = (GridPuzzleUIAction.Type)obj1;

			switch(action)
			{
			case GridPuzzleUIAction.Type.Camera_Side2D:
				this.desiredAngle = Angle.Side2D;
				break;
			case GridPuzzleUIAction.Type.Camera_Isometric:
				this.desiredAngle = Angle.Isometric;
				break;
			case GridPuzzleUIAction.Type.Camera_Front2D:
				this.desiredAngle = Angle.Front2D;
				break;
			default:
				break;
			}
		}
		else if (id == "PlayerSpawned")
		{
			GridPuzzlePlayerController playerSpawned = (obj1 as GridPuzzleActor).gameObject.GetComponent<GridPuzzlePlayerController>();
			this.AssignPlayer(playerSpawned);
		}
	}

	public GridPuzzleCamera.Angle ToggleCamera()
	{
		this.desiredAngle = (this.desiredAngle == Angle.Side2D) ? Angle.Isometric : Angle.Side2D;
		Debug.LogError("ToggleCamera to " + this.desiredAngle);
		return this.desiredAngle;
	}
}

