using UnityEngine;
using System.Collections;

public class GridPuzzlePlayerInput : DSTools.MessengerListener
{
	public GridPuzzlePlayerController player;

	// Use this for initialization
	void Start ()
	{
		this.player = this.gameObject.GetComponent<GridPuzzlePlayerController>();
		this.InitMessenger("GridPuzzlePlayerInput");
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{
		switch(id)
		{
		case "NodeSelected":
			GridPuzzleNode node = obj1 as GridPuzzleNode;
			player.MoveTo(node);
			break;
		}
	}
}

