using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Configs/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("Health and Scoring")]
    [Tooltip("Maximum health of the enemy")]
    public float maxHealth = 25f;
    [Tooltip("Points awarded to the player for defeating the enemy")]
    public int scoreValue = 12;

    [Header("Movement")]
    [Tooltip("Base movement speed of the enemy")]
    public float speed = 2f;
    [Tooltip("Multiplier for speed when chasing the player")]
    public float chaseSpeedMultiplier = 2f;
    [Tooltip("Distance to check for ledges")]
    public float ledgeCheckX = 1.4f;
    [Tooltip("Height to check for ground below ledges")]
    public float ledgeCheckY = 1.2f;
    [Tooltip("Cooldown before the enemy can flip direction")]
    public float flipCooldown = 0.5f;
    [Tooltip("Distance to check for walls")]
    public float wallCheckDistance = 0.5f;
    [Tooltip("Layer mask for walls")]
    public LayerMask wallLayer;
    [Tooltip("Layer mask for ground detection")]
    public LayerMask whatIsGround;

    [Header("Dash (Rush to Player)")]
    [Tooltip("Cooldown before the enemy can dash again")]
    public float dashCooldown = 3f;
    [Tooltip("Duration of the dash movement")]
    public float dashDuration = 0.2f;
    [Tooltip("Multiplier for speed during dash")]
    public float dashSpeedMultiplier = 5f;

    [Header("Attack")]
    [Tooltip("Cooldown between attacks")]
    public float attackCooldown = 0.5f;
    [Tooltip("Delay before performing an attack after detecting the player")]
    public float attackDelay = 0.5f;
    [Tooltip("Damage dealt by quick attack")]
    public float quickDamage = 1f;
    [Tooltip("Damage dealt by heavy attack")]
    public float heavyDamage = 1.8f;
    [Tooltip("Size of the detection zone for the player")]
    public Vector2 detectSize = new Vector2(20f, 3f);
    [Tooltip("Size of the attack hitbox")]
    public Vector2 attackSize = new Vector2(2.5f, 1.5f);

    [Header("Berserk Mode")]
    [Tooltip("Health threshold (as a fraction) for entering berserk mode")]
    public float berserkThreshold = 0.3f;
    [Tooltip("Speed multiplier in berserk mode")]
    public float berserkSpeedMultiplier = 1.5f;
    [Tooltip("Damage multiplier in berserk mode")]
    public float berserkDamageMultiplier = 1.3f;

    [Header("Special Abilities")]
    [Tooltip("Damage reduction multiplier for armored enemies")]
    public float armorDamageReduction = 0.7f;
    [Tooltip("Damage dealt by poison attack")]
    public float poisonDamage = 1.5f;
    [Tooltip("Speed of poison projectile")]
    public float poisonSpeed = 8f;
    [Tooltip("Speed of laser projectile")]
    public float laserSpeed = 10f;
    [Tooltip("Force applied for jump movement")]
    public float jumpForce = 5f;
    [Tooltip("Cooldown between jumps")]
    public float jumpCooldown = 2f;
    [Tooltip("Cooldown for jumps during patrol")]
    public float patrolJumpCooldown = 2f;

    [Header("Death")]
    [Tooltip("Delay before the enemy is deactivated after death")]
    public float destroyDelay = 10f;
}