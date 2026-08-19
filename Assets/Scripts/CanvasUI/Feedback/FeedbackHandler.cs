using UnityEngine;

namespace PolarityBreach.Feedback
{
    public static class FeedbackHandler
    {
        public static void SpawnParticles(GameObject prefab, Vector3 position, Vector3 impactNormal)
        {
            Quaternion rotation = Quaternion.LookRotation(impactNormal);
            GameObject effect = Object.Instantiate(prefab, position, rotation);
            Object.Destroy(effect, 2f);
        }
    }
}