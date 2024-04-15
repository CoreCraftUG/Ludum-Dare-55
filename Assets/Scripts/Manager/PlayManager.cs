using CoreCraft.Core;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using CharacterController = CoreCraft.Core.CharacterController;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;
using MelenitasDev.SoundsGood;

namespace CoreCraft.LudumDare55
{
    public class PlayManager : Singleton<PlayManager>
    {
        [SerializeField] private GameObject _playerCharacterPrefab;
        [SerializeField] private float _gridMoveUpTime;
        [SerializeField] private float _waveTime;
        [SerializeField] private float _diggingTimeReduction;
        [SerializeField] private int _minStartSpawnPosition;
        [SerializeField] private int _maxStartSpawnPosition;
        [SerializeField] private SpawnEnemy[] SpawnEnemies;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _spawnAreaBottomLeftCorner;
        [SerializeField] private Transform _spawnAreaTopRightCorner;
        [SerializeField] private Timer _gridMoveTimer;
        [SerializeField] private Timer _spawnTimer;

        private bool _spawningInProgress = false;
        private int _currentWave;
        private float _currentWaveTimer;
        private float _currentMoveTimer;
        private GameObject _spawnedPlayer;
        private CharacterController _characterController;
        private List<IGoToWaitingArea> _waitingEnemies = new List<IGoToWaitingArea>();
        private GridCell _spawnCell;
        private List<GridCell> _possibleEntrances = new List<GridCell>();

        public bool TimePause;
        public int WaveCounter { get { return _currentWave; } }

        void Start()
        {
            StartCoroutine(GameStartCoroutine());
        }

        private IEnumerator GameStartCoroutine()
        {
            yield return new WaitUntil(() => SummonManager.Instance != null && Grid.Instance != null && EventManager.Instance != null);

            EventManager.Instance.DiggingEvent.AddListener(() => { _currentWaveTimer += _diggingTimeReduction; });

            _minStartSpawnPosition = _minStartSpawnPosition < 0? 0 : _minStartSpawnPosition;
            _maxStartSpawnPosition = _maxStartSpawnPosition >= Grid.Instance.GridWidth ? Grid.Instance.GridWidth : _maxStartSpawnPosition;
            int spawnPosition = Random.Range(_minStartSpawnPosition, _maxStartSpawnPosition + 1);

            GridCell startCell = Grid.Instance.GetCellByIndexWithNull(new Vector2Int(spawnPosition, Grid.Instance.GridHeight - 1));
            Grid.Instance.MineCell(startCell);
            _spawnedPlayer = Instantiate(_playerCharacterPrefab, startCell.WorldPosition, Quaternion.identity);
            if (_spawnedPlayer == null)
                throw new Exception("Character Object is Null!");

            _characterController = _spawnedPlayer.GetComponent<CharacterController>();
            if (_characterController == null)
                throw new Exception("Character Controller is Null!");

            _characterController.Spawn(startCell.GridPosition, Vector2Int.down);
            _possibleEntrances.Add(startCell);


            _spawnTimer.StartTimer((int)_waveTime, () =>
            {
                _currentMoveTimer = 0;
                StartCoroutine(MoveGrid());
            });

            _gridMoveTimer.StartTimer((int)_gridMoveUpTime, () =>
            {
                _currentWaveTimer = 0;
                _currentWaveTimer++;
                StartCoroutine(SpawnWave());
            });

        }

        public GridCell GetGridEntrance()
        {
            if(_possibleEntrances != null && _possibleEntrances.Count > 0)
                return _possibleEntrances[Random.Range(0,_possibleEntrances.Count)];
            else
                return null;
        }

        [Button("Debug Move Grid")]
        private void DebugMoveGrid()
        {
            _currentMoveTimer = 0;
            StartCoroutine(MoveGrid());
        }

        private IEnumerator MoveGrid()
        {
            _gridMoveTimer.PauseTimer();
            //Sound moveGridSound = SFX.
            yield return StartCoroutine(Grid.Instance.MoveUp());

            for(int x = 0; x < Grid.Instance.GridWidth; x++)
            {
                GridCell cell = Grid.Instance.GetCellByIndexWithNull(new Vector2Int(x, Grid.Instance.GridWidth - 1));

                if(cell != null && cell.Block.BlockingType == BlockingType.None)
                {
                    _possibleEntrances.Add(cell);
                }
            }
            _gridMoveTimer.ResumeTimer(() =>
            {
                _currentWaveTimer = 0;
                _currentWaveTimer++;
                StartCoroutine(SpawnWave());
            });
        }

        private IEnumerator SpawnWave()
        {
            _spawnTimer.PauseTimer();
            yield return new WaitUntil(()=>_spawningInProgress);

            foreach(IGoToWaitingArea enemy in _waitingEnemies)
            {
                enemy.GoToStartCell();
            }

            yield return new WaitForSeconds(0.5f);
            _spawnTimer.ResumeTimer(() => 
            {
                _currentMoveTimer = 0;
                StartCoroutine(MoveGrid());
            });
            _waitingEnemies.Clear();
            _spawningInProgress = true;

            foreach (SpawnEnemy spawnEnemy in SpawnEnemies)
            {
                int spawnMaxValue = (spawnEnemy.MinSpawnCount + _currentWave > spawnEnemy.MaxSpawnCount)? spawnEnemy.MaxSpawnCount : spawnEnemy.MinSpawnCount + _currentWave;
                int spawnCounter = 0;
                GameObject enemyObj = null;
                while(spawnCounter < spawnMaxValue)
                {
                    enemyObj = Instantiate(spawnEnemy.EnemyPrefab, _spawnPoint.position, _spawnPoint.rotation);
                    if(enemyObj != null && enemyObj.TryGetComponent<IGoToWaitingArea>(out IGoToWaitingArea go) && enemyObj.TryGetComponent<IInGrid>(out IInGrid inGrid))
                    {
                        spawnCounter++;
                        inGrid.Spawn(_spawnCell.GridPosition, Vector2Int.down);
                        yield return StartCoroutine(go.GoToSpawnArea(_spawnAreaTopRightCorner.position - _spawnAreaBottomLeftCorner.position));

                        go.WanderAround(_spawnAreaBottomLeftCorner.position, _spawnAreaTopRightCorner.position);

                        _waitingEnemies.Add(go);
                    }
                }
            }

            _spawningInProgress = false;
        }
    }

    [Serializable]
    public struct SpawnEnemy
    {
        public int MinSpawnCount;
        public int MaxSpawnCount;
        public GameObject EnemyPrefab;
    }
}