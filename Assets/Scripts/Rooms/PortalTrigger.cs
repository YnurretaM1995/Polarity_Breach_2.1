using UnityEngine;

namespace PolarityBreach.Level
{
    [RequireComponent(typeof(Collider))]
    public class PortalTrigger : MonoBehaviour
    {
        [SerializeField] private RoomPortal portal;

        private void Awake()
        {
            if (portal == null) portal = GetComponentInParent<RoomPortal>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (portal == null) return;

            Transform playerRoot = other.attachedRigidbody != null
                ? other.attachedRigidbody.transform
                : other.transform;

            portal.OnPlayerEntered(playerRoot);
        }
    }
}