using UnityEngine;
using PolarityBreach.Enemy;
using TMPro;

namespace PolarityBreach.Enemy
{
    public class EnemiesRemainingUI : MonoBehaviour
    {
        [SerializeField] private EnemyWaveSpawner enemyWaveSpawner;
        [SerializeField] private EnemyPool enemyPool;
        [SerializeField] private TMP_Text enemyText;
        [SerializeField] private RectTransform panel;
        private EnemyWaveSpawner[] enemyWaveSpawners;

        [Header("Slide")]
        [SerializeField] private Vector2 hiddenPosition = new Vector2(0f, 120f);
        [SerializeField] private Vector2 visiblePosition = new Vector2(0f, -40f);
        [SerializeField] private float slideSpeed = 8f;

        private void Awake()
        {
            if (enemyWaveSpawner == null)
            {
                enemyWaveSpawners = FindObjectsByType<EnemyWaveSpawner>(FindObjectsSortMode.None);
            }

            if (panel == null)
            {
                panel = GetComponent<RectTransform>();
            }

            panel.anchoredPosition = hiddenPosition;
        }

        private void Update()
        {
            if (enemyText == null || panel == null) return;

            int count = GetEnemyCount();

            enemyText.text = count.ToString();

            Vector2 targetPosition = count > 0 ? visiblePosition : hiddenPosition;
            panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPosition, slideSpeed * Time.deltaTime);
        }

        private int GetEnemyCount()
        {
            if (enemyWaveSpawner != null)
            {
                return enemyWaveSpawner.EnemiesRemaining;
            }

            if (enemyWaveSpawners != null && enemyWaveSpawners.Length > 0)
            {
                int totalEnemies = 0;

                for (int i = 0; i < enemyWaveSpawners.Length; i++)
                {
                    if (enemyWaveSpawners[i] != null)
                    {
                        totalEnemies += enemyWaveSpawners[i].EnemiesRemaining;
                    }
                }

                return totalEnemies;
            }

            if (enemyPool != null)
            {
                return enemyPool.ActiveEnemyCount;
            }

            return 0;
        }
    }
}
