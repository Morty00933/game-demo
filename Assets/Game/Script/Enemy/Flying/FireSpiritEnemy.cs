using UnityEngine;
using System.Collections;

public class FireSpiritEnemy : Enemy
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField][Range(1f, 10f)] private float patrolRange = 5f;
    [SerializeField] private string enemyPoolTag = "FireSpiritEnemy";
    [SerializeField] private LayerMask obstacleLayer;

    private Vector2 startPosition;
    private bool isShootable = true;
    private bool playerInDetectZone;
    private float health;
    private bool movingRight = true;

    private enum EnemyStates { Idle, Patrol, Chase, Attack, Death }
    private EnemyStates currentEnemyState;

    protected override void Start()
    {
        base.Start();
        if (!enabled) return;

        startPosition = transform.position;
        health = config != null ? config.maxHealth : 100f;
        ChangeState(EnemyStates.Patrol);
    }

    protected override void Update()
    {
        base.Update();
        if (GlobalController.instance.CurrentPlayer == null)
        {
            ChangeState(EnemyStates.Idle);
            return;
        }

        PlayerHealth playerHealth = GlobalController.instance.CurrentPlayer.GetComponent<PlayerHealth>();
        if (playerHealth == null || playerHealth.Health <= 0)
        {
            ChangeState(EnemyStates.Idle);
            return;
        }

        if (playerInDetectZone)
        {
            distanceToPlayer = Vector2.Distance(transform.position, GlobalController.instance.CurrentPlayer.transform.position);
        }
    }

    protected override void UpdateEnemyState()
    {
        if (!IsAlive()) return;

        float dist = Vector2.Distance(transform.position, GlobalController.instance.CurrentPlayer.transform.position);

        switch (currentEnemyState)
        {
            case EnemyStates.Idle:
                enemyRb.linearVelocity = Vector2.zero;
                ChangeAnimation(EnemyState.Idle);
                if (playerInDetectZone && dist < config.detectSize.x / 2f)
                    ChangeState(EnemyStates.Chase);
                break;

            case EnemyStates.Patrol:
                ChangeAnimation(EnemyState.Walk);
                float yOffset = Mathf.Sin(Time.time * 2f) * 0.5f;
                float patrolSpeed = movingRight ? config.speed : -config.speed;

                if (CheckForObstacle(patrolSpeed))
                {
                    movingRight = !movingRight;
                    patrolSpeed = movingRight ? config.speed : -config.speed;
                }

                enemyRb.linearVelocity = new Vector2(patrolSpeed, yOffset);

                if ((movingRight && transform.position.x >= startPosition.x + patrolRange) ||
                    (!movingRight && transform.position.x <= startPosition.x - patrolRange))
                {
                    movingRight = !movingRight;
                }

                Flip();
                if (playerInDetectZone && dist < config.detectSize.x / 2f)
                    ChangeState(EnemyStates.Chase);
                break;

            case EnemyStates.Chase:
                ChangeAnimation(EnemyState.Walk);
                Vector2 direction = (GlobalController.instance.CurrentPlayer.transform.position - transform.position).normalized;
                enemyRb.linearVelocity = direction * config.speed;

                Flip();
                if (IsPlayerWithinAttackZone())
                    ChangeState(EnemyStates.Attack);
                else if (dist > config.detectSize.x / 2f || !playerInDetectZone)
                    ChangeState(EnemyStates.Patrol);
                break;

            case EnemyStates.Attack:
                enemyRb.linearVelocity = Vector2.zero;
                var playerController = GlobalController.instance.CurrentPlayer.GetComponent<PlayerController>();
                if (playerController != null && !playerController.IsInvincible)
                {
                    Flip();
                    ShootPlayer();
                }
                ChangeAnimation(EnemyState.Attack);
                if (!IsPlayerWithinAttackZone())
                    ChangeState(EnemyStates.Chase);
                else if (!playerInDetectZone || dist > config.detectSize.x / 2f)
                    ChangeState(EnemyStates.Patrol);
                break;

            case EnemyStates.Death:
                ChangeAnimation(EnemyState.Death);
                break;
        }
    }

    private bool CheckForObstacle(float speed)
    {
        Vector2 rayDirection = speed > 0 ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, 1f, obstacleLayer);
        return hit.collider != null;
    }

    private bool IsPlayerWithinAttackZone()
    {
        Vector2 attackCenter = transform.position;
        Vector2 attackSize = config.attackSize;
        Collider2D hit = Physics2D.OverlapBox(attackCenter, attackSize, 0f, LayerMask.GetMask("Player"));
        return hit != null;
    }

    private void Flip()
    {
        if (GlobalController.instance.CurrentPlayer == null) return;

        float playerPosX = GlobalController.instance.CurrentPlayer.transform.position.x;
        float enemyPosX = transform.position.x;

        if (currentEnemyState == EnemyStates.Chase || currentEnemyState == EnemyStates.Attack)
        {
            if (playerPosX < enemyPosX && transform.localScale.x > 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (playerPosX > enemyPosX && transform.localScale.x < 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (currentEnemyState == EnemyStates.Patrol)
        {
            if (enemyRb.linearVelocity.x < 0 && transform.localScale.x > 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (enemyRb.linearVelocity.x > 0 && transform.localScale.x < 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void ShootPlayer()
    {
        if (isShootable && IsPlayerWithinAttackZone())
        {
            isShootable = false;
            Vector2 direction = (GlobalController.instance.CurrentPlayer.transform.position - transform.position).normalized;
            StartCoroutine(ShootPlayerCoroutine(direction, config.attackCooldown));
        }
    }

    private IEnumerator ShootPlayerCoroutine(Vector2 direction, float shootInterval)
    {
        yield return new WaitForSeconds(0.2f);

        if (AudioManager.Instance != null && attackSound != null)
        {
            AudioManager.Instance.PlaySound(attackSound);
        }

        float startAngle = -15f * (3 - 1) / 2;
        for (int i = 0; i < 3; i++)
        {
            GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
            float angle = startAngle + 15f * i;
            Vector2 angledDirection = Quaternion.Euler(0, 0, angle) * direction.normalized;

            Fireball fireballScript = fireball.GetComponent<Fireball>();
            if (fireballScript != null)
                fireballScript.Initialize(angledDirection, config.poisonSpeed, config.poisonDamage, this);
        }

        Vector2 recoilDir = -direction.normalized;
        ApplyRecoil(recoilDir, 2f);

        yield return new WaitForSeconds(shootInterval);
        isShootable = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInDetectZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInDetectZone = false;
        }
    }

    private IEnumerator FadeCoroutine()
    {
        GlobalController.instance.AddScore(config.scoreValue);
        float elapsedTime = 0f;
        float destroyDelay = config.destroyDelay;

        while (elapsedTime < destroyDelay)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / destroyDelay);
            spriteRenderer.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        Reset();
        gameObject.SetActive(false);
    }

    public override void EnemyGetsHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        health -= damageDone;
        ApplyRecoil(hitDirection, hitForce);
        if (!IsAlive())
        {
            ChangeState(EnemyStates.Death);
        }
    }

    public override void AttackPlayer()
    {
        var pc = GlobalController.instance.CurrentPlayer.GetComponent<PlayerController>();
        if (pc != null && !pc.IsInvincible)
        {
            Vector2 hitDirection = (pc.transform.position - transform.position).normalized;
            pc.TakeDamage(config.poisonDamage, hitDirection, 2f);
        }
    }

    public override void Turn()
    {
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
    }

    public override bool IsAlive()
    {
        return health > 0;
    }

    public override void Reset()
    {
        health = config.maxHealth;
        isShootable = true;
        playerInDetectZone = false;
        movingRight = true;
        transform.position = startPosition;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        spriteRenderer.color = new Color(1, 1, 1, 1);
        enemyRb.linearVelocity = Vector2.zero;
        enemyRb.simulated = true;
        collider.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Attackable");
        ChangeState(EnemyStates.Patrol);
    }

    public override void TrapHit(float damageDone)
    {
        health -= damageDone;
        if (!IsAlive())
        {
            ChangeState(EnemyStates.Death);
        }
    }

    private void ChangeState(EnemyStates newState)
    {
        if (currentEnemyState != newState)
        {
            currentEnemyState = newState;
            if (newState == EnemyStates.Death)
            {
                if (audioSource != null && deathSound != null)
                {
                    audioSource.PlayOneShot(deathSound);
                }
                enemyRb.gravityScale = 6f;
                gameObject.layer = LayerMask.NameToLayer("Decoration");
                StartCoroutine(FadeCoroutine());
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isEditor || config == null) return;

        Gizmos.color = Color.yellow;
        Vector3 leftPatrol = new Vector3(startPosition.x - patrolRange, startPosition.y, 0);
        Vector3 rightPatrol = new Vector3(startPosition.x + patrolRange, startPosition.y, 0);
        Gizmos.DrawLine(leftPatrol, rightPatrol);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, config.detectSize.x / 2f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, config.attackSize);
    }
}
