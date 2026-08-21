using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PolarityBreach.Score
{
    public static class ScoreBoard
    {
        private const string FileName = "scores.json";
        private const int MaxEntries = 50;

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static ScoreList Load()
        {
            if (!File.Exists(FilePath))
                return new ScoreList();

            try
            {
                string json = File.ReadAllText(FilePath);
                ScoreList list = JsonUtility.FromJson<ScoreList>(json);
                return list ?? new ScoreList();
            }
            catch (Exception e)
            {
                Debug.LogWarning("ScoreBoard: could not read scores. " + e.Message);
                return new ScoreList();
            }
        }

        public static void Save(ScoreList list)
        {
            try
            {
                string json = JsonUtility.ToJson(list, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("ScoreBoard: could not write scores. " + e.Message);
            }
        }

        public static bool AddTime(float seconds)
        {
            ScoreList list = Load();

            float previousBest = float.MaxValue;
            if (list.entries.Count > 0)
            {
                list.entries.Sort((a, b) => a.seconds.CompareTo(b.seconds));
                previousBest = list.entries[0].seconds;
            }

            ScoreEntry entry = new ScoreEntry();
            entry.seconds = seconds;
            entry.date = DateTime.Now.ToString("yyyy-MM-dd");
            list.entries.Add(entry);

            list.entries.Sort((a, b) => a.seconds.CompareTo(b.seconds));

            if (list.entries.Count > MaxEntries)
                list.entries.RemoveRange(MaxEntries, list.entries.Count - MaxEntries);

            Save(list);

            return seconds < previousBest;
        }

        public static List<ScoreEntry> GetTop(int count)
        {
            ScoreList list = Load();
            list.entries.Sort((a, b) => a.seconds.CompareTo(b.seconds));

            if (list.entries.Count <= count)
                return list.entries;

            return list.entries.GetRange(0, count);
        }

        public static string Format(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            int hundredths = Mathf.FloorToInt((seconds * 100f) % 100f);
            return $"{minutes:00}:{secs:00}:{hundredths:00}";
        }

        public static void Clear()
        {
            Save(new ScoreList());
        }
    }
}