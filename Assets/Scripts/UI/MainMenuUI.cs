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
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;

        [Header("Quit Panel")]
        [SerializeField] private GameObject _quitPanel;
        [SerializeField] private Button _quitYesButton;
        [SerializeField] private Button _quitNoButton;

        [SerializeField] private GameObject _creditsPanel;

        [Header("Camera")]
        [SerializeField] private CinemachineVirtualCamera _uiCamera;
        
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
            _quitPanel.SetActive(false);
            _creditsPanel.SetActive(false);

            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnInputDeviceChanged += SetGamepadFocusMainMenu;
            }

            //CinemachineComponentBase componentBase = _uiCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            //if (componentBase is CinemachineFramingTransposer)
            //{
            //    (componentBase as CinemachineFramingTransposer).m_CameraDistance = 1.25f;
            //}
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
                Loader.Load("GameScene");
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
                _quitPanel.SetActive(true);
                _quitNoButton.Select();
            });
            
            _creditsButton.onClick.AddListener(() =>
            {
                _creditsPanel.SetActive(true);
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
                _quitPanel.SetActive(false);
                _playButton.Select();
            });
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            if (_playButton != null)
            {
                _playButton.Select();
            }

            gameObject.SetActive(true);
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