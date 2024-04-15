using MoreMountains.Tools;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using CoreCraft.Core;
using DG.Tweening;
using System.Collections;

using Random = UnityEngine.Random;

namespace CoreCraft.LudumDare55
{
    [RequireComponent(typeof(BoxCollider))]
    public class Grid : Singleton<Grid>
    {
        [SerializeField] private int _gridWidth;
        [SerializeField] private int _gridHeight;
        [SerializeField] private int _moveSteps;
        [SerializeField] private float _moveTime;
        [SerializeField] private float _gridCellScale;
        [SerializeField] private float _gridCellMargin;
        [SerializeField] private float _weightFactor;
        [SerializeField] private bool _useWeightFactorForBlockWeight;
        [SerializeField] private ResourceDictionary _resourcesDictionary;
        [SerializeField] private Block[] _blockPrefabs;
        [SerializeField] private Block _emptyBlock;

        private GridCell[,] _grid = new GridCell[,] { };
        private int _currentGroundLevelHeight;
        private bool _isMovingUp;

        private BoxCollider _collider => GetComponent<BoxCollider>();

        public int GridWidth { get { return _gridWidth; } }
        public int GridHeight { get { return _gridHeight; } }
        public ResourceDictionary ResourcesDictionary { get { return _resourcesDictionary; } }

        #region Generating Grid

        [Button("Debug Create Grid / Start"), FoldoutGroup("Debug")]
        private void Start()
        {
            _grid = new GridCell[_gridWidth, _gridHeight];
            _currentGroundLevelHeight = _gridHeight;

            _collider.center = new Vector3(((_gridWidth - 1) * _gridCellScale + (_gridWidth - 1) * _gridCellMargin) / 2,
                                            _gridCellScale / 2,
                                            ((_gridHeight - 1) * _gridCellScale + (_gridHeight - 1) * _gridCellMargin) / 2);

            _collider.size = new Vector3((_gridWidth * _gridCellScale + _gridWidth * _gridCellMargin),
                                            _gridCellScale,
                                            (_gridHeight * _gridCellScale + _gridHeight * _gridCellMargin));

            for(int x = 0 ; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _grid[x, y] = new GridCell(new Vector2Int(x,y),
                                                new Vector3(transform.position.x + (x * _gridCellScale) + (x * _gridCellMargin),
                                                            transform.position.y,
                                                            transform.position.z + (y * _gridCellScale) + (y * _gridCellMargin)),
                                                _currentGroundLevelHeight - y,
                                                this);
                }
            }

            PopulateGridCells(0, _gridHeight - 1);
        }

        private void PopulateGridCells(int lowerRowBound, int upperRowBound)
        {
            for (int x = 0 ; x < _gridWidth; x++)
            {
                for(int y = lowerRowBound ; y <= upperRowBound; y++)
                {
                    _grid[x, y].SetBlock(GetRandomBlockByHeightWithWeight(_grid[x, y].Height), transform);
                }
            }
        }

        [Button("Debug Move Up"),FoldoutGroup("Debug")]
        private void DebugMoveUp()
        {
            StartCoroutine(MoveUp());
        }

