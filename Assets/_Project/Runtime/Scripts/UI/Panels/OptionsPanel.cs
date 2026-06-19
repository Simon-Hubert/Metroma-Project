using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

namespace Metroma.UI.Panels
{
    public class OptionsPanel : UIPanel
    {
        // --- UI Bindings ---

        [Header("Menu Buttons")]
        [SerializeField] private Button BackButton;
        [SerializeField] private Button CreditsButton;

        [Header("Credits Settings")]
        [SerializeField] private Camera MainMenuCamera;
        [SerializeField] private Camera CreditsCamera;
        [SerializeField] private Metroma.Credits CreditsScript;

        [Header("Audio Settings")]
        [SerializeField] private AudioMixer MainMixer;
        [SerializeField] private Slider MasterVolumeSlider;
        [SerializeField] private Slider MusicVolumeSlider;
        [SerializeField] private Slider SFXVolumeSlider;

        [Header("Graphics Settings")]
        [SerializeField] private TMP_Dropdown ResolutionDropdown;
        [SerializeField] private TMP_Dropdown QualityDropdown;
        [SerializeField] private Toggle FullscreenToggle;
        [SerializeField] private Toggle VSyncToggle;

        private Resolution[] AvailableResolutions;


        // --- Lifecycle ---

        public override void Initialize()
        {
            base.Initialize();

            if (BackButton != null)
                BackButton.onClick.AddListener(OnBackClicked);
                
            if (CreditsButton != null)
                CreditsButton.onClick.AddListener(OnCreditsClicked);

            // --- Configuration Initiale des Graphismes ---
            SetupResolutions();
            SetupQualityLevels();

            // --- Chargement des PlayerPrefs ---
            LoadSettings();

            if (MasterVolumeSlider != null) MasterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            if (MusicVolumeSlider != null) MusicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            if (SFXVolumeSlider != null) SFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

            if (ResolutionDropdown != null) ResolutionDropdown.onValueChanged.AddListener(SetResolution);
            if (QualityDropdown != null) QualityDropdown.onValueChanged.AddListener(SetQuality);
            if (FullscreenToggle != null) FullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            if (VSyncToggle != null) VSyncToggle.onValueChanged.AddListener(SetVSync);
        }

        private void OnDestroy()
        {
            if (BackButton != null) BackButton.onClick.RemoveListener(OnBackClicked);
            if (CreditsButton != null) CreditsButton.onClick.RemoveListener(OnCreditsClicked);
            
            if (MasterVolumeSlider != null) MasterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            if (MusicVolumeSlider != null) MusicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
            if (SFXVolumeSlider != null) SFXVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);

            if (ResolutionDropdown != null) ResolutionDropdown.onValueChanged.RemoveListener(SetResolution);
            if (QualityDropdown != null) QualityDropdown.onValueChanged.RemoveListener(SetQuality);
            if (FullscreenToggle != null) FullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
            if (VSyncToggle != null) VSyncToggle.onValueChanged.RemoveListener(SetVSync);
        }

        private void OnCreditsClicked()
        {
            if (MainMenuCamera != null) MainMenuCamera.gameObject.SetActive(false);
            if (CreditsCamera != null) CreditsCamera.gameObject.SetActive(true);
            
            if (CreditsScript != null) CreditsScript.Play();
            
            UIManager.Instance.CloseCurrentPanel();
        }


        // --- Setup Logic ---

        private void SetupResolutions()
        {
            if (ResolutionDropdown == null) return;

            AvailableResolutions = Screen.resolutions;
            ResolutionDropdown.ClearOptions();

            List<string> options = new List<string>();
            int currentResIndex = 0;

            for (int i = 0; i < AvailableResolutions.Length; i++)
            {
                string option = AvailableResolutions[i].width + " x " + AvailableResolutions[i].height;
                options.Add(option);

                if (AvailableResolutions[i].width == Screen.currentResolution.width && 
                    AvailableResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResIndex = i;
                }
            }

            ResolutionDropdown.AddOptions(options);
            ResolutionDropdown.value = currentResIndex;
            ResolutionDropdown.RefreshShownValue();
        }

        private void SetupQualityLevels()
        {
            if (QualityDropdown == null) return;

            QualityDropdown.ClearOptions();
            List<string> options = new List<string>(QualitySettings.names);
            QualityDropdown.AddOptions(options);
            QualityDropdown.value = QualitySettings.GetQualityLevel();
            QualityDropdown.RefreshShownValue();
        }


        // --- Load/Save Logic ---

        private void LoadSettings()
        {
            // Audio (Slider values 0.0001 to 1)
            if (MasterVolumeSlider != null) MasterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVol", 0.75f);
            if (MusicVolumeSlider != null) MusicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
            if (SFXVolumeSlider != null) SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.75f);

            SetMasterVolume(MasterVolumeSlider != null ? MasterVolumeSlider.value : 0.75f);
            SetMusicVolume(MusicVolumeSlider != null ? MusicVolumeSlider.value : 0.75f);
            SetSFXVolume(SFXVolumeSlider != null ? SFXVolumeSlider.value : 0.75f);

            // Graphics
            if (QualityDropdown != null) 
            {
                QualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
                SetQuality(QualityDropdown.value);
            }

            if (FullscreenToggle != null) 
            {
                FullscreenToggle.isOn = PlayerPrefs.GetInt("IsFullscreen", Screen.fullScreen ? 1 : 0) == 1;
                SetFullscreen(FullscreenToggle.isOn);
            }
            
            if (VSyncToggle != null)
            {
                VSyncToggle.isOn = PlayerPrefs.GetInt("VSync", QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
                SetVSync(VSyncToggle.isOn);
            }
            
            if (ResolutionDropdown != null)
            {
                int savedResIndex = PlayerPrefs.GetInt("ResIndex", ResolutionDropdown.value);
                if (savedResIndex >= 0 && savedResIndex < AvailableResolutions.Length)
                {
                    ResolutionDropdown.value = savedResIndex;
                    SetResolution(savedResIndex);
                }
            }
        }


        // --- Audio Handlers ---

        private void SetMasterVolume(float volume)
        {
            if (MainMixer != null)
                MainMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
                
            PlayerPrefs.SetFloat("MasterVol", volume);
        }

        private void SetMusicVolume(float volume)
        {
            if (MainMixer != null)
                MainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
                
            PlayerPrefs.SetFloat("MusicVol", volume);
        }

        private void SetSFXVolume(float volume)
        {
            if (MainMixer != null)
                MainMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
                
            PlayerPrefs.SetFloat("SFXVol", volume);
        }


        // --- Graphics Handlers ---

        private void SetResolution(int resIndex)
        {
            if (AvailableResolutions == null || resIndex < 0 || resIndex >= AvailableResolutions.Length) return;
            
            Resolution res = AvailableResolutions[resIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResIndex", resIndex);
        }

        private void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        }

        private void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        }

        private void SetVSync(bool isVSyncOn)
        {
            QualitySettings.vSyncCount = isVSyncOn ? 1 : 0;
            PlayerPrefs.SetInt("VSync", isVSyncOn ? 1 : 0);
        }


        // --- Interaction ---

        private void OnBackClicked()
        {
            PlayerPrefs.Save();
            UIManager.Instance.CloseCurrentPanel();
        }
    }
}
