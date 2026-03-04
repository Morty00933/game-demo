using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    private PlayerStateList pState;
    private PlayerAnimation anim;
    private bool parryActive;

    private void Awake()
    {
        pState = GetComponent<PlayerStateList>();
        anim = GetComponent<PlayerAnimation>();
    }

    public void TryParry()
    {
        if (CanParry())
        {
            Parry();
        }
    }

    public void Parry()
    {
        parryActive = true;
        pState.isInvincible = true;
        anim.SetTrigger("Parry");
        CancelInvoke(nameof(EndParry));
        Invoke(nameof(EndParry), config.parryDuration);
    }

    private void EndParry()
    {
        parryActive = false;
        pState.isInvincible = false;
    }

    public bool CanParry()
    {
        return !parryActive && pState.currentState != PlayerStateList.State.Dead &&
               pState.currentState != PlayerStateList.State.Dashing;
    }

    public bool IsParryBonusActive()
    {
        return parryActive;
    }
}