using UnityEngine;

namespace PolarityBreach.UI
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 4)]
        public string text;
        public string speakerName = "RIO";

        [Header("Portraits")]
        public Sprite leftPortrait;
        public Sprite rightPortrait;
        public bool clearLeftPortrait;
        public bool clearRightPortrait;

        [Header("Control Prompt")]
        public bool isPrompt;
        public string keyboardHint;
        public string gamepadHint;
    }
}