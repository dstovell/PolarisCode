using UnityEngine;
using System.Collections;

public class GridPuzzleUIAction : MonoBehaviour
{
	public enum Type
	{
		None,

		Camera_Side2D,
		Camera_Isometric,
		Camera_Front2D,

		Player_NegativeCharge,
		Player_PositiveCharge,
	}

	public Type actionType = Type.None;

	void OnMouseDown() 
	{
		//GridPuzzleManager.Instance.ChangeCameraAngleNow = true;
		Debug.Log("GridPuzzleAction OnMouseDown this.actionType=" + this.actionType);
		DSTools.Messenger.SendMessageFrom("GridPuzzleUIAction", "GridPuzzleUIAction", this.actionType);
    }
}

