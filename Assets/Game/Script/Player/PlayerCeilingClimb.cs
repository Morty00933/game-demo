using UnityEngine;

public class PlayerCeilingClimb : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    private Rigidbody2D rb;
    private PlayerStateList pState;
    private PlayerPhysics physics;
    private SpriteRenderer sr;
    private PlayerJump playerJump;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pState = GetComponent<PlayerStateList>();
        physics = GetComponent<PlayerPhysics>();
        sr = GetComponent<SpriteRenderer>();
        playerJump = GetComponent<PlayerJump>();

        if (playerJump == null)
            Debug.LogError("PlayerCeilingClimb: PlayerJump component not found!");
    }

    public void CeilingClimb(float xAxis, float yAxis)
    {
        if (physics.IsCeiling() && yAxis > 0)
        {
            pState.currentState = PlayerStateList.State.CeilingClimbing;
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(xAxis * config.walkSpeed, 0);
            sr.flipY = true;

            if (Input.GetButtonDown("Jump"))
            {
                pState.currentState = PlayerStateList.State.Falling;
                rb.gravityScale = playerJump != null ? playerJump.gravity : 1f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -config.jumpForce * 0.5f);
                sr.flipY = false;
            }
        }
        else if (pState.currentState == PlayerStateList.State.CeilingClimbing)
        {
            pState.currentState = PlayerStateList.State.Falling;
            rb.gravityScale = playerJump != null ? playerJump.gravity : 1f;
            sr.flipY = false;
        }
    }
}
