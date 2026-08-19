using UnityEngine;

namespace PolarityBreach.UI
{
    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "Polarity Breach/Dialogue Sequence")]
    public class DialogueSequence : ScriptableObject
    {
        public Sprite fullScreenBackground;
        public DialogueLine[] lines;
    }
}