        public IEnumerator MoveUp()
        {
            if (_isMovingUp)
                yield break;

            _isMovingUp = true;
            GridCell[,] tempGrid = new GridCell[_gridWidth, _gridHeight];
            GameObject tempObj = null;
            for (int x = 0 ; x < _gridWidth; x++)
            {
                for (int y = _gridHeight - _moveSteps; y < _gridHeight; y++)
                {
                    tempObj = _grid[x, y].CellObject;

                    Destroy(tempObj,_moveTime);

                    tempObj.transform.DOMove(new Vector3(transform.position.x + (x * _gridCellScale) + (x * _gridCellMargin), transform.position.y, transform.position.z + +((y + _moveSteps) * _gridCellScale) + ((y + _moveSteps) * _gridCellMargin)), _moveTime).OnComplete(() =>
                    {
                        //TODO: Spawn VFX
                    });
                }
            }

            EventManager.Instance.GridMoveUp.Invoke(new Vector3(0, 0, (_moveSteps * _gridCellScale) + (_moveSteps * _gridCellMargin)), _moveTime, _moveSteps);

            _currentGroundLevelHeight += _moveSteps;
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight - _moveSteps; y++)
                {
                    tempGrid[x, y + _moveSteps] = _grid[x, y];
                    tempGrid[x, y + _moveSteps].GridPosition = tempGrid[x, y + _moveSteps].GridPosition + new Vector2Int(0, _moveSteps);
                    tempGrid[x, y + _moveSteps].WorldPosition = new Vector3(transform.position.x + (x * _gridCellScale) + (x * _gridCellMargin),
                                                                            transform.position.y,
                                                                            transform.position.z + ((y + _moveSteps) * _gridCellScale) + ((y + _moveSteps) * _gridCellMargin));
                    tempGrid[x, y + _moveSteps].CellObject.name = $"{tempGrid[x, y + _moveSteps].Block.BlockPrefab.name} {tempGrid[x, y + _moveSteps].GridPosition}";
                    tempGrid[x, y + _moveSteps].CellObject.transform.DOMove(tempGrid[x, y + _moveSteps].WorldPosition, _moveTime).OnComplete(() =>
                    {
                        //TODO: Spawn VFX
                    });
                }
            }

            yield return new WaitForSeconds(_moveTime);
            
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _moveSteps; y++)
                {
                    tempGrid[x, y] = new GridCell(new Vector2Int(x, y),
                                                new Vector3(transform.position.x + (x * _gridCellScale) + (x * _gridCellMargin),
                                                            transform.position.y,
                                                            transform.position.z + (y * _gridCellScale) + (y * _gridCellMargin)),
                                                _currentGroundLevelHeight - y,
                                                this);

                    tempGrid[x, y].SetBlock(GetRandomBlockByHeightWithWeight(_grid[x, y].Height), transform);
                }
            }

            yield return new WaitForSeconds(0.5f);
            _grid = tempGrid;
            _isMovingUp = false;
        }

        private Block GetRandomBlockByHeightWithWeight(int height)
        {
            List<WeightedBlock> tempBlocks = new List<WeightedBlock>();
            WeightedBlock weightedBlock = new WeightedBlock();
            Block retunBlock = null;

            foreach(Block block in _blockPrefabs.Where(b => b.SpawnMinDepth <= height && b.SpawnMaxDepth >= height))
            {
                weightedBlock.Block = block;
                float weight = (block.SpawnMaxDepth - height) + (block.SpawnMinDepth - height);
                float range = (block.SpawnMaxDepth - block.SpawnMinDepth);
                weightedBlock.Weight = (1 - ((Mathf.Abs(weight) / range) * 0.99f)) * _weightFactor + ((_useWeightFactorForBlockWeight)? block.SpawnWeight * _weightFactor : block.SpawnWeight);

                //(5 - 0 = 5) + (0 - 0 = 0) = 5
                //(5 - 5 = 0) + (0 - 5 = -5) = -5
                //(5 - 2 = 3) + (0 - 2 = -2) = 1

                tempBlocks.Add(weightedBlock);
            }

            float totalWeight = 0;
            foreach (WeightedBlock block in tempBlocks)
                totalWeight += block.Weight;

            tempBlocks.MMShuffle();

            float randomWeightValue = Random.Range(0, totalWeight);

            float processWeight = 0;
            foreach (WeightedBlock block in tempBlocks)
            {
                processWeight += block.Weight;
                if(randomWeightValue<processWeight)
                {
                    retunBlock = block.Block;
                    break;
                }
            }

            return retunBlock;
        }

        private void ReplaceCell(Vector2Int index)
        {
            
            if (_grid[index.x, index.y].CellObject.TryGetComponent<Animator>(out Animator anim))
            {
                anim.SetBool("DestroyBlock", true);
                if (anim != null)
                    Destroy(_grid[index.x, index.y].CellObject, anim.GetCurrentAnimatorStateInfo(0).length);
            }
            _grid[index.x, index.y].Mined();

            //DestroyImmediate(_grid[index.x, index.y].CellObject);
            _grid[index.x, index.y].SetBlock(_emptyBlock, transform);
            
        }

        #endregion

        #region Grid Interaction

        public GridCell GetCellByDirection(Vector3 hitPosition)
        {
            if (_grid == null || _grid.GetLength(0) <= 0 && _grid.GetLength(1) <= 0)
                throw new Exception("Grid has no Cells");

            GridCell returnCell = _grid[0,0];

            Vector2Int index = new Vector2Int();
            float smallestDistance = Vector3.Distance(_grid[0, 0].WorldPosition, hitPosition);

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (smallestDistance > Vector3.Distance(_grid[x, y].WorldPosition, hitPosition))
                    {
                        smallestDistance = Vector3.Distance(_grid[x, y].WorldPosition, hitPosition);
                        index = new Vector2Int(x, y);
                    }
                }
            }

            returnCell = _grid[index.x, index.y];

            return returnCell;
        }

        public GridCell GetCellByIndex(Vector2Int index)
        {
            if (_grid[0, 0] == null)
                throw new Exception("Grid has no Cells");
            if (!(index.x >= 0 && index.x < _gridWidth && index.y >= 0 && index.y < _gridHeight))
                throw new Exception($"Index: {index} is not in the grid!");

            return _grid[index.x, index.y];
        }

        public GridCell GetCellByIndexWithNull(Vector2Int index)
        {
            if (_grid[0, 0] == null)
                throw new Exception("Grid has no Cells");
            if (!(index.x >= 0 && index.x < _gridWidth && index.y >= 0 && index.y < _gridHeight))
                return null;

            return _grid[index.x, index.y];
        }

        public void MineCell(GridCell cell)
        {
            Vector2Int index = cell.GridPosition;

            if (_grid[index.x, index.y] != null)
            {
                if (_grid[index.x, index.y].Block.Destructible)
                    ReplaceCell(index);
            }
            else
                throw new Exception($"Cell: {index} does not exist!");
        }

        public void MineCell(Vector2Int index)
        {
            if (_grid[index.x, index.y] != null)
            {
                if (_grid[index.x, index.y].Block.Destructible)
                    ReplaceCell(index);
            }
            else
                throw new Exception($"Cell: {index} does not exist!");
        }

        public void MineCell(int x, int y)
        {
            Vector2Int index = new Vector2Int(x, y);

            if (_grid[index.x, index.y] != null)
            {
                if (_grid[index.x, index.y].Block.Destructible)
                    ReplaceCell(index);
            }
            else
                throw new Exception($"Cell: {index} does not exist!");
        }

        public void UnblockCell(Vector2Int index)
        {
            if (_grid[index.x, index.y] != null)
            {
                ReplaceCell(index);
            }
            else
                throw new Exception($"Cell: {index} does not exist!");
        }

        #endregion

        #region Debug
