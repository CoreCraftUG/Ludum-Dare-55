using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class Snail : MonoBehaviour, IDamageable, ICanDie, ICanDamage, ICanSee, IInGrid, IMoveInGrid
    {
        [SerializeField] protected int _sightDistance;
        [SerializeField] protected int _hP;
        [SerializeField] protected int _damage;
        [SerializeField] protected float _moveTime;
        [SerializeField] protected float _attackTime;
        [SerializeField] protected LayerMask _sightLayerMask;

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
        public Animator Animator => GetComponent<Animator>();

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
                            _moveSequence.Append(transform.DOMove(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                            {
                                _isMoving = false;
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
                        _enemyValue = 0;
                    }

                    _timer = 0;
                }
            }
        }

        private GridCell GetCellFurthestFromAnySnail()
        {
            List<GridCell> neighbours = AStar.GetNeighbour(_currentPosition);

            if (neighbours == null || neighbours.Count == 0)
                return null;

            int distance = 0;
            GridCell returnCell = null;
            foreach (IInGrid snail in SummonManager.Instance.SnailList)
            {
                foreach (GridCell cell in neighbours)
                {
                    int i = AStar.CalculateDistance(cell.GridPosition, snail.CurrentPosition);
                    if(i > distance)
                    {
                        distance = i;
                        returnCell = cell;
                    }
                }
            }

            return returnCell;
        }

        private void OnTriggerEnter(Collider other)
        {
            
        }

        private void OnTriggerExit(Collider other)
        {
            
        }

        public void CheckSightCone(Collider other)
        {
            throw new System.NotImplementedException();
        }

        public void DealDamage(IDamageable damageable)
        {
            throw new System.NotImplementedException();
        }

        public void Die()
        {
            Destroy(this.gameObject);
        }

        public void Spawn(Vector2Int spawnPosition, Vector2Int spawnRotation)
        {
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
