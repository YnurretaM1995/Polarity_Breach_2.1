using UnityEngine;
using UnityEngine.Audio;

namespace PolarityBreach.Settings
{
    [DefaultExecutionOrder(-900)]
    public class AudioSettingsApplier : MonoBehaviour
    {
        public static AudioSettingsApplier Instance { get; private set; }

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string masterParameter = "MasterVolume";
        [SerializeField] private string musicParameter = "MusicVolume";
        [SerializeField] private string sfxParameter = "SFXVolume";
        [SerializeField] private float minDecibels = -80f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameSettings.Load();
            ApplyAll();
        }

        public void ApplyAll()
        {
            ApplyMaster(GameSettings.MasterVolume);
            ApplyMusic(GameSettings.MusicVolume);
            ApplySfx(GameSettings.SfxVolume);
        }

        public void ApplyMaster(float value) => SetVolume(masterParameter, value);
        public void ApplyMusic(float value) => SetVolume(musicParameter, value);
        public void ApplySfx(float value) => SetVolume(sfxParameter, value);

        private void SetVolume(string parameter, float value)
        {
            if (mixer == null || string.IsNullOrEmpty(parameter)) return;

            float decibels = value <= 0.0001f ? minDecibels : Mathf.Log10(Mathf.Clamp01(value)) * 20f;
            mixer.SetFloat(parameter, decibels);
        }
    }
}
