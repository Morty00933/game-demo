using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    private Rigidbody2D rb;
    private PlayerStateList pState;
    private PlayerPhysics physics;
    private PlayerAnimation anim;
    public bool canMove = true;
    private float iceSlideVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pState = GetComponent<PlayerStateList>();
        physics = GetComponent<PlayerPhysics>();
        anim = GetComponent<PlayerAnimation>();

        if (rb == null) Debug.LogError("Rigidbody2D ����������� �� ������� ������!");
        if (pState == null) Debug.LogError("PlayerStateList ����������� �� ������� ������!");
        if (physics == null) Debug.LogError("PlayerPhysics ����������� �� ������� ������!");
        if (anim == null) Debug.LogError("PlayerAnimation ����������� �� ������� ������!");
        if (config == null) Debug.LogError("PlayerConfig ����������� �� ������� ������!");
    }

    public void Move(float xAxis, bool isRunning)
    {
        if (!canMove || pState.currentState == PlayerStateList.State.Healing)
        {
            return;
        }

        bool isGrounded = physics.IsGrounded();
        float speed = isRunning ? config.runSpeed : config.walkSpeed;

        if (physics.IsOnIce())
        {
            iceSlideVelocity = Mathf.Lerp(iceSlideVelocity, speed * xAxis, config.iceFriction * Time.deltaTime);
            rb.linearVelocity = new Vector2(iceSlideVelocity, rb.linearVelocity.y);
        }
        else
        {
            iceSlideVelocity = 0f;
            if (physics.IsOnSlope())
            {
                Vector2 slopeDirection = physics.GetSlopeDirection();
                rb.linearVelocity = slopeDirection != Vector2.zero
                    ? new Vector2(slopeDirection.x * config.slopeSlideSpeed, slopeDirection.y * config.slopeSlideSpeed)
                    : new Vector2(speed * xAxis, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(xAxis == 0 ? 0 : speed * xAxis, rb.linearVelocity.y);
            }
        }

        if (xAxis != 0)
        {
            anim.SetBool("Walking", !isRunning);
            anim.SetBool("Running", isRunning);
            transform.rotation = Quaternion.Euler(0, xAxis > 0 ? 0 : 180, 0);
            pState.lookingRight = xAxis > 0;
        }
        else
        {
            anim.SetBool("Walking", false);
            anim.SetBool("Running", false);
            anim.SetTrigger("StopTrigger");
        }

        anim.UpdateAnimations(xAxis, isRunning, isGrounded);
    }

    public void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("Walking", false);
        anim.SetBool("Running", false);
        anim.SetTrigger("StopTrigger");
    }
}