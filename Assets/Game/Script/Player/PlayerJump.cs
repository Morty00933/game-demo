using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] public PlayerConfig config;
    private Rigidbody2D rb;
    private PlayerStateList pState;
    private PlayerPhysics physics;
    private PlayerAnimation anim;
    private PlayerAttack attack;
    private int airJumpCounter;
    private int jumpBufferCounter;
    private float coyoteTimeCounter;
    private bool jumpHeld;
    public float gravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pState = GetComponent<PlayerStateList>();
        physics = GetComponent<PlayerPhysics>();
        anim = GetComponent<PlayerAnimation>();
        attack = GetComponent<PlayerAttack>();
        gravity = rb.gravityScale;
        attack.Hit += OnDownwardAttackBounce;
    }

    private void OnDestroy()
    {
        if (attack != null) attack.Hit -= OnDownwardAttackBounce;
    }

    public void UpdateJump()
    {
        UpdateJumpVariables();

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0 && jumpHeld)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            jumpHeld = false;
        }

        if (pState.currentState == PlayerStateList.State.Jumping ||
            pState.currentState == PlayerStateList.State.WallJumping)
            return;

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && CanJump())
        {
            PerformJump();
        }
        else if (!physics.IsGrounded() && airJumpCounter < config.maxAirJumps &&
                 Input.GetButtonDown("Jump") && !physics.IsWalled())
        {
            PerformAirJump();
        }
        else if (physics.IsWalled() && Input.GetButtonDown("Jump"))
        {
            PerformWallJump();
        }

        anim.SetBool("Jumping", !physics.IsGrounded());
    }

    private void UpdateJumpVariables()
    {
        if (physics.IsGrounded())
        {
            coyoteTimeCounter = config.coyoteTime;
            airJumpCounter = 0;
            jumpBufferCounter = Mathf.Max(jumpBufferCounter, 0);
            if (pState.currentState == PlayerStateList.State.Jumping ||
                pState.currentState == PlayerStateList.State.Falling)
            {
                pState.currentState = PlayerStateList.State.Idle;
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (pState.currentState == PlayerStateList.State.Jumping && rb.linearVelocity.y < 0)
            {
                pState.currentState = PlayerStateList.State.Falling;
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = config.jumpBufferFrames;
            jumpHeld = true;
        }
        else if (jumpBufferCounter > 0)
        {
            jumpBufferCounter--;
        }
    }

    private bool CanJump()
    {
        return pState.currentState != PlayerStateList.State.Dead &&
               pState.currentState != PlayerStateList.State.Dashing;
    }

    private void PerformJump()
    {
        pState.currentState = PlayerStateList.State.Jumping;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.jumpForce);
        jumpBufferCounter = 0;
        jumpHeld = true;
        anim.SetTrigger("Jump");
    }

    private void PerformAirJump()
    {
        pState.currentState = PlayerStateList.State.Jumping;
        airJumpCounter++;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.jumpForce);
        jumpBufferCounter = 0;
        jumpHeld = true;
        anim.SetTrigger("DoubleJump");
    }

    private void PerformWallJump()
    {
        pState.currentState = PlayerStateList.State.WallJumping;
        int wallDir = physics.GetWallDirection();
        rb.linearVelocity = new Vector2(-wallDir * config.wallJumpingPower.x, config.wallJumpingPower.y);
        airJumpCounter = 0;
        StartCoroutine(WallJumpDuration());
    }

    private System.Collections.IEnumerator WallJumpDuration()
    {
        yield return new WaitForSeconds(config.wallJumpingDuration);
        if (pState.currentState == PlayerStateList.State.WallJumping)
        {
            pState.currentState = physics.IsGrounded() ? PlayerStateList.State.Idle : PlayerStateList.State.Falling;
        }
    }

    private void OnDownwardAttackBounce(Transform attackTransform, float attackRange, Vector2 recoilDir,
                                        float recoilStrength, float hitDamage, int step, bool isDownAttack)
    {
        if (isDownAttack && !physics.IsGrounded())
        {
            Collider2D[] objectsToHit = Physics2D.OverlapCircleAll(attackTransform.position, attackRange, config.attackableLayer);
            foreach (var hit in objectsToHit)
            {
                if (hit.TryGetComponent(out Enemy enemy) || hit.gameObject.layer == LayerMask.NameToLayer("Spikes"))
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.jumpForce * 0.8f);
                    pState.currentState = PlayerStateList.State.Jumping;
                    anim.SetTrigger("Jump");
                    break;
                }
            }
        }
    }
}