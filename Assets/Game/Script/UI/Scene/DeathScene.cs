using UnityEngine;

public class DeathScene : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKeyDown && GlobalController.instance != null)
        {
            GlobalController.instance.CompleteRespawn();
        }
    }
}
