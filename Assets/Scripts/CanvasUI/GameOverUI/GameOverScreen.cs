using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PolarityBreach.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        public static GameOverScreen Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject root;
        [SerializeField] private CanvasGroup blackBackground;

        [Header("Story Images")]
        [SerializeField] private CanvasGroup[] storyImages;
        [SerializeField] private float[] imageHoldDurations;

        [Header("Game Over Block")]
        [SerializeField] private CanvasGroup gameOverBlock;
        [SerializeField] private Button retryButton;

        [Header("Timing")]
        [SerializeField] private float blackFadeDuration = 1.5f;
        [SerializeField] private float imageFadeDuration = 1f;
        [SerializeField] private float defaultImageHold = 2.5f;
        [SerializeField] private float gameOverFadeDuration = 1f;
        [SerializeField] private float delayBeforeButton = 0.5f;

        [Header("Dim Overlay")]
        [SerializeField] private CanvasGroup dimOverlay;
        [SerializeField] private float dimAlpha = 0.6f;
        [SerializeField] private float dimFadeDuration = 0.8f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (root != null) root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Show()
        {
            if (Instance == null)
            {
                Debug.LogWarning("GameOverScreen: no instance in the scene.");
                return;
            }

            if (UIQueue.Instance == null)
            {
                Debug.LogWarning("GameOverScreen: no UIQueue in the scene.");
                return;
            }

            UIQueue.Instance.Enqueue(new GameOverRequest(Instance));
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }

        public IEnumerator PlaySequence()
        {
            Prepare();

            if (root != null) root.SetActive(true);

            yield return Fade(blackBackground, 0f, 1f, blackFadeDuration);

            for (int i = 0; i < storyImages.Length; i++)
            {
                if (storyImages[i] == null) continue;

                yield return Fade(storyImages[i], 0f, 1f, imageFadeDuration);
                yield return new WaitForSecondsRealtime(GetHold(i));

                bool isLast = i == storyImages.Length - 1;
                if (!isLast)
                    yield return Fade(storyImages[i], 1f, 0f, imageFadeDuration);
            }

            yield return Fade(dimOverlay, 0f, dimAlpha, dimFadeDuration);

            yield return Fade(gameOverBlock, 0f, 1f, gameOverFadeDuration);

            yield return new WaitForSecondsRealtime(delayBeforeButton);
            ShowRetryButton();

            while (true)
                yield return null;
        }

        private float GetHold(int index)
        {
            if (imageHoldDurations != null && index < imageHoldDurations.Length && imageHoldDurations[index] > 0f)
                return imageHoldDurations[index];

            return defaultImageHold;
        }

        private void Prepare()
        {
            if (blackBackground != null) blackBackground.alpha = 0f;
            if (gameOverBlock != null) gameOverBlock.alpha = 0f;
            if (retryButton != null) retryButton.gameObject.SetActive(false);
            if (dimOverlay != null) dimOverlay.alpha = 0f;

            for (int i = 0; i < storyImages.Length; i++)
            {
                if (storyImages[i] != null) storyImages[i].alpha = 0f;
            }
        }

        private void ShowRetryButton()
        {
            if (retryButton == null) return;

            retryButton.gameObject.SetActive(true);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }

        private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;

            float t = 0f;
            group.alpha = from;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }

            group.alpha = to;
        }
    }
}