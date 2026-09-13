using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PolarityBreach.Menus
{
    public class MenuSelectionPulse : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Scale Pulse")]
        [SerializeField] private RectTransform pulseTarget;
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float selectedScale = 1.08f;

        private bool isSelected;
        private bool isPointerOver;
        private Vector3 startingScale;

        private void Awake()
        {
            if (pulseTarget == null)
            {
                pulseTarget = GetComponent<RectTransform>();
            }

            startingScale = pulseTarget != null ? pulseTarget.localScale : Vector3.one;
        }

        private void Update()
        {
            if (pulseTarget == null) return;

            bool selectedByEventSystem = EventSystem.current != null &&
                                         EventSystem.current.currentSelectedGameObject == gameObject;

            if (!isSelected && !selectedByEventSystem)
            {
                pulseTarget.localScale = startingScale;
                return;
            }

            if (isSelected && !selectedByEventSystem && !isPointerOver)
            {
                StopPulse();
                return;
            }

            float wave = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(1f, selectedScale, wave);
            pulseTarget.localScale = startingScale * scale;
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
            isPointerOver = true;
            isSelected = true;

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                return;
            }

            StopPulse();
        }

        private void StopPulse()
        {
            isSelected = false;

            if (pulseTarget != null)
            {
                pulseTarget.localScale = startingScale;
            }
        }
    }
}
