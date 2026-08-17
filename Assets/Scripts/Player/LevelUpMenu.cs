using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PolarityBreach.Player
{
    public class LevelUpMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button attackPowerButton;
        [SerializeField] private Button attackSpeedButton;
        [SerializeField] private Button maxHealthButton;
        [SerializeField] private Button defaultSelectedButton;

        private PlayerLevelUpBonus levelUpBonus;
        private float previousTimeScale = 1f;

        public static bool IsOpen { get; private set; }

        private void Awake()
        {
            if (panel == null)
            {
                panel = gameObject;
            }

            if (defaultSelectedButton == null)
            {
                defaultSelectedButton = attackPowerButton;
            }

            attackPowerButton?.onClick.AddListener(ChooseAttackPower);
            attackSpeedButton?.onClick.AddListener(ChooseAttackSpeed);
            maxHealthButton?.onClick.AddListener(ChooseMaxHealth);

            panel.SetActive(false);
            IsOpen = false;
        }

        private void OnDestroy()
        {
            if (IsOpen)
            {
                IsOpen = false;
            }

            attackPowerButton?.onClick.RemoveListener(ChooseAttackPower);
            attackSpeedButton?.onClick.RemoveListener(ChooseAttackSpeed);
            maxHealthButton?.onClick.RemoveListener(ChooseMaxHealth);
        }

        public void Open(PlayerLevelUpBonus bonus)
        {
            levelUpBonus = bonus;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            panel.SetActive(true);
            IsOpen = true;

            StartCoroutine(SelectDefaultButtonNextFrame());
        }

        private IEnumerator SelectDefaultButtonNextFrame()
        {
            yield return null;

            if (EventSystem.current == null || defaultSelectedButton == null) yield break;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultSelectedButton.gameObject);
        }

        private void ChooseAttackPower()
        {
            if (!IsOpen || levelUpBonus == null) return;

            levelUpBonus.ApplyAttackPowerUpgrade();
            Close();
        }

        private void ChooseAttackSpeed()
        {
            if (!IsOpen || levelUpBonus == null) return;

            levelUpBonus.ApplyAttackSpeedUpgrade();
            Close();
        }

        private void ChooseMaxHealth()
        {
            if (!IsOpen || levelUpBonus == null) return;

            levelUpBonus.ApplyMaxHealthUpgrade();
            Close();
        }

        private void Close()
        {
            IsOpen = false;
            panel.SetActive(false);

            if (PauseMenu.IsPaused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
            }

            levelUpBonus = null;
        }
    }
}
