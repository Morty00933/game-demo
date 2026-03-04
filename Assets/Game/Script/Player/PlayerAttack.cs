using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Transform attackForwardPoint;
    [SerializeField] private Transform upAttackPoint;
    [SerializeField] private Transform downAttackPoint;
    private Rigidbody2D rb;
    private PlayerStateList pState;
    private PlayerPhysics physics;
    private PlayerParry parry;
    private PlayerAnimation anim;
    private ParticlesController particles;
    private float timeSinceAttack;
    private int comboStep;
    private bool attackable = true;

    public event System.Action<Transform, float, Vector2, float, float, int, bool> Hit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pState = GetComponent<PlayerStateList>();
        physics = GetComponent<PlayerPhysics>();
        parry = GetComponent<PlayerParry>();
        anim = GetComponent<PlayerAnimation>();
        particles = GetComponent<ParticlesController>();

        if (rb == null) Debug.LogError("PlayerAttack: Rigidbody2D not found!");
        if (pState == null) Debug.LogError("PlayerAttack: PlayerStateList not found!");
        if (physics == null) Debug.LogError("PlayerAttack: PlayerPhysics not found!");
        if (anim == null) Debug.LogError("PlayerAttack: PlayerAnimation not found!");
        if (config == null) Debug.LogError("PlayerAttack: PlayerConfig not assigned!");
        if (attackForwardPoint == null) Debug.LogError("PlayerAttack: attackForwardPoint not assigned!");
        if (upAttackPoint == null) Debug.LogError("PlayerAttack: upAttackPoint not assigned!");
        if (downAttackPoint == null) Debug.LogError("PlayerAttack: downAttackPoint not assigned!");
    }

    private void Update()
    {
        timeSinceAttack += Time.deltaTime;
        if (timeSinceAttack > config.comboResetTime) comboStep = 0;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAttackState();
        }
    }

    public void PerformAttack(float yAxis)
    {
        if (!attackable) return;
        if (timeSinceAttack < config.attackInterval) return;

        timeSinceAttack = 0;

        if (parry != null && parry.CanParry())
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, config.parryRange, config.attackableLayer);
            foreach (var enemy in enemies)
            {
                if (enemy.GetComponent<Enemy>()?.IsAttacking() == true)
                {
                    parry.Parry();
                    return;
                }
            }
        }

        if (yAxis > 0)
        {
            anim.SetTrigger("AttackUp");
            PerformHit(upAttackPoint, config.upAttackRange, Vector2.up, config.recoilYSpeed, config.damage, 1, false);
            particles?.AttackEffect(0);
            StartCoroutine(AttackCooldown());
        }
        else if (yAxis < 0 && !physics.IsGrounded())
        {
            anim.SetTrigger("AttackDown");
            PerformHit(downAttackPoint, config.downAttackRange, Vector2.down, config.recoilYSpeed, config.damage, 1, true);
            particles?.AttackEffect(-1);
            StartCoroutine(AttackCooldown());
        }
        else
        {
            AttackForward(comboStep + 1);
        }
    }

    private void AttackForward(int step)
    {
        float damageMultiplier = step == 1 ? 1f : step == 2 ? 1.25f : 1.5f;
        float finalDamage = config.damage * damageMultiplier * (parry != null && parry.IsParryBonusActive() ? 2f : 1f);
        int direction = pState.lookingRight ? 1 : -1;

        anim.SetTrigger(step == 1 ? "Attack" : step == 2 ? "SecondAtk" : "ThirdAtk");
        PerformHit(attackForwardPoint, config.sideAttackRange, Vector2.right * direction, config.recoilXSpeed, finalDamage, step, false);
        particles?.AttackEffect(step);
        StartCoroutine(AttackCooldown());
        if (step == 3) comboStep = 0;
        else comboStep++;
    }

    private void PerformHit(Transform attackPoint, float attackRange, Vector2 attackDirection, float recoilSpeed, float damage, int step, bool isDownAttack)
    {
        Hit?.Invoke(attackPoint, attackRange, attackDirection, recoilSpeed, damage, step, isDownAttack);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, config.attackableLayer);
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive())
            {
                Vector2 hitDirection = (hit.transform.position - transform.position).normalized;
                enemy.EnemyGetsHit(damage, hitDirection, recoilSpeed);
            }
            Projectile projectile = hit.GetComponent<Projectile>();
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }
    }

    private IEnumerator AttackCooldown()
    {
        attackable = false;
        yield return new WaitForSeconds(config.attackInterval);
        attackable = true;
    }

    public void ResetAttackState()
    {
        attackable = true;
        timeSinceAttack = config != null ? config.attackInterval : 0.5f;
        comboStep = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        if (attackForwardPoint != null)
        {
            Gizmos.DrawSphere(attackForwardPoint.position, config.sideAttackRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackForwardPoint.position, Vector3.one * 0.1f);
        }
        if (upAttackPoint != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(upAttackPoint.position, config.upAttackRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(upAttackPoint.position, Vector3.one * 0.1f);
        }
        if (downAttackPoint != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(downAttackPoint.position, config.downAttackRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(downAttackPoint.position, Vector3.one * 0.1f);
        }
    }
}