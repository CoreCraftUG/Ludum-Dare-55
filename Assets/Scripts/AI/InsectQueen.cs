using CoreCraft.Core;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class InsectQueen : MonoBehaviour, IDamageable, ICanDie, ICanSee, IInGrid, IMoveInGrid, IPeripheryTrigger
    {
        [SerializeField] protected int _sightDistance;
        [SerializeField] protected int _hP;
        [SerializeField] protected float _moveTime;
        [SerializeField] protected float _rushMoveTime;
        [SerializeField] protected LayerMask _sightLayerMask;
        [SerializeField] private MMFeedbacks _feedback;
        [SerializeField] private GameObject _dronePrefab;
        [SerializeField] private int _maxDroneCount;
        [SerializeField] private float _droneSpawnCoolDown;
        [SerializeField] private Animator _animator;

        protected Vector2Int _currentPosition;
        protected Vector2Int _targetPosition;
        protected Vector2Int _returnPoint;
        protected bool _hasTarget;
        protected bool _isMoving;
        protected Stack<GridCell> _targetPath = new Stack<GridCell>();

        protected IInGrid _currentHorror;
        protected IDamageable _currentEnemy;

        //Damageable
        public int HP => _hP;

        //Can Die
        public Animator Animator => GetComponentInChildren<Animator>();

        //Can See
        public int SightDistance => _sightDistance;
        public LayerMask SightLayerMask => _sightLayerMask;

        //In Grid
        public Vector2Int CurrentPosition => _currentPosition;

        //Move In Grid
        public float MoveTime => _moveTime;
        public Vector2Int TargetPosition => _targetPosition;
        public Vector2Int ReturnPoint => _returnPoint;
        public bool HasTarget => _hasTarget;
        public bool IsMoving => _isMoving;
        public Stack<GridCell> TargetPath => _targetPath;

        private float _timer;
        private Sequence _moveSequence;
        private Vector2Int _lookOrientation;

        private bool _goingBackToEntrance;

        private List<Simp> _drones = new List<Simp>();
        private float _droneSpawnCooldownTimer;

        void Start()
        {
            _moveSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause();

            EventManager.Instance.GridMoveUp.AddListener((Vector3 moveVector, float moveTime, int moveIncrements) =>
            {
                StartCoroutine(ReturnToGrid(moveVector, moveTime, moveIncrements));
            });
        }

        private IEnumerator ReturnToGrid(Vector3 moveVector, float moveTime, int moveIncrements)
        {
            bool moveDone = false;
            transform.DOMove(transform.position + moveVector, moveTime).OnComplete(() =>
            {
                moveDone = true;
            });

            yield return new WaitUntil(() => moveDone);

            if (_currentPosition.y + moveIncrements >= Grid.Instance.GridHeight)
            {
                GridCell cell = null;
                yield return new WaitUntil(() =>
                {
                    cell = PlayManager.Instance.GetGridEntrance();
                    return cell != null;
                });
                _currentPosition = cell.GridPosition;

                _goingBackToEntrance = true;
                transform.DOMove(cell.WorldPosition, _moveTime).OnComplete(() =>
                {
                    _goingBackToEntrance = false;
                });
            }
        }

        void Update()
        {
            if (_goingBackToEntrance)
                return;

            _droneSpawnCooldownTimer = Time.deltaTime;
            if (_droneSpawnCooldownTimer >= _droneSpawnCoolDown)
            {
                SpawnDrone();
                _droneSpawnCooldownTimer = 0;
            }

            if (_hasTarget)
            {
                if (!_isMoving)
                {
                    foreach(Simp drone in _drones)
                    {
                        drone.CallToAttack(_currentEnemy);
                    }

                    List<GridCell> neighbours = Pathfinding.GetNeighbour(CurrentPosition).Where(n => n.Block.BlockingType == BlockingType.None).ToList();
                    if (neighbours == null || neighbours.Count <= 0)
                        return;

                    int distance = 0;
                    Vector2Int index = _currentPosition;
                    foreach (GridCell cell in neighbours)
                    {
                        int i = Pathfinding.CalculateDistance(_currentPosition, cell.GridPosition);
                        if (i > distance)
                        {
                            distance = i;
                            index = cell.GridPosition;
                        }
                    }

                    _isMoving = true;

                    _lookOrientation = _targetPath.Peek().GridPosition - _currentPosition;

                    transform.DOLookAt(_targetPath.Peek().WorldPosition, _rushMoveTime).OnComplete(() =>
                    {

                        AnimateQueen(AnimationState.Walking);
                        _moveSequence.Append(transform.DOMove(_targetPath.Peek().WorldPosition, _rushMoveTime).OnComplete(() =>
                        {
                            _isMoving = false;

                            AnimateQueen(AnimationState.Walking);
                            _hasTarget = _targetPath.Count > 0;
                        }));
                        _currentPosition = _targetPath.Pop().GridPosition;

                        _moveSequence.PlayForward();
                    });
                }
            }
            else
            {
                if (!_isMoving)
                {
                    // Roaming
                    List<GridCell> neighbours = Pathfinding.GetNeighbour(_currentPosition).Where(n => n.Block.BlockingType == BlockingType.None).ToList();

                    if (neighbours == null || neighbours.Count == 0)
                        return;

                    GridCell cellToTest = neighbours[Random.Range(0, neighbours.Count)];

                    Vector2 normalizedDirection = cellToTest.GridPosition - _currentPosition;
                    normalizedDirection.Normalize();

                    _lookOrientation = Vector2Int.RoundToInt(normalizedDirection);

                    _isMoving = true;

                    transform.DOLookAt(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                    {
                        AnimateQueen(AnimationState.Walking);
                        _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                        {
                            _isMoving = false;

                            AnimateQueen(AnimationState.Walking);
                        }));
                        _currentPosition = cellToTest.GridPosition;

                        _moveSequence.PlayForward();
                    });
                }
            }
        }

        private void SpawnDrone()
        {
            for(int i = _drones.Count - 1; i >= 0; i--)
            {
                if (_drones[i] == null)
                    _drones.RemoveAt(i);
            }

            if(_drones.Count < _maxDroneCount)
            {
                GridCell cell = Grid.Instance.GetCellByIndexWithNull(CurrentPosition);
                GameObject obj = Instantiate(_dronePrefab, cell.WorldPosition,Quaternion.identity);
                if(obj != null && obj.TryGetComponent<IInGrid>(out IInGrid inGrid) && obj.TryGetComponent<Simp>(out Simp drone))
                {
                    AnimateQueen(AnimationState.Spawning);
                    inGrid.Spawn(cell.GridPosition, _lookOrientation);
                    _drones.Add(drone);
                }
                else
                    Destroy(obj);
            }
        }

        public void TriggerEnter(Collider other)
        {
            if (!_hasTarget && other.gameObject.TryGetComponent<IInGrid>(out IInGrid inGrid) && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                _moveSequence.Pause();
                _isMoving = false;
                _hasTarget = true;
                _currentHorror = inGrid;
                _currentEnemy = damageable;
            }
        }

        public void TriggerExit(Collider other)
        {
            if (_currentHorror != null && other.gameObject.TryGetComponent<IInGrid>(out IInGrid inGrid))
            {
                if (_currentHorror == inGrid)
                {
                    _currentHorror = null;
                }
            }
        }

        public void CheckSightCone(Collider other)
        {
            if (!_hasTarget && _sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer)) && other.gameObject.TryGetComponent<IInGrid>(out IInGrid inGrid) && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                _hasTarget = _targetPath != null && _targetPath.Count > 0;
                _currentHorror = inGrid;
                _currentEnemy = damageable;
            }
        }

        [Button("Debug Die")]
        public void Die()
        {
            AnimateQueen(AnimationState.Dead);
            _moveSequence.Kill();
            _isMoving = false;
            _hasTarget = false;
            _currentEnemy = null;
            _currentHorror = null;
            Destroy(this.gameObject, 1f);
        }

        public void Spawn(Vector2Int spawnPosition, Vector2Int spawnRotation)
        {
            if (SummonManager.Instance != null)
                SummonManager.Instance.RegisterSnail(this);
            else
                Destroy(gameObject);
            _currentPosition = spawnPosition;
            _lookOrientation = spawnRotation;
        }

        public bool TakeDamage(int damage)
        {
            _feedback?.PlayFeedbacks();
            if (_hP - damage <= 0 && this.TryGetComponent<ICanDie>(out ICanDie die))
            {
                die.Die();
                return true;
            }
            else
            {
                _hP -= damage;
            }
            return false;
        }


        private void AnimateQueen(AnimationState state)
        {
            switch (state)
            {
                case AnimationState.Walking:
                    Animator.SetBool("Walking", _isMoving);
                    Animator.SetBool("Spawning", false);
                    break;
                case AnimationState.Spawning:
                    Animator.SetBool("Walking", false);
                    Animator.SetBool("Spawning", true);
                    break;
                case AnimationState.Dead:
                    Animator.SetBool("Walking", false);
                    Animator.SetBool("Spawning", false);
                    Animator.SetBool("Dead", false);

                    break;

            }
        }

        private enum AnimationState
        {
            Idle,
            Walking,
            Spawning,
            Dead
        }
    }
}