using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class Snail : MonoBehaviour, IDamageable, ICanDie, ICanDamage, ICanSee, IInGrid, IMoveInGrid, IPeripheryTrigger
    {
        [SerializeField] protected int _sightDistance;
        [SerializeField] protected int _hP;
        [SerializeField] protected int _damage;
        [SerializeField] protected float _moveTime;
        [SerializeField] protected float _attackTime;
        [SerializeField] protected LayerMask _sightLayerMask;
        [SerializeField] protected GameObject _mucousObject;
        [SerializeField] protected int _maxMucousTrail;
        [SerializeField] private Animator _animator;

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
        private Queue<Mucous> _mucousTrail = new Queue<Mucous>();

        void Start()
        {
            _moveSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause();
            if (SummonManager.Instance != null)
                SummonManager.Instance.RegisterSnail(this);
            else
                Destroy(gameObject);
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

                        AnimateSnail(AnimationState.Walking);
                        transform.DOLookAt(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                        {
                            
                            _moveSequence.Append(transform.DOMove(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                            {
                                SlimeAround();

                                _isMoving = false;
                                AnimateSnail(AnimationState.Walking);
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
                        GridCell cellToTest = GetCellFurthestFromAnySnail();
                        if (cellToTest == null)
                            return;

                        Vector2 normalizedDirection = cellToTest.GridPosition - _currentPosition;
                        normalizedDirection.Normalize();

                        _lookOrientation = Vector2Int.RoundToInt(normalizedDirection);

                        if (cellToTest != null && cellToTest.Block.BlockingType == BlockingType.None)
                        {
                            _isMoving = true;
                            AnimateSnail(AnimationState.Walking);
                            transform.DOLookAt(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                            {
                               
                                _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                                {
                                    SlimeAround();

                                    _isMoving = false;

                                    AnimateSnail(AnimationState.Walking);
                                }));
                                _currentPosition = cellToTest.GridPosition;

                                _moveSequence.PlayForward();
                            });
                            return;
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
                AnimateSnail(AnimationState.Walking);

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

        private void SlimeAround()
        {
            if(_mucousTrail.Count >= _maxMucousTrail)
            {
                Mucous mucous = _mucousTrail.Dequeue();
                if(mucous != null)
                    mucous.Despawn();
            }

            _mucousTrail.Enqueue(Instantiate(_mucousObject, Grid.Instance.GetCellByIndex(_currentPosition).WorldPosition, Quaternion.identity).GetComponent<Mucous>());
        }

        private GridCell GetCellFurthestFromAnySnail()
        {
            List<GridCell> neighbours = Pathfinding.GetNeighbour(_currentPosition);

            if (neighbours == null || neighbours.Count == 0)
                return null;

            int distance = int.MaxValue;
            GridCell returnCell = neighbours[Random.Range(0, neighbours.Count)];

            IInGrid nearestSnail = null;
            if (SummonManager.Instance.SnailList.Count <= 0)
                return returnCell;

            foreach (IInGrid snail in SummonManager.Instance.SnailList.Where(s => 10 > Pathfinding.CalculateDistance(_currentPosition, s.CurrentPosition)))
            {
                int i = Pathfinding.CalculateDistance(_currentPosition, snail.CurrentPosition);
                if (i < distance)
                {
                    distance = i;
                    nearestSnail = snail;
                }
            }

            distance = 0;
            foreach (GridCell cell in neighbours)
            {
                int i = Pathfinding.CalculateDistance(cell.GridPosition, nearestSnail.CurrentPosition);
                if (i > distance)
                {
                    distance = i;
                    returnCell = cell;
                }
                else if (i == distance)
                {
                    int j = Random.Range(0, 2);
                    if (j > 0)
                        returnCell = cell;
                }

            }

            return returnCell;
        }

        public void TriggerEnter(Collider other)
        {
            if (_currentEnemy == null && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                _moveSequence.Pause();
                _isMoving = false;
                _hasTarget = false;
                _currentEnemy = damageable;
            }
        }

        public void TriggerExit(Collider other)
        {
            if (_currentEnemy != null && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
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
                if (Pathfinding.StraightCheck(_currentPosition, inGrid.CurrentPosition))
                {
                    _targetPosition = inGrid.CurrentPosition;

                    _targetPath = Pathfinding.StandardAStar(_currentPosition, _targetPosition, PathfindingMode.Default);

                    _hasTarget = _targetPath != null && _targetPath.Count > 0;
                }
            }
        }

        public void DealDamage(IDamageable damageable)
        {
            AnimateSnail(AnimationState.Attacking);
            damageable.TakeDamage(_damage);
            _animator.SetBool("Attacking", false);
        }

        [Button("Debug Die")]
        public void Die()
        {
            AnimateSnail(AnimationState.Dead);
            _moveSequence.Kill();
            _isMoving = false;
            _hasTarget = false;
            _currentEnemy = null;
            Destroy(this.gameObject,1f);
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

        private void AnimateSnail(AnimationState state)
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