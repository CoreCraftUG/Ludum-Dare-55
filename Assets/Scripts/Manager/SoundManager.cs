using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;

namespace CoreCraft.Core
{
    public class SoundManager : Singleton<SoundManager>
    {
        public bool EventManagerReady;

        [Header("Sound Mixer")] 
        [SerializeField] private List<AudioClip> _allClips = new List<AudioClip>();
        [SerializeField] private AudioSource _sfxSource;

        [FoldoutGroup("Music"), Header("Audio Sources"), SerializeField] private AudioSource _mainTrack;
        [FoldoutGroup("Music"), SerializeField] private AudioSource _track1;
        [FoldoutGroup("Music"), SerializeField] private AudioSource _track2;
        [FoldoutGroup("Music"), SerializeField] private AudioSource _track3;
        [FoldoutGroup("Music"), SerializeField] private AudioSource _track4;
        [FoldoutGroup("Music"), SerializeField] private AudioSource _track5;
        [FoldoutGroup("Music"), SerializeField] private AudioSource _track6;
        [FoldoutGroup("Music"), SerializeField] private AudioSource[] _tracks;

        [SerializeField] private float _easeInTime;
        [SerializeField] private float _easeOutTime;

        [ShowInInspector, ReadOnly] private bool _wasMultiplied;
        [ShowInInspector, ReadOnly] private float _easeTimer;
        [ShowInInspector, ReadOnly] private List<int> _playingTrackIndexes = new List<int>();
        [ShowInInspector, ReadOnly] private float _lastMultiply;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
        
        private void Start()
        {
            StartCoroutine(SetUpCoroutine());
        }

        private void Update()
        {
            if (_wasMultiplied)
                return;

            _easeTimer += Time.deltaTime;

            for(int i = _playingTrackIndexes.Count - 1; i >= 0; i--)
            {
                int index = _playingTrackIndexes[i];

                if (index >= 0 && index < _tracks.Length && _tracks[index].volume > 1f - (_easeTimer / _easeOutTime))
                    _tracks[index].volume = (1f - (_easeTimer / _easeOutTime) > 0) ? 1f - (_easeTimer / _easeOutTime) : 0f;

                if (1f - (_easeTimer / _easeOutTime) <= 0)
                {
                    _playingTrackIndexes.RemoveAt(i);
                }
            }
        }

        private IEnumerator SetUpCoroutine()
        {
            yield return new WaitUntil(() =>
            {
                return EventManagerReady;
            });

            foreach(AudioSource source in _tracks)
            {
                source.volume = 0f;
            }

            EventManager.Instance.PlayAudio.AddListener(PlaySFXDelayed);
        }

        private IEnumerator EaseInCoroutine(int index)
        {
            float currentEase = 0f;
            while(currentEase < _easeInTime)
            {
                currentEase += Time.deltaTime;
                if(_tracks[index].volume < currentEase / _easeInTime)
                    _tracks[index].volume = (currentEase / _easeInTime <= 1) ? currentEase / _easeInTime : 1f;

                yield return null;
            }
            _tracks[index].volume = 1f;
        }

        private void MusicReset()
        {
            _wasMultiplied = false;
            _lastMultiply = 1;
        }

        private void MusicMultiply(float value)
        {
            if(value - _lastMultiply >= 0.2f)
            {
                int newIndex = UnityEngine.Random.Range(0,_tracks.Length);

                while(_playingTrackIndexes.Count < _tracks.Length && _playingTrackIndexes.Contains(newIndex))
                {
                    newIndex = UnityEngine.Random.Range(0, _tracks.Length);
                }
                _playingTrackIndexes.Add(newIndex);
                _wasMultiplied = true;
                _easeTimer = 0f;
                _lastMultiply = value;
                StartCoroutine(EaseInCoroutine(newIndex));

                foreach(int index in _tracks.Select((s, i) => new { i, s }).Where(t => _playingTrackIndexes.Contains(t.i) && t.s.volume < 1f).Select(t => t.i).ToList())
                {
                    StartCoroutine(EaseInCoroutine(index));
                }
            }
        }

        private void SyncAudioSources()
        {

        }
        
        public void PlaySFXDelayed(int clip, float delay)
        {
            _sfxSource.clip = _allClips[clip];

            _sfxSource.PlayDelayed(delay);
        }

        public void PlaySFX(int clip)
        {
            _sfxSource.clip = _allClips[clip];

            _sfxSource.PlayOneShot(_allClips[clip]);
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.PlayAudio.RemoveAllListeners();
            }
        }

        private void OnApplicationQuit()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.PlayAudio.RemoveAllListeners();
            }
        }
    }
}