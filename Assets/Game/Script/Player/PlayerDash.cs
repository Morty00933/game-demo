using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    private Rigidbody2D rb;
    private PlayerStateList pState;
    private PlayerPhysics physics;
    private PlayerMovement movement;
    private PlayerAnimation anim;

    private bool canDash = true;
    private bool isDashing = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pState = GetComponent<PlayerStateList>();
        physics = GetComponent<PlayerPhysics>();
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<PlayerAnimation>();
    }

    public void PerformDash(float xAxis)
    {
        if (!canDash || isDashing || pState.currentState == PlayerStateList.State.Dead)
            return;

        int direction = xAxis != 0 ? (xAxis > 0 ? 1 : -1) : (pState.lookingRight ? 1 : -1);
        StartCoroutine(DashRoutine(direction));
    }

    private IEnumerator DashRoutine(int direction)
    {
        isDashing = true;
        canDash = false;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        pState.currentState = PlayerStateList.State.Dashing;
        pState.isInvincible = true;
        movement.canMove = false;
        anim.SetTrigger("Dash");

        float dashSpeed = config.dashDistance / config.dashTime;
        float duration = config.dashTime;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rb.linearVelocity = new Vector2(dashSpeed * direction, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = originalGravity;
        pState.isInvincible = false;
        movement.canMove = true;

        if (!physics.IsGrounded())
            pState.currentState = PlayerStateList.State.Falling;
        else
            pState.currentState = PlayerStateList.State.Idle;

        isDashing = false;

        yield return new WaitForSeconds(config.dashCooldown);
        canDash = true;
    }
}
