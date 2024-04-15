using DG.Tweening;
using MoreMountains.Feel;
using Sirenix.OdinInspector;
using System.Collections;
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

        private bool _isInverted;
        private int _targetValue;
        private int _enemyValue;
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

                        Vector3 rotaion = Vector3.zero;

                        if (_targetPath.Peek().GridPosition - _currentPosition == Vector2Int.right)
                            rotaion = new Vector3(0, 180, 0);
                        else if (_targetPath.Peek().GridPosition - _currentPosition == Vector2Int.up)
                            rotaion = new Vector3(0, 90, 0);
                        else if (_targetPath.Peek().GridPosition - _currentPosition == Vector2Int.left)
                            rotaion = new Vector3(0, 360, 0);
                        else if (_targetPath.Peek().GridPosition - _currentPosition == Vector2Int.down)
                            rotaion = new Vector3(0, 270, 0);

                        _lookOrientation = _targetPath.Peek().GridPosition - _currentPosition;

                        transform.DOLookAt(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                        {
                            
                            this.Animator.SetBool("Walking", true);
                            _moveSequence.Append(transform.DOMove(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                            {
                                SlimeAround();

                                _isMoving = false;
                                
                                this.Animator.SetBool("Walking", false);
                                _hasTarget = _targetPath.Count > 0;
                                if (!_hasTarget)
                                    _targetValue = 0;
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

                            transform.DOLookAt(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                            {
                                
                                this.Animator.SetBool("Walking", true);
                                _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                                {
                                    SlimeAround();

                                    _isMoving = false;
                                    
                                    this.Animator.SetBool("Walking", false);
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
                this.Animator.SetBool("Other", true);
                this.Animator.SetBool("Walking", false);
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
                        _enemyValue = 0;
                        
                        this.Animator.SetBool("Other", false);
                        this.Animator.SetBool("Walking", false);
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
            if (_currentEnemy != null && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
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
                    _enemyValue = 0;
                }
            }
        }

        public void CheckSightCone(Collider other)
        {
            if (_sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer)) && other.gameObject.TryGetComponent<IInGrid>(out IInGrid inGrid))
            {
                if (Pathfinding.StraightCheck(_currentPosition, inGrid.CurrentPosition))
                {
                    _targetPosition = inGrid.CurrentPosition;

                    _targetPath = Pathfinding.StandardAStar(_currentPosition, _targetPosition, PathfindingMode.Default);

                    _hasTarget = _targetPath != null && _targetPath.Count > 0;
                    if (!_hasTarget)
                        _targetValue = 0;
                }
            }
        }

        public void DealDamage(IDamageable damageable)
        {
            damageable.TakeDamage(_damage);
        }

        [Button("Debug Die")]
        public void Die()
        {
            this.Animator.Play("anim_die");
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
    }
}