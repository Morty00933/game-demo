using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerStateList pState;
    private PlayerPhysics physics;
    private PlayerShield shield;

    // Кэшированные имена параметров аниматора
    private static class AnimatorParameters
    {
        public const string Speed = "Speed";
        public const string IsGrounded = "IsGrounded";
        public const string Walking = "Walking";
        public const string Running = "Running";
        public const string Jumping = "Jumping";
        public const string Falling = "Falling";
        public const string Dashing = "Dashing";
        public const string Healing = "Healing";
        public const string WallClimbing = "WallClimbing";
        public const string CeilingClimbing = "CeilingClimbing";
        public const string WallJumping = "WallJumping";
        public const string Parrying = "Parrying";
        public const string Sliding = "Sliding";
        public const string IsDead = "IsDead";
        public const string InstantDeath = "InstantDeath";
        public const string Shielded = "Shielded";
        public const string Attack = "Attack";
        public const string SecondAtk = "SecondAtk";
        public const string ThirdAtk = "ThirdAtk";
        public const string AttackUp = "AttackUp";
        public const string AttackDown = "AttackDown";
        public const string StopTrigger = "StopTrigger";
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        pState = GetComponent<PlayerStateList>();
        physics = GetComponent<PlayerPhysics>();
        shield = GetComponent<PlayerShield>();

        if (animator == null) Debug.LogError("Animator отсутствует на объекте игрока!");
        if (pState == null) Debug.LogError("PlayerStateList отсутствует на объекте игрока!");
        if (physics == null) Debug.LogError("PlayerPhysics отсутствует на объекте игрока!");
        if (shield == null) Debug.LogError("PlayerShield отсутствует на объекте игрока!");
    }

    public void UpdateAnimations(float xAxis, bool isRunning, bool isGrounded)
    {
        if (animator == null || pState == null) return;

        // Основные параметры движения
        animator.SetFloat(AnimatorParameters.Speed, Mathf.Abs(xAxis));
        animator.SetBool(AnimatorParameters.IsGrounded, isGrounded);
        animator.SetBool(AnimatorParameters.Walking, xAxis != 0 && !isRunning);
        animator.SetBool(AnimatorParameters.Running, xAxis != 0 && isRunning);

        // Состояния, зависящие от PlayerStateList
        animator.SetBool(AnimatorParameters.Jumping, pState.currentState == PlayerStateList.State.Jumping);
        animator.SetBool(AnimatorParameters.Falling, pState.currentState == PlayerStateList.State.Falling || (!isGrounded && pState.currentState != PlayerStateList.State.Jumping));
        animator.SetBool(AnimatorParameters.Dashing, pState.currentState == PlayerStateList.State.Dashing);
        animator.SetBool(AnimatorParameters.Healing, pState.currentState == PlayerStateList.State.Healing);
        animator.SetBool(AnimatorParameters.WallClimbing, pState.currentState == PlayerStateList.State.WallClimbing);
        animator.SetBool(AnimatorParameters.CeilingClimbing, pState.currentState == PlayerStateList.State.CeilingClimbing);
        animator.SetBool(AnimatorParameters.WallJumping, pState.currentState == PlayerStateList.State.WallJumping);
        animator.SetBool(AnimatorParameters.Parrying, pState.currentState == PlayerStateList.State.Parrying);
        animator.SetBool(AnimatorParameters.Sliding, pState.currentState == PlayerStateList.State.Sliding);
        animator.SetBool(AnimatorParameters.IsDead, pState.currentState == PlayerStateList.State.Dead);
        animator.SetBool(AnimatorParameters.InstantDeath, pState.currentState == PlayerStateList.State.InstantDeath);
        animator.SetBool(AnimatorParameters.Shielded, shield != null && shield.IsShieldActive());
    }

    public void SetFloat(string paramName, float value)
    {
        if (animator != null) animator.SetFloat(paramName, value);
    }

    public void SetBool(string paramName, bool value)
    {
        if (animator != null) animator.SetBool(paramName, value);
    }

    public void SetTrigger(string triggerName)
    {
        if (animator != null) animator.SetTrigger(triggerName);
    }

    public void ResetTrigger(string triggerName)
    {
        if (animator != null) animator.ResetTrigger(triggerName);
    }
}