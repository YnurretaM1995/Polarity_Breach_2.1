using UnityEngine;
using UnityEngine.UI;
using PolarityBreach.Player;
using PolarityBreach.PolaritySystem;

namespace PolarityBreach.UI
{
    public class ChargeBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform barContainer;
        [SerializeField] private Image fillBar;

        [Header("Player")]
        [SerializeField] private TestShooter shooter;
        [SerializeField] private PlayerStatsData playerStats;
        [SerializeField] private PolarityComponent polarity;

        [Header("Colors")]
        [SerializeField] private Color whiteColor = new Color(0.95f, 0.95f, 0.98f);
        [SerializeField] private Color blackColor = new Color(0.12f, 0.12f, 0.16f);

        [Header("Shake")]
        [SerializeField] private float shakeAmount = 5f;
        [SerializeField] private float shakeSpeed = 45f;

        private Vector2 basePosition;

        private void Awake()
        {
            if (barContainer != null) basePosition = barContainer.anchoredPosition;
            if (root != null) root.SetActive(false);
            if (fillBar != null) fillBar.fillAmount = 0f;
        }

        private void Update()
        {
            if (shooter == null || playerStats == null || fillBar == null) return;

            if (!playerStats.chargeShotUnlocked || !shooter.IsCharging)
            {
                Hide();
                return;
            }

            if (root != null && !root.activeSelf) root.SetActive(true);

            fillBar.fillAmount = shooter.ChargeProgress;
            fillBar.color = GetPolarityColor();

            if (shooter.ChargeReady)
                ApplyShake();
            else
                ResetPosition();
        }

        private void Hide()
        {
            if (fillBar != null) fillBar.fillAmount = 0f;
            ResetPosition();

            if (root != null && root.activeSelf) root.SetActive(false);
        }

        private Color GetPolarityColor()
        {
            if (polarity == null) return whiteColor;
            return polarity.CurrentPolarity == Polarity.Black ? blackColor : whiteColor;
        }

        private void ApplyShake()
        {
            if (barContainer == null) return;

            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float y = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeAmount * 0.5f;

            barContainer.anchoredPosition = basePosition + new Vector2(x, y);
        }

        private void ResetPosition()
        {
            if (barContainer == null) return;
            barContainer.anchoredPosition = basePosition;
        }
    }
}