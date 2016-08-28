using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class VectorItem
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
	public string originalText = string.Empty;
	public VectorItem parent = null;
	public List<VectorItem> children = new List<VectorItem>();
	public Vector2 position = Vector2.zero;
	public float size = 1.0f;
	private float minSize = 0.05f;
	public PolygonVectorShape shape = null;
	public int startSlot = 0;
	public int maxChildren = 0;

	public VectorItem(string _text, Type _type, VectorItem _parent, Vector2 _position, float _size, float delayTime = 0.0f)
	{
		this.text = _text;
		this.originalText = _text;
		this.type = _type;
		this.parent = _parent;
		this.position = _position;
		this.size = Mathf.Max(_size, this.minSize);
		Vector2 startPos = (this.parent != null) ? this.parent.position : this.position;
		if (this.type == Type.Square)
		{
			this.maxChildren = 8;
			this.shape = new SquareVectorShape(Color.white, this.position, this.size/2.0f, startPos, 0.5f, delayTime);
		}
		else if (this.type == Type.Diamond)
		{
			this.maxChildren = 8;
			this.shape = new DiamondVectorShape(Color.white, this.position, this.size/2.0f, startPos, 0.5f, delayTime);
		}
		else if (this.type == Type.Hex)
		{
			this.maxChildren = 6;
			this.shape = new HexVectorShape(Color.white, this.position, this.size/2.0f, startPos, 0.5f, delayTime);
		}

		else 
		{
			this.maxChildren = 8;
			this.shape = new CircleVectorShape(Color.white, this.position, this.size/2.0f, startPos, 0.5f, delayTime);
		}

		//this.shape.lineWidth = 0.04f * _size;
		VectorShapeManager.Instance.AddShape("info", this.shape);
	}

	public VectorItem(Vector2 _position, float _size)
	{
		this.position = _position;
		this.size = Mathf.Max(_size, this.minSize);
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

		VectorItem child = new VectorItem(_text, this.type, this, childPosition, childSize, delayTime);
		this.children.Add(child);
	}

	public void AddChild(VectorItem child)
	{
		Debug.Log("AddChild " + this.GetType().ToString() + " pos= " + child.position.x + ", " + child.position.y); 
		this.children.Add(child);
	}

	public void Update() 
	{
		if ((this.shape == null) || (this.shape.state == VectorShape.State.Ready))
		{
			/*if (this.parent == null)
			{
				if (this.children.Count < 3)
				{
					float delay = this.children.Count * 0.1f;
					AddChild("OPT " + (this.children.Count+1), delay);
				}
			}*/

			for (int i=0; i<this.children.Count; i++)
			{
				this.children[i].Update();
			}
		}

		if ((this.shape != null) && this.shape.state == VectorShape.State.Ready)
		{
			if (this.shape.state == VectorShape.State.Finished)
			{
				this.Destroy();
			}
		}
	}

	public void OnGUIUpdate(VectorMenu menuManager) 
	{
		if ((this.shape == null) || (this.shape.state == VectorShape.State.Ready))
		{
			this.OnGUI(menuManager);

			for (int i=0; i<this.children.Count; i++)
			{
				this.children[i].OnGUIUpdate(menuManager);
			}
		}
	}

	protected virtual void OnGUI(VectorMenu menuManager) 
	{
	}
}

/*
public class SolarSystemViewVectorItem : VectorItem
{
	public SolarSystemViewVectorItem(VectorItem _parent, Vector2 _position, float _size, int _starId) 
	: base("View System", VectorItem.Type.Square, _parent, _position, _size)
	{
	}

	protected override void OnGUI(VectorMenu menuManager) 
	{
		GUIStyle style = new GUIStyle(GUI.skin.button);
		style.fontStyle = FontStyle.Bold;
		style.richText = true;
		style.fontSize = Mathf.FloorToInt(VectorShape.ScaleSize(0.03f));
		style.wordWrap = true;
		//style.fontStyle
		float boarderSize = 2.0f;
		float sizeX =  (shape.points[0].x - shape.points[2].x - 2.0f*boarderSize);
		float sizeY =  (shape.points[0].y - shape.points[2].y - 2.0f*boarderSize);
		float x = shape.points[2].x + boarderSize;
		float y = ((Screen.height - shape.points[2].y)+boarderSize-sizeY- 2.0f*boarderSize);
		Rect buttonRect = new Rect(x, y, sizeX, sizeY);
		//if (GUI.Button(buttonRect, "<color=cyan>" + this.text + "</color>", style))
		if (GUI.Button(buttonRect, menuManager.solarSystemIcon, style))
		{
			if (this.parent != null)
			{
				this.parent.CloseChildren();
			}
		}
	}
}
*/

public class RootVectorItem : VectorItem
{
	public bool IsOpen { get { return (this.children.Count > 0); } }

	private float childSize = 0.18f;

	public RootVectorItem(Vector2 _position, float _size) 
	: base(_position, _size)
	{
		this.maxChildren = 8;
	}

	private void AddStarItem(VectorItem childItem, int pos)
	{
		Vector2 childPosition = this.GetChildPosition(this.childSize, pos);
		this.AddChild(childItem);
	}

	public void Open()
	{
		if (!this.IsOpen)
		{
			//AddMenuItem(0);
		}
	}


}

public class VectorMenu : DSTools.MessengerListener 
{
	public Texture solarSystemIcon;
	public Texture starIcon;
	public Texture galaxyIcon;

	private RootVectorItem rootMenu = null;
	private VectorItem starMenu = null;

	public GameObject homeButton;

	public void Awake()
	{
	}

	public void Start() 
	{
		Screen.orientation = ScreenOrientation.Landscape;
		InitMessenger("VectorMenu");
		if (this.homeButton != null)
		{
			float size = 0.25f;
			Vector3 screenPos = Camera.main.WorldToViewportPoint( homeButton.transform.position );
			Debug.Log("screenPos=" + screenPos.x + "," + screenPos.y + "," + screenPos.z);
			this.rootMenu = new RootVectorItem( new Vector2(screenPos.x, screenPos.y), size );
		}
	}

	public void Update() 
	{
		if (this.rootMenu != null)
		{
			this.rootMenu.Update();
		}
		if (this.starMenu != null)
		{
			starMenu.Update();
		}
	}

	public void OnGUI()
	{
		if (this.rootMenu != null)
		{
			this.rootMenu.OnGUIUpdate(this);
		}
		if (this.starMenu != null)
		{
			starMenu.OnGUIUpdate(this);
		}
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{

		switch(id)
		{
			case "starview_reached":
			{
			}
			break;

			default:break;
		}
	}

}

