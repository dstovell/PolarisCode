﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridPuzzleTurnCounter : DSTools.MessengerListener 
{
	public int startCount = 0;
	private int count;

	public Text text;

	private GridPuzzleCamera.Angle angle = GridPuzzleCamera.Angle.Isometric;

	void Awake()
	{
		if (this.text == null)
		{
			this.text = this.gameObject.GetComponent<Text>();
		}

		this.count = this.startCount;
	}

	void Start()
	{
		this.InitMessenger("GridPuzzleTurnCounter");
	}

	void Update()
	{
		if (this.text != null)
		{
			this.text.text = this.count.ToString();
		}
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{
		if (id == "ActorTurn")
		{
			GridPuzzleActor actor = obj1 as GridPuzzleActor;
			int turnCount = (int)obj2;
			if ((actor != null) && (actor.IsPlayer))
			{
				this.count += turnCount;
			}
		}
		else if (id == "CameraPositionUpdate")
		{
			GridPuzzleCamera.Angle newAngle = (GridPuzzleCamera.Angle)obj1;
			if (this.angle != newAngle)
			{
				this.count++;
				this.angle = newAngle;
			}
		}
		else if (id == "OnUpdatedPuzzlePositions")
		{
			this.count = 0;
		}
		else if (id == "ActorKilled")
		{
			GridPuzzleActor actor = obj1 as GridPuzzleActor;
			if ((actor != null) && (actor.IsPlayer))
			{
				this.count = 0;
			}
		}
		else if (id == "OnTeleportedTo")
		{
			this.count = 0;
		}
	}
}
