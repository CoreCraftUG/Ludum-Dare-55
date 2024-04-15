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
    public class Gargoyle : MonoBehaviour, ICanDie, ICanDamage, ICanSee, IInGrid, IMoveInGrid
    {
        [SerializeField] protected int _sightDistance;
        [SerializeField] protected int _hP;
        [SerializeField] protected int _damage;
        [SerializeField] protected float _moveTime;
        [SerializeField] protected float _attackTime;
        [SerializeField] protected LayerMask _sightLayerMask;
        [SerializeField] private Animator _animator;
        [SerializeField] private int _attackHPLoss;


        protected Vector2Int _currentPosition;
        protected Vector2Int _targetPosition;
        protected Vector2Int _returnPoint;
        protected bool _hasTarget;
        protected bool _isMoving;
        protected Stack<GridCell> _targetPath = new Stack<GridCell>();
        [SerializeField] private MMFeedbacks _feedback;

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

        private bool _isSitting;
        private AlchemyTable _currentTable;

        void Start()
        {
            _moveSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause();
            if (SummonManager.Instance != null)
                SummonManager.Instance.RegisterSnail(this);
            else
                Destroy(gameObject);

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
            else
            {
                _currentPosition = new Vector2Int(_currentPosition.x, _currentPosition.y + moveIncrements);
            }
        }

        void Update()
        {
            if (_goingBackToEntrance)
                return;

            if( SummonManager.Instance.SummoningTables.Count > 0  && (_currentTable == null || _currentTable != null && !SummonManager.Instance.SummoningTables.Contains(_currentTable)))
            {
                int distance = int.MaxValue;
                Vector2Int index = _currentPosition;
                foreach (AlchemyTable table in SummonManager.Instance.SummoningTables)
                {
                    int i = Pathfinding.CalculateDistance(_currentPosition, table.CurrentPosition);
                    if (i < distance)
                    {
                        distance = i;
                        index = table.CurrentPosition;
                        _currentTable = table;
                    }
                }

                List<GridCell> NeighbouringCells = new List<GridCell>() { Grid.Instance.GetCellByIndexWithNull(index) };
                int j = 0;
                while ( NeighbouringCells.Count - 1 >= j )
                {
                    if (NeighbouringCells[j].Block.BlockingType != BlockingType.None)
                    {
                        index = NeighbouringCells[j].GridPosition;
                    }
                    j++;
                    if(NeighbouringCells.Count - 1 < j)
                    {
                        foreach (GridCell cell in NeighbouringCells)
                        {
                            foreach (GridCell cell2 in Pathfinding.GetNeighbour(cell.GridPosition))
                                if (!NeighbouringCells.Contains(cell))
                                    NeighbouringCells.Add(cell);
                        }
                    }
                }

                _targetPath = Pathfinding.StandardAStar(_currentPosition, index, PathfindingMode.Gargoyle);

                if (_targetPath != null && _targetPath.Count > 0)
                {
                    _hasTarget = true;
                }
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
                            AnimateGargoyle(AnimationState.Walking);
                            _moveSequence.Append(transform.DOMove(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                            {
                                _isMoving = false;
                                AnimateGargoyle(AnimationState.Walking);
                                _hasTarget = _targetPath.Count > 0;
                                _isSitting = _targetPath.Count <= 0;
                            }));
                            _currentPosition = _targetPath.Pop().GridPosition;

                            _moveSequence.PlayForward();
                        });
                    }
                }
            }
            else if(_isSitting)
            {
                _moveSequence.Pause();

                _targetPath.Clear();
                _isMoving = false;
                AnimateGargoyle(AnimationState.Walking);

                _timer += Time.deltaTime;
                if (_timer >= _attackTime)
                {
                    if (_currentEnemy.TakeDamage(_damage))
                    {
                        _currentEnemy = null;
                    }
                    _hP -= _attackHPLoss;
                    if (_hP <= 0)
                        Die();

                    _timer = 0;
                }
            }
        }

        public void CheckSightCone(Collider other)
        {
            if (_isSitting && _currentEnemy == null && _sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer)) && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                _currentEnemy = damageable;
            }
        }

        public void DealDamage(IDamageable damageable)
        {
            AnimateGargoyle(AnimationState.Attacking);
            damageable.TakeDamage(_damage);
            _animator.SetBool("Attacking", false);
        }

        [Button("Debug Die")]
        public void Die()
        {
            AnimateGargoyle(AnimationState.Dead);
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

        private void AnimateGargoyle(AnimationState state)
        {
            switch (state)
            {
                case AnimationState.Walking:
                    _animator.SetBool("Walking", _isMoving);
                    _animator.SetBool("Attacking", false);
                    break;
                case AnimationState.Attacking:
                    _animator.SetBool("Walking", false);
                    _animator.SetBool("Attacking", true);
                    break;
                case AnimationState.Dead:
                    _animator.SetBool("Walking", false);
                    _animator.SetBool("Attacking", false);
                    _animator.SetBool("Dead", false);

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