using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    private PlayerStateList pState;
    private PlayerPhysics physics;
    private PlayerAnimation anim;
    private int health;
    private float healTimer;
    private bool isHealingButtonHeld;
    public UnityEvent onHealthChanged;

    public int Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, config.maxHealth);
            onHealthChanged.Invoke();
            if (health <= 0) Die();
        }
    }

    public PlayerConfig Config => config;

    private void Awake()
    {
        pState = GetComponent<PlayerStateList>();
        physics = GetComponent<PlayerPhysics>();
        anim = GetComponent<PlayerAnimation>();
        Health = config.maxHealth;
    }

    public void UpdateHealing()
    {
        isHealingButtonHeld = Input.GetButton("Heal");
        if (isHealingButtonHeld && CanHeal())
        {
            if (healTimer == 0)
            {
                pState.currentState = PlayerStateList.State.Healing;
                anim.SetBool("isHealing", true);
            }
            healTimer += Time.deltaTime;
            if (healTimer >= config.timeToHeal)
            {
                Health += config.healAmount;
                healTimer = 0;
                StopHealing();
            }
        }
        else if (healTimer > 0)
            StopHealing();
    }

    private bool CanHeal()
    {
        return Health < config.maxHealth && physics.IsGrounded() && pState.currentState != PlayerStateList.State.Dashing;
    }

    private void StopHealing()
    {
        pState.currentState = PlayerStateList.State.Idle;
        anim.SetBool("isHealing", false);
        healTimer = 0;
    }

    public void TakeDamage(float damage)
    {
        if (!pState.isInvincible)
        {
            Health -= (int)damage;
            pState.isInvincible = true;
            StartCoroutine(ResetInvincibility());
        }
    }

    public void InstantDie()
    {
        Health = 0;
        Die();
    }

    private void Die()
    {
        pState.currentState = PlayerStateList.State.Dead;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<PlayerController>().enabled = false;
        Debug.Log("Player died, PlayerController disabled");
    }

    private IEnumerator ResetInvincibility()
    {
        yield return new WaitForSeconds(1f);
        pState.isInvincible = false;
    }
}