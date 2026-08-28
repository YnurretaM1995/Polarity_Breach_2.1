using UnityEngine;

namespace PolarityBreach.Menus
{
    public class GameMusicController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioSource musicSource;

        [Header("Music")]
        [SerializeField] private AudioClip introMusic;
        [SerializeField] private AudioClip[] levelMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip gameOverMusic;
        [SerializeField] private AudioClip winScreenMusic;
        [SerializeField] private AudioClip levelCompleteMusic;

        private void Awake()
        {
            if (musicSource == null)
            {
                musicSource = GetComponent<AudioSource>();
            }
        }

        private void Start()
        {
            PlayIntroMusic();
        }

        public void PlayIntroMusic()
        {
            PlayMusic(introMusic);
        }

        public void PlayLevelMusic(int levelIndex)
        {
            if (levelMusic == null || levelIndex < 0 || levelIndex >= levelMusic.Length) return;

            PlayMusic(levelMusic[levelIndex]);
        }

        public void PlayBossMusic()
        {
            PlayMusic(bossMusic);
        }

        public void StopMusic()
        {
            if (musicSource == null) return;

            musicSource.Stop();
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;

            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
