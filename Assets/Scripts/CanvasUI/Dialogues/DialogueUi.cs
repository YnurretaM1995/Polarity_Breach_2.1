using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PolarityBreach.UI
{
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject root;
        [SerializeField] private Image fullScreenBackground;

        [Header("Text")]
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private GameObject speakerBox;

        [Header("Portrait Slots")]
        [SerializeField] private Image leftPortrait;
        [SerializeField] private Image rightPortrait;

        [Header("Next Indicator")]
        [SerializeField] private GameObject nextIndicator;

        [Header("Typewriter")]
        [SerializeField] private float charDelay = 0.03f;
        [SerializeField] private float inputLockDuration = 0.15f;

        private Sprite sequenceLeftSprite;
        private Sprite sequenceRightSprite;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (root != null) root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Show(DialogueSequence sequence)
        {
            if (sequence == null || sequence.lines == null || sequence.lines.Length == 0) return;

            if (Instance == null)
            {
                Debug.LogWarning("DialogueUI: no instance in the scene.");
                return;
            }

            if (UIQueue.Instance == null)
            {
                Debug.LogWarning("DialogueUI: no UIQueue in the scene.");
                return;
            }

            UIQueue.Instance.Enqueue(new DialogueRequest(Instance, sequence));
        }

        public IEnumerator PlaySequence(DialogueSequence sequence)
        {
            OpenPanel(sequence);

            for (int i = 0; i < sequence.lines.Length; i++)
                yield return PlayLine(sequence.lines[i]);

            ClosePanel();
        }

        private void OpenPanel(DialogueSequence sequence)
        {
            if (root != null) root.SetActive(true);

            sequenceLeftSprite = sequence.leftPortrait;
            sequenceRightSprite = sequence.rightPortrait;

            if (fullScreenBackground != null)
            {
                bool hasBackground = sequence.fullScreenBackground != null;
                fullScreenBackground.gameObject.SetActive(hasBackground);
                if (hasBackground) fullScreenBackground.sprite = sequence.fullScreenBackground;
            }
        }

        private void ClosePanel()
        {
            if (root != null) root.SetActive(false);
        }

        private IEnumerator PlayLine(DialogueLine line)
        {
            ApplyPortraits(line);

            string content = line.isPrompt ? BuildPromptText(line) : line.text;

            if (speakerBox != null) speakerBox.SetActive(!line.isPrompt);
            if (speakerText != null) speakerText.text = line.speakerName;
            if (nextIndicator != null) nextIndicator.SetActive(false);

            yield return new WaitForSecondsRealtime(inputLockDuration);

            bool skipped = false;
            bodyText.text = "";

            for (int i = 0; i < content.Length; i++)
            {
                if (NextPressed())
                {
                    skipped = true;
                    break;
                }

                bodyText.text += content[i];
                yield return new WaitForSecondsRealtime(charDelay);
            }

            bodyText.text = content;

            if (skipped) yield return new WaitForSecondsRealtime(inputLockDuration);

            if (nextIndicator != null) nextIndicator.SetActive(true);

            while (!NextPressed())
                yield return null;

            if (nextIndicator != null) nextIndicator.SetActive(false);
        }

        private void ApplyPortraits(DialogueLine line)
        {
            ApplyPortrait(leftPortrait, sequenceLeftSprite, line.leftPortrait);
            ApplyPortrait(rightPortrait, sequenceRightSprite, line.rightPortrait);
        }

        private void ApplyPortrait(Image image, Sprite sprite, PortraitState state)
        {
            if (image == null) return;

            if (state == PortraitState.None || sprite == null)
            {
                image.gameObject.SetActive(false);
                return;
            }

            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }

        private string BuildPromptText(DialogueLine line)
        {
            bool usingGamepad = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
            string hint = usingGamepad ? line.gamepadHint : line.keyboardHint;

            if (string.IsNullOrEmpty(hint))
                hint = string.IsNullOrEmpty(line.keyboardHint) ? line.gamepadHint : line.keyboardHint;

            return string.IsNullOrEmpty(line.text) ? hint : line.text + "\n" + hint;
        }

        private bool NextPressed()
        {
            bool keyboard = Keyboard.current != null &&
                            (Keyboard.current.spaceKey.wasPressedThisFrame ||
                             Keyboard.current.enterKey.wasPressedThisFrame);

            bool gamepad = Gamepad.current != null &&
                           Gamepad.current.buttonSouth.wasPressedThisFrame;

            bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

            return keyboard || gamepad || mouse;
        }
    }
}