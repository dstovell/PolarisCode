using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript;
using TouchScript.Gestures;

public class GridPuzzlePathFollower : MonoBehaviour
{
	public float Speed = 1f;
	public float TimeAtTarget = 1f;
	public List<Vector3> VectorPath = new List<Vector3>();

	private int currentTarget;

	public GameObject gameObject;

	public GameObject EnableOnComplete;

	private bool isDone = false;
	private float timeOnTarget = 0f;

	private Vector3 position
	{
		get
		{
			return (gameObject != null) ? gameObject.transform.position : Vector3.zero;
		}

		set
		{
			if (gameObject != null)
			{
				gameObject.transform.position = value;
			}
		}
	}

	private void Init()
	{
		this.position = (this.VectorPath.Count > 0) ? this.VectorPath[0] : Vector3.zero;
		this.currentTarget = (this.VectorPath.Count > 1) ? 1 : 0;
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (this.isDone)
		{
			if (this.EnableOnComplete != null)
			{
				this.EnableOnComplete.SetActive(true);
			}
			this.timeOnTarget += Time.deltaTime;
			if (this.timeOnTarget >= this.TimeAtTarget)
			{
				GameObject.Destroy(this.gameObject);
			}
			return;
		}

		this.isDone = this.Move(this.Speed, Time.deltaTime);
	}

	public void FollowPath(List<Vector3> points, float speed, GameObject toEnable = null)
	{
		this.EnableOnComplete = toEnable;
		this.Speed = speed;
		VectorPath = points;
		Init();
	}

	public Vector3 GetDirection()
	{
		if (this.VectorPath.Count == 0)
		{
			return Vector3.forward;
		}

		Vector3 dest = this.VectorPath[this.currentTarget];
		return (dest - this.transform.position).normalized;
	}

	private bool Move(float speed, float deltaT)
	{
		if (this.VectorPath.Count == 0)
		{
			return true;
		}

		Vector3 dest = this.VectorPath[this.currentTarget];
		this.position = Vector3.MoveTowards(this.position, dest, speed*deltaT);

		if (Vector3.Distance(this.position, dest) < 0.1)
		{
			if (this.currentTarget == (this.VectorPath.Count - 1))
			{
				this.position = this.VectorPath[this.VectorPath.Count - 1];
				return true;
			}
			else
			{
				this.currentTarget++;
			}
		}

		return false;
	}
}

