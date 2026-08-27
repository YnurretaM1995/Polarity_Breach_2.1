using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PolarityBreach.Menus
{
    public class MenuSelectionPulse : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Scale Pulse")]
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float selectedScale = 1.08f;

        private bool isSelected;
        private RectTransform rectTransform;
        private Vector3 startingScale;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            startingScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        }

        private void Update()
        {
            if (!isSelected || rectTransform == null) return;

            float wave = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(1f, selectedScale, wave);
            rectTransform.localScale = startingScale * scale;
        }

        public void OnSelect(BaseEventData eventData)
        {
            isSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            StopPulse();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isSelected = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                return;
            }

            StopPulse();
        }

        private void StopPulse()
        {
            isSelected = false;

            if (rectTransform != null)
            {
                rectTransform.localScale = startingScale;
            }
        }
    }
}
