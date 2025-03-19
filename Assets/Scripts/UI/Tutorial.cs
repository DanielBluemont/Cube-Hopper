using CubeHopper.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            if (videoPlayer.clip == dragTutorial)
            {
                videoPlayer.clip = landingTutorial;
            }
            else
            {
                videoPlayer.clip = dragTutorial;
            }

            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            source.Play();
            source.prepareCompleted -= OnVideoPrepared;
        }

        private void Start()
        {
            if (PlayerPrefs.GetInt("turorialWasShown", 0) <= 0)
            {
                PlayerPrefs.SetInt("turorialWasShown", 1);
                Open();
            }
        }
        public void Open()
        {
            tutorialPanel.gameObject.SetActive(true);
            videoPlayer.clip = dragTutorial;
            videoPlayer.Play();
            AudioManager.Instance.PlayAudio(_ClickSound);
            LeanTween.value(tutorialPanel.gameObject, (x) => { tutorialPanel.alpha = x; }, 0, 1, 0.5f).setEaseOutQuad();
        }
      
        public void Close()
        {
            videoPlayer.Stop();
            AudioManager.Instance.PlayAudio(_ClickSound);
            LeanTween.value(tutorialPanel.gameObject, (x) => { tutorialPanel.alpha = x; }, 1, 0, 0.4f).setEaseOutQuad().setOnComplete(() =>
            {
                tutorialPanel.gameObject.SetActive(false);
            });
        }
    }
}
