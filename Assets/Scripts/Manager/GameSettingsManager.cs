using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

namespace CoreCraft.Core
{
    public class GameSettingsManager : MonoBehaviour
    {
        public static GameSettingsManager Instance { get; private set; }

        [Header("URP")]
        [SerializeField] private UniversalRenderPipelineAsset _urpAsset;
        [SerializeField] private UniversalRendererData _urpRendererData;

        [Header("Audio")]
        [SerializeField] private AudioMixer _audioMixer;
        private const string MAIN_VOLUME = "Master";
        private const string MUSIC_VOLUME = "Music";
        private const string SFX_VOLUME = "SFX";

        [Header("Camera")]
        public CinemachineVirtualCamera VirtualCamera;

        private bool _moveWindowInProgress;
        

        public event EventHandler OnMoveWindowOperationComplete;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one {this} instance in the scene!");
            }

            Instance = this;

            DontDestroyOnLoad(this.gameObject);
            
            // Get all supported resolutions.
            GetSupportedResolutions();

            // Get all connected displays.
            GetConnectedDisplays();

            // Configure Game Settings based on UserSettingsFile.
            ConfigureGameSettings();
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
        }

        /// <summary>
        /// Get all available resolutions for the current display.
        /// </summary>
        public void GetSupportedResolutions()
        {
            Resolution[] resolutions = Screen.resolutions;
            Array.Reverse(resolutions);
            GameSettingsFile.Instance.SupportedResolutions = resolutions;
        }

        /// <summary>
        /// Sets the resolution of the application.
        /// </summary>
        /// <param name="index">Selected option of the dropdown - 0 equals highest available resolution. </param>
        public void SetResolution(int index)
        {
            Screen.SetResolution(GameSettingsFile.Instance.SupportedResolutions[index].width, GameSettingsFile.Instance.SupportedResolutions[index].height, GameSettingsFile.Instance.LastSavedWindowMode, GameSettingsFile.Instance.SupportedResolutions[index].refreshRateRatio);
            GameSettingsFile.Instance.ResolutionIndex = index;
            GameSettingsFile.Instance.LastSavedResolution = GameSettingsFile.Instance.SupportedResolutions[index];

            ES3.Save(GameSettingsFile.USERSETTINGS_RESOLUTION, GameSettingsFile.Instance.ResolutionIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_LASTSAVEDRESOLUTION, GameSettingsFile.Instance.LastSavedResolution, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }

        /// <summary>
        /// Adds all connected displays to the dropdown.
        /// </summary>
        private void GetConnectedDisplays()
        {
            // Get the supported displays and their information.
            Screen.GetDisplayLayout(GameSettingsFile.Instance.SupportedDisplays);
        }

        /// <summary>
        /// Sets the current display to the index of the display dropdown.
        /// </summary>
        /// <param name="index">Selected display of the dropdown.</param>
        /// <remarks>
        /// Only called if the current display is not equal to the last saved display.
        /// </remarks>
        public void SetDisplay(int index)
        {
            // Check if the current display is equal to the last saved display.
            //if (!Screen.mainWindowDisplayInfo.Equals(GameSettings.Instance.SupportedDisplays[index]))
            //{
            //    StartCoroutine(MoveToDisplay(index));
            //}

            StartCoroutine(MoveToDisplay(index));
        }

        /// <summary>
        /// Moves the main window to the selected display.
        /// </summary>
        /// <param name="index">Selected display.</param>
        /// <remarks>
        /// Upon completion, refresh the available resolution options in case the new display supports different resolutions.
        /// Set the resolution to the highest available.
        /// </remarks>
        private IEnumerator MoveToDisplay(int index)
        {
            _moveWindowInProgress = true;

            try
            {
                var display = GameSettingsFile.Instance.SupportedDisplays[index];

                Vector2Int targetCoordinates = new Vector2Int(0, 0);

                if (Screen.fullScreenMode != FullScreenMode.Windowed)
                {
                    // Target the center of the display. Doing it this way shows off
                    // that MoveMainWindow snaps the window to the top left corner
                    // of the display when running in fullscreen mode.
                    targetCoordinates.x += display.width / 2;
                    targetCoordinates.y += display.height / 2;
                }

                var moveOperation = Screen.MoveMainWindowTo(display, targetCoordinates);
                yield return moveOperation;
            }
            finally
            {
                _moveWindowInProgress = false;

                GameSettingsFile.Instance.DisplayIndex = index;
                GameSettingsFile.Instance.LastSavedDisplay = GameSettingsFile.Instance.SupportedDisplays[index];

                ES3.Save(GameSettingsFile.USERSETTINGS_DISPLAY, GameSettingsFile.Instance.DisplayIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
                ES3.Save(GameSettingsFile.USERSETTINGS_LASTSAVEDDISPLAY, GameSettingsFile.Instance.LastSavedDisplay, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);

                // Get supported resolutions for the new display.
                GetSupportedResolutions();

                // Notify the UI.
                OnMoveWindowOperationComplete?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets the window mode of the application.
        /// </summary>
        /// <param name="index">Selected window mode of the dropdown.</param>
        public void SetWindowMode(int index)
        {
            switch (index)
            {
                case 0: // Fullscreen Mode
                    Screen.SetResolution(GameSettingsFile.Instance.LastSavedResolution.width, GameSettingsFile.Instance.LastSavedResolution.height, FullScreenMode.ExclusiveFullScreen, GameSettingsFile.Instance.LastSavedResolution.refreshRateRatio);
                    GameSettingsFile.Instance.LastSavedWindowMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case 1: // Borderless Fullscreen Mode
                    Screen.SetResolution(GameSettingsFile.Instance.LastSavedResolution.width, GameSettingsFile.Instance.LastSavedResolution.height, FullScreenMode.FullScreenWindow, GameSettingsFile.Instance.LastSavedResolution.refreshRateRatio);
                    GameSettingsFile.Instance.LastSavedWindowMode = FullScreenMode.FullScreenWindow;
                    break;
                case 2: // Windowed Mode
                    Screen.SetResolution(GameSettingsFile.Instance.LastSavedResolution.width, GameSettingsFile.Instance.LastSavedResolution.height, FullScreenMode.Windowed, GameSettingsFile.Instance.LastSavedResolution.refreshRateRatio);
                    GameSettingsFile.Instance.LastSavedWindowMode = FullScreenMode.Windowed;
                    break;
            }

            GameSettingsFile.Instance.WindowModeIndex = index;

            ES3.Save(GameSettingsFile.USERSETTINGS_WINDOW_MODE, GameSettingsFile.Instance.WindowModeIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_LASTSAVEDWINDOWMODE, GameSettingsFile.Instance.LastSavedWindowMode, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }

        /// <summary>
        /// Enables or disables V-Sync based on the toggle state.
        /// </summary>
        /// <param name="isChecked">V-Sync Toggle state.</param>
        public void SetVSync(bool isChecked)
        {
            QualitySettings.vSyncCount = isChecked ? 1 : 0;
            GameSettingsFile.Instance.VSync = isChecked;

            ES3.Save(GameSettingsFile.USERSETTINGS_VSYNC, GameSettingsFile.Instance.VSync, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }

        /// <summary>
        /// Sets the texture quality of the application.
        /// </summary>
        /// <param name="index">Selected texture quality of the dropdown.</param>
        public void SetTextureQuality(int index)
        {
            QualitySettings.globalTextureMipmapLimit = index;
            GameSettingsFile.Instance.TextureQualityIndex = index;

            ES3.Save(GameSettingsFile.USERSETTINGS_TEXTURE_QUALITY, GameSettingsFile.Instance.TextureQualityIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }

        /// <summary>
        /// Sets the shadow quality of the application
        /// </summary>
        /// <param name="index">Selected shadow quality of the dropdown.</param>
        public void SetShadowQuality(int index)
        {
            switch (index)
            {
                case 0: // Shadow Quality Very High
                    SetShadows(ShadowmaskMode.DistanceShadowmask, true, ShadowResolution._4096, LightRenderingMode.PerPixel,
                        8, true, ShadowResolution._4096, LightCookieResolution._4096, LightCookieFormat.ColorHDR, true, true, 150, 4);
                    break;
                case 1: // Shadow Quality High
                    SetShadows(ShadowmaskMode.DistanceShadowmask, true, ShadowResolution._2048, LightRenderingMode.PerPixel,
                        4, true, ShadowResolution._2048, LightCookieResolution._2048, LightCookieFormat.ColorHigh, true, true, 70, 3);
                    break;
                case 2: // Shadow Quality Medium
                    SetShadows(ShadowmaskMode.DistanceShadowmask, true, ShadowResolution._1024, LightRenderingMode.PerPixel,
                        2, true, ShadowResolution._1024, LightCookieResolution._1024, LightCookieFormat.GrayscaleHigh, true, true, 40, 2);
                    break;
                case 3: // Shadow Quality Low
                    SetShadows(ShadowmaskMode.Shadowmask, true, ShadowResolution._512, LightRenderingMode.Disabled,
                        0, false, ShadowResolution._512, LightCookieResolution._512, LightCookieFormat.ColorLow, false, false, 20, 1);
                    break;
                case 4: // Shadow Quality Very Low
                    SetShadows(ShadowmaskMode.Shadowmask, true, ShadowResolution._256, LightRenderingMode.Disabled,
                        0, false, ShadowResolution._256, LightCookieResolution._256, LightCookieFormat.GrayscaleLow, false, false, 15, 1);
                    break;
                case 5: // Shadow Quality Off
                    SetShadows(ShadowmaskMode.Shadowmask, false, ShadowResolution._256, LightRenderingMode.Disabled,
                        0, false, ShadowResolution._256, LightCookieResolution._256, LightCookieFormat.GrayscaleLow, false, false, 10, 1);
                    break;
            }

            GameSettingsFile.Instance.ShadowQualityIndex = index;
            
            ES3.Save(GameSettingsFile.USERSETTINGS_SHADOW_QUALITY, GameSettingsFile.Instance.ShadowQualityIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }

        /// <summary>
        /// Sets the shadow properties of the URP Asset.
        /// </summary>
        /// <param name="mask">The rendering mode of Shadowmask.</param>
        /// <param name="mainLightCastShadows">If enabled the main light can be a shadow casting light.</param>
        /// <param name="resolution">Resolution of the main light shadowmap texture. If cascades are enabled, cascades will be packed into an atlas and this setting controls the maximum shadows atlas resolution.</param>
        /// <param name="additionalLights">Additional lights support.</param>
        /// <param name="perObjectLights">Maximum amount of additional lights. These lights are sorted and culled per-object.</param>
        /// <param name="additionalLightsCastShadows">If enabled shadows will be supported for spot lights.</param>
        /// <param name="additionalLightsResolution">All additional lights are packed into a single shadowmap atlas. This setting controls the atlas size.</param>
        /// <param name="additionalLightsCookieResolution">All additional lights are packed into a single cookie atlas. This setting controls the atlas size.</param>
        /// <param name="additionalLightsCookieFormat">All additional lights are packed into a single cookie atlas. This setting controls the atlas format.</param>
        /// <param name="probeBlending">If enabled smooth transitions will be created between reflection probes.</param>
        /// <param name="boxProjection">If enabled reflections appear based on the object's position within the probe's box, while still using a single probe as the source of the reflection.</param>
        /// <param name="shadowDistance">Maximum shadow rendering distance.</param>
        /// <param name="shadowCascades">Number of cascade splits used for directional shadows.</param>
        /// <remarks>Uses the "URP Graphic Settings" helper since most of the properties don't have built-in getters and setters.</remarks>
        private void SetShadows(ShadowmaskMode mask, bool mainLightCastShadows, ShadowResolution resolution, LightRenderingMode additionalLights, int perObjectLights, bool additionalLightsCastShadows, ShadowResolution additionalLightsResolution, LightCookieResolution additionalLightsCookieResolution, LightCookieFormat additionalLightsCookieFormat, bool probeBlending, bool boxProjection, float shadowDistance, int shadowCascades)
        {
            QualitySettings.shadowmaskMode = mask;
            URPGraphicSettings.MainLightCastShadows = mainLightCastShadows;
            URPGraphicSettings.MainLightShadowResolution = resolution;
            URPGraphicSettings.AdditionalLightsRenderingMode = additionalLights;
            _urpAsset.maxAdditionalLightsCount = perObjectLights;
            URPGraphicSettings.AdditionalLightCastShadows = additionalLightsCastShadows;
            URPGraphicSettings.AdditionalLightShadowResolution = additionalLightsResolution;
            URPGraphicSettings.AdditionalLightsCookieResolution = additionalLightsCookieResolution;
            URPGraphicSettings.AdditionalLightsCookieFormat = additionalLightsCookieFormat;
            URPGraphicSettings.ReflectionProbeBlending = probeBlending;
            URPGraphicSettings.ReflectionProbeBoxProjection = boxProjection;
            _urpAsset.shadowDistance = shadowDistance;
            _urpAsset.shadowCascadeCount = shadowCascades;
        }

        /// <summary>
        /// Enables or disables Soft Shadows based on the toggle state.
        /// </summary>
        /// <param name="isChecked">Soft Shadows Toggle state.</param>
        public void SetSoftShadows(bool isChecked)
        {
            URPGraphicSettings.SoftShadowsEnabled = isChecked;
            GameSettingsFile.Instance.SoftShadows = isChecked;

            ES3.Save(GameSettingsFile.USERSETTINGS_SOFT_SHADOWS, GameSettingsFile.Instance.SoftShadows, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }

        /// <summary>
        /// Enables or disables HDR based on the toggle state.
        /// </summary>
        /// <param name="isChecked">HDR Toggle state.</param>
        //public void SetHDR(bool isChecked)
        //{
        //    _urpAsset.supportsHDR = isChecked;
        //    GameSettingsFile.Instance.HDR = isChecked;
        //
        //    ES3.Save(GameSettingsFile.USERSETTINGS_HDR, GameSettingsFile.Instance.HDR, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        //}

        /// <summary>
        /// Enables or disables SSAO based on the toggle state.
        /// </summary>
        /// <param name="isChecked">SSAO Toggle state</param>
        //public void SetSSAO(bool isChecked)
        //{
        //    _urpRendererData.rendererFeatures[0].SetActive(isChecked); // TODO: Change this.
        //    GameSettingsFile.Instance.SSAO = isChecked;
        //
        //    ES3.Save(GameSettingsFile.USERSETTINGS_SSAO, GameSettingsFile.Instance.SSAO, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        //}

        /// <summary>
        /// Set the Main Volume of the application.
        /// </summary>
        /// <param name="value">Value of the Main Volume slider.</param>
        /// <remarks>Only called when the handle of the slider is moved.</remarks>
        public void SetMainVolume(float value)
        {
            // Set SfxVolume to the slider value and do some magic fuckery to display a correct value in the text.
            _audioMixer.SetFloat(MAIN_VOLUME, Mathf.Log10(value) * 20);
            GameSettingsFile.Instance.MainVolume = value;

            ES3.Save(GameSettingsFile.USERSETTINGS_MAIN_VOLUME, GameSettingsFile.Instance.MainVolume, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }

        /// <summary>
        /// Set the Music Volume of the application.
        /// </summary>
        /// <param name="value">Value of the Music Volume slider.</param>
        /// <remarks>Only called when the handle of the slider is moved.</remarks>
        public void SetMusicVolume(float value)
        {
            // Set SfxVolume to the slider value and do some magic fuckery to display a correct value in the text.
            _audioMixer.SetFloat(MUSIC_VOLUME, Mathf.Log10(value) * 20);
            GameSettingsFile.Instance.MusicVolume = value;

            ES3.Save(GameSettingsFile.USERSETTINGS_MUSIC_VOLUME, GameSettingsFile.Instance.MusicVolume, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }

        /// <summary>
        /// Set the SFX Volume of the application.
        /// </summary>
        /// <param name="value">Value of the SFX Volume slider.</param>
        /// <remarks>Only called when the handle of the slider is moved.</remarks>
        public void SetSFXVolume(float value)
        {
            // Set SfxVolume to the slider value and do some magic fuckery to display a correct value in the text.
            _audioMixer.SetFloat(SFX_VOLUME, Mathf.Log10(value) * 20);
            GameSettingsFile.Instance.SfxVolume = value;

            ES3.Save(GameSettingsFile.USERSETTINGS_SFX_VOLUME, GameSettingsFile.Instance.SfxVolume, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }

        private void ConfigureGameSettings()
        {
            if (!Screen.currentResolution.Equals(GameSettingsFile.Instance.LastSavedResolution))
            {
                SetResolution(GameSettingsFile.Instance.ResolutionIndex);
            }

            if (!Screen.mainWindowDisplayInfo.Equals(GameSettingsFile.Instance.LastSavedDisplay))
            {
                SetDisplay(GameSettingsFile.Instance.DisplayIndex);
            }

            if (Screen.fullScreenMode != GameSettingsFile.Instance.LastSavedWindowMode)
            {
                SetWindowMode(GameSettingsFile.Instance.WindowModeIndex);
            }

            int vSync = GameSettingsFile.Instance.VSync ? 1 : 0;

            if (QualitySettings.vSyncCount != vSync)
            {
                SetVSync(GameSettingsFile.Instance.VSync);
            }

            if (QualitySettings.globalTextureMipmapLimit != GameSettingsFile.Instance.TextureQualityIndex)
            {
                SetTextureQuality(GameSettingsFile.Instance.TextureQualityIndex);
            }

            //if (Shadow)
            //{
            // TODO: Find a way out to validate this shit.
            //}

            if (URPGraphicSettings.SoftShadowsEnabled != GameSettingsFile.Instance.SoftShadows)
            {
                SetSoftShadows(GameSettingsFile.Instance.SoftShadows);
            }
            
            _audioMixer.GetFloat(MAIN_VOLUME, out float mainVolume);

            if (mainVolume != GameSettingsFile.Instance.MainVolume)
            {
                SetMainVolume(GameSettingsFile.Instance.MainVolume);
            }

            _audioMixer.GetFloat(MUSIC_VOLUME, out float musicVolume);

            if (musicVolume != GameSettingsFile.Instance.MusicVolume)
            {
                SetMusicVolume(GameSettingsFile.Instance.MusicVolume);
            }

            _audioMixer.GetFloat(SFX_VOLUME, out float sfxVolume);

            if (sfxVolume != GameSettingsFile.Instance.SfxVolume)
            {
                SetSFXVolume(GameSettingsFile.Instance.SfxVolume);
            }

            //SetResolution(GameSettingsFile.Instance.ResolutionIndex);
            //SetDisplay(GameSettingsFile.Instance.DisplayIndex);
            //SetWindowMode(GameSettingsFile.Instance.WindowModeIndex);
            //SetVSync(GameSettingsFile.Instance.VSync);
            //SetTextureQuality(GameSettingsFile.Instance.TextureQualityIndex);
            SetShadowQuality(GameSettingsFile.Instance.ShadowQualityIndex);
            //SetSoftShadows(GameSettingsFile.Instance.SoftShadows);
            //SetHDR(GameSettingsFile.Instance.HDR);
            //SetSSAO(GameSettingsFile.Instance.SSAO);

            //SetMainVolume(GameSettingsFile.Instance.MainVolume);
            //SetMusicVolume(GameSettingsFile.Instance.MusicVolume);
            //SetSFXVolume(GameSettingsFile.Instance.SfxVolume);
        }

        public void SaveSettings()
        {
            ES3.Save(GameSettingsFile.USERSETTINGS_RESOLUTION, GameSettingsFile.Instance.ResolutionIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_LASTSAVEDRESOLUTION, GameSettingsFile.Instance.LastSavedResolution, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_DISPLAY, GameSettingsFile.Instance.DisplayIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_LASTSAVEDDISPLAY, GameSettingsFile.Instance.LastSavedDisplay, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_WINDOW_MODE, GameSettingsFile.Instance.WindowModeIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_LASTSAVEDWINDOWMODE, GameSettingsFile.Instance.LastSavedWindowMode, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_VSYNC, GameSettingsFile.Instance.VSync, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_TEXTURE_QUALITY, GameSettingsFile.Instance.TextureQualityIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_SHADOW_QUALITY, GameSettingsFile.Instance.ShadowQualityIndex, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_SOFT_SHADOWS, GameSettingsFile.Instance.SoftShadows, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            //ES3.Save(GameSettingsFile.USERSETTINGS_HDR, GameSettingsFile.Instance.HDR, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            //ES3.Save(GameSettingsFile.USERSETTINGS_SSAO, GameSettingsFile.Instance.SSAO, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);

            ES3.Save(GameSettingsFile.USERSETTINGS_MAIN_VOLUME, GameSettingsFile.Instance.MainVolume, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_MUSIC_VOLUME, GameSettingsFile.Instance.MusicVolume, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
            ES3.Save(GameSettingsFile.USERSETTINGS_SFX_VOLUME, GameSettingsFile.Instance.SfxVolume, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
        }
    }
}