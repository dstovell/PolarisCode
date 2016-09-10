using UnityEngine;
using System.Collections;

public enum MagneticCharge
{
	None,
	Positive,
	Negative
}

public class GridPuzzleMagnetic : DSTools.MessengerListener
{
	public MagneticCharge charge = MagneticCharge.None;

	private MagneticCharge lastCharge = MagneticCharge.None;

	public GameObject NegitiveFX;
	public GameObject PositiveFX;


	public void SetCharge(MagneticCharge _charge)
	{
		this.charge = _charge;
	}

	public void UpdateMagnetic() 
	{
		if (this.charge != this.lastCharge)
		{
			if ((this.NegitiveFX != null) && (this.PositiveFX != null)) 
			{
				if (this.charge == MagneticCharge.Positive)
				{
					this.PositiveFX.SetActive(true);
					this.NegitiveFX.SetActive(false);
				}
				else if (this.charge == MagneticCharge.Negative)
				{
					this.PositiveFX.SetActive(false);
					this.NegitiveFX.SetActive(true);
				}
				else
				{
					this.PositiveFX.SetActive(false);
					this.NegitiveFX.SetActive(false);
				}
			}

			this.lastCharge = this.charge;
		}
	}
}

