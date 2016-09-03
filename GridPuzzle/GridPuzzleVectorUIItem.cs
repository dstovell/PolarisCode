using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleVectorUIItem : EasyVectorButton
{
	public enum GridPuzzleEditorAction
	{
		None,
		GeneratePrefab,
		OptimizePrefab,

		SwitchCamera,
	}

	public GridPuzzleEditorAction action;
	public GridPuzzleCamera camera;

	protected override void OnButtonPressed() 
	{
		switch(this.action)
		{
			case GridPuzzleEditorAction.GeneratePrefab:
				GridPuzzleEditor.Instance.GeneratePrefab();
				break;

			case GridPuzzleEditorAction.OptimizePrefab:
				GridPuzzleEditor.Instance.OptimizePuzzle();
				break;

			case GridPuzzleEditorAction.SwitchCamera:
				GridPuzzleCamera.Angle angle = camera.ToggleCamera();
				GridPuzzleManager.Instance.SetCameraAngle(angle);
				break;

			default:
				break;
		}
	}
}
