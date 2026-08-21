using System.Collections;
using UnityEngine;
using PolarityBreach.Boss;
using PolarityBreach.Level;

namespace PolarityBreach.UI
{
    public class BossDefeatedDialogue : MonoBehaviour
    {
        [SerializeField] private BossHealth bossHealth;
        [SerializeField] private DialogueTrigger endingDialogue;
        [SerializeField] private GameTimer gameTimer;
        [SerializeField] private float deathAnimationDuration = 2f;
        [SerializeField] private float extraDelay = 1f;

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
            StartCoroutine(PlayEndingDelayed());
        }

        private IEnumerator PlayEndingDelayed()
        {
            yield return new WaitForSecondsRealtime(deathAnimationDuration + extraDelay);

            if (endingDialogue != null) endingDialogue.Play();

            if (gameTimer == null) gameTimer = FindFirstObjectByType<GameTimer>();
            if (gameTimer != null) VictoryScreen.Show(gameTimer.ElapsedTime);
        }
    }
}