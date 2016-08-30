using UnityEngine;
using System.Collections;

namespace DSTools
{

public class MaterialManager : MonoBehaviour 
{
	public Material MetalMat;
	public Material PlasticMat;
	public Material GlassMat;

	public Material PositiveChargeMat;
	public Material NegativeChargeMat;

	static public MaterialManager Instance { get; private set; }

	public Material GetSufaceMaterial(SurfaceMaterial materialType, MagneticCharge charge = MagneticCharge.None)
	{
		if (charge != MagneticCharge.None)
		{
			return this.GetMagneticChargeMaterial(charge);
		}

		switch(materialType)
		{
			case SurfaceMaterial.Metal:
				return this.MetalMat;
			case SurfaceMaterial.Plastic:
				return this.PlasticMat;
			case SurfaceMaterial.Glass:
				return this.GlassMat;
			default:
				return null;
		}
	}

	public Material GetMagneticChargeMaterial(MagneticCharge charge)
	{
		switch(charge)
		{
			case MagneticCharge.Positive:
				return this.PositiveChargeMat;
			case MagneticCharge.Negative:
				return this.NegativeChargeMat;
			default:
				return null;
		}
	}

	void Awake() 
	{
		MaterialManager.Instance = this;
	}
}

}