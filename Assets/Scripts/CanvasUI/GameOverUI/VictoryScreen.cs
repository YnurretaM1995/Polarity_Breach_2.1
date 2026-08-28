using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PolarityBreach.Menus;
using PolarityBreach.Score;

namespace PolarityBreach.UI
{
    public class VictoryScreen : MonoBehaviour
    {
        public static VictoryScreen Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject root;
        [SerializeField] private CanvasGroup blackBackground;
        [SerializeField] private CanvasGroup victoryImage;

        [Header("Music")]
        [SerializeField] private GameMusicController musicController;

        [Header("Victory Block")]
        [SerializeField] private RectTransform victoryBlock;
        [SerializeField] private CanvasGroup victoryBlockGroup;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private GameObject recordLabel;

        [Header("Stamp Animation")]
        [SerializeField] private float startScale = 8f;
        [SerializeField] private float fallDuration = 0.5f;
        [SerializeField] private float holdDuration = 2f;
        [SerializeField] private float moveUpDuration = 0.5f;
        [SerializeField] private float topY = 320f;

        [Header("Table")]
        [SerializeField] private CanvasGroup tableGroup;
        [SerializeField] private Transform tableContent;
        [SerializeField] private ScoreRowUI rowPrefab;
        [SerializeField] private int rowsToShow = 10;

        [Header("Play Again")]
        [SerializeField] private Button playAgainButton;

        [Header("Timing")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float tableFadeDuration = 0.4f;
        [SerializeField] private float delayBeforeTable = 0.5f;

        private float blockCenterY;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (victoryBlock != null) blockCenterY = victoryBlock.anchoredPosition.y;
            if (root != null) root.SetActive(false);

            if (musicController == null)
                musicController = FindFirstObjectByType<GameMusicController>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Show(float runSeconds)
        {
            if (Instance == null)
            {
                Debug.LogWarning("VictoryScreen: no instance in the scene.");
                return;
            }

            if (UIQueue.Instance == null)
            {
                Debug.LogWarning("VictoryScreen: no UIQueue in the scene.");
                return;
            }

            UIQueue.Instance.Enqueue(new VictoryRequest(Instance, runSeconds));
        }

        public void PlayAgain()
        {
            Time.timeScale = 1f;
            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }

        public IEnumerator PlaySequence(float runSeconds)
        {
            bool isRecord = ScoreBoard.AddTime(runSeconds);

            Prepare(runSeconds, isRecord);

            if (root != null) root.SetActive(true);
            if (musicController != null) musicController.PlayWinScreenMusic();

            yield return Fade(victoryImage, 0f, 1f, fadeInDuration);
            yield return Fade(blackBackground, 0f, 1f, fadeInDuration);
            yield return BlockFall();
            yield return new WaitForSecondsRealtime(holdDuration);
            yield return BlockMoveUp();

            yield return new WaitForSecondsRealtime(delayBeforeTable);
            BuildTable(runSeconds);
            yield return Fade(tableGroup, 0f, 1f, tableFadeDuration);

            ShowPlayAgainButton();

            while (true)
                yield return null;
        }

        private void ShowPlayAgainButton()
        {
            if (playAgainButton == null) return;

            playAgainButton.gameObject.SetActive(true);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(playAgainButton.gameObject);
        }

        private void Prepare(float runSeconds, bool isRecord)
        {
            if (blackBackground != null) blackBackground.alpha = 0f;
            if (victoryImage != null) victoryImage.alpha = 0f;
            if (tableGroup != null) tableGroup.alpha = 0f;
            if (victoryBlockGroup != null) victoryBlockGroup.alpha = 0f;
            if (playAgainButton != null) playAgainButton.gameObject.SetActive(false);

            if (victoryBlock != null)
            {
                victoryBlock.localScale = Vector3.one * startScale;
                victoryBlock.anchoredPosition = new Vector2(victoryBlock.anchoredPosition.x, blockCenterY);
            }

            if (timeText != null) timeText.text = ScoreBoard.Format(runSeconds);
            if (recordLabel != null) recordLabel.SetActive(isRecord);
        }

        private IEnumerator BlockFall()
        {
            if (victoryBlock == null) yield break;

            float t = 0f;

            while (t < fallDuration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / fallDuration);
                float eased = 1f - Mathf.Pow(1f - p, 4f);

                victoryBlock.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, eased);

                if (victoryBlockGroup != null)
                    victoryBlockGroup.alpha = Mathf.Clamp01(p * 3f);

                yield return null;
            }

            victoryBlock.localScale = Vector3.one;
            if (victoryBlockGroup != null) victoryBlockGroup.alpha = 1f;

            yield return Punch();
        }

        private IEnumerator Punch()
        {
            if (victoryBlock == null) yield break;

            float duration = 0.12f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / duration);
                float scale = 1f + Mathf.Sin(p * Mathf.PI) * 0.12f;
                victoryBlock.localScale = Vector3.one * scale;
                yield return null;
            }

            victoryBlock.localScale = Vector3.one;
        }

        private IEnumerator BlockMoveUp()
        {
            if (victoryBlock == null) yield break;

            float t = 0f;
            float startY = victoryBlock.anchoredPosition.y;

            while (t < moveUpDuration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / moveUpDuration);
                float eased = 1f - Mathf.Pow(1f - p, 3f);

                float y = Mathf.Lerp(startY, topY, eased);
                victoryBlock.anchoredPosition = new Vector2(victoryBlock.anchoredPosition.x, y);

                yield return null;
            }

            victoryBlock.anchoredPosition = new Vector2(victoryBlock.anchoredPosition.x, topY);
        }

        private void BuildTable(float runSeconds)
        {
            if (tableContent == null || rowPrefab == null) return;

            for (int i = tableContent.childCount - 1; i >= 0; i--)
                Destroy(tableContent.GetChild(i).gameObject);

            List<ScoreEntry> top = ScoreBoard.GetTop(rowsToShow);
            bool highlighted = false;

            for (int i = 0; i < top.Count; i++)
            {
                ScoreRowUI row = Instantiate(rowPrefab, tableContent);
                bool isThisRun = !highlighted && Mathf.Approximately(top[i].seconds, runSeconds);
                if (isThisRun) highlighted = true;

                row.Set(i + 1, top[i], isThisRun);
            }
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
