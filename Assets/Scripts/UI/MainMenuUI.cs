using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft.Core
{
    public class MainMenuUI : MonoBehaviour
    {
        public static MainMenuUI Instance { get; private set; }

        [Header("UI Buttons")] 
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _coreCraftButton;

        [Header("Quit Panel")]
        [SerializeField] private TextMeshProUGUI _quitPanelText;
        [SerializeField] private Button _quitYesButton;
        [SerializeField] private Button _quitNoButton;

        [SerializeField] private CinemachineVirtualCamera _uiCamera;

        public Transform MainMenuCenterTransform;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one {this} instance in the scene!");
            }

            Instance = this;
            
            SetupUIButtons();

            Time.timeScale = 1f;
        }

        private void Start()
        {
            _quitYesButton.gameObject.SetActive(false);
            _quitNoButton.gameObject.SetActive(false);

            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnInputDeviceChanged += SetGamepadFocusMainMenu;
            }

            GameSettingsManager.Instance.VirtualCamera = _uiCamera;

            _uiCamera.Follow = MainMenuCenterTransform.transform;

            CinemachineComponentBase componentBase = _uiCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (componentBase is CinemachineFramingTransposer)
            {
                (componentBase as CinemachineFramingTransposer).m_CameraDistance = 1.25f;
            }
        }

        private void SetGamepadFocusMainMenu(object sender, GameInputManager.ControlScheme controlScheme)
        {
            if (controlScheme == GameInputManager.ControlScheme.Gamepad)
            {
                if (!gameObject.activeSelf) return;
                
                _playButton.Select();
            }
        }

        private void SetupUIButtons()
        {
            // Load the game scene.
            _playButton.onClick.AddListener(() =>
            {
                _uiCamera.Follow = _playButton.transform;
            });

            // Show the options menu and hide the pause menu.
            _optionsButton.onClick.AddListener(() =>
            {
                GameOptionsUI.Instance.Show();
                Hide();
            });

            // Show quit panel on click.
            _quitButton.onClick.AddListener(() =>
            {
                _quitYesButton.gameObject.SetActive(true);
                _quitNoButton.gameObject.SetActive(true);
                _quitYesButton.Select();
                _uiCamera.Follow = _quitButton.transform;
            });
            
            _coreCraftButton.onClick.AddListener(() =>
            {
                //Credits.Instance.Show();
                //Hide();
            });

            // Close the program.
            _quitYesButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            // Close the quit panel.
            _quitNoButton.onClick.AddListener(() =>
            {
                _uiCamera.Follow = MainMenuCenterTransform.transform;
                _playButton.Select();
            });
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            _uiCamera.Follow = MainMenuCenterTransform.transform;

            if (_playButton != null)
            {
                _playButton.Select();
            }

            gameObject.SetActive(true);

            CinemachineComponentBase componentBase = _uiCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (componentBase is CinemachineFramingTransposer)
            {
                (componentBase as CinemachineFramingTransposer).m_CameraDistance = 1.25f;
            }
        }

        private void OnDestroy()
        {
            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnInputDeviceChanged -= SetGamepadFocusMainMenu;
            }
        }

        private void OnApplicationQuit()
        {
            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnInputDeviceChanged -= SetGamepadFocusMainMenu;
            }
        }
    }
}