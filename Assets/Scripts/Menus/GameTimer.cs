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

        private void OnEnable()
        {
            SubscribeToBoss();
        }

        private void OnDisable()
        {
            UnsubscribeFromBoss();
        }

        private void Start()
        {
            elapsedTime = 0f;
            isStopped = false;
        }

        private void Update()
        {
            if (isStopped) return;
            if (IsBlocked) return;

            elapsedTime += Time.unscaledDeltaTime;
        }

        public void StopTimer()
        {
            isStopped = true;
        }

        private void SubscribeToBoss()
        {
            if (subscribedToBoss) return;

            if (bossHealth == null)
                bossHealth = FindFirstObjectByType<BossHealth>(FindObjectsInactive.Include);

            if (bossHealth == null) return;

            bossHealth.OnDied += StopTimer;
            subscribedToBoss = true;
        }

        private void UnsubscribeFromBoss()
        {
            if (subscribedToBoss && bossHealth != null)
                bossHealth.OnDied -= StopTimer;

            subscribedToBoss = false;
        }
    }
}