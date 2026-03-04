using UnityEngine;

public class Fireball : Projectile
{
    protected override void Awake()
    {
        base.Awake();
        useGravity = false;
    }

    protected override void OnHitPlayer()
    {
        base.OnHitPlayer();
    }
}