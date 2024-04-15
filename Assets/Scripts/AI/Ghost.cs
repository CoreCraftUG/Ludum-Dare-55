using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace CoreCraft.LudumDare55
{
    public class Ghost : MonoBehaviour, IDamageable, ICanDie, ICanSee, IInGrid, IMoveInGrid, IPeripheryTrigger
    {
        [SerializeField] protected int _sightDistance;
        [SerializeField] protected int _hP;
        [SerializeField] protected float _moveTime;
        [SerializeField] protected float _rushMoveTime;
        [SerializeField] protected LayerMask _sightLayerMask;
        [SerializeField] protected GameObject _crystalPrefab;
        [SerializeField] private MMFeedbacks _feedback;

        protected Vector2Int _currentPosition;
        protected Vector2Int _targetPosition;
        protected Vector2Int _returnPoint;
        protected bool _hasTarget;
        protected bool _isMoving;
        protected Stack<GridCell> _targetPath = new Stack<GridCell>();

        protected ICanDie _currentEnemy;

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

        void Start()
        {
            _moveSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause();
        }

        void Update()
        {
            if (_currentEnemy == null)
            {
                if (_hasTarget)
                {
                    if (!_isMoving)
                    {
                        _isMoving = true;

                        _lookOrientation = _targetPath.Peek().GridPosition - _currentPosition;

                        transform.DOLookAt(_targetPath.Peek().WorldPosition, _rushMoveTime).OnComplete(() =>
                        {

                            AnimateGhost(AnimationState.Walking);
                            _moveSequence.Append(transform.DOMove(_targetPath.Peek().WorldPosition, _rushMoveTime).OnComplete(() =>
                            {
                                _isMoving = false;

                                AnimateGhost(AnimationState.Walking);
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
                        GridCell cellToTest = Grid.Instance.GetCellByIndexWithNull(_currentPosition + _lookOrientation);
                        if (cellToTest == null || cellToTest.Block.BlockingType != BlockingType.None)
                        {
                            List<GridCell> neighbours = Pathfinding.GetNeighbour(_currentPosition).Where(n => n.Block.BlockingType != BlockingType.None).ToList();

                            if (neighbours == null || neighbours.Count == 0)
                                return;

                            cellToTest = neighbours[Random.Range(0, neighbours.Count)];
                        }

                        Vector2 normalizedDirection = cellToTest.GridPosition - _currentPosition;
                        normalizedDirection.Normalize();

                        _lookOrientation = Vector2Int.RoundToInt(normalizedDirection);

                        _isMoving = true;

                        transform.DOLookAt(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                        {

                            AnimateGhost(AnimationState.Walking);
                            _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                            {
                                _isMoving = false;

                                AnimateGhost(AnimationState.Walking);
                            }));
                            _currentPosition = cellToTest.GridPosition;

                            _moveSequence.PlayForward();
                        });
                    }
                }
            }
            else if(_hasTarget)
            {
                AnimateGhost(AnimationState.Attacking);
                _moveSequence.Pause();

                _targetPath.Clear();
                _isMoving = false;
                _hasTarget = false;

                _currentEnemy.Die();
                GameObject temp = MonoBehaviour.Instantiate(_crystalPrefab, Grid.Instance.GetCellByIndexWithNull(_currentPosition).WorldPosition, new Quaternion(0, 0, 0, 0));
                temp.GetComponent<Resource>().PosCell = _currentPosition;
                Die();
            }
        }

        public void TriggerEnter(Collider other)
        {
            if (_currentEnemy == null && other.gameObject.TryGetComponent<ICanDie>(out ICanDie damageable))
            {
                _moveSequence.Pause();
                _isMoving = false;
                _currentEnemy = damageable;
            }
        }

        public void TriggerExit(Collider other)
        {
            if (_currentEnemy != null && other.gameObject.TryGetComponent<ICanDie>(out ICanDie damageable))
            {
                if (_currentEnemy == damageable)
                {
                    _currentEnemy = null;
                }
            }
        }

        public void CheckSightCone(Collider other)
        {
            if (!_hasTarget && _sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer)) && other.gameObject.TryGetComponent<IInGrid>(out IInGrid inGrid))
            {
                _targetPosition = inGrid.CurrentPosition;

                _targetPath = Pathfinding.StandardAStar(_currentPosition, _targetPosition, PathfindingMode.NoWalls);

                _hasTarget = _targetPath != null && _targetPath.Count > 0;
            }
        }

        [Button("Debug Die")]
        public void Die()
        {
            AnimateGhost(AnimationState.Dead);
            _moveSequence.Kill();
            _isMoving = false;
            _hasTarget = false;
            _currentEnemy = null;
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


        private void AnimateGhost(AnimationState state)
        {
            switch (state)
            {
                case AnimationState.Walking:
                    Animator.SetBool("Walking", _isMoving);
                    Animator.SetBool("Attacking", false);
                    break;
                case AnimationState.Attacking:
                    Animator.SetBool("Walking", false);
                    Animator.SetBool("Attacking", true);
                    break;
                case AnimationState.Dead:
                    Animator.SetBool("Walking", false);
                    Animator.SetBool("Attacking", false);
                    Animator.SetBool("Dead", false);

                    break;

            }
        }

        private enum AnimationState
        {
            Idle,
            Walking,
            Attacking,
            Dead
        }
    }
}
