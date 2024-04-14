using UnityEngine;
using System;

using Random = UnityEngine.Random;

namespace CoreCraft.LudumDare55
{
    public class GridCell
    {
        public Vector2Int GridPosition;
        public Vector3 WorldPosition;
        public int Height;
        public Block Block;
        public GameObject CellObject;

        private float _maxRandomOffset = 0.2f;
        private int _randomWeight = 8;
        private Grid _grid;

        public GridCell(Vector2Int gridPosition,Vector3 worldPosition,int height, Grid grid)
        {
            GridPosition = gridPosition;
            WorldPosition = worldPosition;
            Height = height;
            _grid = grid;
        }

        public void SetBlock(Block block, Transform parentGrid)
        {
            Block = block;

            if (Block == null)
                throw new Exception($"Cell: {GridPosition} block = null");

            float yOffset = 0f;
            if(Random.Range(0,10) > _randomWeight)
                yOffset = Random.Range(0,_maxRandomOffset);


            if (Block.BlockPrefab != null)
                CellObject = MonoBehaviour.Instantiate(Block.BlockPrefab, WorldPosition - new Vector3(0f, yOffset, 0f), GetRandom90DegreeYRotation(), parentGrid);
        }

        public void IncreaseHeight(int heightIncrease)
        {
            Height += heightIncrease;
        }

        public void Mined()
        {

        }

        private Quaternion GetRandom90DegreeYRotation()
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    return Quaternion.Euler(0, 90, 0);
                case 1:
                    return Quaternion.Euler(0, 180, 0);
                case 2:
                    return Quaternion.Euler(0, 270, 0);
                default:
                    return Quaternion.Euler(0, 0, 0);
            }
        }
    }
}