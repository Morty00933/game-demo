using System.Collections;
using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private GameObject shieldVisual;

    private PlayerStateList pState;
    private PlayerHealth health;
    private PlayerPhysics physics;
    private bool canShield = true;
    private float shieldTimer;
    private bool isShieldActive;

    private void Awake()
    {
        pState = GetComponent<PlayerStateList>();
        health = GetComponent<PlayerHealth>();
        physics = GetComponent<PlayerPhysics>();
        isShieldActive = false;
    }

    private void Update()
    {
        if (shieldTimer > 0)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0)
                DeactivateShield();
        }
    }

    public void ActivateShield()
    {
        if (canShield && pState.currentState != PlayerStateList.State.Dead)
        {
            isShieldActive = true;
            pState.isInvincible = true;
            canShield = false;
            shieldTimer = config.shieldDuration;

            if (shieldVisual != null)
                shieldVisual.SetActive(true);

            StartCoroutine(ShieldCooldown());
            Debug.Log("ўит активирован");
        }
    }

    public void DeactivateShield()
    {
        isShieldActive = false;
        pState.isInvincible = false;

        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        Debug.Log("ўит деактивирован");
    }

    private IEnumerator ShieldCooldown()
    {
        yield return new WaitForSeconds(config.shieldCooldown);
        canShield = true;
        Debug.Log("ўит готов к повторной активации");
    }

    public bool IsShieldActive()
    {
        return isShieldActive;
    }
}