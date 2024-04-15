using CoreCraft.Core;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class RangeCombatEnemy : MonoBehaviour, IDamageable, ICanDie, ICanDamage, ICanSee, IInGrid, IMoveInGrid, IPeripheryTrigger, IGoToWaitingArea
    {
        [SerializeField] protected int _sightDistance;
        [SerializeField] protected int _hP;
        [SerializeField] protected int _damage;
        [SerializeField] protected float _moveTime;
        [SerializeField] protected float _attackTime;
        [SerializeField] protected LayerMask _sightLayerMask;
        [SerializeField] protected bool _canInvert;
        [SerializeField, PropertyRange(0f, 1f)] protected float _invertWeight;
        [SerializeField] List<MeshRenderer> _renderers;
        [SerializeField] Material _invertMaterial;
        [SerializeField] private MMFeedbacks _feedback;
        [SerializeField] private Animator _animator;

        protected Vector2Int _currentPosition;
        protected Vector2Int _targetPosition;
        protected Vector2Int _returnPoint;
        protected bool _hasTarget;
        protected bool _isMoving;
        protected bool _waveStart;
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

        public bool WaveStart => _waveStart;

        private bool _isInverted;
        private int _targetValue;
        private int _enemyValue;
        private float _timer;
        private Sequence _moveSequence;
        private Vector2Int _lookOrientation;

        private bool _goingBackToEntrance;

        protected virtual void Start()
        {
            _moveSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause();

            _isInverted = (Random.Range(0f, 1f) < _invertWeight && _canInvert) ? true : false;
            if (_isInverted)
                foreach (MeshRenderer renderer in _renderers)
                    renderer.material = _invertMaterial;

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

        protected virtual void Update()
        {
            if (!_waveStart)
                return;

            if (_goingBackToEntrance)
                return;

            if (_currentEnemy == null)
            {
                if (!_isMoving)
                {
                    // Roaming
                    if (_isInverted)
                        RoamingDefault();
                    else
                        RoamingInverted();
                }
            }
            else
            {
                _moveSequence.Pause();

                _targetPath.Clear();
                _isMoving = false;
                AnimateRange(AnimationState.Walking);
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

        public virtual void TriggerEnter(Collider other) { }

        public virtual void TriggerExit(Collider other)
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

        public virtual void CheckSightCone(Collider other)
        {
            if (_sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer)) && other.gameObject.TryGetComponent<IInGrid>(out IInGrid inGrid) && other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                if (other.gameObject.layer == 7 && _targetValue < 4)
                {
                    _targetValue = 4;
                }
                else if (other.gameObject.layer == 8 && _targetValue < 3)
                {
                    _targetValue = 3;
                }
                else if (other.gameObject.layer == 10 && _targetValue < 2)
                {
                    _targetValue = 2;
                }
                else if (other.gameObject.layer == 9 && _targetValue < 1)
                {
                    _targetValue = 1;
                }

                if (Pathfinding.StraightCheck(_currentPosition, inGrid.CurrentPosition))
                {
                    _moveSequence.Pause();
                    _isMoving = false;
                    AnimateRange(AnimationState.Walking);
                    _hasTarget = false;
                    _currentEnemy = damageable;
                }
            }
        }

        private void RoamingDefault()
        {
            GridCell cellToTest = null;

            //Test Right
            Vector2Int testDirection = TurnRightVector();
            cellToTest = Grid.Instance.GetCellByIndexWithNull(_currentPosition + testDirection);
            Vector3 rotaion = Vector3.zero;

            if (cellToTest != null && cellToTest.Block.BlockingType == BlockingType.None)
            {
                _isMoving = true;

                transform.DOLookAt(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                {
                    AnimateRange(AnimationState.Walking);
                    _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                    {
                        _isMoving = false;
                        AnimateRange(AnimationState.Walking);
                    }));
                    _currentPosition = cellToTest.GridPosition;

                    _moveSequence.PlayForward();
                    _lookOrientation = testDirection;
                });
                return;
            }

            //Test Straight
            cellToTest = Grid.Instance.GetCellByIndexWithNull(_currentPosition + _lookOrientation);

            if (cellToTest != null && cellToTest.Block.BlockingType == BlockingType.None)
            {
                _isMoving = true;
                AnimateRange(AnimationState.Walking);
                _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                {
                    _isMoving = false;
                    AnimateRange(AnimationState.Walking);
                }));
                _currentPosition = cellToTest.GridPosition;

                _moveSequence.PlayForward();
                return;
            }

            _isMoving = true;
            _lookOrientation = _lookOrientation * -1;

            if (_lookOrientation == Vector2Int.right)
                rotaion = new Vector3(0, 90, 0);
            else if (_lookOrientation == Vector2Int.up)
                rotaion = new Vector3(0, 360, 0);
            else if (_lookOrientation == Vector2Int.left)
                rotaion = new Vector3(0, 270, 0);
            else if (_lookOrientation == Vector2Int.down)
                rotaion = new Vector3(0, 180, 0);

            transform.DORotate(rotaion, _moveTime, RotateMode.FastBeyond360).OnComplete(() =>
            {
                RoamingDefault();
            });
        }

        private void RoamingInverted()
        {
            GridCell cellToTest = null;

            //Test left
            Vector2Int testDirection = TurnLeftVector();
            cellToTest = Grid.Instance.GetCellByIndexWithNull(_currentPosition + testDirection);
            Vector3 rotaion = Vector3.zero;

            if (cellToTest != null && cellToTest.Block.BlockingType == BlockingType.None)
            {
                _isMoving = true;

                transform.DOLookAt(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                {
                    AnimateRange(AnimationState.Walking);
                    _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                    {
                        _isMoving = false;
                        AnimateRange(AnimationState.Walking);
                    }));
                    _currentPosition = cellToTest.GridPosition;

                    _moveSequence.PlayForward();
                    _lookOrientation = testDirection;
                });
                return;
            }

            //Test Straight
            cellToTest = Grid.Instance.GetCellByIndexWithNull(_currentPosition + _lookOrientation);

            if (cellToTest != null && cellToTest.Block.BlockingType == BlockingType.None)
            {
                _isMoving = true;
                AnimateRange(AnimationState.Walking);
                _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                {
                    _isMoving = false;
                    AnimateRange(AnimationState.Walking);
                }));
                _currentPosition = cellToTest.GridPosition;

                _moveSequence.PlayForward();
                return;
            }

            _isMoving = true;
            _lookOrientation = _lookOrientation * -1;

            if (_lookOrientation == Vector2Int.right)
                rotaion = new Vector3(0, 90, 0);
            else if (_lookOrientation == Vector2Int.up)
                rotaion = new Vector3(0, 360, 0);
            else if (_lookOrientation == Vector2Int.left)
                rotaion = new Vector3(0, 270, 0);
            else if (_lookOrientation == Vector2Int.down)
                rotaion = new Vector3(0, 180, 0);

            transform.DORotate(rotaion, _moveTime, RotateMode.FastBeyond360).OnComplete(() =>
            {
                RoamingDefault();
            });
        }

        private Vector2Int TurnRightVector()
        {
            if (_lookOrientation == Vector2Int.right)
                return Vector2Int.down;
            else if (_lookOrientation == Vector2Int.up)
                return Vector2Int.right;
            else if (_lookOrientation == Vector2Int.left)
                return Vector2Int.up;
            else if (_lookOrientation == Vector2Int.down)
                return Vector2Int.left;
            return Vector2Int.zero;
        }

        private Vector2Int TurnLeftVector()
        {
            if (_lookOrientation == Vector2Int.right)
                return Vector2Int.up;
            else if (_lookOrientation == Vector2Int.up)
                return Vector2Int.left;
            else if (_lookOrientation == Vector2Int.left)
                return Vector2Int.down;
            else if (_lookOrientation == Vector2Int.down)
                return Vector2Int.right;
            return Vector2Int.zero;
        }

        public virtual bool TakeDamage(int damage)
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

        public virtual void Die()
        {
            AnimateRange(AnimationState.Dead);
            _moveSequence.Pause();
            _moveSequence.Kill();
            _isMoving = false;
            _hasTarget = false;
            _currentEnemy = null;
            Destroy(this.gameObject, 1f);
        }

        public virtual void DealDamage(IDamageable damageable)
        {
            AnimateRange(AnimationState.Attacking);
            damageable.TakeDamage(_damage);
            _animator.SetBool("Attacking", false);
        }

        public void Spawn(Vector2Int spawnPosition, Vector2Int spawnRotation)
        {
            _currentPosition = spawnPosition;
            _lookOrientation = spawnRotation;
        }

        public IEnumerator GoToSpawnArea(Vector3 position)
        {
            bool done = false;
            _moveSequence.Append(transform.DOMove(position, _moveTime).OnComplete(() =>
            {
                done = true;
            }));
            _moveSequence.PlayForward();
            yield return new WaitUntil(() => done);
        }

        public void WanderAround(Vector3 bottomLeftCorner, Vector3 topRightCorner)
        {
            if (_waveStart == true)
                return;

            Vector3 position = new Vector3(Random.Range(bottomLeftCorner.x, topRightCorner.x), bottomLeftCorner.y, Random.Range(bottomLeftCorner.z, topRightCorner.z));
            _moveSequence.Append(transform.DOMove(position, _moveTime).OnComplete(() =>
            {
                WanderAround(bottomLeftCorner, topRightCorner);
            }));
            _moveSequence.PlayForward();
        }

        public void GoToStartCell()
        {
            GridCell cell = Grid.Instance.GetCellByIndexWithNull(_currentPosition);
            if (cell == null)
                Destroy(gameObject);

            _moveSequence.Append(transform.DOMove(cell.WorldPosition, _moveTime).OnComplete(() =>
            {
                _waveStart = true;

                _moveSequence.Pause();
                _moveSequence.Kill();
                _moveSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause();
            }));
            _moveSequence.PlayForward();
        }

        private void AnimateRange(AnimationState state)
        {
            switch (state)
            {
                case AnimationState.Walking:
                    _animator.SetBool("Walking", _isMoving);
                    _animator.SetBool("Attacking", false);
                    break;
                case AnimationState.Attacking:
                    _animator.SetBool("Attacking", true);
                    _animator.SetBool("Walking", false);
                    break;
                case AnimationState.Dead:
                    _animator.SetBool("Attacking", false);
                    _animator.SetBool("Walking", false);
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