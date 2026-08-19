using UnityEngine;
using UnityEngine.UI;

namespace PolarityBreach.Player
{
    public class BloodOverlay : MonoBehaviour
    {
        [SerializeField] private Image bloodImage;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Intensity")]
        [SerializeField] private float minAlpha = 0f;
        [SerializeField] private float maxAlpha = 0.7f;

        [Header("Heartbeat (pulse)")]
        [SerializeField] private float pulseAmount = 0.15f;
        [SerializeField] private float slowPulse = 1.5f;
        [SerializeField] private float fastPulse = 6f;

        [Header("Recent Damage")]
        [SerializeField] private float hitFlash = 0.4f;
        [SerializeField] private float hitFadeSpeed = 2f;

        private float _hitAlpha;
        private float _pulseTimer;

        private void Awake()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();
            SetAlpha(0f);
        }

        public void OnDamaged()
        {
            _hitAlpha = hitFlash;
        }

        private void Update()
        {
            if (playerHealth == null || bloodImage == null) return;

            float healthPercent = playerHealth.CurrentHealth / playerHealth.MaxHealth;
            float lowHealth = 1f - Mathf.Clamp01(healthPercent);

            float baseAlpha = Mathf.Lerp(minAlpha, maxAlpha, lowHealth);

            float pulseSpeed = Mathf.Lerp(slowPulse, fastPulse, lowHealth);
            _pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(_pulseTimer) + 1f) * 0.5f * pulseAmount * lowHealth;

            if (_hitAlpha > 0f)
                _hitAlpha -= hitFadeSpeed * Time.deltaTime;

            float finalAlpha = baseAlpha + pulse + Mathf.Max(0f, _hitAlpha);
            SetAlpha(Mathf.Clamp01(finalAlpha));
        }

        private void SetAlpha(float a)
        {
            Color c = bloodImage.color;
            c.a = a;
            bloodImage.color = c;
        }
    }
}