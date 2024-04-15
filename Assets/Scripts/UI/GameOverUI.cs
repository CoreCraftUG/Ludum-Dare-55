using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreCraft.Core
{
    public class GameOverUI : MonoBehaviour
    {
        public static GameOverUI Instance { get; private set; }

        [Header("UI Buttons")] 
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        //[Header("UI Texts")]

        //[SerializeField] private CinemachineVirtualCamera _virtualCamera;
        //[SerializeField] private Transform _gameOverUICenterTransform;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one {this} instance in the scene!");
            }

            Instance = this;

            EventManager.Instance.GameOverEvent.AddListener(() =>
            {
                GameStateManager.Instance.IsGameOver = true;
                Show();
            });
            
            _retryButton.onClick.AddListener(() =>
            {
                Loader.Load(SceneManager.GetActiveScene().name);
            });

            _mainMenuButton.onClick.AddListener(() =>
            {
                Loader.Load("mainmenu_scene");
            });
        }

        private void Start()
        {
            EventManager.Instance.GameOverEvent.AddListener(() =>
            {
                GameStateManager.Instance.IsGameOver = true;
                Show();
            });

            Hide();
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                _retryButton.onClick.RemoveAllListeners();
                _mainMenuButton.onClick.RemoveAllListeners();
                EventManager.Instance.GameOverEvent.RemoveAllListeners();
            }
        }

        private void OnApplicationQuit()
        {
            if (EventManager.Instance != null)
            {
                _retryButton.onClick.RemoveAllListeners();
                _mainMenuButton.onClick.RemoveAllListeners();
                EventManager.Instance.GameOverEvent.RemoveAllListeners();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);

            _retryButton.Select();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}