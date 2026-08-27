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
        [SerializeField] private SceneFadeIn sceneFader;
        [SerializeField] private CanvasGroup menuButtonsGroup;
        [SerializeField] private Selectable defaultSelectedButton;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private string gameSceneName = "Game V2";

        [Header("Optional Options Menu")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Selectable optionsSelectedButton;

        [Header("Timing")]
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

            if (sceneFader == null)
            {
                sceneFader = FindFirstObjectByType<SceneFadeIn>();
            }

            if (musicSource == null)
            {
                musicSource = FindFirstObjectByType<AudioSource>();
            }
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

        private IEnumerator StartupRoutine()
        {
            if (sceneFader != null)
            {
                yield return sceneFader.FadeFromBlack();
            }

            if (buttonsDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(buttonsDelay);
            }

            if (sceneFader != null)
            {
                yield return sceneFader.FadeGroup(menuButtonsGroup, 0f, 1f, buttonsFadeDuration);
            }
            else if (menuButtonsGroup != null)
            {
                menuButtonsGroup.alpha = 1f;
            }

            if (menuButtonsGroup != null)
            {
                menuButtonsGroup.interactable = true;
                menuButtonsGroup.blocksRaycasts = true;
            }

            SelectButton(defaultSelectedButton);
        }

        private IEnumerator StartGameRoutine()
        {
            isStarting = true;
            DisableMenuInput();

            if (sceneFader != null)
            {
                yield return sceneFader.FadeOutAndLoadSceneRoutine(gameSceneName, fadeOutDuration);
            }
            else
            {
                Debug.LogWarning("TitleScreenMenu: SceneFadeIn is not assigned.");

                if (menuController != null)
                {
                    menuController.NewGameDialogYes();
                }
            }
        }

        private IEnumerator ExitGameRoutine()
        {
            isStarting = true;
            DisableMenuInput();

            if (sceneFader != null)
            {
                yield return sceneFader.FadeToBlack(fadeOutDuration);
            }

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

        private void SelectButton(Selectable button)
        {
            if (button == null || EventSystem.current == null) return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
