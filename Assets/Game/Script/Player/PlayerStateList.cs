using UnityEngine;

public class PlayerStateList : MonoBehaviour
{
    public enum State
    {
        Idle, Walking, Running, Jumping, Falling, Dashing, Healing, WallClimbing, WallJumping,
        Parrying, Sliding, Dead, Cutscene, CeilingClimbing, InstantDeath
    }
    public State currentState = State.Idle;
    public bool isPoisoned;
    public bool isInvincible;
    public bool lookingRight = true;
}