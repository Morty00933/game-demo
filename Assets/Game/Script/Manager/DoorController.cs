using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.LogError("DoorController: Animator component not found!");
    }

    public void OpenDoor()
    {
        if (anim == null) return;
        anim.SetBool("isOpening", true);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
            box.enabled = false;
    }
}
