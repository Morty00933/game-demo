using UnityEngine;
using System.Collections;

public class SerpentEnemy : Enemy
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private GameObject bloodEffectPrefab;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private GameObject poisonProjectilePrefab;
    [SerializeField] private Transform poisonSpawnPoint;

    private float health;
    private EnemyState currentState = EnemyState.Idle;
    private bool facingRight = true;
    private float lastFlipTime;
    private float lastAttackTime;
    private readonly Collider2D[] overlapResults = new Collider2D[1];
    private bool isPreparingAttack;
    private bool isMoving;

    protected override void Start()
    {
        base.Start();
        if (!enabled) return;
        health = config.maxHealth;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
        SetState(EnemyState.Walk);
    }

    protected override void Update()
    {
        base.Update();
        if (!IsAlive() || playerTransform == null || currentState == EnemyState.Death) return;

        if (isMoving && currentState == EnemyState.Walk)
        {
            float moveDirection = facingRight ? 1f : -1f;
            transform.position += new Vector3(moveDirection * config.speed * Time.deltaTime, 0f, 0f);
        }
    }

    protected override void UpdateEnemyState()
    {
        if (!IsAlive() || playerTransform == null || currentState == EnemyState.Death) return;

        bool playerDetected = IsPlayerInDetectZone();
        bool playerInRange = IsPlayerInAttackZone();
        float moveDirection = facingRight ? 1f : -1f;
        bool canMoveForward = !IsAtEdgeOrWall(moveDirection);

        if (isPreparingAttack || currentState == EnemyState.HeavyAttack) return;

        if (playerDetected && Time.time >= lastAttackTime + config.attackCooldown)
        {
            SetState(EnemyState.Idle);
            StartCoroutine(PrepareAttack());
        }
        else if (currentState == EnemyState.Walk)
        {
            if (!canMoveForward)
            {
                FlipAtEdgeOrWall();
            }
        }
        else if (currentState == EnemyState.Idle && !playerDetected)
        {
            SetState(EnemyState.Walk);
        }
    }

    private IEnumerator PrepareAttack()
    {
        isPreparingAttack = true;
        yield return new WaitForSeconds(config.attackDelay);
        if (IsAlive() && IsPlayerInAttackZone() && currentState != EnemyState.Death)
        {
            SetState(EnemyState.HeavyAttack);
        }
        else
        {
            SetState(EnemyState.Walk);
        }
        isPreparingAttack = false;
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
        SpawnBloodEffect();
        ApplyRecoil(hitDirection, hitForce);
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
        if (!isPreparingAttack && currentState != EnemyState.HeavyAttack)
        {
            SetState(EnemyState.Walk);
        }
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
    }

    public void OnHeavyAttack()
    {
        if (!IsPlayerInAttackZone() || poisonProjectilePrefab == null || poisonSpawnPoint == null) return;

        GameObject projectile = Instantiate(poisonProjectilePrefab, poisonSpawnPoint.position, Quaternion.identity);
        PoisonProjectile projectileScript = projectile.GetComponent<PoisonProjectile>();
        if (projectileScript != null)
        {
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
            projectileScript.Initialize(direction, config.poisonSpeed, config.poisonDamage, this);
        }
        AudioManager.Instance?.PlaySound(hitSound);
    }

    public void OnAttackEnd()
    {
        lastAttackTime = Time.time;
        if (IsPlayerInDetectZone())
        {
            SetState(EnemyState.Idle);
        }
        else
        {
            SetState(EnemyState.Walk);
        }
    }

    public void OnWalkStart()
    {
        if (currentState != EnemyState.Walk || !IsAlive()) return;
        isMoving = true;
    }

    public void OnWalkStop()
    {
        if (currentState != EnemyState.Walk || !IsAlive()) return;
        isMoving = false;
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
        collider.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Attackable");
        isPreparingAttack = false;
        isMoving = false;
        SetState(EnemyState.Walk);
    }

    private void SetState(EnemyState newState)
    {
        if (currentState == newState) return;
        if (currentState == EnemyState.Walk && newState != EnemyState.Walk)
        {
            OnWalkStop();
        }
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
        Vector3 ledgeCheckStart = collider.bounds.center + (facingRight ? new Vector3(config.ledgeCheckX, 0) : new Vector3(-config.ledgeCheckX, 0));
        Gizmos.DrawLine(ledgeCheckStart, ledgeCheckStart + Vector3.down * config.ledgeCheckY);

        Gizmos.color = Color.blue;
        Vector2 wallCheckDir = facingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(collider.bounds.center, collider.bounds.center + (Vector3)wallCheckDir * config.ledgeCheckX);
    }
}