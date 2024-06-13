// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using UnityEngine;

namespace PixelCrushers.GridController
{

    [AddComponentMenu("Grid Controller/Light Flicker")]
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {

        [SerializeField] private float flickerFrequency = 0.1f;
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 2.5f;
        [SerializeField] private bool playOnAwake = true;

        private Light myLight;
        private WaitForSeconds seconds;

        private void Start()
        {
            myLight = GetComponent<Light>();
            seconds = new WaitForSeconds(flickerFrequency);
            if (playOnAwake) Play();
        }

        public void Play()
        {
            StartCoroutine(Flicker());
        }

        public void Stop()
        {
            StopAllCoroutines();
        }

        private IEnumerator Flicker()
        {
            while (true)
            {
                myLight.intensity = Random.Range(minIntensity, maxIntensity);
                yield return seconds;
            }
        }
    }
}
