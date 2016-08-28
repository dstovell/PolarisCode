using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeScreen : DSTools.MessengerListener 
{
	public bool fadingIn = false;
	public bool fadingOut = false;

	public Image FadeImg;
    public float fadeSpeed = 1.5f;


    void Awake()
    {
        FadeImg.rectTransform.localScale = new Vector2(Screen.width, Screen.height);
    }


	// Use this for initialization
	public void Start () 
	{
		this.InitMessenger("FadeScreen");

		this.fadingIn = true;
	}

	float FadeToClear()
    {
        // Lerp the colour of the image between itself and transparent.
        FadeImg.color = Color.Lerp(FadeImg.color, Color.clear, fadeSpeed * Time.deltaTime);
        return FadeImg.color.a;
    }


	float FadeToBlack()
    {
        // Lerp the colour of the image between itself and black.
        FadeImg.color = Color.Lerp(FadeImg.color, Color.black, fadeSpeed * Time.deltaTime);
		return FadeImg.color.a;
    }

	public void Update()
	{
		if (this.fadingIn)
		{
			float newAlpha = FadeToClear();
			if (newAlpha == 0.0f)
			{
				this.fadingIn = false;
			}
		}

		if (this.fadingOut)
		{
			float newAlpha = FadeToBlack();
			if (newAlpha == 1.0f)
			{
				this.fadingOut = false;
			}
		}
	}

	public override void OnMessage(string id, object obj1, object obj2)
	{

		switch(id)
		{
			case "fade_in":
			{
				this.fadingIn = true;
				break;
			}

			case "fade_out":
			{
				this.fadingIn = false;
				break;
			}

			default:break;
		}
	}

}
