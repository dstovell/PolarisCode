using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridPuzzleSafeZone : MonoBehaviour
{
	public BoxCollider box;
	public BoxCollider parentCollider;

	private List<GridPuzzleGuts> currentSafeGuts;

	void Start()
	{
		if (this.transform.parent != null)
		{
			this.parentCollider = this.transform.parent.gameObject.GetComponent<BoxCollider>();
		}

		currentSafeGuts = new List<GridPuzzleGuts>();
	}

	public void AddBox(Vector3 size)
	{
		if (this.box == null)
		{
			this.box = this.gameObject.AddComponent<BoxCollider>();
			this.box.center = Vector3.zero;
			this.box.size = size;
			this.box.isTrigger = true;
		}
	}

	public bool IsInSafeZone(Collider other)
	{
		if ((this.box == null) || (other == null))
		{
			return false;
		}

		return this.box.bounds.Intersects(other.bounds);
	}

	public bool IsInSafeZone(GameObject obj)
	{
		return this.IsInSafeZone( obj.GetComponent<Collider>() );
	}

	public void MakeSafe(Collider other)
	{
		if ((this.parentCollider == null) || (other == null) || !this.IsInSafeZone(other))
		{
			return;
		}

		//Debug.LogError("MakeSafe " + other.gameObject.name);

		Physics.IgnoreCollision(this.parentCollider, other);
	}

	public void MakeSafe(GameObject obj)
	{
		this.MakeSafe( obj.GetComponent<Collider>() );
	}

	public void MakeUnSafe(Collider other)
	{
		if ((this.parentCollider == null) || (other == null))
		{
			return;
		}

		Physics.IgnoreCollision(this.parentCollider, other, false);
	}

	public void MakeUnSafe(GameObject obj)
	{
		this.MakeUnSafe( obj.GetComponent<Collider>() );
	}

	public bool IsAnySafe()
    {
		if (this.currentSafeGuts == null)
		{
			return false;
		}

		for (int i=0; i<currentSafeGuts.Count; i++)
		{
			GridPuzzleGuts guts = currentSafeGuts[i];
			if (this.IsInSafeZone(guts.gameObject))
			{
				return true;
			}
		}
		return false;
    }

	public void UpdateSafe()
    {
		if (this.currentSafeGuts == null)
		{
			return;
		}

		for (int i=0; i<currentSafeGuts.Count; i++)
		{
			GridPuzzleGuts guts = currentSafeGuts[i];
			if (this.IsInSafeZone(guts.gameObject))
			{
				this.MakeSafe(guts.gameObject);
			}
			else
			{
				this.MakeUnSafe(guts.gameObject);
			}
		}
    }

	void Update()
    {
		UpdateSafe();
    }

	void OnTriggerEnter(Collider other)
    {
		GridPuzzleGuts guts = other.gameObject.GetComponent<GridPuzzleGuts>();
		if (guts != null)
		{
			this.MakeSafe(guts.gameObject);
			if (!this.currentSafeGuts.Contains(guts))
			{
				this.currentSafeGuts.Add(guts);
			}
		}
    }

	void OnTriggerExit(Collider other)
    {
    }
}

