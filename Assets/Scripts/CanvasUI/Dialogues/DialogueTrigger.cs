using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PolarityBreach.UI
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueSequence sequence;
        [SerializeField] private bool playOnStart;
        [SerializeField] private float startDelay;
        [SerializeField] private bool playOnce = true;
        [SerializeField] private UnityEvent onDialogueFinished;

        private bool hasPlayed;

        private void Start()
        {
            if (playOnStart) StartCoroutine(PlayDelayed());
        }

        private IEnumerator PlayDelayed()
        {
            if (startDelay > 0f)
                yield return new WaitForSecondsRealtime(startDelay);

            Play();
        }

        public void Play()
        {
            Play(null);
        }

        public void Play(Action onFinished)
        {
            if (playOnce && hasPlayed)
            {
                onFinished?.Invoke();
                return;
            }

            hasPlayed = true;
            DialogueUI.Show(sequence, () =>
            {
                onDialogueFinished?.Invoke();
                onFinished?.Invoke();
            });
        }
    }
}
