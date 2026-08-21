using TMPro;
using UnityEngine;

namespace PolarityBreach.Score
{
    public class ScoreRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private Color highlightColor = Color.yellow;

        public void Set(int rank, ScoreEntry entry, bool highlight)
        {
            if (rankText != null) rankText.text = rank.ToString();
            if (timeText != null) timeText.text = ScoreBoard.Format(entry.seconds);
            if (dateText != null) dateText.text = entry.date;

            if (!highlight) return;

            if (rankText != null) rankText.color = highlightColor;
            if (timeText != null) timeText.color = highlightColor;
            if (dateText != null) dateText.color = highlightColor;
        }
    }
}