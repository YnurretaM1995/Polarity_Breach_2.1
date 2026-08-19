using PolarityBreach.Enemy;
using PolarityBreach.Player;
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

        [Header("Unlock On Cross")]
        [SerializeField] private PlayerStatsData playerStats;
        [SerializeField] private bool unlocksDash;
        [SerializeField] private bool unlocksChargeShot;

        [Header("Dialogue")]
        [SerializeField] private DialogueTrigger[] dialoguesOnCross;
        [SerializeField] private DialogueTrigger dialogueOnRoomCleared;

        private bool isOpen;

        private void Awake()
        {
            if (portalVisuals != null) portalVisuals.SetActive(false);
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
            if (portalVisuals != null) portalVisuals.SetActive(true);
            if (dialogueOnRoomCleared != null) dialogueOnRoomCleared.Play();
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

            if (dialoguesOnCross != null)
            {
                for (int i = 0; i < dialoguesOnCross.Length; i++)
                    if (dialoguesOnCross[i] != null) dialoguesOnCross[i].Play();
            }
            if (closeAfterUse && portalVisuals != null)
                portalVisuals.SetActive(false);
            else
                isOpen = true;
        }
    }
}