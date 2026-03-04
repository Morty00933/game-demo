using UnityEngine;
using System.Collections;

public class PlayerClimb : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    private Rigidbody2D rb;
    private PlayerStateList pState;
    private PlayerPhysics physics;
    private PlayerJump playerJump;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pState = GetComponent<PlayerStateList>();
        physics = GetComponent<PlayerPhysics>();
        playerJump = GetComponent<PlayerJump>();

        if (playerJump == null)
            Debug.LogError("PlayerClimb: PlayerJump component not found!");
    }

    public void WallClimb(float xAxis, float yAxis)
    {
        if (physics.IsWalled() && yAxis > 0)
        {
            pState.currentState = PlayerStateList.State.WallClimbing;
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.wallSlidingSpeed * yAxis);

            if (Input.GetButtonDown("Jump"))
            {
                pState.currentState = PlayerStateList.State.WallJumping;
                float jumpForce = playerJump != null ? playerJump.config.jumpForce : config.jumpForce;
                rb.linearVelocity = new Vector2(-xAxis * config.runSpeed, jumpForce);
                StartCoroutine(ResetGravity());
            }
        }
        else if (pState.currentState == PlayerStateList.State.WallClimbing)
        {
            pState.currentState = PlayerStateList.State.Falling;
            rb.gravityScale = playerJump != null ? playerJump.gravity : 1f;
        }
    }

    private IEnumerator ResetGravity()
    {
        yield return new WaitForSeconds(0.5f);
        rb.gravityScale = playerJump != null ? playerJump.gravity : 1f;
        pState.currentState = PlayerStateList.State.Falling;
    }
}