using PolarityBreach.Player;
using TMPro;
using UnityEngine;

namespace PolarityBreach.Menus
{
    public class PlayerStatsHUD : MonoBehaviour
    {
        [SerializeField] private PlayerStatsData playerStats;
        [SerializeField] private TMP_Text attackDamageText;
        [SerializeField] private TMP_Text attackSpeedText;
        [SerializeField] private TMP_Text maxHealthText;

        private void Awake()
        {
            if (playerStats == null)
            {
                playerStats = FindFirstObjectByType<PlayerStatsData>();
            }
        }

        private void Update()
        {
            if (playerStats == null) return;

            if (attackDamageText != null)
            {
                attackDamageText.text = playerStats.attackDamage.ToString("0.0");
            }

            if (attackSpeedText != null)
            {
                attackSpeedText.text = playerStats.attackSpeedDelay.ToString("0.00");
            }

            if (maxHealthText != null)
            {
                maxHealthText.text = Mathf.RoundToInt(playerStats.maxHealth).ToString();
            }
        }
    }
}
