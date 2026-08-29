using System;
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

        [Header("Skip")]
        [SerializeField] private GameObject skipBarRoot;
        [SerializeField] private Image skipBar;
        [SerializeField] private float skipHoldDuration = 2f;

        [Header("Skip Hint")]
        [SerializeField] private TMP_Text skipHintText;
        [SerializeField] private float hintBlinkSpeed = 5f;
        [SerializeField] private float hintMinAlpha = 0.1f;
        [SerializeField] private float hintMaxAlpha = 0.5f;

        [Header("SFX")]
        [SerializeField] private AudioSource typingSfxSource;
        [SerializeField] private AudioSource nextSfxSource;
        [SerializeField] private AudioClip nextSound;

        private Sprite sequenceLeftSprite;
        private Sprite sequenceRightSprite;

        private float holdTimer;
        private bool skipRequested;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (root != null) root.SetActive(false);
            if (skipBarRoot != null) skipBarRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            StopTypingSfx();
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (skipHintText == null) return;
            if (root == null || !root.activeSelf) return;

            float wave = (Mathf.Sin(Time.unscaledTime * hintBlinkSpeed) + 1f) * 0.5f;

            Color c = skipHintText.color;
            c.a = Mathf.Lerp(hintMinAlpha, hintMaxAlpha, wave);
            skipHintText.color = c;
        }

        public static void Show(DialogueSequence sequence)
        {
            Show(sequence, null);
        }

        public static void Show(DialogueSequence sequence, Action onFinished)
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

            UIQueue.Instance.Enqueue(new DialogueRequest(Instance, sequence, onFinished));
        }

        public IEnumerator PlaySequence(DialogueSequence sequence)
        {
            OpenPanel(sequence);

            holdTimer = 0f;
            skipRequested = false;

            for (int i = 0; i < sequence.lines.Length; i++)
            {
                yield return PlayLine(sequence.lines[i]);
                if (skipRequested) break;
            }

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
            StopTypingSfx();
            if (root != null) root.SetActive(false);
            if (skipBarRoot != null) skipBarRoot.SetActive(false);
            if (skipBar != null) skipBar.fillAmount = 0f;
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
            if (!string.IsNullOrEmpty(content)) StartTypingSfx();

            for (int i = 0; i < content.Length; i++)
            {
                UpdateSkipHold();
                if (skipRequested)
                {
                    StopTypingSfx();
                    yield break;
                }

                if (NextPressed())
                {
                    StopTypingSfx();
                    skipped = true;
                    break;
                }

                bodyText.text += content[i];
                yield return new WaitForSecondsRealtime(charDelay);
            }

            StopTypingSfx();
            bodyText.text = content;

            if (skipped) yield return new WaitForSecondsRealtime(inputLockDuration);

            if (nextIndicator != null) nextIndicator.SetActive(true);

            while (true)
            {
                UpdateSkipHold();
                if (skipRequested) yield break;

                if (NextPressed())
                {
                    PlayNextSfx();
                    break;
                }

                yield return null;
            }

            if (nextIndicator != null) nextIndicator.SetActive(false);
        }

        private void UpdateSkipHold()
        {
            if (HoldPressed())
            {
                holdTimer += Time.unscaledDeltaTime;

                if (holdTimer >= skipHoldDuration)
                    skipRequested = true;
            }
            else
            {
                holdTimer = 0f;
            }

            UpdateSkipBar();
        }

        private void UpdateSkipBar()
        {
            float progress = Mathf.Clamp01(holdTimer / skipHoldDuration);

            if (skipBar != null) skipBar.fillAmount = progress;
            if (skipBarRoot != null) skipBarRoot.SetActive(progress > 0.01f);
        }

        private bool HoldPressed()
        {
            bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            bool gamepad = Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;
            return keyboard || gamepad;
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

        private void PlayNextSfx()
        {
            if (nextSfxSource == null || nextSound == null) return;

            nextSfxSource.PlayOneShot(nextSound);
        }

        private void StartTypingSfx()
        {
            if (typingSfxSource == null) return;

            if (!typingSfxSource.isPlaying)
                typingSfxSource.Play();
        }

        private void StopTypingSfx()
        {
            if (typingSfxSource == null) return;

            typingSfxSource.Stop();
        }
    }
}
