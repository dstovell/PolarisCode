using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript;
using TouchScript.Gestures;

public class GridPuzzlePlayerInput : DSTools.MessengerListener
{
	public GridPuzzlePlayerController player;
	public GameObject touchManager;

	// Use this for initialization
	void Start ()
	{
		this.InitMessenger("GridPuzzlePlayerInput");
	}
	
	// Update is called once per frame
	void Update() 
	{
        int fingerCount = 0;
        foreach (Touch touch in Input.touches) 
        {
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled) {
                fingerCount++;
			}
           
        }
        if (fingerCount > 0)
        {
			Debug.Log("User has " + fingerCount + " finger(s) touching the screen");
		}
	}

	private void OnEnable()
    {
		FlickGesture [] flicks = this.touchManager.GetComponents<FlickGesture>();
		for (int i=0; i<flicks.Length; i++)
		{
			Debug.LogError("PlayerInput flick=" + flicks[i].name);
			flicks[i].Flicked += this.FlickedHandler;
		}

		TapGesture [] taps = this.touchManager.GetComponents<TapGesture>();
		for (int i=0; i<taps.Length; i++)
		{
			Debug.LogError("PlayerInput tap=" + taps[i].name);
			taps[i].Tapped += this.TapHandler;
		}
    }

    private void OnDisable()
    {
		FlickGesture [] flicks = this.touchManager.GetComponents<FlickGesture>();
		for (int i=0; i<flicks.Length; i++)
		{
			flicks[i].Flicked -= this.FlickedHandler;
		}

		TapGesture [] taps = this.touchManager.GetComponents<TapGesture>();
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
					Debug.LogError("Right");
				}
				else
				{
					//Left
					Debug.LogError("Left");
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
		Debug.LogError("TapHandler");
		TapGesture gesture = sender as TapGesture;
		if (gesture != null)
		{
			Vector2 tapPoint = gesture.NormalizedScreenPosition;

			RaycastHit hitPoint = new RaycastHit();
	        Ray ray = new Ray(transform.position, transform.forward);

	        // Optimize this later with length and mask
	        if (Physics.Raycast(ray, out hitPoint))
	        {
	        	GameObject obj = hitPoint.transform.gameObject;
				GridPuzzleCube cube = obj.GetComponent<GridPuzzleCube>();
				GridPuzzleCubeRow cubeRow = obj.GetComponent<GridPuzzleCubeRow>();

				if (cube != null)
				{
					Debug.Log("Hit cube " + cube.name);
					player.MoveTo(cube);
				}
				else if (cubeRow != null)
				{
					Debug.Log("Hit cube " + cubeRow.name);
				}
			}
		}
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{
		switch(id)
		{
		case "CubeSelected":
			GridPuzzleCube cube = obj1 as GridPuzzleCube;
			if (this.player != null)
			{
				player.MoveTo(cube);
			}
			break;
		case "PlayerSpawned":
			this.player = obj1 as GridPuzzlePlayerController;
			break;
		}
	}
}

