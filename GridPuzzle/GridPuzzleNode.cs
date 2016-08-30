using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DSTools;

public class GridPuzzleNode : MessengerListener 
{
	public BoxCollider box;

	public GameObject floor;
	public GameObject ceiling;
	public GameObject back;

	//Legacy
	public MagneticCharge floorCharge = MagneticCharge.None;
	public MagneticCharge ceilingCharge = MagneticCharge.None;

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

	public Vector3 EvaluateMagneticGravity(GridPuzzlePlayerController controller, bool defaultToGravity=true)
	{
		return DSTools.SurfaceComponent.EvaluateMagneticGravity(controller.currentCharge, this.floorCharge, this.ceilingCharge, defaultToGravity);
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

	static public Vector3 GetBoxSize(GameObject obj)
	{
		BoxCollider thisBox = obj.GetComponent<BoxCollider>();
		return (thisBox != null) ? thisBox.size : Vector3.one;
	}

	static public GridPuzzleNode GeneratePrefab(GridPuzzle.Settings settings, Vector3 position)
	{
		GameObject nodeObj = GameObject.Instantiate(settings.blankNodePrefab, position, Quaternion.identity) as GameObject;
		GridPuzzleNode node = nodeObj.GetComponent<GridPuzzleNode>();
		if (node == null)
		{
			GameObject.Destroy(nodeObj);
			return null;
		}

		BoxCollider nodeBox = nodeObj.GetComponent<BoxCollider>();
		Vector3 desiredBoxSize = new Vector3(settings.GridNodeWidth, settings.GridNodeHeight, settings.GridFloorDepth);
		nodeBox.size = desiredBoxSize;

		node.floor = GameObject.Instantiate(settings.metalFloorPrefab) as GameObject;
		node.floor.transform.SetParent(nodeObj.transform);
		node.floor.transform.localPosition = new Vector3(0f, -0.5f*settings.GridNodeHeight+0.5f*settings.GridFloorHeight, 0f);

		node.ceiling = GameObject.Instantiate(settings.metalCeilingPrefab) as GameObject;
		node.ceiling.transform.SetParent(nodeObj.transform);
		node.ceiling.transform.localPosition = new Vector3(0f, 0.5f*settings.GridNodeHeight-0.5f*settings.GridFloorHeight, 0f);

		node.back = GameObject.Instantiate(settings.PickRandomPrefab(settings.backWallPrefabs)) as GameObject;
		node.back.transform.SetParent(nodeObj.transform);
		node.back.transform.localPosition = new Vector3(0f, 0f, 0.5f*settings.GridFloorDepth);
		Vector3 backBoxSize = GetBoxSize(node.back);
		Vector3 scale = new Vector3(settings.GridNodeWidth/backBoxSize.x, settings.GridNodeHeight/backBoxSize.y, 1f);
		node.back.transform.localScale = scale;

		return node;
	}

	static public GridPuzzleNode GeneratePrefabOld(GridPuzzle.Settings settings, Vector3 position)
	{
		GameObject nodeObj = GameObject.Instantiate(PickRandomPrefab(settings.nodePrefabs), position, Quaternion.identity) as GameObject;
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

	void OnTriggerEnter(Collider other)
    {
		GridPuzzlePlayerController controller = other.gameObject.GetComponent<GridPuzzlePlayerController>();
		if (controller != null)
		{
			controller.currentNode = this;
		}
    }
}
