using CubeHopper.Audio;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CubeHopper.UI
{
    public class Settings : MonoBehaviour
    {
        [SerializeField] private AudioClip _ClickSound;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _goHomePanel;
        [SerializeField] private GameObject _exitButton;
        [SerializeField] private Toggle _vibro, _sound, _music;
        public static bool isPaused { get; private set; }

       
        private void Start()
        {
            _settingsPanel.transform.localScale = Vector3.zero;
            LoadData();
        }

        public void LoadToggle(bool value, string name)
        {
            switch (name) 
            {
                case "Vibration":
                    _vibro.isOn = value;
                    PlayerPrefs.SetInt("Vibration", value ? 1 : 0);
                    break;
                case "Sound":
                    _sound.isOn = value;
                    AudioManager.Instance.SetMuteStateAudio(value);
                    PlayerPrefs.SetInt("Sound", value ? 1 : 0);
                    break;
                case "Music":
                    _music.isOn = value;
                    AudioManager.Instance.SetMuteStateMusic(value);
                    PlayerPrefs.SetInt("Music", value ? 1 : 0);
                    break;
            }
            PlayerPrefs.Save();
        }
        private void LoadData()
        {
            _vibro.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
            _sound.isOn = PlayerPrefs.GetInt("Sound", 1) == 1;
            _music.isOn = PlayerPrefs.GetInt("Music", 1) == 1;
        }
        public void Open()
        {
            isPaused = true;
            Time.timeScale = 0f;
            _settingsPanel.SetActive(true);
            AudioManager.Instance.PlayAudio(_ClickSound);
            _settingsPanel.transform.LeanScale(Vector3.one, 0.3f).setEaseOutQuad().setIgnoreTimeScale(true);
        }
        public void Close() 
        {
            isPaused = false;
            Time.timeScale = 1f;
            AudioManager.Instance.PlayAudio(_ClickSound);
            _settingsPanel.transform.LeanScale(Vector3.zero, 0.2f).setIgnoreTimeScale(true).setEaseOutQuad().setOnComplete(()=> 
            _settingsPanel.SetActive(false));
        }
        public void GoHome()
        {
            _goHomePanel.SetActive(true);
            _exitButton.SetActive(false);
            AudioManager.Instance.PlayAudio(_ClickSound);
            _goHomePanel.transform.LeanScale(Vector3.one, 0.3f).setEaseOutQuad().setIgnoreTimeScale(true);
        }
        public void YesGoHome() 
        {
            isPaused = false;
            AudioManager.Instance.PlayAudio(_ClickSound);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        public void NoGoHome() 
        {
            _exitButton.SetActive(true);
            AudioManager.Instance.PlayAudio(_ClickSound);
            _goHomePanel.transform.LeanScale(Vector3.zero, 0.2f).setIgnoreTimeScale(true).setEaseOutQuad().setOnComplete(() =>
            _goHomePanel.SetActive(false));
        }
        public void RateApp()
        {
            Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
        }
    }
}
