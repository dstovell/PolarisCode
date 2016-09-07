using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterDemo : MonoBehaviour 
{

	public float DemoTime = 2f;
	private float accumulatedTime = 0f;

	// Use this for initialization
	void Start () 
	{
		Animator anim = this.gameObject.GetComponent<Animator>();
		anim.SetBool("Run", true);

		#if UNITY_EDITOR
		this.DemoTime = 2f;
		#elif UNITY_IOS
		this.DemoTime += 4f;
		#endif
	}
	
	// Update is called once per frame
	void Update () 
	{
		this.accumulatedTime += Time.deltaTime;
		if (this.accumulatedTime > this.DemoTime)
		{
			SceneManager.LoadScene("Winter");
		}
	}
}
