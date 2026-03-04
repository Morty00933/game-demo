using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    private ParticlesEffect particlesEffect;

    private void Start()
    {
        particlesEffect = GetComponentInParent<ParticlesEffect>();
        if (particlesEffect == null)
        {
            Debug.LogError("ParticlesEffect не найден на родительском объекте!");
        }
    }

    public void AttackEffect(int attackType)
    {
        if (particlesEffect != null)
        {
            particlesEffect.AttackEffect(attackType);
        }
        else
        {
            Debug.LogWarning("AttackEffect не вызван: particlesEffect отсутствует!");
        }
    }
}