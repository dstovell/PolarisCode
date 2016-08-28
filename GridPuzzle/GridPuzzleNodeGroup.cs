using UnityEngine;
using System.Collections;

public class GridPuzzleNodeGroup : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	static public GridPuzzleNodeGroup GeneratePrefab(GridPuzzle.Settings settings, int groupHeight, int groupWidth, Vector3 basePostion)
	{
		GameObject groupObj = new GameObject("GridPuzzleNodeGroup");
		GridPuzzleNodeGroup groupComp = groupObj.AddComponent<GridPuzzleNodeGroup>();
		for (int j=0; j<groupHeight; j++)
		{
			for (int i=0; i<groupWidth; i++)
			{
				Vector3 pos = basePostion + new Vector3((float)i*settings.GridNodeWidth, (float)j*settings.GridNodeHeight, 0f);
				GridPuzzleNode node = GridPuzzleNode.GeneratePrefab(settings, pos);
				node.gameObject.transform.parent = groupObj.transform;
			}
		}
		return groupComp;
	}
}
