using UnityEngine;
using PolarityBreach.Player;

namespace PolarityBreach.Settings
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(PlayerStatsData))]
    public class DifficultyApplier : MonoBehaviour
    {
        private void Awake()
        {
            GameSettings.Load();

            PlayerStatsData stats = GetComponent<PlayerStatsData>();
            if (stats == null) return;

            stats.maxHealth = GameSettings.GetStartingHealth();
        }
    }
}