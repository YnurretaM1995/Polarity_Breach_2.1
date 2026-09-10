using PolarityBreach.Enemy;
using PolarityBreach.Menus;
using PolarityBreach.Player;
using PolarityBreach.PolaritySystem;
using PolarityBreach.UI;
using System.Collections;
using UnityEngine;

namespace PolarityBreach.Level
{
    public class RoomPortal : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyWaveSpawner roomSpawner;
        [SerializeField] private GameObject portalVisuals;
        [SerializeField] private Transform destinationPoint;

        [Header("Teleport")]
        [SerializeField] private float teleportDelay = 0.1f;
        [SerializeField] private bool closeAfterUse = true;
        [SerializeField] private CameraControlScript cameraControl;

        [Header("Next Room")]
        [SerializeField] private GameObject nextRoomSpawner;

        [Header("Player Shooting")]
        [SerializeField] private TestShooter playerShooter;

        [Header("Unlock On Cross")]
        [SerializeField] private PlayerStatsData playerStats;
        [SerializeField] private bool unlocksDash;
        [SerializeField] private bool unlocksChargeShot;

        [Header("Dialogue")]
        [SerializeField] private DialogueTrigger[] dialoguesOnCross;
        [SerializeField] private DialogueTrigger dialogueOnRoomCleared;

        [Header("Music")]
        [SerializeField] private GameMusicController musicController;
        [SerializeField] private bool playDialogueMusicOnRoomCleared;
        [SerializeField] private bool playLevelCompleteMusicOnRoomCleared;
        [SerializeField] private bool playLevelCompleteMusicAfterRoomClearedDialogue;
        [SerializeField] private bool playDialogueMusicOnCross;
        [SerializeField] private bool playBossMusicAfterCrossDialogues;
        [SerializeField] private bool playLevelMusicAfterCrossDialogues;
        [SerializeField] private int levelMusicIndexAfterCrossDialogues = -1;

        private bool isOpen;

        private void Awake()
        {
            if (portalVisuals != null) portalVisuals.SetActive(false);

            if (musicController == null)
                musicController = FindFirstObjectByType<GameMusicController>();

            if (playerShooter == null)
                playerShooter = FindFirstObjectByType<TestShooter>();
        }

        private void OnEnable()
        {
            if (roomSpawner != null)
                roomSpawner.OnRoomCleared += OpenPortal;
        }

        private void OnDisable()
        {
            if (roomSpawner != null)
                roomSpawner.OnRoomCleared -= OpenPortal;
        }

        private void OpenPortal()
        {
            isOpen = true;
            SetPlayerShooting(false);
            if (portalVisuals != null) portalVisuals.SetActive(true);

            if (dialogueOnRoomCleared != null)
            {
                if (playDialogueMusicOnRoomCleared && musicController != null)
                    musicController.PlayDialogueMusic();

                if (playLevelCompleteMusicAfterRoomClearedDialogue)
                    dialogueOnRoomCleared.Play(PlayLevelCompleteMusic);
                else
                    dialogueOnRoomCleared.Play();
            }

            if (playLevelCompleteMusicOnRoomCleared && !playLevelCompleteMusicAfterRoomClearedDialogue && musicController != null)
                musicController.PlayLevelCompleteMusic();
            else if (dialogueOnRoomCleared == null && playLevelCompleteMusicAfterRoomClearedDialogue)
                PlayLevelCompleteMusic();
        }

        public void OnPlayerEntered(Transform playerRoot)
        {
            if (!isOpen) return;
            if (destinationPoint == null) return;

            isOpen = false; 
            StartCoroutine(TeleportRoutine(playerRoot));
        }

        private IEnumerator TeleportRoutine(Transform player)
        {
            GameObject playerObj = player.gameObject;
            playerObj.SetActive(false);
            yield return new WaitForSecondsRealtime(teleportDelay);
            player.position = destinationPoint.position;

            Rigidbody rb = playerObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = destinationPoint.position;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            playerObj.SetActive(true);
            if (cameraControl == null) cameraControl = FindFirstObjectByType<CameraControlScript>();
            if (cameraControl != null) cameraControl.SnapToPlayer();

            if (playerStats == null) playerStats = playerObj.GetComponent<PlayerStatsData>();
            if (playerStats != null)
            {
                if (unlocksDash) playerStats.dashUnlocked = true;
                if (unlocksChargeShot) playerStats.chargeShotUnlocked = true;
            }


            if (nextRoomSpawner != null)
                nextRoomSpawner.SetActive(true);

            SetPlayerShooting(true);

            if (playDialogueMusicOnCross && musicController != null)
                musicController.PlayDialogueMusic();

            PlayDialoguesOnCross();

            if (closeAfterUse && portalVisuals != null)
                portalVisuals.SetActive(false);
            else
                isOpen = true;
        }

        private void PlayDialoguesOnCross()
        {
            int lastDialogueIndex = GetLastDialogueIndex();

            if (lastDialogueIndex < 0)
            {
                FinishCrossTransition();
                return;
            }

            for (int i = 0; i < dialoguesOnCross.Length; i++)
            {
                if (dialoguesOnCross[i] == null) continue;

                if (i == lastDialogueIndex)
                    dialoguesOnCross[i].Play(FinishCrossTransition);
                else
                    dialoguesOnCross[i].Play();
            }
        }

        private int GetLastDialogueIndex()
        {
            if (dialoguesOnCross == null) return -1;

            for (int i = dialoguesOnCross.Length - 1; i >= 0; i--)
            {
                if (dialoguesOnCross[i] != null) return i;
            }

            return -1;
        }

        private void PlayLevelCompleteMusic()
        {
            if (musicController == null) return;

            musicController.PlayLevelCompleteMusic();
        }

        private void PlayLevelMusicAfterCrossDialogues()
        {
            if (musicController == null) return;

            if (playBossMusicAfterCrossDialogues)
            {
                musicController.PlayBossMusic();
                return;
            }

            if (!playLevelMusicAfterCrossDialogues) return;
            if (levelMusicIndexAfterCrossDialogues < 0) return;

            musicController.PlayLevelMusic(levelMusicIndexAfterCrossDialogues);
        }

        private void FinishCrossTransition()
        {
            PlayLevelMusicAfterCrossDialogues();
            SetPlayerShooting(true);
        }

        private void SetPlayerShooting(bool enabled)
        {
            if (playerShooter == null)
                playerShooter = FindFirstObjectByType<TestShooter>();

            if (playerShooter != null)
                playerShooter.SetShootingEnabled(enabled);
        }
    }
}
