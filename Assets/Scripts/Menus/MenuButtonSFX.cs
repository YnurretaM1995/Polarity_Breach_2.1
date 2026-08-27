using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PolarityBreach.Menus
{
    [RequireComponent(typeof(Button))]
    public class MenuButtonSFX : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip navigationClip;
        [SerializeField] private AudioClip selectClip;

        private bool hasBeenSelected;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = FindFirstObjectByType<AudioSource>();
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            hasBeenSelected = true;
            Play(navigationClip);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Play(navigationClip);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Play(selectClip);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Play(selectClip);
        }

        private void Play(AudioClip clip)
        {
            if (audioSource == null || clip == null) return;

            audioSource.PlayOneShot(clip);
        }
    }
}
