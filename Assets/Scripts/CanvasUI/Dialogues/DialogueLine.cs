using UnityEngine;

namespace PolarityBreach.UI
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 4)]
        public string text;

        public string speakerName = "RIO";

        public PortraitState leftPortrait = PortraitState.Keep;
        public PortraitState rightPortrait = PortraitState.Keep;

        [Header("Control Prompt")]
        public bool isPrompt;
        public string keyboardHint;
        public string gamepadHint;
    }
}