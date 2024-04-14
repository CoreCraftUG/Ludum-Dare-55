using DG.Tweening;
using ES3Types;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    [RequireComponent(typeof(Collider), typeof(Animator))]
    public class BaseAICharacter : MonoBehaviour , IDamageable , ICanDie , ICanDamage, ICanSee, IInGrid, IMoveInGrid
    {
        [SerializeField] protected int _sightDistance;
        [SerializeField] protected int _hP;
        [SerializeField] protected int _damage;
        [SerializeField] protected float _moveTime;
        [SerializeField] protected float _attackTime;
        [SerializeField] protected LayerMask _sightLayerMask;
        [SerializeField] protected bool _canInvert;
        [SerializeField, MinMaxSlider(0f, 1f)] protected float _invertWeight;

        protected Vector2Int _currentPosition;
        protected Vector2Int _targetPosition;
        protected Vector2Int _returnPoint;
        protected bool _hasTarget;
        protected bool _isMoving;
        protected Stack<GridCell> _targetPath = new Stack<GridCell>();

        protected IDamageable _currentEnemy;

        public Vector2Int CurrentPosition { get { return _currentPosition; } }
        public int SightDistance { get { return _sightDistance; } }
        public LayerMask SightLayerMask { get { return _sightLayerMask; } }

        public int HP { get => _hP; }
        public int Damage { get => _damage; }
        public Animator Animator { get => GetComponent<Animator>(); }

        Vector2Int IMoveInGrid.TargetPosition => _targetPosition;
        Vector2Int IMoveInGrid.ReturnPoint => _returnPoint;
        public bool HasTarget => _hasTarget;
        public bool IsMoving => _isMoving;
        public Stack<GridCell> TargetPath => _targetPath;
        public float MoveTime => _moveTime;
        public float AttackTime => _attackTime;

        private bool _isInverted;
        private float _timer;
        private Sequence _moveSequence;
        private Vector2Int _lookOrientation;

        protected virtual void Start()
        {
            _moveSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause();

            _isInverted = (Random.Range(0, 1) < _invertWeight && _canInvert) ? true : false;
        }

        protected virtual void Update()
        {
            if (_currentEnemy == null)
            {
                if (_hasTarget)
                {
                    if (!_isMoving)
                    {
                        _isMoving = true;

                        _moveSequence.Append(transform.DOMove(_targetPath.Pop().WorldPosition, _moveTime).OnComplete(() =>
                        {
                            _isMoving = false;
                        }));

                        _moveSequence.PlayForward();
                    }
                    _hasTarget = _targetPath.Count > 0;
                }
                else
                {
                    if (!_isMoving)
                    {
                        // Roaming
                        if (_isInverted)
                            RoamingInverted();
                        else
                            RoamingDefault();
                    }
                }
            }
            else
            {
                _moveSequence.Kill();

                _targetPath.Clear();
                _isMoving = false;
                _hasTarget = false;

                _timer += Time.deltaTime;
                if (_timer >= _attackTime)
                {
                    _currentEnemy = _currentEnemy.TakeDamage(_damage) ? null : _currentEnemy;
                    _timer = 0;
                }
            }
        }

        private void RoamingDefault()
        {
            GridCell cellToTest = null;

            //Test Right
            cellToTest = Grid.Instance.GetCellByIndex(_currentPosition + TurnRightVector());

            if (cellToTest != null && cellToTest.Block.BlockingType == BlockingType.None)
            {

            }

            //Test Straight


            _isMoving = true;

            _moveSequence.Append(transform.DOMove(_targetPath.Pop().WorldPosition, _moveTime).OnComplete(() =>
            {
                _isMoving = false;
            }));

            _moveSequence.PlayForward();
        }

        private void RoamingInverted()
        {

        }

        private Vector2Int TurnRightVector()
        {
            if(_lookOrientation == Vector2Int.right)
                return Vector2Int.down;
            else if(_lookOrientation == Vector2Int.up)
                return Vector2Int.right;
            else if(_lookOrientation == Vector2Int.left)
                return Vector2Int.up;
            else if( _lookOrientation == Vector2Int.down)
                return Vector2Int.left;
            return Vector2Int.zero;
        }

        private Vector2Int TurnLeftVector()
        {
            if(_lookOrientation == Vector2Int.right)
                return Vector2Int.up;
            else if(_lookOrientation == Vector2Int.up)
                return Vector2Int.left;
            else if(_lookOrientation == Vector2Int.left)
                return Vector2Int.down;
            else if( _lookOrientation == Vector2Int.down)
                return Vector2Int.right;
            return Vector2Int.zero;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if(_currentEnemy != null && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                _currentEnemy = damageable;
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (_currentEnemy != null && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                if (_currentEnemy == damageable)
                    _currentEnemy = null;
            }
        }

        public virtual void CheckSightCone(Collider other)
        {
            if(_sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer)) && other.gameObject.TryGetComponent<IInGrid>(out IInGrid inGrid))
            {
                if (AStar.StraightCheck(_currentPosition, inGrid.CurrentPosition))
                {
                    _targetPosition = inGrid.CurrentPosition;

                    _targetPath = AStar.StandardAStar(_currentPosition, _targetPosition, PathfindingMode.Default);

                    _hasTarget = _targetPath != null && _targetPath.Count > 0;
                }
            }
        }

        public virtual bool TakeDamage(int damage)
        {
            if (_hP - damage <= 0 && this.TryGetComponent<ICanDie>(out ICanDie die))
            {
                die.Die();
                return true;
            }
            return false;
        }

        public virtual void Die()
        {
            throw new System.NotImplementedException();
        }

        public virtual void DealDamage(IDamageable damageable)
        {
            damageable.TakeDamage(_damage);
        }

        public void Spawn(Vector2Int spawnPosition)
        {
            _currentPosition = spawnPosition;
        }

        [Button("Debug Move Forward")]
        private void DebugMoveForward()
        {
            transform.DOMove(transform.position + transform.forward, 2f);
        }
    }
}