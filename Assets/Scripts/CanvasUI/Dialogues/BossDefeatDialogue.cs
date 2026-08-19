using UnityEngine;
using PolarityBreach.Boss;

namespace PolarityBreach.UI
{
    public class BossDefeatedDialogue : MonoBehaviour
    {
        [SerializeField] private BossHealth bossHealth;
        [SerializeField] private DialogueTrigger endingDialogue;

        private bool subscribed;

        private void Update()
        {
            if (subscribed) return;

            if (bossHealth == null)
                bossHealth = FindFirstObjectByType<BossHealth>();

            if (bossHealth != null)
            {
                bossHealth.OnDied += PlayEnding;
                subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (bossHealth != null) bossHealth.OnDied -= PlayEnding;
            subscribed = false;
        }

        private void PlayEnding()
        {
            if (endingDialogue != null) endingDialogue.Play();
        }
    }
}