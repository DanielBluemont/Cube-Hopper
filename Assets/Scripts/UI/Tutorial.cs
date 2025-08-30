using CubeHopper.Audio;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CubeHopper.UI
{
    public class Tutorial : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private CanvasGroup tutorialPanel;
        [SerializeField] private AudioClip _ClickSound;
        [SerializeField] private VideoClip dragTutorial;
        [SerializeField] private VideoClip landingTutorial;

        private void OnEnable()
        {
            videoPlayer.loopPointReached += OnLoopPointReached;
        }

        private void OnDisable()
        {
            videoPlayer.loopPointReached -= OnLoopPointReached;
        }

      

        private void OnLoopPointReached(VideoPlayer source)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;

            videoPlayer.Stop();
            videoPlayer.clip = (videoPlayer.clip == dragTutorial) ? landingTutorial : dragTutorial;
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            source.Play();
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        public void Open()
        {
            tutorialPanel.gameObject.SetActive(true);

            videoPlayer.Stop();
            videoPlayer.clip = dragTutorial;
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;

            AudioManager.Instance.PlayAudio(_ClickSound);
            LeanTween.value(tutorialPanel.gameObject, x => tutorialPanel.alpha = x, 0, 1, 0.5f)
                .setEaseOutQuad();
        }

        public void Close()
        {
            videoPlayer.Stop();
            AudioManager.Instance.PlayAudio(_ClickSound);
            LeanTween.value(tutorialPanel.gameObject, x => tutorialPanel.alpha = x, 1, 0, 0.4f)
                .setEaseOutQuad()
                .setOnComplete(() => tutorialPanel.gameObject.SetActive(false));
        }
    }
}
