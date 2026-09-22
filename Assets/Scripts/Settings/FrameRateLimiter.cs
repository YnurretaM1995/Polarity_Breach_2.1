using UnityEngine;

namespace PolarityBreach.Settings
{
    public static class FrameRateLimiter
    {
        private const int TargetFps = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFps;
        }
    }
}