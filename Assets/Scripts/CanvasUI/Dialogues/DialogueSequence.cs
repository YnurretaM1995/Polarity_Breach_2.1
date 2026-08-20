using UnityEngine;

namespace PolarityBreach.UI
{
    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "Polarity Breach/Dialogue Sequence")]
    public class DialogueSequence : ScriptableObject
    {
        [Header("Background")]
        public Sprite fullScreenBackground;

        [Header("Portraits")]
        public Sprite leftPortrait;
        public Sprite rightPortrait;

        [Header("Lines")]
        public DialogueLine[] lines;
    }
}