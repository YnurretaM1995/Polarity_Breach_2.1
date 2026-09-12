using PolarityBreach.PolaritySystem;
using UnityEngine;

namespace PolarityBreach.UI
{
    public class PolarityBorderUI : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private PolarityComponent playerPolarity;

        [Header("Borders")]
        [SerializeField] private CanvasGroup whiteBorder;
        [SerializeField] private CanvasGroup blackBorder;

        [Header("Visibility")]
        [SerializeField, Range(0f, 1f)] private float activeAlpha = 0.35f;
        [SerializeField, Range(0f, 1f)] private float inactiveAlpha = 0f;
        [SerializeField] private bool smoothFade = true;
        [SerializeField] private float fadeSpeed = 8f;

        private float targetWhiteAlpha;
        private float targetBlackAlpha;

        private void Awake()
        {
            if (playerPolarity == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerPolarity = player.GetComponentInChildren<PolarityComponent>();
            }
            
            ConfigureBorder(whiteBorder);
            ConfigureBorder(blackBorder);
        }

        private void OnEnable()
        {
            if (playerPolarity != null)
            {
                playerPolarity.OnPolarityChanged += SetPolarity;
                SetPolarity(playerPolarity.CurrentPolarity, true);
            }
        }

        private void OnDisable()
        {
            if (playerPolarity != null)
            {
                playerPolarity.OnPolarityChanged -= SetPolarity;
            }
        }

        private void Update()
        {
            if (!smoothFade) return;

            FadeTowardTarget(whiteBorder, targetWhiteAlpha);
            FadeTowardTarget(blackBorder, targetBlackAlpha);
        }

        private void SetPolarity(Polarity polarity)
        {
            SetPolarity(polarity, false);
        }

        private void SetPolarity(Polarity polarity, bool instant)
        {
            bool isWhite = polarity == Polarity.White;
            targetWhiteAlpha = isWhite ? activeAlpha : inactiveAlpha;
            targetBlackAlpha = isWhite ? inactiveAlpha : activeAlpha;

            if (instant || !smoothFade)
            {
                SetAlpha(whiteBorder, targetWhiteAlpha);
                SetAlpha(blackBorder, targetBlackAlpha);
            }
        }

        private CanvasGroup ResolveCanvasGroup(CanvasGroup group, GameObject borderObject)
        {
            if (group != null) return group;
            if (borderObject == null) return null;

            CanvasGroup foundGroup = borderObject.GetComponent<CanvasGroup>();
            if (foundGroup == null) foundGroup = borderObject.AddComponent<CanvasGroup>();

            return foundGroup;
        }

        private void ConfigureBorder(CanvasGroup group)
        {
            if (group == null) return;

            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private void FadeTowardTarget(CanvasGroup group, float targetAlpha)
        {
            if (group == null) return;

            float speed = Mathf.Max(0.01f, fadeSpeed);
            group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, speed * Time.unscaledDeltaTime);
        }

        private void SetAlpha(CanvasGroup group, float alpha)
        {
            if (group == null) return;
            group.alpha = alpha;
        }
    }
}
