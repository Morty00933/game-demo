using UnityEngine;
using UnityEngine.Video;

public class VidPlayer : MonoBehaviour
{
    [SerializeField] private string videoUrl = "https://quochung2497.github.io/TestVideo/Voidheart.mp4";
    private VideoPlayer videoplayer;

    private void Awake()
    {
        videoplayer = GetComponent<VideoPlayer>();
        if (videoplayer != null)
        {
            videoplayer.url = videoUrl;
            videoplayer.Prepare();
            videoplayer.prepareCompleted += OnVideoPrepared;
        }
    }

    public void OnVideoPrepared(VideoPlayer source)
    {
        videoplayer.Play();
    }

    public void StopVideo()
    {
        if (videoplayer != null && videoplayer.isPlaying)
            videoplayer.Stop();
    }
}
