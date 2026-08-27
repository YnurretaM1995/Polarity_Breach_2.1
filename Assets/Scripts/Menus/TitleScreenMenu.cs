using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PolarityBreach.Menus
{
    public class TitleScreenMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MenuController menuController;
        [SerializeField] private CanvasGroup menuButtonsGroup;
        [SerializeField] private CanvasGroup blackFadeGroup;
        [SerializeField] private Selectable defaultSelectedButton;
        [SerializeField] private AudioSource musicSource;

        [Header("Optional Options Menu")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Selectable optionsSelectedButton;

        [Header("Timing")]
        [SerializeField] private float fadeInDuration = 1.5f;
        [SerializeField] private float buttonsDelay = 2f;
        [SerializeField] private float buttonsFadeDuration = 0.75f;
        [SerializeField] private float fadeOutDuration = 1f;

        private bool isStarting;

        private void Awake()
        {
            if (menuController == null)
            {
                menuController = FindFirstObjectByType<MenuController>();
            }

            if (musicSource == null)
            {
                musicSource = FindFirstObjectByType<AudioSource>();
            }

            FixBlackFadeGroupIfNeeded();
        }

        private void Start()
        {
            Time.timeScale = 1f;

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            if (menuButtonsGroup != null)
            {
                menuButtonsGroup.alpha = 0f;
                menuButtonsGroup.interactable = false;
                menuButtonsGroup.blocksRaycasts = false;
            }

            if (blackFadeGroup != null)
            {
                blackFadeGroup.alpha = 1f;
                blackFadeGroup.blocksRaycasts = true;
            }

            if (musicSource != null)
            {
                musicSource.Play();
            }

            StartCoroutine(StartupRoutine());
        }

        public void StartGame()
        {
            if (isStarting) return;
            StartCoroutine(StartGameRoutine());
        }

        public void OpenOptions()
        {
            if (optionsPanel == null) return;

            optionsPanel.SetActive(true);

            if (menuButtonsGroup != null)
            {
                menuButtonsGroup.interactable = false;
                menuButtonsGroup.blocksRaycasts = false;
            }

            SelectButton(optionsSelectedButton);
        }

        public void CloseOptions()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            if (menuButtonsGroup != null)
            {
                menuButtonsGroup.interactable = true;
                menuButtonsGroup.blocksRaycasts = true;
            }

            SelectButton(defaultSelectedButton);
        }

        public void ExitGame()
        {
            if (isStarting) return;
            StartCoroutine(ExitGameRoutine());
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

        private IEnumerator StartupRoutine()
        {
            yield return FadeCanvasGroup(blackFadeGroup, 1f, 0f, fadeInDuration);

            if (buttonsDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(buttonsDelay);
            }

            yield return FadeCanvasGroup(menuButtonsGroup, 0f, 1f, buttonsFadeDuration);

            if (menuButtonsGroup != null)
            {
                menuButtonsGroup.interactable = true;
                menuButtonsGroup.blocksRaycasts = true;
            }

            if (blackFadeGroup != null)
            {
                blackFadeGroup.blocksRaycasts = false;
            }

            SelectButton(defaultSelectedButton);
        }

        private IEnumerator StartGameRoutine()
        {
            isStarting = true;
            DisableMenuInput();

            if (blackFadeGroup != null)
            {
                blackFadeGroup.blocksRaycasts = true;
            }

            yield return FadeCanvasGroup(blackFadeGroup, 0f, 1f, fadeOutDuration);

            if (menuController != null)
            {
                menuController.NewGameDialogYes();
            }
            else
            {
                Debug.LogWarning("TitleScreenMenu: MenuController is not assigned.");
            }
        }

        private IEnumerator ExitGameRoutine()
        {
            isStarting = true;
            DisableMenuInput();

            if (blackFadeGroup != null)
            {
                blackFadeGroup.blocksRaycasts = true;
            }

            yield return FadeCanvasGroup(blackFadeGroup, 0f, 1f, fadeOutDuration);

            if (menuController != null)
            {
                menuController.ExitButton();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private void DisableMenuInput()
        {
            if (menuButtonsGroup != null)
            {
                menuButtonsGroup.interactable = false;
                menuButtonsGroup.blocksRaycasts = false;
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
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

        private void SelectButton(Selectable button)
        {
            if (button == null || EventSystem.current == null) return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}


