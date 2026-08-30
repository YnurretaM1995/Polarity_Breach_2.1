using UnityEngine;
using UnityEngine.Audio;

namespace PolarityBreach.Audio
{
    public static class AudioHandler
    {
        public static AudioMixerGroup DefaultSfxMixerGroup { get; private set; }

        public static void SetDefaultSfxMixerGroup(AudioMixerGroup mixerGroup)
        {
            DefaultSfxMixerGroup = mixerGroup;
        }

        public static void ClearDefaultSfxMixerGroup(AudioMixerGroup mixerGroup)
        {
            if (DefaultSfxMixerGroup == mixerGroup)
            {
                DefaultSfxMixerGroup = null;
            }
        }

        public static void Play3DSound(AudioClip clip, Vector3 position)
        {
            Play3DSound(clip, position, 1f);
        }

        public static void Play3DSound(AudioClip clip, Vector3 position, float volume)
        {
            Play3DSound(clip, position, volume, DefaultSfxMixerGroup);
        }

        public static void Play3DSound(AudioClip clip, Vector3 position, float volume, AudioMixerGroup mixerGroup)
        {
            if (clip == null) return;

            if (mixerGroup == null)
            {
                AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volume));
                return;
            }

            GameObject audioObject = new GameObject("One Shot 3D Audio");
            audioObject.transform.position = position;
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.spatialBlend = 1f;
            source.outputAudioMixerGroup = mixerGroup;
            source.Play();

            Object.Destroy(audioObject, clip.length);
        }

        public static void Play2DSound(AudioClip clip, float volume = 1f)
        {
            Play2DSound(clip, volume, DefaultSfxMixerGroup);
        }

        public static void Play2DSound(AudioClip clip, float volume, AudioMixerGroup mixerGroup)
        {
            if (clip == null) return;

            GameObject audioObject = new GameObject("One Shot 2D Audio");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = mixerGroup;
            source.Play();

            Object.Destroy(audioObject, clip.length);
        }
    }
}
