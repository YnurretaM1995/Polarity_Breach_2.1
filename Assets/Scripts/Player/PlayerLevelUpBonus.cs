using UnityEngine;

namespace PolarityBreach.Player
{
    [RequireComponent(typeof(PlayerXP))]
    [RequireComponent(typeof(PlayerStatsData))]
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerLevelUpBonus : MonoBehaviour
    {
        [Header("Stat Increase per upgrade")]
        [SerializeField] private float attackDamageIncrease = 5f;
        [SerializeField] private float attackSpeedIncrease = 0.1f;
        [SerializeField] private float maxHealthIncrease = 30f;
        [SerializeField] private float attackSpeedDelayReduction = 0.02f; // cooldown between attacks

        [Header("UI")]
        [SerializeField] private LevelUpMenu levelUpMenu;

        private PlayerXP playerXP;
        private PlayerStatsData stats;
        private PlayerHealth health;

        private void Awake()
        {
            playerXP = GetComponent<PlayerXP>();
            stats = GetComponent<PlayerStatsData>();
            health = GetComponent<PlayerHealth>();

            if (levelUpMenu == null)
            {
                levelUpMenu = FindFirstObjectByType<LevelUpMenu>();
            }
        }

        private void OnEnable()
        {
            playerXP.OnLevelUp += HandleLevelUp;
        }

        private void OnDisable()
        {
            playerXP.OnLevelUp -= HandleLevelUp;
        }

        private void HandleLevelUp(int newLevel)
        {
            if (levelUpMenu != null)
            {
                levelUpMenu.Open(this);
                return;
            }

            ApplyAttackPowerUpgrade();
        }

        public void ApplyAttackPowerUpgrade()
        {
            stats.attackDamage += attackDamageIncrease;

            Debug.Log("Attack power upgraded. Damage: " + stats.attackDamage);
        }

        public void ApplyAttackSpeedUpgrade()
        {
            stats.attackSpeed += attackSpeedIncrease;
            stats.attackSpeedDelay = Mathf.Max(0.05f, stats.attackSpeedDelay - attackSpeedDelayReduction);

            Debug.Log("Attack speed upgraded. AttackSpeed: " + stats.attackSpeed + "AttackDelay: " + stats.attackSpeedDelay);
        }

        public void ApplyMaxHealthUpgrade()
        {
            stats.maxHealth += maxHealthIncrease;

            health.IncreaseMaxHealth(maxHealthIncrease);

            Debug.Log("Max health upgraded. MaxHP:" + stats.maxHealth);
        }
    }
}
