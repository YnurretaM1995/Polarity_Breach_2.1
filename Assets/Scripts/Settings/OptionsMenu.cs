using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace PolarityBreach.Settings
{
    public class OptionsMenu : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Selectable firstSelected;

        [Header("Volume")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_Text masterValueText;
        [SerializeField] private TMP_Text musicValueText;
        [SerializeField] private TMP_Text sfxValueText;

        [Header("Difficulty")]
        [SerializeField] private bool allowDifficultyChange = true;
        [SerializeField] private GameObject difficultyControls;
        [SerializeField] private Button previousDifficultyButton;
        [SerializeField] private Button nextDifficultyButton;
        [SerializeField] private TMP_Text difficultyNameText;
        [SerializeField] private TMP_Text difficultyDescriptionText;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        private AudioSettingsApplier audioApplier;
        private bool isRefreshing;

        public bool IsOpen => panel != null && panel.activeSelf;

        private void Awake()
        {
            GameSettings.Load();

            if (panel == null) panel = gameObject;

            audioApplier = AudioSettingsApplier.Instance;
            if (audioApplier == null) audioApplier = FindFirstObjectByType<AudioSettingsApplier>();

            if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);

            if (previousDifficultyButton != null) previousDifficultyButton.onClick.AddListener(PreviousDifficulty);
            if (nextDifficultyButton != null) nextDifficultyButton.onClick.AddListener(NextDifficulty);
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
            if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);

            if (previousDifficultyButton != null) previousDifficultyButton.onClick.RemoveListener(PreviousDifficulty);
            if (nextDifficultyButton != null) nextDifficultyButton.onClick.RemoveListener(NextDifficulty);
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }

        public void Open()
        {
            GameSettings.Load();
            RefreshAll();

            panel.SetActive(true);
            SelectFirst();
        }

        public void Close()
        {
            GameSettings.Save();
            panel.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        private void RefreshAll()
        {
            isRefreshing = true;

            if (masterSlider != null) masterSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(GameSettings.SfxVolume);

            UpdateVolumeLabels();
            RefreshDifficulty();

            isRefreshing = false;
        }

        private void UpdateVolumeLabels()
        {
            if (masterValueText != null) masterValueText.text = ToPercent(GameSettings.MasterVolume);
            if (musicValueText != null) musicValueText.text = ToPercent(GameSettings.MusicVolume);
            if (sfxValueText != null) sfxValueText.text = ToPercent(GameSettings.SfxVolume);
        }

        private string ToPercent(float value)
        {
            return Mathf.RoundToInt(value * 100f) + "%";
        }

        private void OnMasterChanged(float value)
        {
            if (isRefreshing) return;

            GameSettings.SetMasterVolume(value);
            if (audioApplier != null) audioApplier.ApplyMaster(value);
            UpdateVolumeLabels();
            GameSettings.Save();
        }

        private void OnMusicChanged(float value)
        {
            if (isRefreshing) return;

            GameSettings.SetMusicVolume(value);
            if (audioApplier != null) audioApplier.ApplyMusic(value);
            UpdateVolumeLabels();
            GameSettings.Save();
        }

        private void OnSfxChanged(float value)
        {
            if (isRefreshing) return;

            GameSettings.SetSfxVolume(value);
            if (audioApplier != null) audioApplier.ApplySfx(value);
            UpdateVolumeLabels();
            GameSettings.Save();
        }

        private void PreviousDifficulty()
        {
            if (!allowDifficultyChange) return;

            int count = System.Enum.GetValues(typeof(DifficultyLevel)).Length;
            int index = ((int)GameSettings.Difficulty - 1 + count) % count;

            GameSettings.SetDifficulty((DifficultyLevel)index);
            RefreshDifficulty();
        }

        private void NextDifficulty()
        {
            if (!allowDifficultyChange) return;

            int count = System.Enum.GetValues(typeof(DifficultyLevel)).Length;
            int index = ((int)GameSettings.Difficulty + 1) % count;

            GameSettings.SetDifficulty((DifficultyLevel)index);
            RefreshDifficulty();
        }

        private void RefreshDifficulty()
        {
            if (difficultyNameText != null)
                difficultyNameText.text = GameSettings.GetDifficultyName(GameSettings.Difficulty);

            if (difficultyDescriptionText != null)
                difficultyDescriptionText.text = Mathf.RoundToInt(GameSettings.GetStartingHealth()) + " HP";

            if (previousDifficultyButton != null)
            {
                previousDifficultyButton.gameObject.SetActive(allowDifficultyChange);
                previousDifficultyButton.interactable = allowDifficultyChange;
            }

            if (nextDifficultyButton != null)
            {
                nextDifficultyButton.gameObject.SetActive(allowDifficultyChange);
                nextDifficultyButton.interactable = allowDifficultyChange;
            }

            if (difficultyControls != null)
                difficultyControls.SetActive(true);
        }

        private void SelectFirst()
        {
            if (firstSelected == null || EventSystem.current == null) return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }
    }
}
