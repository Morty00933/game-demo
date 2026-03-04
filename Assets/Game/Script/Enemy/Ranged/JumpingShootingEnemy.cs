using UnityEngine;
using System.Collections;

public class JumpingShootingEnemy : Enemy
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private GameObject bloodEffectPrefab;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private GameObject heavyLaserPrefab;
    [SerializeField] private GameObject quickLaserPrefab;
    [SerializeField] private Transform laserSpawnPoint;

    private float health;
    private EnemyState currentState = EnemyState.Idle;
    private bool facingRight = true;
    private float lastFlipTime;
    private float lastAttackTime;
    private float lastJumpTime;
    private readonly Collider2D[] overlapResults = new Collider2D[1];
    private bool hasFiredInAnimation;
    private bool isPreparingAttack = false;

    protected override void Start()
    {
        base.Start();
        if (!enabled) return;
        health = config.maxHealth;
        transform.rotation = Quaternion.identity;
        SetState(EnemyState.Idle);
    }

    protected override void UpdateEnemyState()
    {
        if (!IsAlive() || playerTransform == null || currentState == EnemyState.Death)
            return;

        bool playerInRange = IsPlayerInAttackZone();

        if (playerInRange)
        {
            enemyRb.linearVelocity = Vector2.zero;
            if (!isPreparingAttack && currentState == EnemyState.Idle && Time.time >= lastAttackTime + config.attackCooldown)
            {
                isPreparingAttack = true;
                StartCoroutine(PrepareAttack());
            }
            return;
        }

        if (currentState != EnemyState.Jump)
        {
            if (Time.time >= lastJumpTime + config.patrolJumpCooldown)
            {
                float dir = facingRight ? 1f : -1f;
                if (IsAtEdgeOrWall(dir))
                {
                    FlipAtEdgeOrWall();
                    dir = facingRight ? 1f : -1f;
                }
                OnJumpStart();
                return;
            }
            enemyRb.linearVelocity = Vector2.zero;
            SetState(EnemyState.Idle);
        }
    }

    private IEnumerator PrepareAttack()
    {
        yield return new WaitForSeconds(config.attackDelay);

        hasFiredInAnimation = false;
        if (Random.value > 0.5f)
            SetState(EnemyState.HeavyAttack);
        else
            SetState(EnemyState.QuickAttack);
    }

    private bool IsAtEdgeOrWall(float direction)
    {
        Vector2 ledgeStart = (Vector2)collider.bounds.center + new Vector2(direction * config.ledgeCheckX, 0f);
        bool isGrounded = Physics2D.Raycast(ledgeStart, Vector2.down, config.ledgeCheckY, config.whatIsGround).collider != null;
        Vector2 wallDir = direction > 0 ? Vector2.right : Vector2.left;
        bool hitWall = Physics2D.Raycast(collider.bounds.center, wallDir, config.ledgeCheckX, config.whatIsGround).collider != null;
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
        SetState(EnemyState.Idle);
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
        if (!IsPlayerInAttackZone() || heavyLaserPrefab == null || laserSpawnPoint == null)
            return;

        GameObject laser = Instantiate(heavyLaserPrefab, laserSpawnPoint.position, Quaternion.identity);
        Laser laserScript = laser.GetComponent<Laser>();
        if (laserScript != null)
        {
            laserScript.Initialize(
                facingRight ? Vector2.right : Vector2.left,
                config.laserSpeed,
                config.heavyDamage,
                this
            );
            AudioManager.Instance?.PlaySound(hitSound);
        }
    }

    public void OnQuickAttack()
    {
        if (!IsPlayerInAttackZone() || quickLaserPrefab == null || laserSpawnPoint == null)
            return;

        GameObject laser = Instantiate(quickLaserPrefab, laserSpawnPoint.position, Quaternion.identity);
        Laser laserScript = laser.GetComponent<Laser>();
        if (laserScript != null)
        {
            laserScript.Initialize(
                facingRight ? Vector2.right : Vector2.left,
                config.laserSpeed,
                config.quickDamage,
                this
            );
            AudioManager.Instance?.PlaySound(hitSound);
        }
    }

    public void OnAttackEnd()
    {
        lastAttackTime = Time.time;
        SetState(EnemyState.Idle);
        hasFiredInAnimation = false;
        isPreparingAttack = false;
    }

    public void OnJumpStart()
    {
        if (Time.time < lastJumpTime + config.jumpCooldown) return;
        float dir = facingRight ? 1f : -1f;
        enemyRb.AddForce(new Vector2(dir * config.speed, config.jumpForce), ForceMode2D.Impulse);
        SetState(EnemyState.Jump);
        lastJumpTime = Time.time;
    }

    public void OnJumpEnd()
    {
        SetState(EnemyState.Idle);
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
        SetState(EnemyState.Idle);
        spriteRenderer.color = new Color(1, 1, 1, 1);
        enemyRb.simulated = true;
        collider.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Attackable");
        hasFiredInAnimation = false;
        isPreparingAttack = false;
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
        Vector3 ledgeCheckStart = collider.bounds.center + (facingRight ? new Vector3(config.ledgeCheckX, 0) : new Vector3(-config.ledgeCheckX, 0));
        Gizmos.DrawLine(ledgeCheckStart, ledgeCheckStart + Vector3.down * config.ledgeCheckY);

        Gizmos.color = Color.blue;
        Vector2 wallCheckDir = facingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(collider.bounds.center, collider.bounds.center + (Vector3)wallCheckDir * config.ledgeCheckX);
    }
}