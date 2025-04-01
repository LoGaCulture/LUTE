using LoGaCulture.LUTE;
using UnityEngine;
using UnityEngine.Video;

[OrderInfo("Video",
              "PlayVideo",
              "Plays a video either using a default plane or a given mesh renderer supplied by authors")]
[AddComponentMenu("")]
public class PlayVideo : Order
{
    [Tooltip("Video clip to play")]
    [SerializeField] protected VideoClip videoClip;
    [Tooltip("Mesh type to play video on - default is a plane centered on screen")]
    [SerializeField] protected Mesh videoMesh;
    [Tooltip("If true, the video will loop")]
    [SerializeField] protected bool loop = false;
    [Tooltip("Playback speed of the video")]
    [SerializeField] protected float playbackSpeed = 1.0f;
    [Tooltip("If true, the next order will not execute until the video has finished playing")]
    [SerializeField] protected bool waitUntilFinished = false;
    [Tooltip("If true, the video mesh will fade when complete - otherwise will persist on screen")]
    [SerializeField] protected bool fadeOnComplete = false;
    [Tooltip("Duration of fade out")]
    [SerializeField] protected float fadeDuration = 1.0f;
    [Tooltip("The desired size of the video player - setting to 0.25 is good for mobile")]
    [SerializeField] protected float videoSize = 0.25f;
    [Tooltip("Whether clicking mouse or touch will force the video to stop and move to next order")]
    [SerializeField] protected bool forceFinishClick;
    public override void OnEnter()
    {
        LUTEVideoPlayer videoPlayer = LUTEVideoPlayer.GetVideoPlayer();

        if (waitUntilFinished)
        {
            videoPlayer.PlayVideo(videoMesh, videoClip, loop, playbackSpeed, () =>
            {
                Continue();
            }, videoSize, fadeOnComplete, fadeDuration);
        }
        else
        {
            videoPlayer.PlayVideo(videoMesh, videoClip, loop, playbackSpeed, null, videoSize, fadeOnComplete, fadeDuration, forceFinishClick);
            Continue();
        }
    }

    public override string GetSummary()
    {
        return "";
    }
}