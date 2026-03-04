using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Recoil Settings")]
    [SerializeField] protected float recoilLength = 0.2f;
    [SerializeField] protected float recoilFactor = 5f;
    [SerializeField] protected bool isBounceable = true;

    protected bool isRecoiling;
    protected bool isAttacking;

    protected Rigidbody2D enemyRb;
    protected Transform playerTransform;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected AudioSource audioSource;
    protected float distanceToPlayer;
    protected BoxCollider2D collider;

    public static event System.Action<float> OnDealDamage;

    private static readonly int IsIdleHash = Animator.StringToHash("IsIdle");
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsAttackingQuickHash = Animator.StringToHash("IsAttackingQuick");
    private static readonly int IsAttackingHeavyHash = Animator.StringToHash("IsAttackingHeavy");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
    private static readonly int IsStunnedHash = Animator.StringToHash("IsStunned");

    private readonly Dictionary<EnemyState, int> animationHashes = new Dictionary<EnemyState, int>
    {
        { EnemyState.Idle, IsIdleHash },
        { EnemyState.Walk, IsWalkingHash },
        { EnemyState.QuickAttack, IsAttackingQuickHash },
        { EnemyState.HeavyAttack, IsAttackingHeavyHash },
        { EnemyState.Attack, IsAttackingHash },
        { EnemyState.Jump, IsJumpingHash },
        { EnemyState.Death, IsDeadHash },
        { EnemyState.Stunned, IsStunnedHash }
    };

    private EnemyState? currentAnimationState;
    private bool isStunned;
    private float stunTimer;
    private float recoilTimer;

    protected virtual void Start()
    {
        enemyRb = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (enemyRb == null || collider == null || animator == null || spriteRenderer == null)
        {
            enabled = false;
            Debug.LogError($"Enemy {gameObject.name} is missing required components!");
            return;
        }

        StartCoroutine(InitializeEnemy());
    }

    private IEnumerator InitializeEnemy()
    {
        float timeout = 5f;
        float elapsed = 0f;
        while (GlobalController.instance == null || GlobalController.instance.CurrentPlayer == null)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogError($"Enemy {gameObject.name} failed to initialize: GlobalController or CurrentPlayer not found!");
                enabled = false;
                yield break;
            }
            yield return null;
        }

        playerTransform = GlobalController.instance.CurrentPlayer.transform;
        currentAnimationState = null;
        Debug.Log($"Enemy {gameObject.name} initialized. Player found: {playerTransform.gameObject.name}");
    }

    protected virtual void Update()
    {
        if (GlobalController.instance == null || GlobalController.instance.CurrentPlayer == null)
            return;

        if (playerTransform == null) return;

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                ChangeAnimation(EnemyState.Idle);
            }
            return;
        }

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
                recoilTimer += Time.deltaTime;
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
                enemyRb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            UpdateEnemyState();
        }
    }

    public float GetDistanceToPlayer() => distanceToPlayer;

    public virtual void EnemyGetsHit(float damage, Vector2 hitDirection, float hitForce)
    {
        ApplyRecoil(hitDirection, hitForce);
    }

    protected void ApplyRecoil(Vector2 hitDirection, float hitForce)
    {
        if (!isRecoiling && enemyRb != null)
        {
            enemyRb.linearVelocity = hitForce * recoilFactor * hitDirection.normalized;
            isRecoiling = true;
        }
    }

    public virtual void TrapHit(float damage)
    {
        EnemyGetsHit(damage, Vector2.zero, 0f);
    }

    public void Stun(float duration)
    {
        if (!isStunned && IsAlive())
        {
            isStunned = true;
            stunTimer = duration;
            enemyRb.linearVelocity = Vector2.zero;
            ChangeAnimation(EnemyState.Stunned);
        }
    }

    public bool IsBounceable() => isBounceable;
    public bool IsAttacking() => isAttacking;

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (GlobalController.instance == null || GlobalController.instance.CurrentPlayer == null)
            return;

        if (collision.collider != null &&
            collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var playerState = collision.collider.GetComponent<PlayerStateList>();
            var playerHealth = collision.collider.GetComponent<PlayerHealth>();
            var playerAttack = collision.collider.GetComponent<PlayerAttack>();
            var pc = GlobalController.instance.CurrentPlayer.GetComponent<PlayerController>();

            if (playerState != null && playerHealth != null && playerAttack != null &&
                pc != null && !pc.IsInvincible &&
                playerState.currentState != PlayerStateList.State.Cutscene &&
                IsAlive() &&
                !isStunned)
            {
                AttackPlayer();
            }
        }
    }

    public abstract void Turn();
    public abstract void AttackPlayer();
    public abstract bool IsAlive();
    public abstract void Reset();

    protected virtual void UpdateEnemyState() { }

    protected void ChangeAnimation(EnemyState state)
    {
        if (animator == null || currentAnimationState == state || !animationHashes.ContainsKey(state)) return;

        if (currentAnimationState.HasValue && animationHashes.ContainsKey(currentAnimationState.Value))
        {
            animator.SetBool(animationHashes[currentAnimationState.Value], false);
        }

        animator.SetBool(animationHashes[state], true);
        currentAnimationState = state;
    }

    public void DealDamageToPlayer(float damage)
    {
        isAttacking = true;

        if (GlobalController.instance == null || GlobalController.instance.CurrentPlayer == null)
            return;

        var pc = GlobalController.instance.CurrentPlayer.GetComponent<PlayerController>();
        if (pc != null && !pc.IsInvincible)
        {
            pc.TakeDamage(damage);
            Debug.Log($"Enemy {gameObject.name} dealt {damage} damage to player.");
        }

        Invoke(nameof(ResetAttack), 0.5f);
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    protected enum EnemyState
    {
        Idle,
        Walk,
        QuickAttack,
        HeavyAttack,
        Attack,
        Jump,
        Death,
        Stunned
    }

    protected IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }
}
