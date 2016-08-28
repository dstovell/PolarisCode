using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleNode : DSTools.MessengerListener 
{
	public BoxCollider box;

	void Awake()
	{
		//this.InitMessenger("GridPuzzleNode");

		this.box = this.gameObject.GetComponent<BoxCollider>();
	}

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	private static GameObject PickRandomPrefab(GameObject [] array)
    {
		int count = array.Length;
		if (count == 0)
    	{
    		return null;
    	}

		int randomIndex = Random.Range(0, count);

		return array[randomIndex];
    }

	static public GridPuzzleNode GeneratePrefab(GameObject [] nodePrefabs, GridPuzzle.Settings settings, Vector3 position)
	{
		GameObject nodeObj = GameObject.Instantiate(PickRandomPrefab(nodePrefabs), position, Quaternion.identity) as GameObject;
		GridPuzzleNode node = nodeObj.GetComponent<GridPuzzleNode>();
		if (node == null)
		{
			GameObject.Destroy(nodeObj);
			return null;
		}

		BoxCollider newBox = nodeObj.GetComponent<BoxCollider>();
		Vector3 boxSize = newBox.size;
		Vector3 scale = new Vector3(settings.GridNodeWidth/boxSize.x, settings.GridNodeHeight/boxSize.y, 1f);
		nodeObj.transform.localScale = scale;

		return node;
	}

	void OnMouseDown() 
	{
		this.SendMessengerMsg("NodeSelected", this);
    }
}
