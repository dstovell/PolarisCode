using UnityEngine;
using System.Collections;

public class GridPuzzlePortal : MonoBehaviour
{
	public GridPuzzlePortal target;
	public GameObject portalInFX;
	//public GameObject portalOutFX;

	private IEnumerator EnableForTime(GameObject obj, float time)
    {
		obj.SetActive(true);
        yield return new WaitForSeconds(time);        
		obj.SetActive(false);
    }

	void OnTriggerEnter(Collider other)
    {
		GridPuzzlePlayerController controller = other.gameObject.GetComponent<GridPuzzlePlayerController>();
		if ((controller != null) && (this.target != null))
		{
			if (this.portalInFX != null)
			{
				this.StartCoroutine(EnableForTime(this.portalInFX, 0.3f));
			}

			controller.TeleportTo(this.target.gameObject);

			if (target.portalInFX != null)
			{
				this.StartCoroutine(EnableForTime(target.portalInFX, 0.3f));
			}
		}
    }
}

