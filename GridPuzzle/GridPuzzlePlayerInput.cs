using UnityEngine;
using System.Collections;

public class GridPuzzlePlayerInput : DSTools.MessengerListener
{
	public GridPuzzleActor actor;

	// Use this for initialization
	void Start ()
	{
		this.actor = this.gameObject.GetComponent<GridPuzzleActor>();
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
			actor.MoveTo(node);
			break;
		}
	}
}

