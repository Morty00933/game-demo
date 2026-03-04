using UnityEngine;
using System.Collections;

public class BerserkEnemy : Enemy
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private GameObject bloodEffectPrefab;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    private float health;
    private EnemyState currentState = EnemyState.Idle;
    private bool facingRight = true;
    private float lastFlipTime;
    private float lastAttackTime;
    private bool isCharging;
    private readonly Collider2D[] overlapResults = new Collider2D[1];

    protected override void Start()
    {
        base.Start();
        if (!enabled) return;
        health = config.maxHealth;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        StartCoroutine(StartPatrolling());
    }

    private IEnumerator StartPatrolling()
    {
        yield return new WaitForSeconds(0.1f);
        SetState(EnemyState.Walk);
    }

    protected override void UpdateEnemyState()
    {
        if (!IsAlive() || playerTransform == null || currentState == EnemyState.Death) return;

        bool playerDetected = IsPlayerInDetectZone();
        bool playerInRange = IsPlayerInAttackZone();
        float moveDirection = facingRight ? 1f : -1f;
        bool canMoveForward = !IsAtEdgeOrWall(moveDirection);

        if (playerInRange && Time.time >= lastAttackTime + config.attackCooldown)
        {
            enemyRb.linearVelocity = Vector2.zero;
            SetState(EnemyState.Idle);
            StartCoroutine(PrepareAttack());
        }
        else if (currentState == EnemyState.Walk)
        {
            if (playerDetected && !isCharging)
            {
                StartCoroutine(ChargeCoroutine(moveDirection));
            }
            else if (!canMoveForward)
            {
                FlipAtEdgeOrWall();
            }
            else
            {
                enemyRb.linearVelocity = new Vector2(moveDirection * config.speed, enemyRb.linearVelocity.y);
            }
        }
        else if (currentState == EnemyState.Idle && !playerInRange)
        {
            SetState(EnemyState.Walk);
        }
    }

    private IEnumerator PrepareAttack()
    {
        yield return new WaitForSeconds(config.attackDelay);
        if (currentState == EnemyState.Idle && IsPlayerInAttackZone())
        {
            SetState(EnemyState.QuickAttack);
        }
    }

    private IEnumerator ChargeCoroutine(float moveDirection)
    {
        isCharging = true;
        enemyRb.linearVelocity = new Vector2(moveDirection * config.speed * config.chaseSpeedMultiplier, enemyRb.linearVelocity.y);
        yield return new WaitForSeconds(config.dashDuration);
        isCharging = false;
        if (IsPlayerInDetectZone())
            enemyRb.linearVelocity = new Vector2(moveDirection * config.speed * config.chaseSpeedMultiplier, enemyRb.linearVelocity.y);
        else
            enemyRb.linearVelocity = new Vector2(moveDirection * config.speed, enemyRb.linearVelocity.y);
    }

    private bool IsAtEdgeOrWall(float direction)
    {
        Vector2 ledgeCheckStart = (Vector2)collider.bounds.center + new Vector2(direction * config.ledgeCheckX, 0f);
        bool isGrounded = Physics2D.Raycast(ledgeCheckStart, Vector2.down, config.ledgeCheckY, config.whatIsGround).collider != null;
        Vector2 wallCheckDir = direction > 0 ? Vector2.right : Vector2.left;
        bool hitWall = Physics2D.Raycast(collider.bounds.center, wallCheckDir, config.ledgeCheckX, config.whatIsGround).collider != null;
        return !isGrounded || hitWall;
    }

    private void FlipAtEdgeOrWall()
    {
        if (Time.time - lastFlipTime < config.flipCooldown) return;
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
        lastFlipTime = Time.time;
    }

    public override void EnemyGetsHit(float damage, Vector2 hitDirection, float hitForce)
    {
        health -= damage * config.armorDamageReduction;
        ApplyRecoil(hitDirection, hitForce);
        SpawnBloodEffect();
        StartCoroutine(HurtCoroutine());
    }

    public override void TrapHit(float damage)
    {
        health -= damage * config.armorDamageReduction;
        StartCoroutine(HurtCoroutine());
    }

    private IEnumerator HurtCoroutine()
    {
        if (!IsAlive())
        {
            Die();
            yield break;
        }
        yield return new WaitForSeconds(recoilLength);
        SetState(EnemyState.Walk);
    }

    private void Die()
    {
        AudioManager.Instance?.PlaySound(deathSound);
        SetState(EnemyState.Death);
        enemyRb.linearVelocity = Vector2.zero;
        enemyRb.simulated = false;
        collider.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Decoration");
        GlobalController.instance.AddScore(config.scoreValue);
        StartCoroutine(FadeCoroutine());
    }

    private IEnumerator FadeCoroutine()
    {
        float alpha = spriteRenderer.color.a;
        float elapsedTime = 0f;
        while (elapsedTime < config.destroyDelay)
        {
            elapsedTime += Time.deltaTime;
            alpha = Mathf.Lerp(1f, 0f, elapsedTime / config.destroyDelay);
            spriteRenderer.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        Reset();
        gameObject.SetActive(false);
    }

    public override void AttackPlayer()
    {
        // �����, ����������� ����� OnAttackHit
    }

    public void OnAttackHit()
    {
        lastAttackTime = Time.time;
        var player = GlobalController.instance.CurrentPlayer?.GetComponent<PlayerController>();
        if (IsPlayerInAttackZone() && player != null && !player.IsInvincible)
        {
            Vector2 hitDirection = (player.transform.position - transform.position).normalized;
            player.TakeDamage(config.quickDamage, hitDirection, 2f);
            AudioManager.Instance?.PlaySound(hitSound);
            Debug.Log($"BerserkEnemy dealt {config.quickDamage} damage to player.");
        }
        Invoke(nameof(OnAttackEnd), 0.2f);
    }

    public void OnAttackEnd()
    {
        if (IsPlayerInAttackZone())
        {
            SetState(EnemyState.Idle);
        }
        else
        {
            SetState(EnemyState.Walk);
        }
    }

    private bool IsPlayerInDetectZone()
    {
        Vector2 detectBoxOffset = (facingRight ? Vector2.right : Vector2.left) * (config.detectSize.x / 2f);
        Vector2 detectBoxCenter = (Vector2)collider.bounds.center + detectBoxOffset;
        int hitCount = Physics2D.OverlapBoxNonAlloc(detectBoxCenter, config.detectSize, 0f, overlapResults, LayerMask.GetMask("Player"));
        return hitCount > 0;
    }

    private bool IsPlayerInAttackZone()
    {
        Vector2 attackBoxCenter = (Vector2)collider.bounds.center + (facingRight ? Vector2.right : Vector2.left) * (config.attackSize.x / 2f);
        int hitCount = Physics2D.OverlapBoxNonAlloc(attackBoxCenter, config.attackSize, 0f, overlapResults, LayerMask.GetMask("Player"));
        return hitCount > 0;
    }

    private void SpawnBloodEffect()
    {
        if (bloodEffectPrefab == null) return;
        GameObject blood = ObjectPool.Instance?.GetPooledObject("BloodEffect");
        if (blood != null)
        {
            blood.transform.position = collider.bounds.center;
            blood.SetActive(true);
            StartCoroutine(DeactivateAfterDelay(blood, 2.5f));
        }
    }

    public override void Turn() => FlipAtEdgeOrWall();

    public override bool IsAlive() => health > 0;

    public override void Reset()
    {
        health = config.maxHealth;
        facingRight = true;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        spriteRenderer.color = new Color(1, 1, 1, 1);
        enemyRb.simulated = true;
        enemyRb.linearVelocity = Vector2.zero;
        collider.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Attackable");
        isCharging = false;
        lastAttackTime = 0f;
        lastFlipTime = 0f;
        SetState(EnemyState.Walk);
    }

    private void SetState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        ChangeAnimation(newState);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isEditor) return;
        if (collider == null) collider = GetComponent<BoxCollider2D>();
        if (collider == null || config == null) return;

        Gizmos.color = Color.red;
        Vector3 attackBoxCenter = collider.bounds.center + (facingRight ? Vector3.right : Vector3.left) * (config.attackSize.x / 2f);
        Gizmos.DrawWireCube(attackBoxCenter, config.attackSize);

        Gizmos.color = Color.green;
        Vector3 detectBoxCenter = collider.bounds.center + (facingRight ? Vector3.right : Vector3.left) * (config.detectSize.x / 2f);
        Gizmos.DrawWireCube(detectBoxCenter, config.detectSize);

        Gizmos.color = Color.yellow;
        Vector3 ledgeCheckStart = collider.bounds.center + (facingRight ? new Vector3(config.ledgeCheckX, 0) : new Vector3(-config.ledgeCheckX, 0));
        Gizmos.DrawLine(ledgeCheckStart, ledgeCheckStart + Vector3.down * config.ledgeCheckY);
    }
}
