using UnityEngine;
using System.Collections;

public class EndlessAnimator : MonoBehaviour
{

	public Vector3 PathVector;
	public float Speed;

	public GameObject RotateObject;
	public float MaxRotate;
	public float RotateSpeed;
	private Quaternion StartRotate;
	private Quaternion GoalRotate = Quaternion.identity;

	public GameObject [] FX;

	private Vector3 StartPoint;
	private Vector3 EndPoint;

	// Use this for initialization
	void Start ()
	{
		this.StartPoint = this.gameObject.transform.position;
		this.EndPoint = this.StartPoint + this.PathVector;
		this.StartRotate = this.gameObject.transform.rotation;
	}

	void EnableFX()
	{
		for (int i=0; i<FX.Length; i++)
		{
			FX[i].SetActive(true);
		}
	}

	void DisableFX()
	{
		for (int i=0; i<FX.Length; i++)
		{
			FX[i].SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Vector3.Distance(this.gameObject.transform.position, this.EndPoint) < 0.01)
		{
			DisableFX();
			this.gameObject.transform.position = this.StartPoint;
		}
		else
		{
			EnableFX();
			this.gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, this.EndPoint, this.Speed*Time.deltaTime);
		}

		if (this.GoalRotate != Quaternion.identity)
		{
			this.RotateObject.transform.rotation = Quaternion.RotateTowards(this.RotateObject.transform.rotation, this.GoalRotate, this.RotateSpeed*Time.deltaTime);
			if (this.RotateObject.transform.rotation == this.GoalRotate) 
			{
				this.GoalRotate = Quaternion.identity;
			}
		}
		else 
		{
			this.RotateObject.transform.rotation = Quaternion.RotateTowards(this.RotateObject.transform.rotation, this.StartRotate, this.RotateSpeed*Time.deltaTime);
			if (this.RotateObject.transform.rotation == this.StartRotate)
			{
				float randomT = Random.Range(-1f, 1f);
				float randomAngle = this.MaxRotate*randomT*randomT;
				this.GoalRotate = Quaternion.AngleAxis(randomAngle, this.PathVector.normalized) * this.StartRotate;
			}
		}
	}
}

