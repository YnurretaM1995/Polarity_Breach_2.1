using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace PolarityBreach.PolaritySystem
{
    public class PolarityPostProcessPulse : MonoBehaviour
    {
        [SerializeField] private Volume whiteVolume;
        [SerializeField] private Volume blackVolume;

        [Header("Pulse")]
        [SerializeField] private float peakWeight = 1f;
        [SerializeField] private float fadeInDuration = 0.03f;
        [SerializeField] private float fadeOutDuration = 0.22f;

        private Coroutine pulseCoroutine;

        private void Awake()
        {
            SetWeight(whiteVolume, 0f);
            SetWeight(blackVolume, 0f);
        }

        public void Play(Polarity polarity)
        {
            Volume targetVolume = polarity == Polarity.White ? whiteVolume : blackVolume;
            Volume otherVolume = polarity == Polarity.White ? blackVolume : whiteVolume;

            if (targetVolume == null) return;

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }

            SetWeight(otherVolume, 0f);
            pulseCoroutine = StartCoroutine(Pulse(targetVolume));
        }

        private IEnumerator Pulse(Volume volume)
        {
            yield return Fade(volume, 0f, peakWeight, fadeInDuration);
            yield return Fade(volume, peakWeight, 0f, fadeOutDuration);

            SetWeight(volume, 0f);
            pulseCoroutine = null;
        }

        private IEnumerator Fade(Volume volume, float startWeight, float endWeight, float duration)
        {
            if (duration <= 0f)
            {
                SetWeight(volume, endWeight);
                yield break;
            }

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / duration);
                SetWeight(volume, Mathf.Lerp(startWeight, endWeight, t));

                yield return null;
            }
        }

        private void SetWeight(Volume volume, float weight)
        {
            if (volume == null) return;

            volume.weight = weight;
        }
    }
}
