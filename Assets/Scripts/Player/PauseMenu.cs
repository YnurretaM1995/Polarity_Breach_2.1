using PolarityBreach.Enemy;
using PolarityBreach.UI;
using System;
using UnityEngine;

namespace PolarityBreach.Player
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject cheatPanel;
        
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
        
        public void ResumeButton() => Resume();
        public void ToggleDash(bool value) => playerStats.dashUnlocked = value;
        public void ToggleChargeShot(bool value) => playerStats.chargeShotUnlocked = value;
        public void ToggleGodMode(bool value) => playerHealth.GodMode = value;
        public void FullHeal() => playerHealth.FullHeal();
        public void SetAttackDamage(float value) => playerStats.attackDamage = value;
        
        
        private void Awake()
        {
            controls = new PlayerInputActions();
            controls.Player.Pause.performed += ctx => TogglePause();
        }

        private void OnEnable() => controls.Player.Pause.Enable();
        private void OnDisable() => controls.Player.Pause.Disable();

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
            Time.timeScale = 0f;
            OnPauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (LevelUpMenu.IsOpen) return;

            IsPaused = false;
            pausePanel.SetActive(false);
            if (cheatPanel != null) cheatPanel.SetActive(false); 
            if (cheatMenu != null) cheatMenu.CloseMenu();
            Time.timeScale = 1f;
            OnPauseChanged?.Invoke(false);
        }
        
        public void OpenCheatPanel()
        {
            pausePanel.SetActive(false);
            cheatPanel.SetActive(true);
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
            OnPauseChanged?.Invoke(false);
        }
        

        public void BackToPauseMenu()
        {
            cheatPanel.SetActive(false);
            pausePanel.SetActive(true);
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