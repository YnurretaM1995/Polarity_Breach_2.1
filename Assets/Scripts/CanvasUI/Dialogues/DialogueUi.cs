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

        [Header("Portraits")]
        [SerializeField] private Image leftPortrait;
        [SerializeField] private Image rightPortrait;

        [Header("Next Indicator")]
        [SerializeField] private GameObject nextIndicator;

        [Header("Typewriter")]
        [SerializeField] private float charDelay = 0.03f;
        [SerializeField] private float inputLockDuration = 0.15f;

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
            OpenPanel(sequence.fullScreenBackground);

            for (int i = 0; i < sequence.lines.Length; i++)
                yield return PlayLine(sequence.lines[i]);

            ClosePanel();
        }

        private void OpenPanel(Sprite background)
        {
            if (root != null) root.SetActive(true);

            if (fullScreenBackground != null)
            {
                bool hasBackground = background != null;
                fullScreenBackground.gameObject.SetActive(hasBackground);
                if (hasBackground) fullScreenBackground.sprite = background;
            }

            SetPortrait(leftPortrait, null, true);
            SetPortrait(rightPortrait, null, true);
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

        private string BuildPromptText(DialogueLine line)
        {
            bool usingGamepad = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
            string hint = usingGamepad ? line.gamepadHint : line.keyboardHint;

            if (string.IsNullOrEmpty(hint))
                hint = string.IsNullOrEmpty(line.keyboardHint) ? line.gamepadHint : line.keyboardHint;

            return string.IsNullOrEmpty(line.text) ? hint : line.text + "\n" + hint;
        }

        private void ApplyPortraits(DialogueLine line)
        {
            SetPortrait(leftPortrait, line.leftPortrait, line.clearLeftPortrait);
            SetPortrait(rightPortrait, line.rightPortrait, line.clearRightPortrait);
        }

        private void SetPortrait(Image image, Sprite sprite, bool clear)
        {
            if (image == null) return;

            if (clear)
            {
                image.gameObject.SetActive(false);
                return;
            }

            if (sprite == null) return;

            image.sprite = sprite;
            image.gameObject.SetActive(true);
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