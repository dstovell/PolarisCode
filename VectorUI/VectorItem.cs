using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class VectorItem : DSTools.MessengerListener
{
	public enum Type
	{
		Square,
		Diamond,
		Hex,
		Circle
	}
	public Type type;

	public string text = string.Empty;

	private string originalText = string.Empty;
	public VectorItem parent = null;
	public List<VectorItem> children = new List<VectorItem>();
	public Vector2 position = Vector2.zero;
	public float size = 1.0f;
	private float minSize = 0.05f;
	public PolygonVectorShape shape = null;
	public int startSlot = 0;
	public int maxChildren = 0;
	public float delayTime = 0f;

	public void Awake()
	{
		this.originalText = this.text;

		this.size = Mathf.Max(this.size, this.minSize);
	}

	public void Start()
	{
		Vector2 startPos = (this.parent != null) ? this.parent.position : this.position;
		if (this.type == Type.Square)
		{
			this.maxChildren = 8;
			this.shape = new SquareVectorShape(Color.white, this.position, this.size/2.0f, startPos, 0.5f, this.delayTime);
		}
		else if (this.type == Type.Diamond)
		{
			this.maxChildren = 8;
			this.shape = new DiamondVectorShape(Color.white, this.position, this.size/2.0f, startPos, 0.5f, this.delayTime);
		}
		else if (this.type == Type.Hex)
		{
			this.maxChildren = 6;
			this.shape = new HexVectorShape(Color.white, this.position, this.size/2.0f, startPos, 0.5f, this.delayTime);
		}
		else 
		{
			this.maxChildren = 8;
			this.shape = new CircleVectorShape(Color.white, this.position, this.size/2.0f, startPos, 0.5f, delayTime);
		}

		//this.shape.lineWidth = 0.04f * _size;
		VectorShapeManager.Instance.AddShape("info", this.shape);
	}

	public void CloseChildren()
	{
		for(int i=0; i<this.children.Count; i++)
		{
			this.children[i].CloseAndDestroy();
		}
		this.children.Clear();
	}

	public void CloseAndDestroy()
	{
		CloseChildren();
		if (this.shape != null)
		{
			this.shape.state = VectorShape.State.Outro;
		}
	}

	public void Destroy()
	{
		for (int i=0; i<this.children.Count; i++)
		{
			this.children[i].Destroy();
		}
		this.children.Clear();

		if (this.shape != null)
		{
			VectorLine.Destroy(ref this.shape.line);
		}
	}

	public Vector2 GetChildPosition(float childSize, int index)
	{
		float thetaStep = 2*Mathf.PI / this.maxChildren;
		float theta = (this.maxChildren - index) * thetaStep;
		float childPosRadius = this.size*0.5f + childSize*0.6f;
		float aspectRatio = ((float)Screen.width/(float)Screen.height);
		float x = this.position.x + childPosRadius/aspectRatio * (float)System.Math.Cos(theta);
		float y = this.position.y + childPosRadius * (float)System.Math.Sin(theta);
		Vector2 position = new Vector2(x, y);
		return position;
	}

	public void AddChild(string _text, float delayTime = 0.0f)
	{
		//Debug.Log("AddChild " + _text);
		float childScale = 0.8f;
		float childSize = this.size*childScale;
		Vector2 childPosition = this.GetChildPosition(childSize, this.children.Count + this.startSlot);

		//VectorItem child = new VectorItem(_text, this.type, this, childPosition, childSize, delayTime);
		//this.children.Add(child);
	}

	public void AddChild(VectorItem child)
	{
		Debug.Log("AddChild " + this.GetType().ToString() + " pos= " + child.position.x + ", " + child.position.y); 
		this.children.Add(child);
	}

	public void Update() 
	{
		if ((this.shape != null) && this.shape.state == VectorShape.State.Ready)
		{
			if (this.shape.state == VectorShape.State.Finished)
			{
				this.Destroy();
			}
		}
	}
}



