using UnityEngine;
using TMPro;

namespace PolarityBreach.Level
{
    public class GameTimerUI : MonoBehaviour
    {
        [SerializeField] private GameTimer gameTimer;
        [SerializeField] private TMP_Text timerText;

        private void Awake()
        {
            if (timerText == null) timerText = GetComponent<TMP_Text>();
            if (gameTimer == null) gameTimer = FindFirstObjectByType<GameTimer>();
        }

        private void Update()
        {
            if (gameTimer == null || timerText == null) return;
            timerText.text = gameTimer.FormattedTime;
        }
    }
}