#if UNITY_EDITOR

        [Button("Kill All Children"), FoldoutGroup("Debug")]
        private void DebugKillAllChildren()
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            while (transform.childCount > 0)
            {
                DebugKillAllChildren();
            }
        }

        [SerializeField, FoldoutGroup("Debug")] private Vector2Int _debugSelectedCell;
        [Button("Mine Selected Cell"), FoldoutGroup("Debug")]
        private void DebugMineSelectedCell()
        {
            MineCell(_debugSelectedCell);
        }

        [SerializeField, FoldoutGroup("Debug")] private GameObject _debugEnemyObject;
        [SerializeField, FoldoutGroup("Debug")] private Vector2Int _debugEnemySpawnCell;
        [Button("Spawn Debug Enemy"), FoldoutGroup("Debug")]
        private void DebugSpawnDebugEnemy()
        {
            GameObject obj = Instantiate(_debugEnemyObject, _grid[_debugEnemySpawnCell.x, _debugEnemySpawnCell.y].WorldPosition,Quaternion.identity);
            obj.GetComponent<IInGrid>().Spawn(_debugEnemySpawnCell, Vector2Int.up);
        }

        [SerializeField, FoldoutGroup("Debug")] private GameObject _debugSummonObject;
        [SerializeField, FoldoutGroup("Debug")] private Vector2Int _debugSummonSpawnCell;
        [Button("Spawn Debug Summon"), FoldoutGroup("Debug")]
        private void DebugSpawnDebugSummon()
        {
            GameObject obj = Instantiate(_debugSummonObject, _grid[_debugSummonSpawnCell.x, _debugSummonSpawnCell.y].WorldPosition,Quaternion.identity);
            obj.GetComponent<IInGrid>().Spawn(_debugSummonSpawnCell, Vector2Int.up);
        }

#endif
        #endregion

        private struct WeightedBlock
        {
            public float Weight;
            public Block Block;
        }
    }

    [Serializable]
    public class ResourceDictionary : SerializableDictionary<BlockResources, GameObject> { }

}