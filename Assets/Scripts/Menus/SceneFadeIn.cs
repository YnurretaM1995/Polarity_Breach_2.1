using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PolarityBreach.Menus
{
    public class SceneFadeIn : MonoBehaviour
    {
        [SerializeField] private CanvasGroup blackFadeGroup;
        [SerializeField] private bool fadeInOnStart = true;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 1f;

        private void Awake()
        {
            if (blackFadeGroup == null)
            {
                blackFadeGroup = GetComponent<CanvasGroup>();
            }

            FixBlackFadeGroupIfNeeded();
        }

        private void Start()
        {
            if (fadeInOnStart)
            {
                StartCoroutine(FadeFromBlack());
            }
        }

        public void PlayFadeFromBlack()
        {
            StartCoroutine(FadeFromBlack());
        }

        public void PlayFadeToBlack(float duration)
        {
            StartCoroutine(FadeToBlack(duration));
        }

        public void FadeOutAndLoadScene(string sceneName)
        {
            StartCoroutine(FadeOutAndLoadSceneRoutine(sceneName));
        }

        public IEnumerator FadeOutAndLoadSceneRoutine(string sceneName)
        {
            yield return FadeToBlack(fadeOutDuration);
            SceneManager.LoadScene(sceneName);
        }

        public IEnumerator FadeOutAndLoadSceneRoutine(string sceneName, float duration)
        {
            yield return FadeToBlack(duration);
            SceneManager.LoadScene(sceneName);
        }

        public IEnumerator FadeFromBlack()
        {
            yield return FadeFromBlack(fadeInDuration);
        }

        public IEnumerator FadeFromBlack(float duration)
        {
            if (blackFadeGroup == null) yield break;

            blackFadeGroup.alpha = 1f;
            blackFadeGroup.blocksRaycasts = true;

            yield return FadeGroup(blackFadeGroup, 1f, 0f, duration);

            blackFadeGroup.blocksRaycasts = false;
        }

        public IEnumerator FadeToBlack(float duration)
        {
            if (blackFadeGroup == null) yield break;

            blackFadeGroup.blocksRaycasts = true;
            yield return FadeGroup(blackFadeGroup, blackFadeGroup.alpha, 1f, duration);
        }

        public IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;

            if (duration <= 0f)
            {
                group.alpha = to;
                yield break;
            }

            float timer = 0f;
            group.alpha = from;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / duration);
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            group.alpha = to;
        }

        private void FixBlackFadeGroupIfNeeded()
        {
            if (blackFadeGroup == null) return;
            if (blackFadeGroup.GetComponentInChildren<Graphic>() != null) return;
            if (blackFadeGroup.transform.parent == null) return;
            if (blackFadeGroup.transform.parent.GetComponentInChildren<Graphic>() == null) return;

            CanvasGroup parentGroup = blackFadeGroup.transform.parent.GetComponent<CanvasGroup>();

            if (parentGroup == null)
            {
                parentGroup = blackFadeGroup.transform.parent.gameObject.AddComponent<CanvasGroup>();
            }

            blackFadeGroup = parentGroup;
        }
    }
}
