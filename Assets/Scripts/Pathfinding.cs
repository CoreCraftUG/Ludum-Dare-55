using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public static class Pathfinding
    {
        public static Stack<GridCell> StandardAStar(Vector2Int startIndex, Vector2Int targetIndex, PathfindingMode mode)
        {
            if (Grid.Instance == null)
                throw new Exception("There is no grid in ths Scene!");

            if (Grid.Instance.GetCellByIndex(startIndex) == null || Grid.Instance.GetCellByIndex(targetIndex) == null)
                throw new Exception("The start or target Index is out od bounds!");

            if (Grid.Instance.GetCellByIndex(startIndex).Block.BlockingType != BlockingType.None || Grid.Instance.GetCellByIndex(targetIndex).Block.BlockingType != BlockingType.None && mode != PathfindingMode.Gargoyle)
                throw new Exception("The start or target Index is blocked!");

            Node[,] pathGrid = new Node[Grid.Instance.GridWidth, Grid.Instance.GridHeight];

            for(int x = 0; x< pathGrid.GetLength(0); x++)
            {
                for (int y = 0; y < pathGrid.GetLength(1); y++)
                {
                    pathGrid[x, y] = new Node(new Vector2Int(x, y));

                    pathGrid[x, y].GCost = int.MaxValue;
                    pathGrid[x, y].CalculateFCost();
                    pathGrid[x, y].PreviousNode = null;
                }
            }

            Node startNode = pathGrid[startIndex.x, startIndex.y];
            Node endNode = pathGrid[targetIndex.x, targetIndex.y];

            List<Node> openList = new List<Node>() { startNode };
            List<Node> closeList = new List<Node>();


            startNode.GCost = 0;
            startNode.HCost = CalculateDistance(startNode.Index, endNode.Index) * 10;
            startNode.CalculateFCost();

            while(openList.Count > 0)
            {
                Node currentNode = GetSmallestFCostNode(openList);
                if (currentNode == endNode)
                    return CalculatePath(endNode);

                openList.Remove(currentNode);
                closeList.Add(currentNode);

                List<Node> neighbourNodes = mode switch
                {
                    PathfindingMode.Default => GetNeighbourNodesDefaultMode(currentNode, pathGrid),
                    PathfindingMode.NoWalls => GetNeighbourNodesNoWallsMode(currentNode, pathGrid),
                    PathfindingMode.Gargoyle => GetNeighbourNodesGargoyleMode(currentNode, pathGrid, targetIndex),
                    _ => new List<Node>()
                };

                foreach (Node neighbourNode in neighbourNodes)
                {
                    if (closeList.Contains(neighbourNode))
                        continue;

                    if(neighbourNode.GCost > currentNode.GCost + 10)
                    {
                        neighbourNode.PreviousNode = currentNode;
                        neighbourNode.GCost = currentNode.GCost + 10;
                        neighbourNode.HCost = CalculateDistance(startNode.Index, endNode.Index) * 10;
                        neighbourNode.CalculateFCost();
                        if(!openList.Contains(neighbourNode))
                            openList.Add(neighbourNode);
                    }
                }
            }

            return null;
        }

        private static List<Node> GetNeighbourNodesDefaultMode(Node currentNode, Node[,] pathGrid)
        {
            List<Node> neighbourNodes = new List<Node>();

            if (currentNode.Index.x - 1 >= 0 && Grid.Instance.GetCellByIndex(new Vector2Int(currentNode.Index.x - 1, currentNode.Index.y)).Block.BlockingType == BlockingType.None)
                neighbourNodes.Add(pathGrid[currentNode.Index.x - 1, currentNode.Index.y]);
            if (currentNode.Index.x + 1 < pathGrid.GetLength(0) && Grid.Instance.GetCellByIndex(new Vector2Int(currentNode.Index.x + 1, currentNode.Index.y)).Block.BlockingType == BlockingType.None)
                neighbourNodes.Add(pathGrid[currentNode.Index.x + 1, currentNode.Index.y]);
            if (currentNode.Index.y - 1 >= 0 && Grid.Instance.GetCellByIndex(new Vector2Int(currentNode.Index.x, currentNode.Index.y - 1)).Block.BlockingType == BlockingType.None)
                neighbourNodes.Add(pathGrid[currentNode.Index.x, currentNode.Index.y - 1]);
            if (currentNode.Index.y + 1 < pathGrid.GetLength(1) && Grid.Instance.GetCellByIndex(new Vector2Int(currentNode.Index.x, currentNode.Index.y + 1)).Block.BlockingType == BlockingType.None)
                neighbourNodes.Add(pathGrid[currentNode.Index.x, currentNode.Index.y + 1]);
            
            return neighbourNodes;
        }

        private static List<Node> GetNeighbourNodesGargoyleMode(Node currentNode, Node[,] pathGrid, Vector2Int targetIndex)
        {
            List<Node> neighbourNodes = new List<Node>();

            if (currentNode.Index.x - 1 >= 0 && (Grid.Instance.GetCellByIndex(new Vector2Int(currentNode.Index.x - 1, currentNode.Index.y)).Block.BlockingType == BlockingType.None || new Vector2Int(currentNode.Index.x - 1, currentNode.Index.y) == targetIndex))
                neighbourNodes.Add(pathGrid[currentNode.Index.x - 1, currentNode.Index.y]);
            if (currentNode.Index.x + 1 < pathGrid.GetLength(0) && (Grid.Instance.GetCellByIndex(new Vector2Int(currentNode.Index.x + 1, currentNode.Index.y)).Block.BlockingType == BlockingType.None || new Vector2Int(currentNode.Index.x + 1, currentNode.Index.y) == targetIndex))
                neighbourNodes.Add(pathGrid[currentNode.Index.x + 1, currentNode.Index.y]);
            if (currentNode.Index.y - 1 >= 0 && (Grid.Instance.GetCellByIndex(new Vector2Int(currentNode.Index.x, currentNode.Index.y - 1)).Block.BlockingType == BlockingType.None || new Vector2Int(currentNode.Index.x, currentNode.Index.y - 1) == targetIndex))
                neighbourNodes.Add(pathGrid[currentNode.Index.x, currentNode.Index.y - 1]);
            if (currentNode.Index.y + 1 < pathGrid.GetLength(1) && (Grid.Instance.GetCellByIndex(new Vector2Int(currentNode.Index.x, currentNode.Index.y + 1)).Block.BlockingType == BlockingType.None || new Vector2Int(currentNode.Index.x, currentNode.Index.y + 1) == targetIndex))
                neighbourNodes.Add(pathGrid[currentNode.Index.x, currentNode.Index.y + 1]);
            
            return neighbourNodes;
        }

        private static List<Node> GetNeighbourNodesNoWallsMode(Node currentNode, Node[,] pathGrid)
        {
            List<Node> neighbourNodes = new List<Node>();

            if (currentNode.Index.x - 1 >= 0)
                neighbourNodes.Add(pathGrid[currentNode.Index.x - 1, currentNode.Index.y]);
            if (currentNode.Index.x + 1 < pathGrid.GetLength(0))
                neighbourNodes.Add(pathGrid[currentNode.Index.x + 1, currentNode.Index.y]);
            if (currentNode.Index.y - 1 >= 0)
                neighbourNodes.Add(pathGrid[currentNode.Index.x, currentNode.Index.y - 1]);
            if (currentNode.Index.y + 1 < pathGrid.GetLength(1))
                neighbourNodes.Add(pathGrid[currentNode.Index.x, currentNode.Index.y + 1]);
            
            return neighbourNodes;
        }

        public static int CalculateDistance(Vector2Int start, Vector2Int end) => Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);

        private static Node GetSmallestFCostNode(List<Node> list)
        {
            if (list.Count <= 0)
                throw new Exception("List is empty!");

            float lowestFCost = list[0].FCost;
            Node returnNode = list[0];

            foreach (Node node in list)
            {
                if (node.FCost < lowestFCost)
                {
                    returnNode = node;
                    lowestFCost = node.FCost;
                }
            }

            return returnNode;
        }

        private static Stack<GridCell> CalculatePath(Node endNode)
        {
            Stack<GridCell> returnStack = new Stack<GridCell>();
            
            returnStack.Push(Grid.Instance.GetCellByIndex(endNode.Index));
            Node currentNode = endNode;
            while (currentNode.PreviousNode != null)
            {
                returnStack.Push(Grid.Instance.GetCellByIndex(currentNode.PreviousNode.Index));
                currentNode = currentNode.PreviousNode;
            }

            returnStack.Reverse();

            return returnStack;
        }

        public static bool StraightCheck(Vector2Int start, Vector2Int end)
        {
            if( MathF.Abs(start.x-end.x) != 0 &&  MathF.Abs(start.y-end.y) != 0)
                return false;

            Vector2 normalizationVector = end - start;
            Vector2Int increment = Vector2Int.RoundToInt(normalizationVector.normalized);

            while(start != end)
            {
                if(Grid.Instance.GetCellByIndex(start).Block.Material != BlockMaterial.None)
                    return false;
                start += increment;
            }

            return true;
        }

        public static bool HalloIchBinJulianUndIchWillWissenObIchNebenIhnenStehe(Vector2Int juliansPosition,  Vector2Int juliansZiel) => Mathf.Abs(juliansPosition.x - juliansZiel.x) + Mathf.Abs(juliansPosition.y - juliansZiel.y) == 1;

        public static List<GridCell> GetNeighbour(Vector2Int index)
        {
            if (Grid.Instance == null || Grid.Instance.GetCellByIndexWithNull(index) == null)
                return null;

            List<GridCell> neighbours = new List<GridCell>();
            GridCell neihghbour1 = Grid.Instance.GetCellByIndexWithNull(index + Vector2Int.down);
            GridCell neihghbour2 = Grid.Instance.GetCellByIndexWithNull(index + Vector2Int.up);
            GridCell neihghbour3 = Grid.Instance.GetCellByIndexWithNull(index + Vector2Int.right);
            GridCell neihghbour4 = Grid.Instance.GetCellByIndexWithNull(index + Vector2Int.left);

            if (neihghbour1 != null && neihghbour1.Block.BlockingType == BlockingType.None)
                neighbours.Add(neihghbour1);
            if (neihghbour2 != null && neihghbour2.Block.BlockingType == BlockingType.None)
                neighbours.Add(neihghbour2);
            if (neihghbour3 != null && neihghbour3.Block.BlockingType == BlockingType.None)
                neighbours.Add(neihghbour3);
            if (neihghbour4 != null && neihghbour4.Block.BlockingType == BlockingType.None)
                neighbours.Add(neihghbour4);

            return neighbours;
        }
    }

    public class Node
    {
        public float GCost;
        public float HCost;
        public float FCost;
        public Vector2Int Index;
        public Node PreviousNode;

        public Node(Vector2Int index)
        {
            Index = index;
        }

        public void CalculateFCost() => FCost = GCost + HCost;
    }

    public enum PathfindingMode
    {
        Default,
        NoWalls,
        Gargoyle
    }
}