using System;
using System.Collections;
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

        private IEnumerator _timerCoroutine;

        public static bool AutoStartTimer;
        public Action OnTimerFinished;

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
        public void ResumeTimer()
        {
            StartTimer(_timeRemaining, OnTimerExpired);
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

            Debug.Log((float)time / _timeRemainingSet);

            _timerProgressbar.fillAmount = (float)time / _timeRemainingSet;
            _timerProgressbar.color = Color.Lerp(Color.red, Color.white, (float)time / _timeRemainingSet);
            _timerProgressbarBorder.color = Color.Lerp(Color.red, Color.white, (float)time / _timeRemainingSet);
        }
    }
}