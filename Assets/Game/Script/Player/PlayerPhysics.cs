using UnityEngine;

public class PlayerPhysics : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private Transform footCheckPoint;
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private Transform ceilingCheckPoint;
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private float ceilingCheckDistance = 0.3f;

    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(footCheckPoint.position, groundCheckSize, 0f, config.groundLayer);
    }

    public bool IsWalled()
    {
        return Physics2D.Raycast(wallCheckPoint.position, Vector2.right, wallCheckDistance, config.groundLayer) ||
               Physics2D.Raycast(wallCheckPoint.position, Vector2.left, wallCheckDistance, config.groundLayer);
    }

    public int GetWallDirection()
    {
        if (Physics2D.Raycast(wallCheckPoint.position, Vector2.right, wallCheckDistance, config.groundLayer))
            return -1;
        else if (Physics2D.Raycast(wallCheckPoint.position, Vector2.left, wallCheckDistance, config.groundLayer))
            return 1;
        return 0;
    }

    public bool IsOnIce()
    {
        return Physics2D.OverlapBox(footCheckPoint.position, groundCheckSize, 0f, config.iceLayer);
    }

    public bool IsOnSlope()
    {
        return Physics2D.OverlapBox(footCheckPoint.position, groundCheckSize, 0f, config.slopeLayer);
    }

    public Vector2 GetSlopeDirection()
    {
        RaycastHit2D hit = Physics2D.Raycast(footCheckPoint.position, Vector2.down, groundCheckSize.y, config.slopeLayer);
        return hit ? hit.normal : Vector2.zero;
    }

    public bool IsCeiling()
    {
        return Physics2D.Raycast(ceilingCheckPoint.position, Vector2.up, ceilingCheckDistance, config.groundLayer);
    }
}