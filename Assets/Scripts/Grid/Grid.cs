using MoreMountains.Tools;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CoreCraft.LudumDare55
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private int _gridWidth;
        [SerializeField] private int _gridHeight;
        [SerializeField] private int _moveSteps;
        [SerializeField] private float _gridCellScale;
        [SerializeField] private float _gridCellMargin;
        [SerializeField] private float _weightFactor;
        [SerializeField] private Block[] _blockPrefabs;

        private GridCell[,] _grid = new GridCell[,] { };
        private int _currentGroundLevelHeight;

        #region Generating Grid

        [Button("Start")]
        private void Start()
        {
            _grid = new GridCell[_gridWidth, _gridHeight];
            _currentGroundLevelHeight = _gridHeight;

            for(int x = 0 ; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _grid[x, y] = new GridCell(new Vector2Int(x,y),
                                                new Vector3(transform.position.x + (x * _gridCellScale) + (x * _gridCellMargin),
                                                            transform.position.y,
                                                            transform.position.z + (y * _gridCellScale) + (y * _gridCellMargin)),
                                                _currentGroundLevelHeight - y);
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

        private void MoveUp()
        {
            GridCell[,] tempGrid = new GridCell[_gridWidth, _gridHeight];

            _currentGroundLevelHeight += _moveSteps;
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y <= _gridHeight - _moveSteps; y++)
                {
                    tempGrid[x, y + _moveSteps] = _grid[x, y];
                    tempGrid[x, y + _moveSteps].GridPosition = tempGrid[x, y + _moveSteps].GridPosition + new Vector2Int(0, _moveSteps);
                    tempGrid[x, y + _moveSteps].WorldPosition = new Vector3(transform.position.x + (x * _gridCellScale) + (x * _gridCellMargin),
                                                                            transform.position.y,
                                                                            transform.position.z + ((y + _moveSteps) * _gridCellScale) + ((y + _moveSteps) * _gridCellMargin));
                }
            }

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y <= _moveSteps; y++)
                {
                    tempGrid[x, y].Height = _currentGroundLevelHeight - y;
                    tempGrid[x, y].SetBlock(GetRandomBlockByHeightWithWeight(tempGrid[x, y].Height), transform);
                }
            }

            _grid = tempGrid;
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
                weightedBlock.Weight = (1 - ((Mathf.Abs(weight) / range) * 0.99f)) * _weightFactor;

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

        #endregion

        #region GEt Information

        public GridCell GetCellByDirection(Vector3 hitPosition)
        {
            if (_grid[0, 0] != null)
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

        #endregion

#if UNITY_EDITOR
        [Button("Kill All Children")]
        private void KillAllChildren()
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            while (transform.childCount > 0)
            {
                KillAllChildren();
            }
        }
#endif

        private struct WeightedBlock
        {
            public float Weight;
            public Block Block;
        }
    }


}