using UnityEngine;
using UnityEngine.Serialization;

namespace PolarityBreach.Menus
{
    public class GameMusicController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioSource musicSource;

        [Header("Music")]
        [FormerlySerializedAs("introMusic")]
        [SerializeField] private AudioClip introDialogueMusic;
        [SerializeField] private AudioClip dialogueTutorialMusic;
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
            PlayIntroDialogueMusic();
        }

        public void PlayIntroDialogueMusic()
        {
            PlayMusic(introDialogueMusic);
        }

        public void PlayIntroMusic()
        {
            PlayIntroDialogueMusic();
        }

        public void PlayDialogueMusic()
        {
            PlayMusic(dialogueTutorialMusic != null ? dialogueTutorialMusic : introDialogueMusic);
        }

        public void PlayLevelMusic(int levelIndex)
        {
            if (levelMusic == null || levelIndex < 0 || levelIndex >= levelMusic.Length)
            {
                Debug.LogWarning("GameMusicController: No level music clip exists at index " + levelIndex);
                return;
            }

            PlayMusic(levelMusic[levelIndex]);
        }

        public void PlayBossMusic()
        {
            PlayMusic(bossMusic);
        }

        public void PlayLevelCompleteMusic()
        {
            PlayMusic(levelCompleteMusic);
        }

        public void PlayGameOverMusic()
        {
            PlayMusic(gameOverMusic);
        }

        public void PlayWinScreenMusic()
        {
            PlayMusic(winScreenMusic);
        }

        public void StopMusic()
        {
            if (musicSource == null) return;

            musicSource.Stop();
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource == null)
            {
                Debug.LogWarning("GameMusicController: No AudioSource assigned.");
                return;
            }

            if (clip == null)
            {
                Debug.LogWarning("GameMusicController: Tried to play a missing music clip.");
                return;
            }

            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
