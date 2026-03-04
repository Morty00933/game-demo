using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [Header("Horizontal Movement")]
    public float walkSpeed = 6f;
    public float runSpeed = 10f;
    public float iceFriction = 0.1f;
    public float slopeSlideSpeed = 5f;

    [Header("Vertical Movement")]
    public float jumpForce = 25f;
    public int jumpBufferFrames = 10;
    public float coyoteTime = 0.2f;
    public int maxAirJumps = 1;

    [Header("Dash")]
    public float dashDistance = 12f;
    public float dashTime = 0.1f;
    public float dashCooldown = 0.3f;

    [Header("Attack")]
    public float attackInterval = 0.5f;
    public float sideAttackRange = 0.5f;
    public float upAttackRange = 0.5f;
    public float downAttackRange = 0.5f;
    public float damage = 10f;
    public float comboResetTime = 0.8f;

    [Header("Recoil")]
    public float recoilXSpeed = 5f;
    public float recoilYSpeed = 5f;

    [Header("Health")]
    public int maxHealth = 100;
    public float timeToHeal = 2f;
    public int healAmount = 20;

    [Header("Parry")]
    public float parryWindow = 0.2f;
    public float parryCooldown = 1f;
    public float parryStunDuration = 1f;
    public float parryBonusDuration = 2f;
    public float parryRange = 1.5f;
    public float parryDuration = 0.3f;

    [Header("Shield")]
    public float shieldDuration = 5f;
    public float shieldCooldown = 10f;

    [Header("Wall Jump")]
    public float wallSlidingSpeed = 2f;
    public float wallJumpingDuration = 0.4f;
    public Vector2 wallJumpingPower = new Vector2(8f, 16f);

    [Header("Layers")]
    public LayerMask attackableLayer;
    public LayerMask groundLayer;
    public LayerMask iceLayer;
    public LayerMask slopeLayer;
    public LayerMask enemyLayer;
}