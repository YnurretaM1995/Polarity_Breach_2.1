using UnityEngine;

namespace PolarityBreach.Settings
{
    public static class GameSettings
    {
        private const string MasterKey = "settings_master_volume";
        private const string MusicKey = "settings_music_volume";
        private const string SfxKey = "settings_sfx_volume";
        private const string DifficultyKey = "settings_difficulty";

        private const float DefaultVolume = 0.8f;

        public static float MasterVolume { get; private set; } = DefaultVolume;
        public static float MusicVolume { get; private set; } = DefaultVolume;
        public static float SfxVolume { get; private set; } = DefaultVolume;
        public static DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Normal;

        private static bool loaded;

        public static void Load()
        {
            if (loaded) return;

            MasterVolume = PlayerPrefs.GetFloat(MasterKey, DefaultVolume);
            MusicVolume = PlayerPrefs.GetFloat(MusicKey, DefaultVolume);
            SfxVolume = PlayerPrefs.GetFloat(SfxKey, DefaultVolume);
            Difficulty = (DifficultyLevel)PlayerPrefs.GetInt(DifficultyKey, (int)DifficultyLevel.Normal);

            loaded = true;
        }

        public static void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MasterKey, MasterVolume);
        }

        public static void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, MusicVolume);
        }

        public static void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, SfxVolume);
        }

        public static void SetDifficulty(DifficultyLevel value)
        {
            Difficulty = value;
            PlayerPrefs.SetInt(DifficultyKey, (int)value);
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static float GetStartingHealth()
        {
            switch (Difficulty)
            {
                case DifficultyLevel.Easy: return 200f;
                case DifficultyLevel.Normal: return 100f;
                case DifficultyLevel.Hard: return 50f;
                case DifficultyLevel.God: return 1f;
                default: return 100f;
            }
        }

        public static string GetDifficultyName(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Easy: return "EASY";
                case DifficultyLevel.Normal: return "NORMAL";
                case DifficultyLevel.Hard: return "HARD";
                case DifficultyLevel.God: return "GOD";
                default: return "NORMAL";
            }
        }
    }
}