using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    private PlayerStateList pState;
    private PlayerMovement movement;
    private PlayerJump jump;
    private PlayerDash dash;
    private PlayerAttack attack;
    private PlayerParry parry;
    private PlayerHealth health;
    private PlayerClimb climb;
    private PlayerCeilingClimb ceilingClimb;
    private PlayerShield shield;
    private PlayerAnimation anim;
    private PlayerPhysics physics;
    private Rigidbody2D rb;

    private bool inputEnable = true;
    private bool isRecoiling;
    private float recoilTimer;
    private float recoilLength = 0.2f;
    private float recoilFactor = 5f;

    public bool InputEnable { get => inputEnable; set => inputEnable = value; }
    public bool IsAlive => pState != null && pState.currentState != PlayerStateList.State.Dead;
    public bool IsInvincible => pState != null && pState.isInvincible;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        if (health != null && config != null)
        {
            health.Health = config.maxHealth;
            health.onHealthChanged?.Invoke();
        }
    }

    private void Update()
    {
        if (!IsAlive || !inputEnable) return;

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
                recoilTimer += Time.deltaTime;
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);
            }
            return;
        }

        float xAxis = Input.GetAxisRaw("Horizontal");
        float yAxis = Input.GetAxisRaw("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isGrounded = physics.IsGrounded();
        bool attackPressed = Input.GetButtonDown("Attack");
        bool dashPressed = Input.GetButtonDown("Dash");
        bool parryPressed = Input.GetButtonDown("Parry");
        bool shieldPressed = Input.GetButtonDown("Shield");

        movement?.Move(xAxis, isRunning);
        jump?.UpdateJump();
        if (dashPressed) dash?.PerformDash(xAxis);
        if (attackPressed) attack?.PerformAttack(yAxis);
        if (parryPressed) parry?.TryParry();
        if (shieldPressed) shield?.ActivateShield();
        climb?.WallClimb(xAxis, yAxis);
        ceilingClimb?.CeilingClimb(xAxis, yAxis);
        health?.UpdateHealing();
        anim?.UpdateAnimations(xAxis, isRunning, isGrounded);
    }

    public void TakeDamage(float damage, Vector2 hitDirection, float hitForce)
    {
        if (!IsInvincible && IsAlive)
        {
            health?.TakeDamage(damage);
            if (health.Health <= 0)
            {
                Die();
            }
            else
            {
                ApplyRecoil(hitDirection, hitForce);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, Vector2.zero, 0f);
    }

    private void Die()
    {
        if (pState != null)
            pState.currentState = PlayerStateList.State.Dead;

        rb.linearVelocity = Vector2.zero;
        inputEnable = false;
        gameObject.SetActive(false);
        UIManager.Instance?.ShowDeathMenu();
    }

    private void ApplyRecoil(Vector2 hitDirection, float hitForce)
    {
        if (!isRecoiling && rb != null && hitForce > 0)
        {
            rb.linearVelocity = hitForce * recoilFactor * hitDirection.normalized;
            isRecoiling = true;
        }
    }

    public void Respawned()
    {
        Debug.Log($"[Respawned] activeSelf={gameObject.activeSelf}, position={transform.position}");

        gameObject.SetActive(true);
        InitializeComponents(); // Гарантия, что все компоненты актуальны

        if (config != null && health != null)
            health.Health = config.maxHealth;

        if (pState != null)
        {
            pState.isInvincible = false;
            pState.currentState = PlayerStateList.State.Idle;
            pState.lookingRight = true;
        }

        inputEnable = true;
        enabled = true;
        if (movement != null) movement.canMove = true;

        attack?.ResetAttackState();
        anim?.SetTrigger("Idle");

        isRecoiling = false;
    }

    private void InitializeComponents()
    {
        pState = GetComponent<PlayerStateList>();
        movement = GetComponent<PlayerMovement>();
        jump = GetComponent<PlayerJump>();
        dash = GetComponent<PlayerDash>();
        attack = GetComponent<PlayerAttack>();
        parry = GetComponent<PlayerParry>();
        health = GetComponent<PlayerHealth>();
        climb = GetComponent<PlayerClimb>();
        ceilingClimb = GetComponent<PlayerCeilingClimb>();
        shield = GetComponent<PlayerShield>();
        anim = GetComponent<PlayerAnimation>();
        physics = GetComponent<PlayerPhysics>();
        rb = GetComponent<Rigidbody2D>();
    }

    // 🔧 Этот метод нужен для GlobalController.SetInputBlocked(...)
    public void SetInputBlocked(bool value)
    {
        inputEnable = !value;
        if (movement != null)
            movement.canMove = !value;
    }
}
