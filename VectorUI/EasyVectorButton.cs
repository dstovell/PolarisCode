using UnityEngine;
using System.Collections;

public class EasyVectorButton : VectorItem
{
	public Texture2D buttonTexture;

	private bool IsButtonPressed()
	{
		GUIStyle style = new GUIStyle(GUI.skin.button);
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

		if (this.buttonTexture != null)
		{
			return GUI.Button(buttonRect, this.buttonTexture, style);
		}
		else
		{
			return GUI.Button(buttonRect, this.text, style);
		}
	}

	protected virtual void OnButtonPressed() 
	{
	}

	void OnGUI() 
	{
		if (this.IsButtonPressed())
		{
			OnButtonPressed();
		}
	}
}
