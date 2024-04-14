using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Core
{
    public class GameSettingsFile : MonoBehaviour
    {
        #region Variables

        public static GameSettingsFile Instance { get; private set; }

        private readonly string _userSettingsFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\My Games\\Ludum Dare 55\\GameUserSettings.ini";
        private const string USERSETTINGS_ENCRIPTIONPASSWORD = "CoreCraftsSuperSavePassword";
        private ES3Settings _es3settings;
        public ES3Settings ES3Settings { get => _es3settings; }

        public string UserSettingsFilePath { get => _userSettingsFilePath; }

        // Graphics Settings
        [SerializeField] private int _resolutionIndex;
        [SerializeField] private int _previousResolutionIndex;
        [SerializeField] private Resolution _lastSavedResolution;
        [SerializeField] private Resolution[] _supportedResolutions;
        [SerializeField] private int _displayIndex;
        [SerializeField] private int _previousDisplayIndex;
        [SerializeField] private DisplayInfo _lastSavedDisplay;
        [SerializeField] private List<DisplayInfo> _supportedDisplays;
        [SerializeField] private int _windowModeIndex;
        [SerializeField] private int _previousWindowModeIndex;
        [SerializeField] private FullScreenMode _lastSavedWindowMode;
        [SerializeField] private int _frameRateIndex;
        [SerializeField] private int _lastSavedframeRate;
        [SerializeField] private bool _vSync;
        [SerializeField] private int _textureQualityIndex;
        [SerializeField] private int _shadowQualityIndex;
        [SerializeField] private bool _softShadows;
        //[SerializeField] private bool _hdr;
        //[SerializeField] private bool _ssao;

        // Audio Settings
        [SerializeField] private float _mainVolume;
        [SerializeField] private float _musicVolume;
        [SerializeField] private float _sfxVolume;

        /// <summary>
        /// Saved index of the resolution dropdown.
        /// </summary>
        public int ResolutionIndex { get => _resolutionIndex; set => _resolutionIndex = value; }

        /// <summary>
        /// Previous index of the resolution dropdown.
        /// </summary>
        public int PreviousResolutionIndex { get => _previousResolutionIndex; set => _previousResolutionIndex = value; }

        /// <summary>
        /// Last saved resolution.
        /// </summary>
        public Resolution LastSavedResolution { get => _lastSavedResolution; set => _lastSavedResolution = value; }

        /// <summary>
        /// Holds all supported resolutions of the current display.
        /// </summary>
        public Resolution[] SupportedResolutions { get => _supportedResolutions; set => _supportedResolutions = value; }

        /// <summary>
        /// Saved index of the display dropdown.
        /// </summary>
        public int DisplayIndex { get => _displayIndex; set => _displayIndex = value; }

        /// <summary>
        /// Previous index of the display dropdown.
        /// </summary>
        public int PreviousDisplayIndex { get => _previousDisplayIndex; set => _previousDisplayIndex = value; }

        /// <summary>
        /// Last saved display.
        /// </summary>
        public DisplayInfo LastSavedDisplay { get => _lastSavedDisplay; set => _lastSavedDisplay = value; }

        /// <summary>
        /// Holds all connected displays.
        /// </summary>
        public List<DisplayInfo> SupportedDisplays { get => _supportedDisplays; set => _supportedDisplays = value; }

        /// <summary>
        /// Saved index of the window mode dropdown.
        /// </summary>
        public int WindowModeIndex { get => _windowModeIndex; set => _windowModeIndex = value; }

        /// <summary>
        /// Previous index of the window mode dropdown.
        /// </summary>
        public int PreviousWindowModeIndex { get => _previousWindowModeIndex; set => _previousWindowModeIndex = value; }

        /// <summary>
        /// Last saved window mode.
        /// </summary>
        public FullScreenMode LastSavedWindowMode { get => _lastSavedWindowMode; set => _lastSavedWindowMode = value; }

        /// <summary>
        /// Saved index of the frame rate dropdown.
        /// </summary>
        public int FrameRateIndex { get => _frameRateIndex; set => _frameRateIndex = value; }

        /// <summary>
        /// Last saved frame rate value.
        /// </summary>
        public int LastSavedFrameRate { get => _lastSavedframeRate; set => _lastSavedframeRate = value; }

        /// <summary>
        /// Saved toggle state of the V-Sync toggle.
        /// </summary>
        public bool VSync { get => _vSync; set => _vSync = value; }

        /// <summary>
        /// Saved index of the texture quality dropdown.
        /// </summary>
        public int TextureQualityIndex { get => _textureQualityIndex; set => _textureQualityIndex = value; }

        /// <summary>
        /// Saved index of the shadow quality dropdown.
        /// </summary>
        public int ShadowQualityIndex { get => _shadowQualityIndex; set => _shadowQualityIndex = value; }

        /// <summary>
        /// Saved toggle state of the Soft Shadows toggle.
        /// </summary>
        public bool SoftShadows { get => _softShadows; set => _softShadows = value; }

        /// <summary>
        /// Saved toggle state of the HDR toggle.
        /// </summary>
        //public bool HDR { get => _hdr; set => _hdr = value; }

        /// <summary>
        /// Saved toggle state of the SSAO toggle.
        /// </summary>
        //public bool SSAO { get => _ssao; set => _ssao = value; }

        /// <summary>
        /// Saved value of the main volume.
        /// </summary>
        public float MainVolume { get => _mainVolume; set => _mainVolume = value; }

        /// <summary>
        /// Saved value of the music volume.
        /// </summary>
        public float MusicVolume { get => _musicVolume; set => _musicVolume = value; }

        /// <summary>
        /// Saved value of the sfx volume.
        /// </summary>
        public float SfxVolume { get => _sfxVolume; set => _sfxVolume = value; }

        /// <summary>
        /// Default volume for sliders.
        /// </summary>
        /// <remarks>Value ensures that the sliders are centered.</remarks>
        private const float DEFAULT_VOLUME = 0.5000499f;

        #endregion

        #region UserSettingsDataStrings
        
        public const string USERSETTINGS_RESOLUTION = "UserSettings_ResolutionValue";
        public const string USERSETTINGS_LASTSAVEDRESOLUTION = "UserSettings_LastSavedResolution";
        public const string USERSETTINGS_DISPLAY = "UserSettings_DisplayValue";
        public const string USERSETTINGS_LASTSAVEDDISPLAY = "UserSettings_LastSavedDisplay";
        public const string USERSETTINGS_WINDOW_MODE = "UserSettings_WindowModeValue";
        public const string USERSETTINGS_LASTSAVEDWINDOWMODE = "UserSettings_LastSavedWindowMode";
        public const string USERSETTINGS_FRAME_RATE = "UserSettings_FrameRateValue";
        public const string USERSETTINGS_LASTSAVEDFRAMERATE = "UserSettings_LastSavedFrameRateValue";
        public const string USERSETTINGS_VSYNC = "UserSettings_VSyncValue";
        public const string USERSETTINGS_TEXTURE_QUALITY = "UserSettings_TextureQualityValue";
        public const string USERSETTINGS_SHADOW_QUALITY = "UserSettings_ShadowQualityValue";
        public const string USERSETTINGS_SOFT_SHADOWS = "UserSettings_SoftShadowsValue";
        //public const string USERSETTINGS_HDR = "UserSettings_HdrValue";
        //public const string USERSETTINGS_SSAO = "UserSettings_SsaoValue";
        public const string USERSETTINGS_MAIN_VOLUME = "UserSettings_MainVolume";
        public const string USERSETTINGS_MUSIC_VOLUME = "UserSettings_MusicVolume";
        public const string USERSETTINGS_SFX_VOLUME = "UserSettings_SfxVolume";

        public const string USERSETTINGS_INPUT_BINDINGS = "UserSettings_InputBindings";

        #endregion

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one {this} instance in the scene!");
            }

            Instance = this;

            _es3settings = new ES3Settings() { encryptionType = ES3.EncryptionType.AES, encryptionPassword = USERSETTINGS_ENCRIPTIONPASSWORD, compressionType = ES3.CompressionType.Gzip, bufferSize = 250000 };

            _supportedDisplays = new List<DisplayInfo>();

            DontDestroyOnLoad(this.gameObject);

            if (ES3.FileExists(UserSettingsFilePath))
            {
                LoadUserSettings();
            }
        }

        private void LoadUserSettings()
        {
            if (ES3.KeyExists(USERSETTINGS_RESOLUTION, UserSettingsFilePath, _es3settings))
            {
                _resolutionIndex = ES3.Load<int>(USERSETTINGS_RESOLUTION, _userSettingsFilePath, 0, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_LASTSAVEDRESOLUTION, UserSettingsFilePath, _es3settings))
            {
                try
                {
                    _lastSavedResolution = ES3.Load<Resolution>(USERSETTINGS_RESOLUTION, _userSettingsFilePath, _es3settings);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //throw;
                }
            }

            if (ES3.KeyExists(USERSETTINGS_DISPLAY, UserSettingsFilePath, _es3settings))
            {
                _displayIndex = ES3.Load<int>(USERSETTINGS_DISPLAY, _userSettingsFilePath, 0, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_RESOLUTION, UserSettingsFilePath, _es3settings))
            {
                try
                {
                    _lastSavedDisplay = ES3.Load<DisplayInfo>(USERSETTINGS_LASTSAVEDDISPLAY, _userSettingsFilePath, _es3settings);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //throw;
                }
            }

            if (ES3.KeyExists(USERSETTINGS_WINDOW_MODE, UserSettingsFilePath, _es3settings))
            {
                _windowModeIndex = ES3.Load<int>(USERSETTINGS_WINDOW_MODE, _userSettingsFilePath, 0, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_LASTSAVEDWINDOWMODE, UserSettingsFilePath, _es3settings))
            {
                try
                {
                    _lastSavedWindowMode = ES3.Load<FullScreenMode>(USERSETTINGS_LASTSAVEDWINDOWMODE, _userSettingsFilePath, _es3settings);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //throw;
                }
            }

            if (ES3.KeyExists(USERSETTINGS_FRAME_RATE, UserSettingsFilePath, _es3settings))
            {
                _frameRateIndex = ES3.Load<int>(USERSETTINGS_FRAME_RATE, _userSettingsFilePath, 5, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_LASTSAVEDFRAMERATE, UserSettingsFilePath, _es3settings))
            {
                _lastSavedframeRate = ES3.Load<int>(USERSETTINGS_LASTSAVEDFRAMERATE, _userSettingsFilePath, 60, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_VSYNC, UserSettingsFilePath, _es3settings))
            {
                _vSync = ES3.Load<bool>(USERSETTINGS_VSYNC, _userSettingsFilePath, false, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_TEXTURE_QUALITY, UserSettingsFilePath, _es3settings))
            {
                _textureQualityIndex = ES3.Load<int>(USERSETTINGS_TEXTURE_QUALITY, _userSettingsFilePath, 0, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_SHADOW_QUALITY, UserSettingsFilePath, _es3settings))
            {
                _shadowQualityIndex = ES3.Load<int>(USERSETTINGS_SHADOW_QUALITY, _userSettingsFilePath, 0, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_SOFT_SHADOWS, UserSettingsFilePath, _es3settings))
            {
                _softShadows = ES3.Load<bool>(USERSETTINGS_SOFT_SHADOWS, _userSettingsFilePath, false, _es3settings);
            }

            //if (ES3.KeyExists(USERSETTINGS_HDR, UserSettingsFilePath, _es3settings))
            //{
            //    _hdr = ES3.Load<bool>(USERSETTINGS_HDR, _userSettingsFilePath, false, _es3settings);
            //}

            //if (ES3.KeyExists(USERSETTINGS_SSAO, UserSettingsFilePath, _es3settings))
            //{
            //    _ssao = ES3.Load<bool>(USERSETTINGS_SSAO, _userSettingsFilePath, false, _es3settings);
            //}

            if (ES3.KeyExists(USERSETTINGS_MAIN_VOLUME, UserSettingsFilePath, _es3settings))
            {
                _mainVolume = ES3.Load<float>(USERSETTINGS_MAIN_VOLUME, _userSettingsFilePath, DEFAULT_VOLUME, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_MUSIC_VOLUME, UserSettingsFilePath, _es3settings))
            {
                _musicVolume = ES3.Load<float>(USERSETTINGS_MUSIC_VOLUME, _userSettingsFilePath, DEFAULT_VOLUME, _es3settings);
            }

            if (ES3.KeyExists(USERSETTINGS_SFX_VOLUME, UserSettingsFilePath, _es3settings))
            {
                _sfxVolume = ES3.Load<float>(USERSETTINGS_SFX_VOLUME, _userSettingsFilePath, DEFAULT_VOLUME, _es3settings);
            }
        }
    }
}