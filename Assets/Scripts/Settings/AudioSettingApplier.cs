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
        [SerializeField] private float masterMaxDecibels = 0f;
        [SerializeField] private float musicMaxDecibels = 0f;
        [SerializeField] private float sfxMaxDecibels = 20f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            GameSettings.Load();
            ApplyAll();
            //Debug.Log($"[AudioSettings] master {GameSettings.MasterVolume} | music {GameSettings.MusicVolume} | sfx {GameSettings.SfxVolume}");
            StartCoroutine(ApplyNextFrame());
        }

        private System.Collections.IEnumerator ApplyNextFrame()
        {
            yield return new WaitForEndOfFrame();
            ApplyAll();
            yield return null;
            ApplyAll();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ApplyAll()
        {
            ApplyMaster(GameSettings.MasterVolume);
            ApplyMusic(GameSettings.MusicVolume);
            ApplySfx(GameSettings.SfxVolume);
        }

        public void ApplyMaster(float value) => SetVolume(masterParameter, value, masterMaxDecibels);
        public void ApplyMusic(float value) => SetVolume(musicParameter, value, musicMaxDecibels);
        public void ApplySfx(float value) => SetVolume(sfxParameter, value, sfxMaxDecibels);

        private void SetVolume(string parameter, float value, float maxDecibels)
        {
            if (mixer == null || string.IsNullOrEmpty(parameter)) return;

            float decibels = value <= 0.0001f ? minDecibels : Mathf.Log10(Mathf.Clamp01(value)) * 20f + maxDecibels;
            mixer.SetFloat(parameter, decibels);
        }
    }
}
