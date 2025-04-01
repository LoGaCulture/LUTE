using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace LoGaCulture.LUTE
{
    public class LUTEVideoPlayer : MonoBehaviour
    {
        public Vector3 desiredSize = new Vector3(1f, 1f, 1f); // Default desired size

        protected static List<LUTEVideoPlayer> activeVidePlayers = new List<LUTEVideoPlayer>();

        public static LUTEVideoPlayer ActiveVideoPlayer { get; set; }
        private bool forceFinishClick = false;
        private Action onComplete;
        private float fadeTime;

        public static LUTEVideoPlayer GetVideoPlayer()
        {
            if (ActiveVideoPlayer == null)
            {
                LUTEVideoPlayer videoPlayer = null;
                if (activeVidePlayers.Count > 0)
                {
                    videoPlayer = activeVidePlayers[0];
                }
                if (videoPlayer != null)
                {
                    ActiveVideoPlayer = videoPlayer;
                }
                if (ActiveVideoPlayer == null)
                {
                    // Create a new dialogue box
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/VideoPlayer");
                    if (prefab != null)
                    {
                        GameObject go = Instantiate(prefab) as GameObject;
                        go.SetActive(false);
                        go.name = "VideoPlayer";
                        ActiveVideoPlayer = go.GetComponent<LUTEVideoPlayer>();
                    }
                }
            }
            return ActiveVideoPlayer;
        }

        private void Update()
        {
            if (forceFinishClick && Input.GetMouseButtonDown(0))
            {
                UnityEngine.Video.VideoPlayer videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
                if (videoPlayer != null && videoPlayer.isPlaying)
                    FinishVideo();
            }
        }

        public void PlayVideo(Mesh mesh = null, VideoClip videoClip = null, bool loop = false, float playbackSpeed = 1.0f, Action onFinished = null, float _desiredSize = 0.25f, bool fadeOnComplete = false, float fadeDuration = 1.0f, bool forceClick = false)
        {
            gameObject.SetActive(true);

            forceFinishClick = forceClick;
            fadeTime = fadeDuration;

            if (mesh != null)
            {
                // Set the mesh to the video player
                var meshFilter = GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    meshFilter.mesh = mesh;
                }
            }
            UnityEngine.Video.VideoPlayer videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null && videoClip != null)
            {
                videoPlayer.playbackSpeed = playbackSpeed;
                videoPlayer.isLooping = loop;
                videoPlayer.clip = videoClip;
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += (source) =>
                {
                    desiredSize = new Vector3(_desiredSize, _desiredSize, _desiredSize);
                    AdjustObjectSize(videoPlayer.texture.width, videoPlayer.texture.height);
                    videoPlayer.Play();
                };
                if (onFinished != null)
                {
                    onComplete = onFinished;
                    videoPlayer.loopPointReached += (source) =>
                    {
                        onFinished();
                    };
                }
                videoPlayer.loopPointReached += (source) =>
                {
                    if (fadeOnComplete)
                    {
                        StartCoroutine(FadeVideo(fadeDuration));
                    }
                };
            }
        }

        protected IEnumerator FadeVideo(float duration)
        {
            float start = Time.time;
            float end = start + duration;
            while (Time.time < end)
            {
                float t = (Time.time - start) / duration;
                Color color = GetComponent<Renderer>().material.color;
                color.a = Mathf.Lerp(1, 0, t);
                GetComponent<Renderer>().material.color = color;
                yield return null;
            }
            gameObject.SetActive(false);
        }

        private void AdjustObjectSize(int width, int height)
        {
            // Your logic to adjust the size of the object based on the video clip size
            // For example, you could scale the object based on the video's aspect ratio
            float aspectRatio = (float)width / height;
            transform.localScale = new Vector3(desiredSize.x * aspectRatio, desiredSize.y, desiredSize.z);
        }

        private void FinishVideo()
        {
            //Stop playing the video and continue - can be called elsewhere but really used for forcing clicking finishing video
            UnityEngine.Video.VideoPlayer videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
                videoPlayer.Stop();

            StartCoroutine(FadeVideo(fadeTime));
            onComplete?.Invoke();
        }
    }
}