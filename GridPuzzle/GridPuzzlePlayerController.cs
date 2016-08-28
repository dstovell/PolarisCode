using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzlePlayerPathNode
{
	public GridPuzzlePlayerPathNode(Vector3 dest)
	{
		this.destination = dest;
	}

	public Vector3 destination;
}

public class GridPuzzlePlayerPath
{
	private List<GridPuzzlePlayerPathNode> nodes;
	private int currentTargetNode;
	private Vector3 postion;

	public bool isDone;

	public GridPuzzlePlayerPath(List<Vector3> points)
	{
		this.nodes = new List<GridPuzzlePlayerPathNode>();
		for (int i=0; i<points.Count; i++)
		{
			this.nodes.Add( new GridPuzzlePlayerPathNode(points[i]) );
		}
		Init();
	}

	public GridPuzzlePlayerPath(List<GridPuzzlePlayerPathNode> points)
	{
		this.nodes = points;
		Init();
	}

	public void Init()
	{
		this.postion = (this.nodes.Count > 0) ? this.nodes[0].destination : Vector3.zero;
		this.currentTargetNode = (this.nodes.Count > 1) ? 1 : 0;
		this.isDone = false;
	}

	public Vector3 GetDirection()
	{
		if ((this.nodes.Count == 0) || this.isDone)
		{
			return Vector3.zero;
		}

		Vector3 dest = this.nodes[this.currentTargetNode].destination;
		return (dest - this.postion).normalized;
	}

	public Vector3 Move(float speed, float deltaT)
	{
		if ((this.nodes.Count == 0) || this.isDone)
		{
			return this.postion;
		}

		Vector3 dest = this.nodes[this.currentTargetNode].destination;
		this.postion = Vector3.MoveTowards(this.postion, dest, speed*deltaT);

		if (Vector3.Distance(this.postion, dest) < 0.1)
		{
			if (this.currentTargetNode == (this.nodes.Count - 1))
			{
				this.isDone = true;
				this.postion = this.nodes[this.nodes.Count - 1].destination;
			}
			else
			{
				this.currentTargetNode++;
			}
		}

		return this.postion;
	}
}


public class GridPuzzlePlayerController : DSTools.MessengerListener
{
	public float MoveSpeed = 1f;

	public enum State
	{
		Idle,
		Run,
		Jump
	};
	public State currentState;

	private Animator anim;

	private GridPuzzlePlayerPath movePath;

	// Use this for initialization
	void Start() 
	{
		this.InitMessenger("GridPuzzlePlayerController");
		this.currentState = State.Idle;
		this.anim = this.gameObject.GetComponent<Animator>();
	}

	public void SetState(State newState)
	{
		if (newState == this.currentState)
		{
			return;
		}

		switch(newState)
		{
		case State.Idle:
			this.anim.SetBool("Run", false);
			break;
		case State.Run:
			this.anim.SetBool("Run", true);
			break;
		case State.Jump:
			break;
		default:
			break;
		}
		this.currentState = newState;
	}

	void Update () 
	{
		switch(this.currentState)
		{
		case State.Idle:
			break;
		case State.Run:
			break;
		case State.Jump:
			break;
		default:
			break;
		}

		if (this.IsMoving())
		{
			Vector3 desiredDir = this.movePath.GetDirection();
			if (!this.IsFacing(desiredDir))
			{
				Vector3 dir = Vector3.RotateTowards(this.transform.forward, desiredDir, 0.1f, 0.1f);
				this.transform.rotation = Quaternion.LookRotation(dir, this.transform.up);
			}
			else if (this.IsPlaying("Run"))
			{
				this.transform.position = this.movePath.Move(this.MoveSpeed, Time.deltaTime);
				if (this.movePath.isDone)
				{
					Stop();
				}
			}
		}
	}

	public bool IsFacing(Vector3 dir)
	{
		return (Vector3.Angle(this.transform.forward, dir) < 0.1);
	}

	public bool IsMoving()
	{
		return (this.currentState == State.Run);
	}

	public bool IsPlaying(string animName)
	{
		return this.anim.GetCurrentAnimatorStateInfo(0).IsName(animName);
	}

	public void MoveTo(GridPuzzleNode node)
	{
		Vector3 pos = this.gameObject.transform.position;
		Vector3 dest = node.gameObject.transform.position;
		dest.y = pos.y;

		List<Vector3> points = new List<Vector3>();
		points.Add(pos);
		points.Add(dest);

		this.movePath = new GridPuzzlePlayerPath(points);
		this.SetState(State.Run);
	}

	public void MovePath(List<GridPuzzleNode> nodes)
	{
		List<Vector3> points = new List<Vector3>();
		for (int i=0; i<nodes.Count; i++)
		{
			points.Add( nodes[i].gameObject.transform.position );
		}

		this.movePath = new GridPuzzlePlayerPath(points);
		this.SetState(State.Run);
	}

	public void TeleportTo(GameObject location)
	{
		this.transform.position = location.transform.position;
		this.transform.rotation = location.transform.rotation;
		this.Stop();
	}

	public void Stop()
	{
		this.movePath = null;
		this.SetState(State.Idle);
	}
}
