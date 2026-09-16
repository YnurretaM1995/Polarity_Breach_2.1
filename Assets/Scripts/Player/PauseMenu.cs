using PolarityBreach.Enemy;
using PolarityBreach.Settings;
using PolarityBreach.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PolarityBreach.Player
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject cheatPanel;
        [SerializeField] private PolarityBreach.Settings.OptionsMenu optionsMenu;

        [Header("Selection")]
        [SerializeField] private Selectable defaultSelectedButton;

        [Header("Cheat")]
        [SerializeField] private CheatMenu cheatMenu;

        [Header("Cheat References")]
        [SerializeField] private PlayerStatsData playerStats;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private EnemyWaveSpawner waveSpawner;
        [SerializeField] private Transform player;
        [SerializeField] private Transform bossRoomPoint;

        public static bool IsPaused { get; private set; } 
        public static event Action<bool> OnPauseChanged;
        private PlayerInputActions controls;
        private Coroutine selectDefaultRoutine;
        
        public void ResumeButton() => Resume();
        public void ToggleDash(bool value) => playerStats.dashUnlocked = value;
        public void ToggleChargeShot(bool value) => playerStats.chargeShotUnlocked = value;
        public void ToggleGodMode(bool value) => playerHealth.GodMode = value;
        public void FullHeal() => playerHealth.FullHeal();
        public void SetAttackDamage(float value) => playerStats.attackDamage = value;
        
        
        private void Awake()
        {
            IsPaused = false;
            Time.timeScale = 1f;

            controls = new PlayerInputActions();
            controls.Player.Pause.performed += ctx => TogglePause();

            if (optionsMenu == null)
            {
                optionsMenu = FindFirstObjectByType<OptionsMenu>(FindObjectsInactive.Include);
            }

            if (optionsMenu != null)
            {
                optionsMenu.Closed += HandleOptionsClosed;
            }

            if (defaultSelectedButton == null)
            {
                defaultSelectedButton = FindResumeButton();
            }
        }

        private void OnEnable() => controls.Player.Pause.Enable();
        private void OnDisable() => controls.Player.Pause.Disable();

        private void OnDestroy()
        {
            if (optionsMenu != null)
            {
                optionsMenu.Closed -= HandleOptionsClosed;
            }
        }

        private void TogglePause()
        {
            if (UIQueue.IsBlocking) return;
            if (LevelUpMenu.IsOpen) return;

            if (IsPaused) Resume();
            else Pause();
        }

        private void Pause()
        {
            if (LevelUpMenu.IsOpen) return;

            IsPaused = true;
            pausePanel.SetActive(true);
            CursorManager.ShowMenuCursor();
            Time.timeScale = 0f;
            OnPauseChanged?.Invoke(true);
            SelectDefaultButtonNextFrame();
        }

        public void Resume()
        {
            if (LevelUpMenu.IsOpen) return;

            IsPaused = false;
            pausePanel.SetActive(false);
            if (cheatPanel != null) cheatPanel.SetActive(false);
            if (optionsMenu != null) optionsMenu.Close();
            if (cheatMenu != null) cheatMenu.CloseMenu();
            Time.timeScale = 1f;
            CursorManager.ShowGameplayCursor();
            OnPauseChanged?.Invoke(false);
        }
        
        public void OpenCheatPanel()
        {
            pausePanel.SetActive(false);
            cheatPanel.SetActive(true);
        }

        public void OpenSettings()
        {
            if (LevelUpMenu.IsOpen) return;

            if (!IsPaused)
            {
                IsPaused = true;
                Time.timeScale = 0f;
                CursorManager.ShowMenuCursor();
                OnPauseChanged?.Invoke(true);
            }

            if (pausePanel != null) pausePanel.SetActive(true);
            if (optionsMenu != null)
            {
                optionsMenu.gameObject.SetActive(true);
                optionsMenu.Open();
            }
        }

        public void ExitGame()
        {
            Time.timeScale = 1f;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        public void OpenCheatMenu()
        {
            cheatMenu.ToggleMenu();  
            ResumeGameplay();           
        }
        
        private void ResumeGameplay()
        {
            if (LevelUpMenu.IsOpen) return;

            IsPaused = false;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            CursorManager.ShowGameplayCursor();
            OnPauseChanged?.Invoke(false);
        }
        

        public void BackToPauseMenu()
        {
            cheatPanel.SetActive(false);
            pausePanel.SetActive(true);
            SelectDefaultButtonNextFrame();
        }

        private void HandleOptionsClosed()
        {
            if (!IsPaused || pausePanel == null || !pausePanel.activeInHierarchy) return;

            SelectDefaultButtonNextFrame();
        }

        private Selectable FindResumeButton()
        {
            if (pausePanel == null) return null;

            Selectable[] selectables = pausePanel.GetComponentsInChildren<Selectable>(true);
            for (int i = 0; i < selectables.Length; i++)
            {
                if (selectables[i] != null && selectables[i].name == "ResumeButton")
                    return selectables[i];
            }

            for (int i = 0; i < selectables.Length; i++)
            {
                if (selectables[i] != null && selectables[i].name.Contains("Resume"))
                    return selectables[i];
            }

            return selectables.Length > 0 ? selectables[0] : null;
        }

        private void SelectDefaultButtonNextFrame()
        {
            if (selectDefaultRoutine != null)
                StopCoroutine(selectDefaultRoutine);

            selectDefaultRoutine = StartCoroutine(SelectDefaultButtonRoutine());
        }

        private IEnumerator SelectDefaultButtonRoutine()
        {
            yield return null;

            if (EventSystem.current != null && defaultSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(defaultSelectedButton.gameObject);
            }

            selectDefaultRoutine = null;
        }
       
        public void SetFireRate(float sliderValue)
        {
            float slowest = 0.6f; 
            float fastest = 0.1f; 
            playerStats.attackSpeedDelay = Mathf.Lerp(slowest, fastest, sliderValue);
        }

        public void SkipWave()
        {
            if (waveSpawner != null) waveSpawner.SkipCurrentWave();
        }

        public void TeleportToBossRoom()
        {
            if (player != null && bossRoomPoint != null)
                player.position = bossRoomPoint.position;
        }
    }
}
