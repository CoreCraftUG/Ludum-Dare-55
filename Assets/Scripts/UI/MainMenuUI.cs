using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JamCraft.GMTK2023.Code
{
    public class MainMenuUI : MonoBehaviour
    {
        public static MainMenuUI Instance { get; private set; }

        [Header("UI Buttons")] 
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _custominsationButton;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _coreCraftButton;
        [SerializeField] private Button _gameMode1Button;
        [SerializeField] private Button _gameMode2Button;

        [Header("Quit Panel")]
        [SerializeField] private TextMeshProUGUI _quitPanelText;
        [SerializeField] private Button _quitYesButton;
        [SerializeField] private Button _quitNoButton;
        

        [SerializeField] private CinemachineVirtualCamera _uiCamera;

        public Transform MainMenuCenterTransform;

        private Sequence _quitButtonSequence;
        private Sequence _gameModeButtonSequence;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one {this} instance in the scene!");
            }

            Instance = this;

            #region QuitButtonAnimation

            _quitButtonSequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(false).Pause();

            _quitButtonSequence.Append(_quitPanelText.gameObject.GetComponent<CanvasGroup>().DOFade(1f, .5f))
                .Join(_quitNoButton.gameObject.GetComponent<RectTransform>().DOLocalRotate(new Vector3(0, 0, -51f), .5f))
                .Join(_quitNoButton.gameObject.GetComponent<RectTransform>().DOLocalMove(new Vector3(3.235f, -2.047f, -0.171f), .5f)).SetEase(Ease.InOutQuad)
                .Join(_quitYesButton.gameObject.GetComponent<RectTransform>().DOLocalRotate(new Vector3(0, 0, 8.819f), .5f))
                .Join(_quitYesButton.gameObject.GetComponent<RectTransform>().DOLocalMove(new Vector3(2.203f, -1.669f, -0.171f), .5f)).SetEase(Ease.InOutQuad);

            #endregion

            #region GameModeButtonAnimation

            _gameModeButtonSequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true).Pause();

            _gameModeButtonSequence.Append(_gameMode1Button.gameObject.GetComponent<RectTransform>()
                    .DOLocalRotate(new Vector3(0, 0, 420f), 1f, RotateMode.LocalAxisAdd))
                .Join(_gameMode1Button.gameObject.GetComponent<RectTransform>()
                    .DOLocalMove(new Vector3(-3.43f, -1.43f, 0), 1f))
                .Join(_gameMode2Button.gameObject.GetComponent<RectTransform>()
                    .DOLocalRotate(new Vector3(0, 0, 420f), 1f, RotateMode.LocalAxisAdd))
                .Join(_gameMode2Button.gameObject.GetComponent<RectTransform>()
                    .DOLocalMove(new Vector3(-2.57f, -0.9989998f, 0), 1f));

            #endregion

            SetupUIButtons();

            Time.timeScale = 1f;
        }

        private void Start()
        {
            _quitYesButton.gameObject.SetActive(false);
            _quitNoButton.gameObject.SetActive(false);

            _gameMode1Button.gameObject.SetActive(false);
            _gameMode2Button.gameObject.SetActive(false);

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

                Navigation navigation = _playButton.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnLeft = _gameMode2Button;
                _playButton.navigation = navigation;

                _gameMode1Button.gameObject.SetActive(true);
                _gameMode2Button.gameObject.SetActive(true);
                _gameMode1Button.Select();

                _gameModeButtonSequence.Play();

                if (_quitButtonSequence.IsComplete())
                {
                    _quitButtonSequence.SmoothRewind();
                }
            });

            _custominsationButton.onClick.AddListener(() =>
            {
                _uiCamera.Follow = _custominsationButton.transform;

                CustomizationUI.Instance.Show();
                Hide();

                if (_quitButtonSequence.IsComplete())
                {
                    _quitButtonSequence.SmoothRewind();
                }
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
                _quitButtonSequence.PlayForward();
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
                _quitButtonSequence.SmoothRewind();
                _uiCamera.Follow = MainMenuCenterTransform.transform;
                _playButton.Select();
            });

            _gameMode1Button.onClick.AddListener(() => 
            {
                Loader.Load("game_scene");
            });
            
            _gameMode2Button.onClick.AddListener(() => 
            {
                Loader.Load("second_game_scene");
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