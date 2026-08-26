using UnityEngine;
using PolarityBreach.Player;
using PolarityBreach.Boss;
using PolarityBreach.UI;

namespace PolarityBreach.Level
{
    public class GameTimer : MonoBehaviour
    {
        [SerializeField] private BossHealth bossHealth;

        private float elapsedTime;
        private bool isStopped;
        private bool subscribedToBoss;

        public float ElapsedTime => elapsedTime;
        public bool IsRunning => !isStopped && !IsBlocked;

        private bool IsBlocked => UIQueue.IsBlocking || PauseMenu.IsPaused || LevelUpMenu.IsOpen;

        public string FormattedTime
        {
            get
            {
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                int hundredths = Mathf.FloorToInt((elapsedTime * 100f) % 100f);
                return $"{minutes:00}:{seconds:00}:{hundredths:00}";
            }
        }

        private void Start()
        {
            elapsedTime = 0f;
            isStopped = false;
        }

        private void Update()
        {
            if (!subscribedToBoss) SubscribeToBoss();

            if (isStopped) return;
            if (IsBlocked) return;

            elapsedTime += Time.unscaledDeltaTime;
        }

        private void OnDisable()
        {
            if (bossHealth != null) bossHealth.OnDied -= StopTimer;
            subscribedToBoss = false;
        }

        public void StopTimer()
        {
            isStopped = true;
            Debug.Log($"Run finished in {FormattedTime}");
        }

        private void SubscribeToBoss()
        {
            if (bossHealth == null)
                bossHealth = FindFirstObjectByType<BossHealth>();

            if (bossHealth != null)
            {
                bossHealth.OnDied += StopTimer;
                subscribedToBoss = true;
            }
        }
    }
}