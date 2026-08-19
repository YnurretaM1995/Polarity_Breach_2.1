using UnityEngine;

namespace PolarityBreach.UI
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueSequence sequence;
        [SerializeField] private bool playOnStart;
        [SerializeField] private bool playOnce = true;

        private bool hasPlayed;

        private void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            if (playOnce && hasPlayed) return;

            hasPlayed = true;
            DialogueUI.Show(sequence);
        }
    }
}