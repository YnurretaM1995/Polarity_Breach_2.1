using UnityEngine;
using UnityEngine.Audio;

namespace PolarityBreach.Audio
{
    [DefaultExecutionOrder(-1000)]
    public class AudioHandlerSettings : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup defaultSfxMixerGroup;

        private void Awake()
        {
            AudioHandler.SetDefaultSfxMixerGroup(defaultSfxMixerGroup);
        }

        private void OnDisable()
        {
            AudioHandler.ClearDefaultSfxMixerGroup(defaultSfxMixerGroup);
        }
    }
}
