using UnityEngine;
using System.Collections;

public class CrystalEnemy : Enemy
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private GameObject crystalProjectilePrefab;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private float patrolRange = 5f;

    private float health;
    private EnemyState currentState = EnemyState.Idle;
    private float lastAttackTime;
    private bool isAttacking;
    private Vector2 startPosition;
    private bool movingRight = true;
    private bool facingRight = true;
    private readonly Collider2D[] overlapResults = new Collider2D[1];
    private bool wasPlayerDetectedLastFrame;

    protected override void Start()
    {
        base.Start();
        if (!enabled) return;
        health = config.maxHealth;
        startPosition = transform.position;
        StartCoroutine(StartPatrolling());
    }

    private IEnumerator StartPatrolling()
    {
        yield return new WaitForSeconds(0.1f);
        ChangeAnimation(EnemyState.Walk);
        currentState = EnemyState.Walk;
    }

    protected override void UpdateEnemyState()
    {
        if (!IsAlive() || playerTransform == null || currentState == EnemyState.Death) return;

        bool playerDetected = IsPlayerInDetectZone();
        bool playerInRange = IsPlayerInAttackZone();

        if (playerTransform != null)
        {
            facingRight = playerTransform.position.x > transform.position.x;
            transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
        }

        if (currentState == EnemyState.Walk)
        {
            Patrol();
            if (playerInRange && Time.time >= lastAttackTime + config.attackCooldown)
            {
                ChangeAnimation(EnemyState.Idle);
                currentState = EnemyState.Idle;
                StartCoroutine(PrepareAttack());
            }
            else if (playerDetected && !wasPlayerDetectedLastFrame && !isAttacking)
            {
            }
        }
        else if (currentState == EnemyState.Idle)
        {
            if (playerInRange && Time.time >= lastAttackTime + config.attackCooldown)
            {
                StartCoroutine(PrepareAttack());
            }
            else if (!playerInRange)
            {
                ChangeAnimation(EnemyState.Walk);
                currentState = EnemyState.Walk;
            }
        }
        else if (currentState == EnemyState.Attack)
        {
        }

        wasPlayerDetectedLastFrame = playerDetected;
    }

    private void Patrol()
    {
        bool playerInRange = IsPlayerInDetectZone();

        if (playerInRange)
        {
            Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            float targetDistance = config.detectSize.x / 2;
            float currentDistance = distanceToPlayer;

            if (currentDistance > targetDistance)
            {
                transform.position += (Vector3)(direction * config.speed * Time.deltaTime);
            }
            else if (currentDistance < targetDistance - 1f)
            {
                transform.position -= (Vector3)(direction * config.speed * Time.deltaTime);
            }
        }
        else
        {
            float newX = transform.position.x;
            if (movingRight)
            {
                newX += config.speed * Time.deltaTime;
                if (newX >= startPosition.x + patrolRange)
                {
                    movingRight = false;
                }
            }
            else
            {
                newX -= config.speed * Time.deltaTime;
                if (newX <= startPosition.x - patrolRange)
                {
                    movingRight = true;
                }
            }
            transform.position = new Vector2(newX, transform.position.y);
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

    private IEnumerator PrepareAttack()
    {
        yield return new WaitForSeconds(config.attackDelay);
        if (currentState == EnemyState.Idle && IsPlayerInAttackZone())
        {
            ChangeAnimation(EnemyState.Attack);
            currentState = EnemyState.Attack;
        }
    }

    public void OnAttackHit()
    {
        if (IsPlayerInAttackZone() && crystalProjectilePrefab != null)
        {
            ShootProjectile();
            AudioManager.Instance?.PlaySound(attackSound);
        }
    }

    public void OnAttackEnd()
    {
        lastAttackTime = Time.time;
        if (IsPlayerInAttackZone())
        {
            ChangeAnimation(EnemyState.Idle);
            currentState = EnemyState.Idle;
        }
        else
        {
            ChangeAnimation(EnemyState.Walk);
            currentState = EnemyState.Walk;
        }
    }

    private void ShootProjectile()
    {
        Vector2 targetPosition = playerTransform.position;
        GameObject projectile = Instantiate(crystalProjectilePrefab, transform.position, Quaternion.identity);
        CrystalProjectile projectileScript = projectile.GetComponent<CrystalProjectile>();
        if (projectileScript != null)
        {
            Vector2 direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
            float angle = 45f * Mathf.Deg2Rad;
            Vector2 launchDirection = new Vector2(Mathf.Cos(angle) * Mathf.Sign(direction.x), Mathf.Sin(angle));
            projectileScript.Initialize(launchDirection, config.poisonSpeed, config.poisonDamage, this);
        }
    }

    public override void EnemyGetsHit(float damage, Vector2 hitDirection, float hitForce)
    {
        health -= damage;
        ApplyRecoil(hitDirection, hitForce);
        StartCoroutine(HurtCoroutine());
    }

    public override void TrapHit(float damage)
    {
        health -= damage;
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
        ChangeAnimation(EnemyState.Walk);
        currentState = EnemyState.Walk;
    }

    private void Die()
    {
        ChangeAnimation(EnemyState.Death);
        currentState = EnemyState.Death;
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

    public override bool IsAlive() => health > 0;

    public override void Reset()
    {
        health = config.maxHealth;
        isAttacking = false;
        enemyRb.simulated = true;
        collider.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Attackable");
        movingRight = true;
        transform.position = startPosition;
        spriteRenderer.color = new Color(1, 1, 1, 1);
        ChangeAnimation(EnemyState.Walk);
        currentState = EnemyState.Walk;
    }

    public override void Turn() { }

    private void OnDrawGizmos()
    {
        if (!Application.isEditor) return;
        if (collider == null) collider = GetComponent<BoxCollider2D>();
        if (collider == null || config == null) return;

        Gizmos.color = Color.yellow;
        Vector3 leftPatrol = new Vector3(startPosition.x - patrolRange, startPosition.y, 0);
        Vector3 rightPatrol = new Vector3(startPosition.x + patrolRange, startPosition.y, 0);
        Gizmos.DrawLine(leftPatrol, rightPatrol);

        Gizmos.color = Color.green;
        Vector3 detectBoxCenter = collider.bounds.center + (facingRight ? Vector3.right : Vector3.left) * (config.detectSize.x / 2f);
        Gizmos.DrawWireCube(detectBoxCenter, config.detectSize);

        Gizmos.color = Color.red;
        Vector3 attackBoxCenter = collider.bounds.center + (facingRight ? Vector3.right : Vector3.left) * (config.attackSize.x / 2f);
        Gizmos.DrawWireCube(attackBoxCenter, config.attackSize);
    }
}