using BehaviorDesigner.Runtime.Tasks.Unity.UnityLayerMask;
using CoreCraft.Core;
using DG.Tweening;
using ES3Types;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoreCraft.LudumDare55
{
    [RequireComponent(typeof(Animator))]
    public class BaseAICharacter : MonoBehaviour , IDamageable , ICanDie , ICanDamage, ICanSee, IInGrid, IMoveInGrid, IPeripheryTrigger, IGoToWaitingArea
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
                        if (_isInverted)
                            RoamingInverted();
                        else
                            RoamingDefault();
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

        public virtual void TriggerEnter(Collider other)
        {

            if (other.gameObject.layer == 7 && _enemyValue < 4)
            {
                _enemyValue = 4;
            }
            else if (other.gameObject.layer == 8 && _enemyValue < 3)
            {
                _enemyValue = 3;
            }
            else if (other.gameObject.layer == 10 && _enemyValue < 2)
            {
                _enemyValue = 2;
            }
            else if (other.gameObject.layer == 9 && _enemyValue < 1)
            {
                _enemyValue = 1;
            }
            else
            {
                return;
            }


            if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                _moveSequence.Pause();
                _isMoving = false;
                _hasTarget = false;
                _currentEnemy = damageable;
            }
        }

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
            if (_sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer)) && other.gameObject.TryGetComponent<IInGrid>(out IInGrid inGrid))
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
                    _targetPosition = inGrid.CurrentPosition;

                    _targetPath = Pathfinding.StandardAStar(_currentPosition, _targetPosition, PathfindingMode.Default);

                    _hasTarget = _targetPath != null && _targetPath.Count > 0;
                    if (!_hasTarget)
                        _targetValue = 0;
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
                    _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                    {
                        _isMoving = false;
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

                _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                {
                    _isMoving = false;
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
                    _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                    {
                        _isMoving = false;
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

                _moveSequence.Append(transform.DOMove(cellToTest.WorldPosition, _moveTime).OnComplete(() =>
                {
                    _isMoving = false;
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

        public virtual bool TakeDamage(int damage)
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

        public virtual void Die()
        {
            _moveSequence.Pause();
            _moveSequence.Kill();
            _isMoving = false;
            _hasTarget = false;
            _currentEnemy = null;
            Destroy(this.gameObject,1f);
        }

        public virtual void DealDamage(IDamageable damageable)
        {
            damageable.TakeDamage(_damage);
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
            Vector3 position = new Vector3(Random.Range(bottomLeftCorner.x,topRightCorner.x), bottomLeftCorner.y, Random.Range(bottomLeftCorner.z, topRightCorner.z));
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

        #region Debug
#if UNITY_EDITOR

        [Button("Debug Move Forward"), FoldoutGroup("Debug")]
        private void DebugMoveForward()
        {
            transform.DOMove(transform.position + transform.forward, 2f);
        }


        [SerializeField, FoldoutGroup("Debug")] private Vector2Int _debugTargetPosition;
        [Button("Debug Set Target"), FoldoutGroup("Debug")]
        private void DebugSetTarget()
        {
            _targetPosition = _debugTargetPosition;

            _targetPath = Pathfinding.StandardAStar(_currentPosition, _targetPosition, PathfindingMode.Default);

            _hasTarget = _targetPath != null && _targetPath.Count > 0;
        }

#endif
        #endregion
    }
}