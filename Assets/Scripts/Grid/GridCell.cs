using UnityEngine;
using System;

namespace CoreCraft.LudumDare55
{
    public class GridCell
    {
        public Vector2Int GridPosition;
        public Vector3 WorldPosition;
        public int Height;
        public Block Block;
        public GameObject CellObject;

        public GridCell(Vector2Int gridPosition,Vector3 worldPosition,int height)
        {
            GridPosition = gridPosition;
            WorldPosition = worldPosition;
            Height = height;
        }

        public void SetBlock(Block block, Transform parentGrid)
        {
            Block = block;

            if (Block == null)
                throw new Exception($"Cell: {GridPosition} block = null");

            if (Block.BlockPrefab != null)
                CellObject = MonoBehaviour.Instantiate(Block.BlockPrefab, WorldPosition, Quaternion.identity, parentGrid);
        }

        public void IncreaseHeight(int heightIncrease)
        {
            Height += heightIncrease;
        }
    }
}