using Cinemachine;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CoreCraft.Core
{
    public class GameOptionsUI : MonoBehaviour
    {
        public static GameOptionsUI Instance { get; private set; }

        #region UI Fields

        [Header("UI Buttons")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _resetToDefaultButton;
        [SerializeField] private Button _backButton;

        [Header("Options Categories Buttons")]
        [SerializeField] private Button _graphicsButton;
        [SerializeField] private Button _soundsButton;
        [SerializeField] private Button _controlsButton;
        [SerializeField] private Button _accessibilityButton;

        [Header("Options Categories Panels")]
        [SerializeField] private GameObject _graphicsPanel;
        [SerializeField] private GameObject _soundsPanel;
        [SerializeField] private GameObject _controlsPanel;
        [SerializeField] private GameObject _accessibilityPanel;

        [Header("UI Dropdowns")]
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private TMP_Dropdown _displayDropdown;
        [SerializeField] private TMP_Dropdown _windowModeDropdown;
        [SerializeField] private TMP_Dropdown _textureQualityDropdown;
        [SerializeField] private TMP_Dropdown _shadowQualityDropdown;

        [Header("UI Slider")]
        [SerializeField] private Slider _mainVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;

        [Header("UI Toggles")]
        [SerializeField] private Toggle _vSyncToggle;
        [SerializeField] private Toggle _softShadowsToggle;
        [SerializeField] private Toggle _hdrToggle;
        [SerializeField] private Toggle _ssaoToggle;

        [Header("UI Texts")]
        [SerializeField] private TextMeshProUGUI _mainVolumeValueText;
        [SerializeField] private TextMeshProUGUI _musicVolumeValueText;
        [SerializeField] private TextMeshProUGUI _sfxVolumeValueText;
        [SerializeField] private TextMeshProUGUI _resolutionOptionText;
        [SerializeField] private TextMeshProUGUI _displayOptionText;
        [SerializeField] private TextMeshProUGUI _windowModeOptionText;
        [SerializeField] private TextMeshProUGUI _mainVolumeOptionText;
        [SerializeField] private TextMeshProUGUI _musicVolumeOptionText;
        [SerializeField] private TextMeshProUGUI _sfxVolumeOptionText;
        [SerializeField] private TextMeshProUGUI _vSyncOptionText;
        [SerializeField] private TextMeshProUGUI _textureQualityOptionText;
        [SerializeField] private TextMeshProUGUI _shadowQualityOptionText;
        [SerializeField] private TextMeshProUGUI _softShadowsOptionText;
        [SerializeField] private TextMeshProUGUI _hdrOptionText;
        [SerializeField] private TextMeshProUGUI _ssaoOptionText;
        [SerializeField] private TextMeshProUGUI _cameraDistanceOptionText;
        [SerializeField] private TextMeshProUGUI _vSyncToggleState;
        [SerializeField] private TextMeshProUGUI _softShadowsToggleState;
        [SerializeField] private TextMeshProUGUI _hdrToggleState;
        [SerializeField] private TextMeshProUGUI _ssaoToggleState;

        [Header("Graphic Options Pop-Up")]
        [SerializeField] private GameObject _graphicOptionsChangedPopUpPanel;
        [SerializeField] private Button _acceptSettingsButton;
        [SerializeField] private Button _revertSettingsButton;
        [SerializeField] private int _maxPopUpTimer = 15;
        [SerializeField] private TextMeshProUGUI _graphicOptionsChangedPopUpText;

        [Header("Unsaved Changes Pop-Up")]
        [SerializeField] private GameObject _unsavedChangesPopUpPanel;
        [SerializeField] private Button _acceptUnsavedChangesButton;
        [SerializeField] private Button _cancelUnsavedChangesButton;

        [Header("Keybindings")]
        [SerializeField] private GameObject _rebindPanel;
        [SerializeField] private TextMeshProUGUI _rebindPanelText;
        [SerializeField] private TextMeshProUGUI _turnTableRightKeybindingText1;
        [SerializeField] private TextMeshProUGUI _turnTableRightKeybindingText2;
        [SerializeField] private Button _turnTableRightKeybindingButton1;
        [SerializeField] private Button _turnTableRightKeybindingButton2;
        [SerializeField] private TextMeshProUGUI _turnTableLeftKeybindingText1;
        [SerializeField] private TextMeshProUGUI _turnTableLeftKeybindingText2;
        [SerializeField] private Button _turnTableLeftKeybindingButton1;
        [SerializeField] private Button _turnTableLeftKeybindingButton2;
        [SerializeField] private TextMeshProUGUI _placeCardKeybindingText1;
        [SerializeField] private TextMeshProUGUI _placeCardKeybindingText2;
        [SerializeField] private Button _placeCardKeybindingButton1;
        [SerializeField] private Button _placeCardKeybindingButton2;

        #endregion

        [Header("UI Colors")]
        [SerializeField] private Color _unsavedChangesColor;
        [SerializeField] private Color _defaultSettingsColor;
        
        [Header("Camera")]
        [SerializeField] private CinemachineVirtualCamera _uiCamera;
        public Transform OptionsCameraFocus;

        public event EventHandler OnResetToDefault;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one {this} instance in the scene!");
            }

            Instance = this;

            SetupButtons();
            SetupDropdowns();
            SetupToggles();
            SetupSliders();
            SetupKeybindings();

            GameSettingsManager.Instance.OnMoveWindowOperationComplete += GameSettingsManager_OnMoveWindowOperationComplete;

            // Fill the resolution dropdown with the supported resolutions.
            FillResolutionDropdown();

            // Fill the display dropdown with the connected displays.
            FillDisplayDropdown();

            EventManager.Instance.OnGameOptionsUIInitialized?.Invoke();
        }

        /// <summary>
        /// Functionality after the MoveWindowOperation has been completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameSettingsManager_OnMoveWindowOperationComplete(object sender, EventArgs e)
        {
            _resolutionDropdown.ClearOptions();
            FillResolutionDropdown();
            ResolutionDropdown(0);
        }

        /// <summary>
        /// Add functions to all buttons.
        /// </summary>
        private void SetupButtons()
        {
            // Save sound values.
            //_saveButton.onClick.AddListener(() =>
            //{
            //    GameSettingsManager.Instance.SaveSettings();
            //});

            _resetToDefaultButton.onClick.AddListener(() =>
            {
                OnResetToDefault?.Invoke(this, EventArgs.Empty);
                UpdateVisual();
            });

            // Close options menu and show pause menu.
            _backButton.onClick.AddListener(() =>
            {
                //if (CheckForUnsavedChanges())
                //{
                //    _unsavedChangesPopUpPanel.SetActive(true);
                //}
                //else
                //{
                //    Hide();
                //}

                Hide();

                if (GamePauseUI.Instance != null)
                {
                    GamePauseUI.Instance.Show();
                }

                if (MainMenuUI.Instance != null)
                {
                    MainMenuUI.Instance.Show();
                }
            });

            _graphicsButton?.onClick.AddListener(() =>
            {
                _graphicsPanel.SetActive(true);

                _soundsPanel.SetActive(false);
                _controlsPanel.SetActive(false);
                _accessibilityPanel.SetActive(false);
            });

            _soundsButton?.onClick.AddListener(() =>
            {
                _soundsPanel.SetActive(true);

                _graphicsPanel.SetActive(false);
                _controlsPanel.SetActive(false);
                _accessibilityPanel.SetActive(false);
            });

            _controlsButton?.onClick.AddListener(() =>
            {
                _controlsPanel.SetActive(true);

                _graphicsPanel.SetActive(false);
                _soundsPanel.SetActive(false);
                _accessibilityPanel.SetActive(false);
            });

            _accessibilityButton?.onClick.AddListener(() =>
            {
                _accessibilityPanel.SetActive(true);

                _graphicsPanel.SetActive(false);
                _soundsPanel.SetActive(false);
                _controlsPanel.SetActive(false);
            });

            _acceptUnsavedChangesButton.onClick.AddListener(AcceptUnsavedChanges);
            _cancelUnsavedChangesButton.onClick.AddListener(CancelUnsavedChanges);
        }

        /// <summary>
        /// Add functions to all dropdowns.
        /// </summary>
        private void SetupDropdowns()
        {
            _resolutionDropdown.onValueChanged.AddListener(delegate { GraphicOptionsChangedPopUpHandler(_resolutionDropdown.value, _resolutionDropdown); });
            _displayDropdown.onValueChanged.AddListener(delegate { GraphicOptionsChangedPopUpHandler(_displayDropdown.value, _displayDropdown); });
            _windowModeDropdown.onValueChanged.AddListener(delegate { GraphicOptionsChangedPopUpHandler(_windowModeDropdown.value, _windowModeDropdown); });
            _textureQualityDropdown.onValueChanged.AddListener(TextureQualityDropdown);
            _shadowQualityDropdown.onValueChanged.AddListener(ShadowQualityDropdown);
        }

        /// <summary>
        /// Add functions to all toggles.
        /// </summary>
        private void SetupToggles()
        {
            _vSyncToggle.onValueChanged.AddListener(VSyncToggle);
            _softShadowsToggle.onValueChanged.AddListener(SoftShadowsToggle);
            //_hdrToggle.onValueChanged.AddListener(HDRToggle);
            //_ssaoToggle.onValueChanged.AddListener(SSAOToggle);
        }

        /// <summary>
        /// Add functions to all sliders.
        /// </summary>
        private void SetupSliders()
        {
            _mainVolumeSlider.onValueChanged.AddListener(OnMainVolumeValueChanged);
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeValueChanged);
            _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeValueChanged);
        }

        /// <summary>
        /// Add functions to all keybindings.
        /// </summary>
        private void SetupKeybindings()
        {
            _turnTableRightKeybindingButton1.onClick.AddListener(() =>
            {
                RebindBinding(GameInputManager.Actions.TurnTableRight, 0);
            });

            _turnTableRightKeybindingButton2.onClick.AddListener(() =>
            {
                RebindBinding(GameInputManager.Actions.TurnTableRight, 1);
            });

            _turnTableLeftKeybindingButton1.onClick.AddListener(() =>
            {
                RebindBinding(GameInputManager.Actions.TurnTableLeft, 0);
            });

            _turnTableLeftKeybindingButton2.onClick.AddListener(() =>
            {
                RebindBinding(GameInputManager.Actions.TurnTableLeft, 1);
            });

            _placeCardKeybindingButton1.onClick.AddListener(() =>
            {
                RebindBinding(GameInputManager.Actions.PlaceCard, 0);
            });

            _placeCardKeybindingButton2.onClick.AddListener(() =>
            {
                RebindBinding(GameInputManager.Actions.PlaceCard, 1);
            });
        }
        
        private void Start()
        {
            // Subscribe to the unpause event.

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameUnpaused += GameStateManager_OnOnGameUnpaused;
            }

            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnDuplicateKeybindingFound += GameInputManager_OnDuplicateKeybindingFound;
                OnResetToDefault += GameInputManager.Instance.GameOptionsUI_OnResetToDefault;
                GameInputManager.Instance.OnInputDeviceChanged += SwapInputIcons;
                GameInputManager.Instance.OnInputDeviceChanged += SetGamepadFocusOptionsMenu;
            }

            SetupPanels();
            UpdateVisual();

            GameSettingsManager.Instance.VirtualCamera = _uiCamera;

            Hide();
            HideRebindPanel();
        }

        /// <summary>
        /// Open the right panel and hide the rest.
        /// </summary>
        private void SetupPanels()
        {
            _graphicsPanel.SetActive(true);
            _soundsPanel.SetActive(false);
            _controlsPanel.SetActive(false);
            _accessibilityPanel.SetActive(false);
        }

        /// <summary>
        /// Fill the resolution dropdown with all available resolutions for the current display.
        /// </summary>
        private void FillResolutionDropdown()
        {
            foreach (Resolution resolution in GameSettingsFile.Instance.SupportedResolutions)
            {
                _resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(ResolutionToString(resolution)));
            }

            _resolutionDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Handles the functionality of the resolution dropdown.
        /// </summary>
        /// <param name="index">Selected option of the dropdown. In this case - the selected resolution.</param>
        private void ResolutionDropdown(int index)
        {
            GameSettingsManager.Instance.SetResolution(index);
            _resolutionDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Formats the resolution options string.
        /// </summary>
        /// <returns>Correctly formatted resolution options string.</returns>
        private string ResolutionToString(Resolution resolution)
        {
            return resolution.width + " x " + resolution.height + " @ " + resolution.refreshRate + " Hz";
        }

        /// <summary>
        /// Fill the display dropdown with all connected displays.
        /// </summary>
        private void FillDisplayDropdown()
        {
            foreach (DisplayInfo displayInfo in GameSettingsFile.Instance.SupportedDisplays)
            {
                _displayDropdown.options.Add(new TMP_Dropdown.OptionData(displayInfo.name));
            }

            _displayDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Handles the functionality of the display dropdown.
        /// </summary>
        /// <param name="index">Selected option of the dropdown. In this case - the selected display.</param>
        private void DisplayDropdown(int index)
        {
            GameSettingsManager.Instance.SetDisplay(index);
            _displayDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Handles the functionality of the window mode dropdown.
        /// </summary>
        /// <param name="index">Selected option of the dropdown. In this case - the selected window mode.</param>
        private void WindowModeDropdown(int index)
        {
            GameSettingsManager.Instance.SetWindowMode(index);
            _windowModeDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Handles the functionality of the V-Sync toggle.
        /// </summary>
        /// <param name="isChecked">Current state of the toggle. In this case - the state of V-Sync.</param>
        private void VSyncToggle(bool isChecked)
        {
            GameSettingsManager.Instance.SetVSync(isChecked);
            _vSyncToggle.SetIsOnWithoutNotify(isChecked);
            _vSyncToggleState.text = isChecked ? "Enabled" : "Disabled";
        }

        /// <summary>
        /// Handles the functionality of the texture quality dropdown.
        /// </summary>
        /// <param name="index">Selected option of the dropdown. In this case - the selected texture quality.</param>
        private void TextureQualityDropdown(int index)
        {
            GameSettingsManager.Instance.SetTextureQuality(index);
            _textureQualityDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Handles the functionality of the shadow quality dropdown.
        /// </summary>
        /// <param name="index">Selected option of the dropdown. In this case - the selected shadow quality.</param>
        private void ShadowQualityDropdown(int index)
        {
            GameSettingsManager.Instance.SetShadowQuality(index);
            _shadowQualityDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Handles the functionality of the Soft Shadows toggle.
        /// </summary>
        /// <param name="isChecked">Current state of the toggle. In this case - the state of Soft Shadows.</param>
        private void SoftShadowsToggle(bool isChecked)
        {
            GameSettingsManager.Instance.SetSoftShadows(isChecked);
            _softShadowsToggle.SetIsOnWithoutNotify(isChecked);
            _softShadowsToggleState.text = isChecked ? "Enabled" : "Disabled";
        }

        ///// <summary>
        ///// Handles the functionality of the HDR toggle.
        ///// </summary>
        ///// <param name="isChecked">Current state of the toggle. In this case - the state of HDR.</param>
        //private void HDRToggle(bool isChecked)
        //{
        //    GameSettingsManager.Instance.SetHDR(isChecked);
        //    _hdrToggle.SetIsOnWithoutNotify(isChecked);
        //    _hdrToggleState.text = isChecked ? "Enabled" : "Disabled";
        //}

        ///// <summary>
        ///// Handles the functionality of the SSAO toggle.
        ///// </summary>
        ///// <param name="isChecked">Current state of the toggle. In this case - the state of SSAO.</param>
        //private void SSAOToggle(bool isChecked)
        //{
        //    GameSettingsManager.Instance.SetSSAO(isChecked);
        //    _ssaoToggle.SetIsOnWithoutNotify(isChecked);
        //    _ssaoToggleState.text = isChecked ? "Enabled" : "Disabled";
        //}

        /// <summary>
        /// Handles the functionality of the Main Volume slider.
        /// </summary>
        /// <param name="value">Value of the slider. In this case - the volume of the Main Volume.</param>
        private void OnMainVolumeValueChanged(float value)
        {
            GameSettingsManager.Instance.SetMainVolume(value);
            _mainVolumeValueText.text = Mathf.Round(GameSettingsFile.Instance.MainVolume * 10).ToString();
        }

        /// <summary>
        /// Handles the functionality of the Music Volume slider.
        /// </summary>
        /// <param name="value">Value of the slider. In this case - the volume of the Music Volume.</param>
        private void OnMusicVolumeValueChanged(float value)
        {
            GameSettingsManager.Instance.SetMusicVolume(value);
            _musicVolumeValueText.text = Mathf.Round(GameSettingsFile.Instance.MusicVolume * 10).ToString();
        }

        /// <summary>
        /// Handles the functionality of the SFX Volume slider.
        /// </summary>
        /// <param name="value">Value of the slider. In this case - the volume of the SFX Volume.</param>
        private void OnSfxVolumeValueChanged(float value)
        {
            GameSettingsManager.Instance.SetSFXVolume(value);
            _sfxVolumeValueText.text = Mathf.Round(GameSettingsFile.Instance.SfxVolume * 10).ToString();
        }

        /// <summary>
        /// Handles the Pop-Up if the resolution, display or the window mode is changed.
        /// </summary>
        /// <param name="index">Selected resolution, display or window mode of the dropdown.</param>
        /// <param name="dropdown">Dropdown of the particular option.</param>
        private void GraphicOptionsChangedPopUpHandler(int index, TMP_Dropdown dropdown)
        {
            if (!_graphicOptionsChangedPopUpPanel.activeSelf)
            {
                _graphicOptionsChangedPopUpPanel.SetActive(true);
                _acceptSettingsButton.onClick.RemoveAllListeners();
                _revertSettingsButton.onClick.RemoveAllListeners();

                if (dropdown == _resolutionDropdown)
                {
                    ResolutionDropdown(index);
                    _acceptSettingsButton.onClick.AddListener(delegate { ApplyGraphicOptionsChangedPopUp(dropdown, index); });
                    _revertSettingsButton.onClick.AddListener(delegate { RevertGraphicsOptionsChangedPopUp(dropdown, ES3.Load<int>(GameSettingsFile.USERSETTINGS_RESOLUTION, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings)); });
                }
                else if (dropdown == _displayDropdown)
                {
                    DisplayDropdown(index);
                    _acceptSettingsButton.onClick.AddListener(delegate { ApplyGraphicOptionsChangedPopUp(dropdown, index); });
                    _revertSettingsButton.onClick.AddListener(delegate { RevertGraphicsOptionsChangedPopUp(dropdown, ES3.Load<int>(GameSettingsFile.USERSETTINGS_DISPLAY, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings)); });
                }
                else
                {
                    WindowModeDropdown(index);
                    _acceptSettingsButton.onClick.AddListener(delegate { ApplyGraphicOptionsChangedPopUp(dropdown, index); });
                    _revertSettingsButton.onClick.AddListener(delegate { RevertGraphicsOptionsChangedPopUp(dropdown, ES3.Load<int>(GameSettingsFile.USERSETTINGS_WINDOW_MODE, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings)); });
                }

                StartCoroutine("GraphicOptionsChangedPopUpTimer", dropdown);
            }
        }

        /// <summary>
        /// Close the Graphic Options Changed Pop-Up.
        /// </summary>
        private void CloseGraphicOptionsChangedPopUp()
        {
            _graphicOptionsChangedPopUpPanel.SetActive(false);
            StopCoroutine("GraphicOptionsChangedPopUpTimer");
        }

        /// <summary>
        /// Apply the graphic option.
        /// </summary>
        /// <param name="dropdown">Dropdown of the particular option.</param>
        /// <param name="newIndex">New resolution, display or window mode.</param>
        private void ApplyGraphicOptionsChangedPopUp(TMP_Dropdown dropdown, int newIndex)
        {
            CloseGraphicOptionsChangedPopUp();
        }

        /// <summary>
        /// Reverts the graphic option to the previous value.
        /// </summary>
        /// <param name="dropdown">Dropdown of the particular option.</param>
        /// <param name="index">Previous resolution, display or window mode.</param>
        private void RevertGraphicsOptionsChangedPopUp(TMP_Dropdown dropdown, int index)
        {
            if (dropdown == _resolutionDropdown)
            {
                ResolutionDropdown(index);
            }
            else if (dropdown == _displayDropdown)
            {
                DisplayDropdown(index);
            }
            else
            {
                WindowModeDropdown(index);
            }

            CloseGraphicOptionsChangedPopUp();
        }

        /// <summary>
        /// Logic of the Timer of the Graphic Options Changed Pop-Up.
        /// </summary>
        /// <param name="dropdown">Dropdown of the particular option.</param>
        private IEnumerator GraphicOptionsChangedPopUpTimer(TMP_Dropdown dropdown)
        {
            int currentTimer = _maxPopUpTimer;

            while (currentTimer >= 0)
            {
                _graphicOptionsChangedPopUpText.text = $"Would you like to apply this option? It will be reverted in {currentTimer} seconds.";
                yield return new WaitForSecondsRealtime(1);
                currentTimer--;

                if (currentTimer < 0)
                {
                    if (dropdown == _resolutionDropdown)
                    {
                        RevertGraphicsOptionsChangedPopUp(dropdown, ES3.Load<int>(GameSettingsFile.USERSETTINGS_RESOLUTION, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
                    }
                    else if (dropdown == _displayDropdown)
                    {
                        RevertGraphicsOptionsChangedPopUp(dropdown, ES3.Load<int>(GameSettingsFile.USERSETTINGS_DISPLAY, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
                    }
                    else
                    {
                        RevertGraphicsOptionsChangedPopUp(dropdown, ES3.Load<int>(GameSettingsFile.USERSETTINGS_WINDOW_MODE, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
                    }
                }
            }
        }

        /// <summary>
        /// Checks for unsaved changes in the user settings.
        /// </summary>
        /// <returns>True if unsaved changes have been found, otherwise false.</returns>
        private bool CheckForUnsavedChanges()
        {
            if (_resolutionDropdown.value != ES3.Load<int>(GameSettingsFile.USERSETTINGS_RESOLUTION, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;

            if (_displayDropdown.value != ES3.Load<int>(GameSettingsFile.USERSETTINGS_DISPLAY, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;

            if (_windowModeDropdown.value != ES3.Load<int>(GameSettingsFile.USERSETTINGS_WINDOW_MODE, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;

            if (_vSyncToggle.isOn != ES3.Load<bool>(GameSettingsFile.USERSETTINGS_VSYNC, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;

            if (_textureQualityDropdown.value != ES3.Load<int>(GameSettingsFile.USERSETTINGS_TEXTURE_QUALITY, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;

            if (_shadowQualityDropdown.value != ES3.Load<int>(GameSettingsFile.USERSETTINGS_SHADOW_QUALITY, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;
            
            if (_softShadowsToggle.isOn != ES3.Load<bool>(GameSettingsFile.USERSETTINGS_SOFT_SHADOWS, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;

            //if (_hdrToggle.isOn != ES3.Load<bool>(GameSettingsFile.USERSETTINGS_HDR, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
            //    return true;

            //if (_ssaoToggle.isOn != ES3.Load<bool>(GameSettingsFile.USERSETTINGS_SSAO, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
            //    return true;

            if (_mainVolumeSlider.value != ES3.Load<float>(GameSettingsFile.USERSETTINGS_MAIN_VOLUME, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;

            if (_musicVolumeSlider.value != ES3.Load<float>(GameSettingsFile.USERSETTINGS_MUSIC_VOLUME, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;

            if (_sfxVolumeSlider.value != ES3.Load<float>(GameSettingsFile.USERSETTINGS_SFX_VOLUME, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
                return true;

            return false;
        }

        /// <summary>
        /// Accept the unsaved changes, close the panel and revert to the previously saved values.
        /// </summary>
        private void AcceptUnsavedChanges()
        {
            _resolutionDropdown.SetValueWithoutNotify(ES3.Load<int>(GameSettingsFile.USERSETTINGS_RESOLUTION, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            //_resolutionOptionText.color = _defaultSettingsColor;

            _displayDropdown.SetValueWithoutNotify(ES3.Load<int>(GameSettingsFile.USERSETTINGS_DISPLAY, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            //_displayOptionText.color = _defaultSettingsColor;

            _windowModeDropdown.SetValueWithoutNotify(ES3.Load<int>(GameSettingsFile.USERSETTINGS_WINDOW_MODE, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            //_windowModeOptionText.color = _defaultSettingsColor;
            
            _vSyncToggle.SetIsOnWithoutNotify(ES3.Load<bool>(GameSettingsFile.USERSETTINGS_VSYNC, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            //_vSyncOptionText.color = _defaultSettingsColor;
            _vSyncToggleState.text = ES3.Load<bool>(GameSettingsFile.USERSETTINGS_VSYNC, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings) ? "Enabled" : "Disabled";

            _textureQualityDropdown.SetValueWithoutNotify(ES3.Load<int>(GameSettingsFile.USERSETTINGS_TEXTURE_QUALITY, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            //_textureQualityOptionText.color = _defaultSettingsColor;

            _shadowQualityDropdown.SetValueWithoutNotify(ES3.Load<int>(GameSettingsFile.USERSETTINGS_SHADOW_QUALITY, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            //_shadowQualityOptionText.color = _defaultSettingsColor;

            _softShadowsToggle.SetIsOnWithoutNotify(ES3.Load<bool>(GameSettingsFile.USERSETTINGS_SOFT_SHADOWS, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            //_softShadowsOptionText.color = _defaultSettingsColor;
            _softShadowsToggleState.text = ES3.Load<bool>(GameSettingsFile.USERSETTINGS_SOFT_SHADOWS, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings) ? "Enabled" : "Disabled";

            //_hdrToggle.SetIsOnWithoutNotify(ES3.Load<bool>(GameSettingsFile.USERSETTINGS_HDR, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            //_hdrOptionText.color = _defaultSettingsColor;
            //_hdrToggleState.text = ES3.Load<bool>(GameSettingsFile.USERSETTINGS_HDR, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings) ? "Enabled" : "Disabled";

            //_ssaoToggle.SetIsOnWithoutNotify(ES3.Load<bool>(GameSettingsFile.USERSETTINGS_SSAO, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            //_ssaoOptionText.color = _defaultSettingsColor;
            //_ssaoToggleState.text = ES3.Load<bool>(GameSettingsFile.USERSETTINGS_SSAO, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings) ? "Enabled" : "Disabled";

            //_mainVolumeOptionText.color = _defaultSettingsColor;
            _mainVolumeValueText.text = Mathf.Round(ES3.Load<float>(GameSettingsFile.USERSETTINGS_MAIN_VOLUME) * 10).ToString();

            //_musicVolumeOptionText.color = _defaultSettingsColor;
            _musicVolumeValueText.text = Mathf.Round(ES3.Load<float>(GameSettingsFile.USERSETTINGS_MUSIC_VOLUME) * 10).ToString();

            //_sfxVolumeOptionText.color = _defaultSettingsColor;
            _sfxVolumeValueText.text = Mathf.Round(ES3.Load<float>(GameSettingsFile.USERSETTINGS_SFX_VOLUME) * 10).ToString();

            _unsavedChangesPopUpPanel.SetActive(false);
            Hide();
        }

        /// <summary>
        /// Close the Pop-Up.
        /// </summary>
        private void CancelUnsavedChanges()
        {
            _unsavedChangesPopUpPanel.SetActive(false);
        }

        private void SetGamepadFocusOptionsMenu(object sender, GameInputManager.ControlScheme controlScheme)
        {
            if (controlScheme == GameInputManager.ControlScheme.Gamepad)
            {
                if (!gameObject.activeSelf) return;

                _graphicsButton.Select();
            }
        }

        private void SwapInputIcons(object sender, GameInputManager.ControlScheme controlScheme)
        {
            switch (controlScheme)
            {
                default:
                case GameInputManager.ControlScheme.Keyboard:
                    //_turnTableRightKeybindingText1.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableRight, 0);
                    //_turnTableRightKeybindingText2.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableRight, 1);
                    //_turnTableLeftKeybindingText1.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableLeft, 0);
                    //_turnTableLeftKeybindingText2.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableLeft, 1);
                    //_placeCardKeybindingText1.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.PlaceCard, 0);
                    //_placeCardKeybindingText2.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.PlaceCard, 1);

                    _turnTableRightKeybindingButton1.onClick.RemoveAllListeners();
                    _turnTableRightKeybindingButton2.onClick.RemoveAllListeners();
                    _turnTableLeftKeybindingButton1.onClick.RemoveAllListeners();
                    _turnTableLeftKeybindingButton2.onClick.RemoveAllListeners();
                    _placeCardKeybindingButton1.onClick.RemoveAllListeners();
                    _placeCardKeybindingButton2.onClick.RemoveAllListeners();

                    _turnTableRightKeybindingButton1.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.TurnTableRight, 0);
                    });

                    _turnTableRightKeybindingButton2.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.TurnTableRight, 1);
                    });

                    _turnTableLeftKeybindingButton1.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.TurnTableLeft, 0);
                    });

                    _turnTableLeftKeybindingButton2.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.TurnTableLeft, 1);
                    });

                    _placeCardKeybindingButton1.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.PlaceCard, 0);
                    });

                    _placeCardKeybindingButton2.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.PlaceCard, 1);
                    });
                    break;
                case GameInputManager.ControlScheme.Gamepad:
                    //_turnTableRightKeybindingText1.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableRight, 2);
                    //_turnTableRightKeybindingText2.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableRight, 3);
                    //_turnTableLeftKeybindingText1.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableLeft, 2);
                    //_turnTableLeftKeybindingText2.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableLeft, 3);
                    //_placeCardKeybindingText1.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.PlaceCard, 2);
                    //_placeCardKeybindingText2.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.PlaceCard, 3);

                    _turnTableRightKeybindingButton1.onClick.RemoveAllListeners();
                    _turnTableRightKeybindingButton2.onClick.RemoveAllListeners();
                    _turnTableLeftKeybindingButton1.onClick.RemoveAllListeners();
                    _turnTableLeftKeybindingButton2.onClick.RemoveAllListeners();
                    _placeCardKeybindingButton1.onClick.RemoveAllListeners();
                    _placeCardKeybindingButton2.onClick.RemoveAllListeners();

                    _turnTableRightKeybindingButton1.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.TurnTableRight, 2);
                    });

                    _turnTableRightKeybindingButton2.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.TurnTableRight, 3);
                    });

                    _turnTableLeftKeybindingButton1.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.TurnTableLeft, 2);
                    });

                    _turnTableLeftKeybindingButton2.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.TurnTableLeft, 3);
                    });

                    _placeCardKeybindingButton1.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.PlaceCard, 2);
                    });

                    _placeCardKeybindingButton2.onClick.AddListener(() =>
                    {
                        RebindBinding(GameInputManager.Actions.PlaceCard, 3);
                    });
                    break;
            }
        }

        private void GameInputManager_OnDuplicateKeybindingFound(object sender, InputBinding binding)
        {
            _rebindPanelText.text = $"Key is already being used for {binding.action}!\nPlease use another key.";
        }

        public void UpdateVisual()
        {
            #region Graphics

            if (_resolutionDropdown.value != GameSettingsFile.Instance.ResolutionIndex)
            {
                //_resolutionDropdown.value = GameSettingsFile.Instance.ResolutionIndex;
                _resolutionDropdown.SetValueWithoutNotify(GameSettingsFile.Instance.ResolutionIndex);
            }

            if (_displayDropdown.value != GameSettingsFile.Instance.DisplayIndex)
            {
                //_displayDropdown.value = GameSettingsFile.Instance.DisplayIndex;
                _displayDropdown.SetValueWithoutNotify(GameSettingsFile.Instance.DisplayIndex);
            }

            if (_windowModeDropdown.value != GameSettingsFile.Instance.WindowModeIndex)
            {
                //_windowModeDropdown.value = GameSettingsFile.Instance.WindowModeIndex;
                _windowModeDropdown.SetValueWithoutNotify(GameSettingsFile.Instance.WindowModeIndex);
            }

            if (_vSyncToggle.isOn != GameSettingsFile.Instance.VSync)
            {
                _vSyncToggle.isOn = GameSettingsFile.Instance.VSync;
            }

            _vSyncToggleState.text = GameSettingsFile.Instance.VSync ? "Enabled" : "Disabled";

            if (_textureQualityDropdown.value != GameSettingsFile.Instance.TextureQualityIndex)
            {
                _textureQualityDropdown.value = GameSettingsFile.Instance.TextureQualityIndex;
            }

            if (_shadowQualityDropdown.value != GameSettingsFile.Instance.ShadowQualityIndex)
            {
                _shadowQualityDropdown.value = GameSettingsFile.Instance.ShadowQualityIndex;
            }

            if (_softShadowsToggle.isOn != GameSettingsFile.Instance.SoftShadows)
            {
                _softShadowsToggle.isOn = GameSettingsFile.Instance.SoftShadows;
            }

            _softShadowsToggleState.text = GameSettingsFile.Instance.SoftShadows ? "Enabled" : "Disabled";

            //if (_hdrToggle.isOn != GameSettingsFile.Instance.HDR)
            //{
            //    //_hdrToggle.isOn = GameSettingsFile.Instance.HDR;    
            //}

            //_hdrToggleState.text = GameSettingsFile.Instance.HDR ? "Enabled" : "Disabled";

            //if (_ssaoToggle.isOn != GameSettingsFile.Instance.SSAO)
            //{
            //    //_ssaoToggle.isOn = GameSettingsFile.Instance.SSAO;    
            //}

            //_ssaoToggleState.text = GameSettingsFile.Instance.SSAO ? "Enabled" : "Disabled";

            #endregion

            #region Audio

            if (_mainVolumeSlider.value != GameSettingsFile.Instance.MainVolume)
            {
                _mainVolumeSlider.value = GameSettingsFile.Instance.MainVolume;
            }

            _mainVolumeValueText.text = Mathf.Round(GameSettingsFile.Instance.MainVolume * 10f).ToString();

            if (_musicVolumeSlider.value != GameSettingsFile.Instance.MusicVolume)
            {
                _musicVolumeSlider.value = GameSettingsFile.Instance.MusicVolume;
            }

            _musicVolumeValueText.text = Mathf.Round(GameSettingsFile.Instance.MusicVolume * 10f).ToString();

            if (_sfxVolumeSlider.value != GameSettingsFile.Instance.SfxVolume)
            {
                _sfxVolumeSlider.value = GameSettingsFile.Instance.SfxVolume;
            }

            _sfxVolumeValueText.text = Mathf.Round(GameSettingsFile.Instance.SfxVolume * 10f).ToString();

            #endregion

            #region Keybindings

            //_turnTableRightKeybindingText1.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableRight, 0);
            //_turnTableRightKeybindingText2.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableRight, 1);
            //_turnTableLeftKeybindingText1.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableLeft, 0);
            //_turnTableLeftKeybindingText2.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.TurnTableLeft, 1);
            //_placeCardKeybindingText1.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.PlaceCard, 0);
            //_placeCardKeybindingText2.text = GameInputManager.Instance.GetBindingText(GameInputManager.Actions.PlaceCard, 1);

            #endregion
        }

        /// <summary>
        /// Rebind a actions.
        /// </summary>
        /// <param name="actions">The actions that gets changed.</param>
        /// <param name="bindingIndex">The actions index of the actions that gets changed. E.g. an action can have multiple bindings, jump can be space and arrow up.</param>
        private void RebindBinding(GameInputManager.Actions actions, int bindingIndex)
        {
            ShowRebindPanel(actions);

            GameInputManager.Instance.RebindBinding(actions, () =>
            {
                HideRebindPanel();
                UpdateVisual();
            }, bindingIndex);
        }

        // If the game unpauses, hide the options menu.
        private void GameStateManager_OnOnGameUnpaused(object sender, EventArgs e)
        {
            Hide();
        }

        public void Show()
        {
            _uiCamera.Follow = OptionsCameraFocus;

            CinemachineComponentBase componentBase = _uiCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (componentBase is CinemachineFramingTransposer)
            {
                (componentBase as CinemachineFramingTransposer).m_CameraDistance = 1.5f;
            }

            gameObject.SetActive(true);

            _graphicsButton.Select();
        }

        private void ShowRebindPanel(GameInputManager.Actions actions)
        {
            _rebindPanelText.text = $"Press a key to rebind {actions}.\nPress {Keyboard.current.escapeKey.displayName} or {Gamepad.current.buttonEast.displayName} to cancel the process.";

            _rebindPanel.SetActive(true);
        }

        public void HideRebindPanel()
        {
            _rebindPanel.SetActive(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
            {
                // Unsubscribe from events in case of destruction.
                GameStateManager.Instance.OnGameUnpaused -= GameStateManager_OnOnGameUnpaused;
            }

            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnInputDeviceChanged -= SetGamepadFocusOptionsMenu;
                GameInputManager.Instance.OnInputDeviceChanged -= SwapInputIcons;

                GameInputManager.Instance.OnDuplicateKeybindingFound -= GameInputManager_OnDuplicateKeybindingFound;
            }

            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.OnMoveWindowOperationComplete -= GameSettingsManager_OnMoveWindowOperationComplete;
            }
        }

        private void OnApplicationQuit()
        {
            if (GameStateManager.Instance != null)
            {
                // Unsubscribe from events in case of destruction.
                GameStateManager.Instance.OnGameUnpaused -= GameStateManager_OnOnGameUnpaused;
            }

            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnInputDeviceChanged -= SetGamepadFocusOptionsMenu;
                GameInputManager.Instance.OnInputDeviceChanged -= SwapInputIcons;

                GameInputManager.Instance.OnDuplicateKeybindingFound -= GameInputManager_OnDuplicateKeybindingFound;
            }

            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.OnMoveWindowOperationComplete -= GameSettingsManager_OnMoveWindowOperationComplete;
            }
        }
    }
}