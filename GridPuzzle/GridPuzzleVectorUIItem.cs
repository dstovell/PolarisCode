using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GridPuzzleEditorAction
{
	None,
	GeneratePrefab,
	OptimizePrefab,
	FixPrefab,

	SwitchCamera,
	CameraUp,
	CameraDown,

	AddCubes,
	RemoveCube,
}

public enum GridPuzzleGameplayAction
{
	None,
	MoveToCube,
	MoveToCubeRow,
}

public class GridPuzzleVectorUIItem : EasyVectorButton
{
	public GridPuzzleEditorAction editorAction;
	public GridPuzzleGameplayAction gameplayAction;
	public GridPuzzleCamera camera;

	protected override void OnButtonPressed() 
	{
		if (this.editorAction != GridPuzzleEditorAction.None)
		{
			DSTools.Messenger.SendMessageFrom("GridPuzzleVectorUIItem", "GridPuzzleEditorAction", this.editorAction, this.gameObject);

			switch(this.editorAction)
			{
				case GridPuzzleEditorAction.GeneratePrefab:
					GridPuzzleEditor.Instance.GeneratePrefab();
					break;

				case GridPuzzleEditorAction.OptimizePrefab:
					GridPuzzleEditor.Instance.OptimizePuzzle();
					break;

				case GridPuzzleEditorAction.FixPrefab:
					GridPuzzleEditor.Instance.FixPuzzle();
					break;

				case GridPuzzleEditorAction.SwitchCamera:
					camera.ToggleCamera();
					break;

				case GridPuzzleEditorAction.CameraUp:
					camera.editorY += 1f;
					break;

				case GridPuzzleEditorAction.CameraDown:
					camera.editorY -= 1f;
					break;

				default:
					break;
			}
		}
		else if (this.gameplayAction != GridPuzzleGameplayAction.None)
		{
			DSTools.Messenger.SendMessageFrom("GridPuzzleVectorUIItem", "GridPuzzleGameplayAction", this.gameplayAction, this.gameObject);
		}
	}
}
