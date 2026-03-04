using UnityEngine;

public class CrystalProjectile : Projectile
{
    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        useGravity = true;
    }

    public override void Initialize(Vector2 direction, float speed, float damage, Enemy owner)
    {
        base.Initialize(direction, speed, damage, owner);
        if (animator != null) animator.Play("BloomAnimation");
    }

    protected override void OnHitPlayer()
    {
        base.OnHitPlayer();
    }
}