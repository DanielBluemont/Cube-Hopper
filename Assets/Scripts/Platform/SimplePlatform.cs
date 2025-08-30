using UnityEngine;
using CubeHopper.GameFlow;
using System;
using System.Collections;
using Unity.VisualScripting;

namespace CubeHopper.Platform
{
    public class SimplePlatform : MonoBehaviour
    {
        [SerializeField] protected WeightData _weightData;
        [SerializeField] protected float _yOffset, _yMaxOffset;
        [SerializeField] protected Collider2D _collider;
        [SerializeField] protected GameObject _spike, _coin;
        [SerializeField] protected bool isFading;
        
        public void SetCollider(bool isActive)
        {
            _collider.enabled = isActive;
        }
        public void SetPlatformState(bool isActive)
        {
            this.enabled = isActive;
            _collider.enabled = isActive;
            if (!isActive && isFading)
            {
                GetComponentInChildren<SpriteRenderer>().color = Color.white;
            }
        }
       
        public void DeactivateAtributes()
        {
            if (_spike != null && _spike.activeSelf) 
            {
                _spike.GetComponent<Collider2D>().enabled = false;
                LeanTween.color(_spike, new Color(1, 1, 1, 0), 0.7f).setOnComplete(() => _spike.SetActive(false));
            }
            if (_coin!= null && _coin.activeSelf)
            {
                _coin.GetComponent<Collider2D>().enabled = false;   
                LeanTween.color(_coin, new Color(1, 1, 1, 0), 0.7f).setOnComplete(()=> _coin.SetActive(false));
            }
            if (isFading) 
            {
                StartCoroutine(Fade(4));
            }
        }
        private Color alphaZero = new Color(1, 1, 1, 0);
        private IEnumerator Fade(float time)
        {
            float t = 0;
            var sr = GetComponentInChildren<SpriteRenderer>();
            while (t < time)
            {
                t+= Time.deltaTime;
                sr.color = Color.Lerp(Color.white, alphaZero, t / time);
                yield return null;
            }
            gameObject.SetActive(false);
            sr.color = Color.white;
        }

        //idk what i'am doing


        public int Weight
        {
            get { return _weightData.Weight; }
        }
        public int Rate
        {
            get { return _weightData.Rate; }
        }

        public float YOffset
        {
            get { return _yOffset; }
        }
        public float YMaxOffset
        {
            get { return _yMaxOffset;}
        }
        

        
    }
}
