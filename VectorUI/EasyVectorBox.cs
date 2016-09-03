using UnityEngine;
using System.Collections;

public class EasyVectorBox : VectorItem
{
	public Texture2D boxTexture;

	protected virtual string GetDynamicText()
	{
		return this.text;
	}

	void OnGUI()
	{
		this.text = this.GetDynamicText();

		GUIStyle style = new GUIStyle(GUI.skin.box);
		style.fontStyle = FontStyle.Bold;
		style.richText = true;
		style.fontSize = Mathf.FloorToInt(VectorShape.ScaleSize(0.03f));
		style.wordWrap = true;

		float boarderSize = 2.0f;
		float sizeX =  (shape.points[0].x - shape.points[2].x - 2.0f*boarderSize);
		float sizeY =  (shape.points[0].y - shape.points[2].y - 2.0f*boarderSize);
		float x = shape.points[2].x + boarderSize;
		float y = ((Screen.height - shape.points[2].y)+boarderSize-sizeY- 2.0f*boarderSize);
		Rect buttonRect = new Rect(x, y, sizeX, sizeY);

		if (this.boxTexture != null)
		{
			GUI.Box(buttonRect, this.boxTexture, style);
		}
		else
		{
			GUI.Box(buttonRect, this.text, style);
		}
	}
}
