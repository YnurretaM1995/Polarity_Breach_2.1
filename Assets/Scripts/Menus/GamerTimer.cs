using UnityEngine;
using PolarityBreach.Player;
using PolarityBreach.Boss;

namespace PolarityBreach.Level
{
    public class GameTimer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossHealth bossHealth;
        private bool subscribedToBoss;

        private float elapsedTime;
        private bool isRunning;
        private bool isStopped;

        public float ElapsedTime => elapsedTime;
        public bool IsRunning => isRunning;

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
            PauseMenu.OnPauseChanged += HandlePause;
            SubscribeToBoss();
        }

        private void OnDisable()
        {
            PauseMenu.OnPauseChanged -= HandlePause;
            if (bossHealth != null) bossHealth.OnDied -= StopTimer;
            subscribedToBoss = false;
        }

        private void Start()
        {
            elapsedTime = 0f;
            isRunning = true;
        }

        private void Update()
        {
            if (!isRunning || isStopped) return;

            elapsedTime += Time.unscaledDeltaTime;

            if (bossHealth == null) SubscribeToBoss();
        }

        private void HandlePause(bool paused)
        {
            if (isStopped) return;
            isRunning = !paused;
        }

        public void StopTimer()
        {
            isStopped = true;
            isRunning = false;
            Debug.Log($"Run finished in {FormattedTime}");
        }

        private void SubscribeToBoss()
        {
            if (subscribedToBoss) return;

            if (bossHealth == null)
                bossHealth = FindFirstObjectByType<BossHealth>();

            if (bossHealth != null)
            {
                bossHealth.OnDied += StopTimer;
                subscribedToBoss = true;
                Debug.Log("GameTimer: subscribed to boss");
            }
        }
    }
}