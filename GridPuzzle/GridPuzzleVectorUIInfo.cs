using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleVectorUIInfo : EasyVectorBox
{
	protected override string GetDynamicText()
	{
		GridPuzzle.Stats stats = GridPuzzleEditor.Instance.GetStats();
		string info = string.Empty;
		info += "Puzzle Stats:   ";
		info += "Cubes:    " + stats.cubeCount + "    ";
		info += "Surfaces: " + stats.activeSurfaceCount + "/" + stats.surfaceCount;

		return info;
	}
}
