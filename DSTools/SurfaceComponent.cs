using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DSTools
{

public enum SurfacePosition
{
	Floor,
	Ceiling,
	Back,
	Left,
	Right,
}

public enum SurfaceMaterial
{
	None,
	Metal,
	Plastic,
	Glass,
}

public class SurfaceComponent : MonoBehaviour
{
	public bool updateMat = false;

	public SurfacePosition SurfacePos;
	public SurfaceMaterial SurfaceMat;
	public MagneticCharge SurfaceCharge;

	private MeshRenderer mesh;

	void Awake()
	{
		this.mesh = this.gameObject.GetComponent<MeshRenderer>();
	}

	public void SetMaterial(SurfaceMaterial mat)
	{
		this.SurfaceMat = mat;
		this.UpdateMaterial();
	}

	public void SetCharge(MagneticCharge charge)
	{
		this.SurfaceCharge = charge;
		this.UpdateMaterial();
	}

	private void UpdateMaterial()
	{
		//if (this.mesh != null)
		{
			Material mat = Instantiate(MaterialManager.Instance.GetSufaceMaterial(this.SurfaceMat, this.SurfaceCharge));
			this.mesh.material = mat;
		}
	}

	public Vector3 GetSurfaceNormal()
	{
		return SurfaceComponent.GetSurfaceNormal(this.SurfacePos);
	}

	public static Vector3 GetSurfaceNormal(SurfacePosition pos)
	{
		switch(pos)
		{
			case SurfacePosition.Floor:
				return Vector3.up;
			case SurfacePosition.Ceiling:
				return Vector3.down;
			case SurfacePosition.Back:
				return Vector3.back;
			default:
				return Vector3.zero;
		}
	}

	static public float EvaluateMagneticForce(MagneticCharge c1, MagneticCharge c2)
	{
		if ((c1 == MagneticCharge.None) && (c2 == MagneticCharge.None))
		{
			return 0f;
		}
		else if ((c1 == MagneticCharge.None) || (c2 == MagneticCharge.None))
		{
			return -1f;
		}
		else 
		{
			return (c1 == c2) ? 2f : -2f;
		}
	}

	static public Vector3 EvaluateMagneticGravity(MagneticCharge dynamicObjectCharge, MagneticCharge floorCharge, MagneticCharge ceilingCharge, bool defaultToGravity=true)
	{
		float floorAmount = EvaluateMagneticForce(dynamicObjectCharge, floorCharge);
		float ceilingAmount = EvaluateMagneticForce(dynamicObjectCharge, ceilingCharge);

		Vector3 forceVector = Vector3.zero;

		if (floorAmount == ceilingAmount)
		{
			forceVector = defaultToGravity ? Vector3.down : Vector3.zero;
		}
		else if (ceilingAmount < floorAmount)
		{
			forceVector = Vector3.up;
		}
		else
		{
			forceVector = Vector3.down;
		}

		//Debug.Log("ceilingAmount=" + ceilingAmount + " floorAmount=" + floorAmount + " forceVector=" + forceVector.x + "," + forceVector.y + "," + forceVector.z);

		return forceVector;
	}

	void Update()
	{
		if (this.updateMat)
		{
			this.UpdateMaterial();
			this.updateMat = false;
		}
	}
}

}