using UnityEngine;
using System.Collections;

public class ParticlesEffect : MonoBehaviour
{
    [Header("Attack Effects")]
    [SerializeField] private ParticleSystem swordSlashForward; // Первая атака комбо
    [SerializeField] private ParticleSystem swordSlashCombo2; // Вторая атака комбо
    [SerializeField] private ParticleSystem swordSlashCombo3; // Третья атака комбо
    [SerializeField] private ParticleSystem swordSlashUp;     // Атака вверх
    [SerializeField] private ParticleSystem swordSlashDown;   // Атака вниз

    private void Awake()
    {
        CheckParticleSystems();
    }

    private void CheckParticleSystems()
    {
        if (swordSlashForward == null) Debug.LogError("swordSlashForward не назначен в ParticlesEffect!");
        if (swordSlashCombo2 == null) Debug.LogError("swordSlashCombo2 не назначен в ParticlesEffect!");
        if (swordSlashCombo3 == null) Debug.LogError("swordSlashCombo3 не назначен в ParticlesEffect!");
        if (swordSlashUp == null) Debug.LogError("swordSlashUp не назначен в ParticlesEffect!");
        if (swordSlashDown == null) Debug.LogError("swordSlashDown не назначен в ParticlesEffect!");
    }

    public void AttackEffect(int attackType)
    {
        ParticleSystem effect = null;
        string attackName = "";
        float duration = 1f; // Запасная длительность на случай отсутствия ParticleSystem

        switch (attackType)
        {
            case 1:
                effect = swordSlashForward;
                attackName = "первая атака комбо";
                break;
            case 2:
                effect = swordSlashCombo2;
                attackName = "вторая атака комбо";
                break;
            case 3:
                effect = swordSlashCombo3;
                attackName = "третья атака комбо";
                break;
            case 0:
                effect = swordSlashUp;
                attackName = "атака вверх";
                break;
            case -1:
                effect = swordSlashDown;
                attackName = "атака вниз";
                break;
            default:
                Debug.LogWarning($"Неизвестный attackType: {attackType}");
                return;
        }

        if (effect != null)
        {
            effect.Play();
            duration = effect.main.duration;
            StartCoroutine(StopEffectAfterDelay(effect, duration));
            Debug.Log($"Эффект атаки {attackName} воспроизведён, длительность: {duration}");
        }
        else
        {
            Debug.LogWarning($"Эффект атаки {attackName} не воспроизведён: ParticleSystem не назначен!");
        }
    }

    private IEnumerator StopEffectAfterDelay(ParticleSystem effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect != null)
        {
            effect.Stop();
        }
    }
}