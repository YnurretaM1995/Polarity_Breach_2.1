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
        [SerializeField, Range(0f, 1f)] private float navigationVolume = 0.4f;
        [SerializeField, Range(0f, 1f)] private float selectVolume = 1f;

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
            Play(navigationClip, navigationVolume);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Play(navigationClip, navigationVolume);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Play(selectClip, selectVolume);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Play(selectClip, selectVolume);
        }

        private void Play(AudioClip clip, float volume)
        {
            if (audioSource == null || clip == null) return;

            audioSource.PlayOneShot(clip, volume);
        }
    }
}
