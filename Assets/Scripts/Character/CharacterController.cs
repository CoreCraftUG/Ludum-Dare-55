using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CoreCraft.LudumDare55;
using Grid = CoreCraft.LudumDare55.Grid;
using DG.Tweening;
using System.Linq;


namespace CoreCraft.Core
{
    public class CharacterController : MonoBehaviour, IInGrid, IMoveInGrid, IDamageable, ICanDie
    {
        [SerializeField] private Grid grid;
        [SerializeField] private GameObject _carriedResource;
        [SerializeField] private GameObject _tempTable;
        [SerializeField] private GameObject _alchemyTable;
        private Sequence _moveSequence;
        private Stack<GridCell> _currentPath;
        private bool _isMoving;
        [SerializeField] private float _moveTime;
        [SerializeField] private float _adaptiveMoveTime;
        [SerializeField] private float _turnTime;
        private Vector2Int _targetPosition;
        private bool _hasTarget;
        [SerializeField] private int _hp;
        [SerializeField] private Animator _animator;
        private float _buildTimer;
        private float _activeBuildTimer;
        private bool _timerActive;
        [SerializeField] private float _baseBuildtimer;
        [SerializeField] private float _minimumBuildTime;
        private Block _tempBlock;
        [SerializeField] Block _blockingBlock;
        [SerializeField] private float _maxMoveSpeed;

        public Vector2Int CurrentPosition => _currentPosition;

        public float MoveTime => _moveTime;

        public Vector2Int TargetPosition => _targetPosition;

        public Vector2Int ReturnPoint => throw new System.NotImplementedException();

        public bool HasTarget => throw new System.NotImplementedException();

        public bool IsMoving => _isMoving;

        public Stack<GridCell> TargetPath => _currentPath;

        public int HP => _hp;

        public Animator Animator => _animator;

        private Vector2Int _currentPosition;

        [SerializeField] private LayerMask _resourceLayer;


        private AnimationState _animState;
        private void Awake()
        {
            GameInputManager.Instance.OnRightClick += RightClick;
            GameInputManager.Instance.OnLeftClick += LeftClick;
            _tempTable = null;
            _isMoving = false;
            _currentPath = null;
            _animState = AnimationState.Idle;
            _moveSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause();
            _timerActive = false;
            _buildTimer = _baseBuildtimer;
            _adaptiveMoveTime = _moveTime;
        }

        private void Start()
        {
            _currentPosition = grid.GetCellByDirection(transform.position).GridPosition;
        }

        private void LeftClick(object sender, System.EventArgs e)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            RaycastHit hit2;
            if (Physics.Raycast(ray, out hit, 1000))
            {
                //Debug.DrawRay(Camera.main.transform.position, hit.transform.forward, Color.green, 10);
                Debug.Log(hit);
                Debug.DrawLine(Camera.main.transform.position, hit.point, Color.green);
                GridCell cell = grid.GetCellByDirection(hit.point);
                if (cell.Block.Material != BlockMaterial.None)
                {
                    grid.MineCell(cell);
                    if (_carriedResource == null)
                        return;
                }

                if (_carriedResource != null)
                {
                    GameObject temp = _carriedResource;
                    _carriedResource = null;
                    temp.GetComponent<BoxCollider>().enabled = true;
                    temp.transform.DOMove(cell.WorldPosition, .1f);
                    temp.GetComponent<Resource>().PosCell = cell.GridPosition;
                    return;

                }                
            }
            if (Physics.Raycast(ray, out hit2, 1000,_resourceLayer))
            {
                _carriedResource = hit2.transform.gameObject;
                _carriedResource.transform.DOMove(Input.mousePosition, .1f);
                _carriedResource.GetComponent<BoxCollider>().enabled = false;
            }
        }

        private void Update()
        {
            if (_carriedResource != null)
                _carriedResource.transform.position = Input.mousePosition;

            if (_timerActive)
            {
                Debug.Log(_timerActive);
                if(_activeBuildTimer > 0)
                {
                    _activeBuildTimer -= Time.deltaTime;
                }
                else
                {
                    GameObject temp = _tempTable;
                    grid.UnblockCell(grid.GetCellByDirection(temp.transform.position).GridPosition);
                    grid.MineCell(grid.GetCellByDirection(temp.transform.position));
                    temp.GetComponent<AlchemyTable>().Activate();
                    _tempTable = null;
                    _tempBlock = null;
                    AnimateCharacter(AnimationState.Idle);
                    _timerActive = false;
                    _activeBuildTimer = _buildTimer;
                }
            }

            if(_currentPath != null && _currentPath.Count > 0)
            {
                if (!_isMoving)
                {
                    _isMoving = true;


                    AnimateCharacter(AnimationState.Walking);
                    transform.DOLookAt(_currentPath.Peek().WorldPosition, _turnTime).OnComplete(() =>
                    {
                        _moveSequence.Append(transform.DOMove(_currentPath.Peek().WorldPosition, _adaptiveMoveTime).OnComplete(() =>
                        {
                            _isMoving = false;
                            AnimateCharacter(AnimationState.Walking);
                            if(_tempTable != null)
                                CheckForConstruction();
                        }));
                        _currentPosition = _currentPath.Pop().GridPosition;

                        _moveSequence.PlayForward();
                    });

                    if (_currentPath.Count <= 0)
                        _hasTarget = false;
                }
            }
        }

