using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected float damage;
    protected float speed;
    protected Vector2 direction;
    protected Rigidbody2D rb;
    protected Enemy owner;
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected bool useGravity = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"{gameObject.name} requires a Rigidbody2D component!");
            enabled = false;
        }
    }

    public virtual void Initialize(Vector2 direction, float speed, float damage, Enemy owner)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.owner = owner;

        if (rb != null)
        {
            rb.linearVelocity = this.direction * this.speed;
            rb.gravityScale = useGravity ? 1f : 0f;
        }

        transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        Destroy(gameObject, lifetime);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") &&
            GlobalController.instance != null && GlobalController.instance.CurrentPlayer != null)
        {
            var playerController = GlobalController.instance.CurrentPlayer.GetComponent<PlayerController>();
            if (playerController != null && !playerController.IsInvincible)
            {
                Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
                playerController.TakeDamage(damage, hitDirection, 2f);
                OnHitPlayer();
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            OnHitEnvironment();
        }
    }


    protected virtual void OnHitPlayer()
    {
        Destroy(gameObject);
    }

    protected virtual void OnHitEnvironment()
    {
        Destroy(gameObject);
    }

    protected virtual void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}