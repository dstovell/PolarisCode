using UnityEngine;
using System.Collections;

public class GridPuzzleActor : DSTools.MessengerListener 
{
	private Vector3 moveTarget = Vector3.zero;
	public float MoveSpeed = 1;

	private Animator anim;

	void Awake()
	{
		this.anim = this.gameObject.GetComponent<Animator>();
	}

	void Start () 
	{
	}
	
	void Update () 
	{
		if (this.IsMoving())
		{
			Vector3 currentPos = this.gameObject.transform.position;
			currentPos.y = 0;

			if (Vector3.Distance(currentPos, this.moveTarget) < 0.1f)
			{
				this.moveTarget = Vector3.zero;
			}
		}
	}

	public bool IsMoving()
	{
		return (this.moveTarget != Vector3.zero);
	}

	public void MoveTo(GridPuzzleNode node)
	{
		this.moveTarget = node.gameObject.transform.position;
		this.moveTarget.y = 0;
		Debug.Log("MoveTo pos=" + this.moveTarget.x + "," + this.moveTarget.y + "," + this.moveTarget.z);
		//this.anim.SetBool("Run", true);
	}

	public void Stop()
	{
		this.moveTarget = Vector3.zero;
	}
}
