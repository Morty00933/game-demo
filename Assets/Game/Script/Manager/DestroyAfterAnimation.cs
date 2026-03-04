using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    void Start()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
            Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
        else
            Destroy(gameObject, 1f);
    }
}
