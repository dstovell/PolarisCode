using UnityEngine;
using System.Collections;

public enum GridPuzzleAction
{
	None,

	Camera_Side2D,
	Camera_Isometric,
	Camera_Front2D,

	Player_NegativeCharge,
	Player_PositiveCharge,
}


public class GridPuzzleUIAction : MonoBehaviour
{
	public GridPuzzleAction actionType = GridPuzzleAction.None;

	void OnMouseDown() 
	{
		//GridPuzzleManager.Instance.ChangeCameraAngleNow = true;
		Debug.Log("GridPuzzleAction OnMouseDown this.actionType=" + this.actionType);
		DSTools.Messenger.SendMessageFrom("GridPuzzleUIAction", "GridPuzzleAction", this.actionType);
    }
}

