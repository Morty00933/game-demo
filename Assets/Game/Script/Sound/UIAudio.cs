using UnityEngine;

public class UIAudio : MonoBehaviour
{
    [SerializeField] private AudioClip click;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("UIAudio: AudioSource component not found!");
    }

    public void SoundOnClick()
    {
        if (audioSource != null && click != null)
            audioSource.PlayOneShot(click);
    }
}
