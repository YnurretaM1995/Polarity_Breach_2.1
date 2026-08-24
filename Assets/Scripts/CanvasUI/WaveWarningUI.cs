using System.Collections;
using UnityEngine;

namespace PolarityBreach.UI
{
    public class WaveWarningUI : MonoBehaviour
    {
        public static WaveWarningUI Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject root;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform imageRect;

        [Header("Timing")]
        [SerializeField] private float startDelay = 0.5f;
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float blinkDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 0.3f;

        [Header("Look")]
        [SerializeField] private float startScale = 1.4f;
        [SerializeField] private float blinkSpeed = 10f;
        [SerializeField] private float blinkMinAlpha = 0.25f;

        private Coroutine current;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (group != null) group.alpha = 0f;
            if (root != null) root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Play()
        {
            if (Instance == null) return;
            Instance.StartWarning();
        }

        public void StartWarning()
        {
            if (current != null) StopCoroutine(current);
            current = StartCoroutine(PlayWarning());
        }

        public IEnumerator PlayWarning()
        {
            if (group != null) group.alpha = 0f;
            if (root != null) root.SetActive(true);

            if (startDelay > 0f)
                yield return new WaitForSecondsRealtime(startDelay);

            float t = 0f;

            while (t < fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / fadeInDuration);

                if (group != null) group.alpha = p;
                if (imageRect != null)
                    imageRect.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, p);

                yield return null;
            }

            if (imageRect != null) imageRect.localScale = Vector3.one;

            t = 0f;

            while (t < blinkDuration)
            {
                t += Time.unscaledDeltaTime;

                if (group != null)
                {
                    float wave = (Mathf.Sin(t * blinkSpeed) + 1f) * 0.5f;
                    group.alpha = Mathf.Lerp(blinkMinAlpha, 1f, wave);
                }

                yield return null;
            }

            t = 0f;
            float from = group != null ? group.alpha : 1f;

            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                if (group != null)
                    group.alpha = Mathf.Lerp(from, 0f, Mathf.Clamp01(t / fadeOutDuration));

                yield return null;
            }

            if (group != null) group.alpha = 0f;
            if (root != null) root.SetActive(false);

            current = null;
        }
        public void HideNow()
        {
            if (current != null)
            {
                StopCoroutine(current);
                current = null;
            }

            if (group != null) group.alpha = 0f;
            if (root != null) root.SetActive(false);
        }
    }
}