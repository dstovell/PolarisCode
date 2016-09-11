using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript;
using TouchScript.Gestures;

public class GridPuzzlePlayerInput : DSTools.MessengerListener
{
	public GridPuzzlePlayerController player;
	public GridPuzzleActor actor;

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

		if (this.actor == null)
		{
			this.actor = this.gameObject.GetComponent<GridPuzzleActor>();
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
			//Debug.LogError("PlayerInput flick=" + flicks[i].name);
			flicks[i].Flicked += this.FlickedHandler;
		}

		TapGesture [] taps = this.cam.gameObject.GetComponents<TapGesture>();
		for (int i=0; i<taps.Length; i++)
		{
			//Debug.LogError("PlayerInput tap=" + taps[i].name);
			taps[i].Tapped += this.TapHandler;
		}


		ScreenTransformGesture [] trans = this.cam.gameObject.GetComponents<ScreenTransformGesture>();
		for (int i=0; i<trans.Length; i++)
		{
			//Debug.LogError("PlayerInput ScreenTransformGesture=" + trans[i].name);
			trans[i].Transformed += this.TransformedHandler;
			trans[i].TransformCompleted += this.TransformCompletedHandler;
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

		TransformGesture [] trans = this.cam.gameObject.GetComponents<TransformGesture>();
		for (int i=0; i<trans.Length; i++)
		{
			trans[i].Transformed -= this.TransformedHandler;
			trans[i].TransformCompleted -= this.TransformCompletedHandler;
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
					//DSTools.Messenger.SendMessageFrom("GridPuzzleUIAction", "GridPuzzleUIAction", GridPuzzleUIAction.Type.Camera_Isometric);
				}
				else
				{
					//Left
					//Debug.LogError("Left");
					//DSTools.Messenger.SendMessageFrom("GridPuzzleUIAction", "GridPuzzleUIAction", GridPuzzleUIAction.Type.Camera_Side2D);
				}
			}
			else
			{
				if (gesture.ScreenFlickVector.y > 0)
				{
					this.actor.RequestJumpUp();
				}
				else
				{
					this.actor.RequestJumpDown();
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
				GridPuzzleCube cube = obj.GetComponent<GridPuzzleCube>();
				GridPuzzleCubeRow cubeRow = obj.GetComponent<GridPuzzleCubeRow>();

				if (cube != null)
				{
					//Debug.Log("Hit cube " + cube.name);
					this.actor.RequestMoveTo(cube);
				}
				else if (cubeRow != null)
				{
					//Debug.Log("Hit row " + cubeRow.name);
					this.actor.RequestMoveTo(cubeRow);
				}
			}
		}
	}

	private void TransformedHandler(object sender, EventArgs e)
	{
		if (GridPuzzleActionManager.Instance.IsAnyoneActing() || GridPuzzleActionManager.Instance.PlayerHasActions())
		{
			this.cam.OnEndManualInput();
			return;
		}

		//ScreenTransformGesture
		ScreenTransformGesture gesture = sender as ScreenTransformGesture;
		if (gesture != null)
		{
			Vector2 dragDelta = (gesture.NormalizedScreenPosition - gesture.PreviousNormalizedScreenPosition);
			if (Mathf.Abs(dragDelta.x) >= Mathf.Abs(dragDelta.y))
			{
				float amount = dragDelta.x;
				//Debug.LogError("TransformedHandler amount=" + amount);
				this.cam.OnManualInput(amount);
			}
		}
	}

	private void TransformCompletedHandler(object sender, EventArgs e)
	{
		this.cam.OnEndManualInput();
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
			break;

		case "PlayerSpawned":
			this.player = obj1 as GridPuzzlePlayerController;
			break;
		}
	}
}

