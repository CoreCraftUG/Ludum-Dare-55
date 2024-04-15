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
    public class Simp : MonoBehaviour, IDamageable, ICanDie, ICanDamage, ICanSee, IInGrid, IMoveInGrid, IPeripheryTrigger
    {
        [SerializeField] protected int _sightDistance;
        [SerializeField] protected int _hP;
        [SerializeField] protected int _damage;
        [SerializeField] protected float _moveTime;
        [SerializeField] protected float _rushMoveTime;
        [SerializeField] protected float _attackTime;
        [SerializeField] protected LayerMask _sightLayerMask;
        [SerializeField] private Animator _animator;
        [SerializeField] private MMFeedbacks _feedback;
        [SerializeField] protected float _minDistanceQueen;
        [SerializeField] protected float _maxDistanceQueen;
        [SerializeField] protected int _nearQueenDistance;

        protected Vector2Int _currentPosition;
        protected Vector2Int _targetPosition;
        protected Vector2Int _returnPoint;
        protected bool _hasTarget;
        protected bool _isMoving;
        protected Stack<GridCell> _targetPath = new Stack<GridCell>();

        protected IDamageable _currentEnemy;

        //Damageable
        public int HP => _hP;

        //Can Die
        public Animator Animator => GetComponentInChildren<Animator>();

        //Can Damage
        public int Damage => _damage;
        public float AttackTime => _attackTime;

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

        private InsectQueen _myQueen;
        private bool _nearQueen;

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

            if (_myQueen == null)
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
                        AnimateSimp(AnimationState.Walking);
                        _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                        {
                            _isMoving = false;

                            AnimateSimp(AnimationState.Walking);
                        }));
                        _currentPosition = cellToTest.GridPosition;

                        _moveSequence.PlayForward();
                    });
                }
                return;
            }

            _nearQueen = Pathfinding.CalculateDistance(_myQueen.CurrentPosition, _currentPosition) <= _nearQueenDistance;

            if (!_nearQueen)
            {
                _targetPath = Pathfinding.StandardAStar(_currentPosition, _myQueen.CurrentPosition, PathfindingMode.Default);
                if (_targetPath == null)
                    return;
            }

            if (_currentEnemy == null)
            {
                if (_hasTarget)
                {
                    if (!_isMoving)
                    {
                        _isMoving = true;

                        _lookOrientation = _targetPath.Peek().GridPosition - _currentPosition;

                        transform.DOLookAt(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                        {
                            AnimateSimp(AnimationState.Walking);
                            _moveSequence.Append(transform.DOMove(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                            {
                                _isMoving = false;

                                AnimateSimp(AnimationState.Walking);
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
                        if (!_nearQueen)
                        {
                            _isMoving = true;

                            _lookOrientation = _targetPath.Peek().GridPosition - _currentPosition;

                            transform.DOLookAt(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                            {
                                AnimateSimp(AnimationState.Walking);
                                _moveSequence.Append(transform.DOMove(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                                {
                                    _isMoving = false;

                                    AnimateSimp(AnimationState.Walking);
                                }));
                                _currentPosition = _targetPath.Pop().GridPosition;

                                _moveSequence.PlayForward();
                            });
                        }
                        else
                        {
                            Vector3 newPosition = _myQueen.transform.position + new Vector3(Random.Range(_minDistanceQueen, _maxDistanceQueen), 0, Random.Range(_minDistanceQueen, _maxDistanceQueen));

                            Vector2 normalizedDirection = _myQueen.CurrentPosition - _currentPosition;
                            normalizedDirection.Normalize();

                            _lookOrientation = Vector2Int.RoundToInt(normalizedDirection);

                            _isMoving = true;

                            transform.DOLookAt(newPosition, _moveTime).OnComplete(() =>
                            {
                                AnimateSimp(AnimationState.Walking);

                                _moveSequence.Append(transform.DOMove(newPosition, _moveTime).OnComplete(() =>
                                {
                                    _isMoving = false;
                                    AnimateSimp(AnimationState.Walking);
                                }));
                                _currentPosition = _myQueen.CurrentPosition;

                                _moveSequence.PlayForward();
                            });
                        }
                    }
                }
            }
            else
            {
                _moveSequence.Pause();

                _targetPath.Clear();
                _isMoving = false;
                _hasTarget = false;

                _timer += Time.deltaTime;
                if (_timer >= _attackTime)
                {
                    if (_currentEnemy.TakeDamage(_damage))
                    {
                        _currentEnemy = null;
                    }

                    _timer = 0;
                }
            }
        }

        public void CallToAttack(IDamageable damageable)
        {
            if (damageable != null)
            {
                _currentEnemy = damageable;
            }
        }

        public void TriggerEnter(Collider other)
        {
            if (!_hasTarget && other.gameObject.TryGetComponent<IDamageable>(out IDamageable inGrid))
            {
                _moveSequence.Pause();
                _isMoving = false;
                _hasTarget = false;
                _currentEnemy = inGrid;
            }
        }

        public void TriggerExit(Collider other)
        {
            if (_currentEnemy != null && other.gameObject.TryGetComponent<IDamageable>(out IDamageable inGrid))
            {
                if (_currentEnemy == inGrid)
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

        public void DealDamage(IDamageable damageable)
        {
            AnimateSimp(AnimationState.Attacking);
            damageable.TakeDamage(_damage);
            //_animator.SetBool("Attacking", false);
        }

        [Button("Debug Die")]
        public void Die()
        {
            AnimateSimp(AnimationState.Dead);
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


        private void AnimateSimp(AnimationState state)
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

        public void GetQueen(InsectQueen queen)
        {
            _myQueen = queen;
        }
    }
}
