using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Dhiraj
{
    public class CutSceneManager : MonoBehaviour
    {
        public VideoPlayer vPlayer;
        public CanvasGroup vScreen;
        public List<VideoClip> videoClips = new List<VideoClip>();

        public void StartPlayingVideo(VideoClip clip)
        {
            vPlayer.clip = clip;
            vScreen.alpha = 1;
            vPlayer.Play();
            vPlayer.loopPointReached += VideoComplete;
        }

        public void VideoComplete(VideoPlayer source)
        {
            vPlayer.Stop();
            vScreen.alpha = 0;
        }


        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                StartPlayingVideo(videoClips[0]);
            }
        }

    }
}
