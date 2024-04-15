using DG.Tweening;
using CoreCraft.Core;
using System.Collections.Generic;
using UnityEngine;
using CharacterController = CoreCraft.Core.CharacterController;

namespace CoreCraft.LudumDare55
{
    public class Elemental : MonoBehaviour, IDamageable, ICanDie, ICanDamage, ICanSee, IInGrid, IMoveInGrid
    {
        [SerializeField] protected int _sightDistance;
        [SerializeField] protected int _hP;
        [SerializeField] protected int _damage;
        [SerializeField] protected float _moveTime;
        [SerializeField] protected float _attackTime;
        [SerializeField] protected LayerMask _sightLayerMask;
        [SerializeField] protected float _attackCoolDown;
        [SerializeField] protected float _minDistancePlayer;
        [SerializeField] protected float _maxDistancePlayer;
        [SerializeField] protected int _nearPlayerDistance;
        [SerializeField] protected float _buildSpeedBoost;
        [SerializeField] protected float _walkSpeedBoost;

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

        public float BuildingSpeedBoos { get { return _buildSpeedBoost; } }
        public float WalkingSpeedBoost { get { return _walkSpeedBoost; } }

        private float _timer;
        private float _coolDownTimer;
        private bool _nearPlayer;
        private Sequence _moveSequence;
        private Vector2Int _lookOrientation;

        private CharacterController _player;

        void Start()
        {
            _moveSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause();
            if (SummonManager.Instance != null)
                _player = SummonManager.Instance.Player;
            else
                Die();
        }

        void Update()
        {
            _coolDownTimer += Time.deltaTime;

            _nearPlayer = Pathfinding.CalculateDistance(_player.CurrentPosition, _currentPosition) <= _nearPlayerDistance;

            if (!_nearPlayer)
            {
                _targetPath = Pathfinding.StandardAStar(_currentPosition, _player.CurrentPosition, PathfindingMode.Default);
                if (_targetPath == null)
                    return;
            }

            if (_currentEnemy == null)
            {
                if (!_nearPlayer)
                {
                    if (!_isMoving)
                    {
                        _isMoving = true;

                        _lookOrientation = _targetPath.Peek().GridPosition - _currentPosition;

                        transform.DOLookAt(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                        {

                            this.Animator.SetBool("Walking", true);
                            _moveSequence.Append(transform.DOMove(_targetPath.Peek().WorldPosition, _moveTime).OnComplete(() =>
                            {
                                _isMoving = false;

                                this.Animator.SetBool("Walking", false);
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
                        Vector3 newPosition = _player.transform.position + new Vector3(Random.Range(_minDistancePlayer, _maxDistancePlayer), 0, Random.Range(_minDistancePlayer, _maxDistancePlayer));

                        Vector2 normalizedDirection = _player.CurrentPosition - _currentPosition;
                        normalizedDirection.Normalize();

                        _lookOrientation = Vector2Int.RoundToInt(normalizedDirection);

                        _isMoving = true;

                        transform.DOLookAt(newPosition, _moveTime).OnComplete(() =>
                        {

                            this.Animator.SetBool("Walking", true);
                            _moveSequence.Append(transform.DOMove(newPosition, _moveTime).OnComplete(() =>
                            {
                                _isMoving = false;

                                this.Animator.SetBool("Walking", false);
                            }));
                            _currentPosition = _player.CurrentPosition;

                            _moveSequence.PlayForward();
                        });
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
                    _currentEnemy.TakeDamage(_damage);

                    _currentEnemy = null;

                    this.Animator.SetBool("Other", false);
                    this.Animator.SetBool("Walking", false);

                    _timer = 0;
                    _coolDownTimer = 0;
                }
            }
        }

        public void CheckSightCone(Collider other)
        {
            if (_nearPlayer
                && _coolDownTimer >= _attackCoolDown
                && _currentEnemy == null
                && _sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer))
                && other.gameObject.TryGetComponent<IInGrid>(out IInGrid inGrid)
                && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                if (Pathfinding.StraightCheck(_currentPosition, inGrid.CurrentPosition))
                {
                    _currentEnemy = damageable;
                }
            }
        }

        public void DealDamage(IDamageable damageable)
        {
            damageable.TakeDamage(_damage);
        }

        public void Die()
        {
            this.Animator.Play("anim_die");
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