using System;
using CoreCraft.Core;
using UnityEngine;

namespace JamCraft.GMTK2023.Code
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public event EventHandler OnGamePaused;
        public event EventHandler OnGameUnpaused;

        public bool IsGamePaused { get; set; } = false;
        public bool IsGameOver { get; set; } = false;

        public Transform LastPlayerFocusPoint
        {
            get => _lastPlayerFocusPoint;
            set => _lastPlayerFocusPoint = value;
        }

        private Transform _lastPlayerFocusPoint;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one {this} instance in the scene!");
            }

            Instance = this;
        }

        private void Start()
        {
            GameInputManager.Instance.OnPauseAction += Instance_OnPauseAction;
        }

        private void Instance_OnPauseAction(object sender, EventArgs e)
        {
            IsGamePaused = !IsGamePaused;

            if (IsGameOver) return;

            if (IsGamePaused)
            {
                Time.timeScale = 0;
                OnGamePaused?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Time.timeScale = 1f;
                OnGameUnpaused?.Invoke(this, EventArgs.Empty);
            }
        }

        public void OnPauseAction()
        {
            Instance_OnPauseAction(this, EventArgs.Empty);
        }

        private void OnDestroy()
        {
            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnPauseAction -= Instance_OnPauseAction;
            }
        }

        private void OnApplicationQuit()
        {
            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnPauseAction -= Instance_OnPauseAction;
            }
        }
    }
}