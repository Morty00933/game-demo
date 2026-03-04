using UnityEngine;

public class Trap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = GlobalController.instance?.CurrentPlayer?.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.InstantDie();
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.TrapHit(9999f);
        }
    }
}
