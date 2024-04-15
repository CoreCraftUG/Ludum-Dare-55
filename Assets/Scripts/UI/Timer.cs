using System;
using System.Collections;
using CoreCraft.Core;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft.LudumDare55
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private int _timeRemaining;
        [SerializeField] private bool _timerPaused;
        [SerializeField] private bool _timerFinished;
        [SerializeField] private bool _timerStarted;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Image _timerProgressbar;
        [SerializeField] private TextMeshProUGUI _timerProgressBar;
        [SerializeField] private int _timeRemainingSet;
        [SerializeField] private Image _timerProgressbarBorder;
        [SerializeField] private Transform _parent;
        [SerializeField] private CameraController _cameraController;
        private IEnumerator _timerCoroutine;

        public static bool AutoStartTimer;
        public Action OnTimerFinished;

        private void Awake()
        {
            GameInputManager.Instance.OnMoveCamera += Instance_OnMoveCamera;
        }

        private void Instance_OnMoveCamera(object sender, Vector2 e)
        {
            _cameraController._xMovement = e.x;
        }

        private void Update()
        {
            if (_cameraController._xMovement != 0)
            {
                _parent.position += 0.025f * new Vector3(_cameraController._xMovement, 0, 0);
                _parent.position = new Vector3(Mathf.Clamp(_parent.position.x, -7f, 9f), _parent.position.y, _parent.position.z);
            }
        }

        private void OnTimerExpired()
        {
            Debug.Log("Timer expired!");
            _timerProgressBar.gameObject.SetActive(true);
            _timerFinished = true;
        }

        private void Start()
        {
            _timerProgressBar.gameObject.SetActive(false);

            DisplayTime(_timeRemaining);

            if (AutoStartTimer)
            {
                StartTimer(_timeRemaining, OnTimerExpired);
            }
        }

        [Button]
        public void RestartTimer(int time, Action onTimerFinished)
        {
            StartTimer(time, onTimerFinished);
        }

        [Button]
        public void StartTimer(int time, Action onTimerFinished)
        {
            OnTimerFinished = onTimerFinished;
            
            _timeRemaining = time;
            _timeRemainingSet = _timeRemaining;
            _timerCoroutine = StartTimerCoroutine(time);
            StartCoroutine(_timerCoroutine);
            _timerStarted = true;
            _timerFinished = false;
            _timerPaused = false;
        }

        private IEnumerator StartTimerCoroutine(int time)
        {
            while (time > 0)
            {
                yield return new WaitForSecondsRealtime(1);
                time--;
                _timeRemaining = time;
                DisplayTime(time);
            }
            
            OnTimerFinished?.Invoke();
        }

        [Button]
        public void ResumeTimer(Action onTimerExpired)
        {
            StartTimer(_timeRemaining, onTimerExpired);
            _timerPaused = false;
        }

        [Button]
        public void PauseTimer()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
                OnTimerFinished = null;
                _timerPaused = true;
            }
        }

        public bool HasTimerStarted()
        {
            return _timerStarted;
        }

        public bool HasTimerFinished()
        {
            return _timerFinished;
        }

        public bool IsTimerPaused()
        {
            return _timerPaused;
        }

        private void DisplayTime(int time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            _timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);

            //Debug.Log((float)time / _timeRemainingSet);

            _timerProgressbar.fillAmount = (float)time / _timeRemainingSet;
            _timerProgressbar.color = Color.Lerp(Color.red, Color.white, (float)time / _timeRemainingSet);
            _timerProgressbarBorder.color = Color.Lerp(Color.red, Color.white, (float)time / _timeRemainingSet);
        }
    }
}