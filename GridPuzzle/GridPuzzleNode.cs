using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MagneticCharge
{
	None,
	Positive,
	Negative
}

public class GridPuzzleNode : DSTools.MessengerListener 
{
	public BoxCollider box;

	public GameObject floor;
	public MagneticCharge floorCharge = MagneticCharge.None;

	public GameObject ceiling;
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

	public float EvaluateMagneticForce(MagneticCharge c1, MagneticCharge c2)
	{
		if ((c1 == MagneticCharge.None) && (c2 == MagneticCharge.None))
		{
			return 0f;
		}
		else if ((c1 == MagneticCharge.None) || (c2 == MagneticCharge.None))
		{
			return -1f;
		}
		else 
		{
			return (c1 == c2) ? 2f : -2f;
		}
	}

	public Vector3 EvaluateMagneticGravity(GridPuzzlePlayerController controller, bool defaultToGravity=true)
	{
		float floorAmount = this.EvaluateMagneticForce(controller.currentCharge, this.floorCharge);
		float ceilingAmount = this.EvaluateMagneticForce(controller.currentCharge, this.ceilingCharge);

		Vector3 forceVector = Vector3.zero;

		if (floorAmount == ceilingAmount)
		{
			forceVector = defaultToGravity ? Vector3.down : Vector3.zero;
		}
		else if (ceilingAmount < floorAmount)
		{
			forceVector = Vector3.up;
		}
		else
		{
			forceVector = Vector3.down;
		}

		//Debug.Log("ceilingAmount=" + ceilingAmount + " floorAmount=" + floorAmount + " forceVector=" + forceVector.x + "," + forceVector.y + "," + forceVector.z);

		return forceVector;
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

	static public GridPuzzleNode GeneratePrefab(GridPuzzle.Settings settings, Vector3 position)
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
