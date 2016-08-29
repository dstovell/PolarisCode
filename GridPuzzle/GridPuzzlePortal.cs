using UnityEngine;
using System.Collections;

public class GridPuzzlePortal : MonoBehaviour
{
	public GridPuzzlePortal target;
	public GameObject portalInFX;

	private GridPuzzle parentPuzzle;

	void Start()
	{
		this.parentPuzzle =  this.transform.parent.gameObject.GetComponent<GridPuzzle>();
	}

	private IEnumerator EnableForTime(GameObject obj, float time)
    {
		obj.SetActive(true);
        yield return new WaitForSeconds(time);        
		obj.SetActive(false);
    }

	public void TriggerInFX()
    {
		if (this.portalInFX != null)
		{
			this.StartCoroutine(EnableForTime(this.portalInFX, 0.3f));
		}
    }

	public void OnTeleportedTo(GameObject obj)
    {
		DSTools.Messenger.SendMessageFrom("portal", "OnTeleportedTo", this.parentPuzzle, obj);
    }

	void OnTriggerEnter(Collider other)
    {
		GridPuzzlePlayerController controller = other.gameObject.GetComponent<GridPuzzlePlayerController>();
		if ((controller != null) && (this.target != null))
		{
			this.TriggerInFX();

			controller.TeleportTo(this.target.gameObject);
			other.gameObject.transform.parent = this.parentPuzzle.gameObject.transform;

			target.TriggerInFX();
			target.OnTeleportedTo(other.gameObject);
		}
    }
}

