using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerController controller;
    private PlayerMovement movement;
    private PlayerShield shield;
    private PlayerStateList pState;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement>();
        shield = GetComponent<PlayerShield>();
        pState = GetComponent<PlayerStateList>();
    }

    public void EnterState(PlayerStateList.State state)
    {
        pState.currentState = state;
        if (state == PlayerStateList.State.Dead)
        {
            GetComponent<Animator>().SetBool("isDead", true);
            movement.Stop();
        }
    }

    public bool CanPerformAction()
    {
        return pState.currentState != PlayerStateList.State.Dead &&
               pState.currentState != PlayerStateList.State.Dashing;
    }
}