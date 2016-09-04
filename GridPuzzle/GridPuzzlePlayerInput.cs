using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript;
using TouchScript.Gestures;

public class GridPuzzlePlayerInput : DSTools.MessengerListener
{
	public GridPuzzlePlayerController player;

	private GridPuzzleCamera cam;

	// Use this for initialization
	void Start ()
	{
		this.InitMessenger("GridPuzzlePlayerInput");
		if (this.cam == null)
		{
			this.cam = Camera.main.gameObject.GetComponent<GridPuzzleCamera>();
		}

		if (this.player == null)
		{
			this.player = this.gameObject.GetComponent<GridPuzzlePlayerController>();
		}
	}
	
	// Update is called once per frame
	void Update() 
	{
	}

	private void OnEnable()
    {
		if (this.cam == null)
		{
			this.cam = Camera.main.gameObject.GetComponent<GridPuzzleCamera>();
		}

		FlickGesture [] flicks = this.cam.gameObject.GetComponents<FlickGesture>();
		for (int i=0; i<flicks.Length; i++)
		{
			Debug.LogError("PlayerInput flick=" + flicks[i].name);
			flicks[i].Flicked += this.FlickedHandler;
		}

		TapGesture [] taps = this.cam.gameObject.GetComponents<TapGesture>();
		for (int i=0; i<taps.Length; i++)
		{
			Debug.LogError("PlayerInput tap=" + taps[i].name);
			taps[i].Tapped += this.TapHandler;
		}
    }

    private void OnDisable()
    {
		if (this.cam == null)
		{
			return;
		}

		FlickGesture [] flicks = this.cam.gameObject.GetComponents<FlickGesture>();
		for (int i=0; i<flicks.Length; i++)
		{
			flicks[i].Flicked -= this.FlickedHandler;
		}

		TapGesture [] taps = this.cam.gameObject.GetComponents<TapGesture>();
		for (int i=0; i<taps.Length; i++)
		{
			taps[i].Tapped -= this.TapHandler;
		}
    }

	private void FlickedHandler(object sender, EventArgs e)
	{
		Debug.LogError("FlickedHandler");
		FlickGesture gesture = sender as FlickGesture;
		if (gesture != null)
		{
			//Debug.LogError("FlickedHandler got FlickGesture Direction=" + gesture.Direction.ToString() + " ScreenFlickVector=" + gesture.ScreenFlickVector.x + "," + gesture.ScreenFlickVector.y);
			if (Mathf.Abs(gesture.ScreenFlickVector.x) >= Mathf.Abs(gesture.ScreenFlickVector.y))
			{
				if (gesture.ScreenFlickVector.x > 0)
				{
					//Right
					//Debug.LogError("Right");
					DSTools.Messenger.SendMessageFrom("GridPuzzleUIAction", "GridPuzzleAction", GridPuzzleAction.Camera_Isometric);
				}
				else
				{
					//Left
					//Debug.LogError("Left");
					DSTools.Messenger.SendMessageFrom("GridPuzzleUIAction", "GridPuzzleAction", GridPuzzleAction.Camera_Side2D);
				}
			}
			else
			{
				if (gesture.ScreenFlickVector.y > 0)
				{
					//Up
				}
				else
				{
					//Down
				}
			}
		}
	}

	private void TapHandler(object sender, EventArgs e)
	{
		TapGesture gesture = sender as TapGesture;
		if (gesture != null)
		{
			Vector2 tapPoint = gesture.ScreenPosition;

			RaycastHit hitPoint = new RaycastHit();
			Ray ray = this.cam.ScreenPointToRay(tapPoint);
			float rayDistance = 100f;
			string [] maskStrings = new string[2]{"Cube","CubeRow"};
			LayerMask mask = LayerMask.GetMask(maskStrings);

	        // Optimize this later with length and mask
			if (Physics.Raycast(ray, out hitPoint, rayDistance, mask))
	        {
	        	GameObject obj = hitPoint.collider.gameObject;
				Debug.LogError("Hit " + obj.name);
				GridPuzzleCube cube = obj.GetComponent<GridPuzzleCube>();
				GridPuzzleCubeRow cubeRow = obj.GetComponent<GridPuzzleCubeRow>();

				if (cube != null)
				{
					Debug.Log("Hit cube " + cube.name);
					player.MoveTo(cube);
				}
				else if (cubeRow != null)
				{
					Debug.Log("Hit row " + cubeRow.name);
					player.MoveTo(cubeRow);
				}
			}
		}
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{
		GridPuzzleCube cube = null;

		switch(id)
		{
		case "GridPuzzleGameplayAction":
			GridPuzzleGameplayAction action = (GridPuzzleGameplayAction)obj1;
			GameObject obj = obj2 as GameObject;
			Debug.Log("GridPuzzlePlayerInput.OnMessage id=" + id + " action=" + action.ToString() + " name=" + obj.name);
			if (action == GridPuzzleGameplayAction.MoveToCube)
			{
				cube = obj.GetComponent<GridPuzzleCube>();
				if (this.player != null)
				{
					player.MoveTo(cube);
				}
			}
			else if (action == GridPuzzleGameplayAction.MoveToCubeRow)
			{
				GridPuzzleCubeRow cubeRow = obj.GetComponent<GridPuzzleCubeRow>();
				if (this.player != null)
				{
					player.MoveTo(cubeRow);
				}
			}
			break;

		case "PlayerSpawned":
			this.player = obj1 as GridPuzzlePlayerController;
			break;
		}
	}
}