        private void CheckForConstruction()
        {
            if (Pathfinding.HalloIchBinJulianUndIchWillWissenObIchNebenIhnenStehe(_currentPosition, grid.GetCellByDirection(_tempTable.transform.position).GridPosition))
            {
                AnimateCharacter(AnimationState.Working);
                _activeBuildTimer = _buildTimer;
                _timerActive = true;
                transform.DOLookAt(grid.GetCellByDirection(_tempTable.transform.position).WorldPosition, .1f);
            }
            else
            {
                _timerActive = false;
                _activeBuildTimer = _buildTimer;
            }
        }

        private bool CharacterTryMove()
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                
                GridCell cell = grid.GetCellByDirection(hit.point);
                //if (cell.GridPosition == _currentPosition)
                //    return false;


                if (cell.Block.Material == BlockMaterial.None)
                {
                    _targetPosition = cell.GridPosition;
                    _currentPath = Pathfinding.StandardAStar(_currentPosition, cell.GridPosition, PathfindingMode.Default);
                    if (_currentPath == null)
                    {
                        _hasTarget = false;
                        return false;
                    }
                    _hasTarget = true;
                }
                if (cell.Block.Material == BlockMaterial.Stone)
                {
                    if (_tempTable != null)
                    {
                        grid.GetCellByDirection(_tempTable.transform.position).SetBlock(_tempBlock, grid.transform);
                        Destroy(_tempTable);
                    }
                    _tempBlock = cell.Block;
                    Destroy(cell.CellObject);
                    _tempTable = Instantiate(_alchemyTable, cell.WorldPosition, new Quaternion(0,0,0,0));
                    cell.BlockForBuilding(_blockingBlock);
                    CheckForConstruction();
                }
                return true;
            }
            return false;
        }


        private void RightClick(object sender, System.EventArgs e)
        {
            //_moveSequence.Kill();
            _timerActive = false;
            _activeBuildTimer = _buildTimer;
            if (CharacterTryMove())
            {

            }
            else return;
        }

        private void AnimateCharacter(AnimationState state)
        {
            switch (state) {
                case AnimationState.Walking:
                    _animator.SetBool("Walking", _isMoving);
                    _animator.SetBool("Summoning", false);
                    _animator.SetBool("Working", false);
                    break;
                case AnimationState.Summoning:
                    _animator.SetBool("Working", false);
                    _animator.SetBool("Summoning", true);
                    _animator.SetBool("Walking", false);
                    break;
                case AnimationState.Working:
                    _animator.SetBool("Working", true);
                    _animator.SetBool("Summoning", false);
                    _animator.SetBool("Walking", false);
                    break;
                case AnimationState.Death:
                    _animator.SetBool("Working", false);
                    _animator.SetBool("Summoning", false);
                    _animator.SetBool("Walking", false);
                    _animator.SetBool("Dead", true);
                    break;
                case AnimationState.Idle:
                    _animator.SetBool("Working", false);
                    _animator.SetBool("Summoning", false);
                    _animator.SetBool("Walking", false);
                    break;

            }
        }


        public bool TakeDamage(int damage)
        {
            _hp -= damage;
            if(_hp <= 0)
            {
                Die();               
            }
            return true;
        }

        public void AdjustSpeed(float buildValue, float speedValue)
        {
            _buildTimer += buildValue;
            if (_buildTimer < _minimumBuildTime)
                _buildTimer = _minimumBuildTime;
            if (_buildTimer > _baseBuildtimer)
                _buildTimer = _baseBuildtimer;

            _adaptiveMoveTime += speedValue;

            if (_adaptiveMoveTime < _moveTime)
                _adaptiveMoveTime = _moveTime;
            if (_adaptiveMoveTime > _maxMoveSpeed)
                _adaptiveMoveTime = _maxMoveSpeed;

        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Elemental")
            {

            }
        }

        public void Die()
        {
            AnimateCharacter(AnimationState.Death);
        }

        public void Spawn(Vector2Int spawnPosition, Vector2Int spawnRotation)
        {
            _currentPosition = spawnPosition;
        }

        private enum AnimationState
        {
            Idle,
            Walking,
            Summoning,
            Working,
            Death
        }
    }
}
