using ES3Types;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Recorder;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public static class AStar
    {
        public static Stack<GridCell> StandardAStar(Vector2Int startIndex, Vector2Int targetIndex)
        {
            if (Grid.Instance == null)
                throw new Exception("There is no grid in ths Scene!");

            if (Grid.Instance.GetCellByIndex(startIndex) == null || Grid.Instance.GetCellByIndex(targetIndex) == null)
                throw new Exception("The start or target Index is out od bounds!");

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


                List<Node> neighbourNodes = new List<Node>();
                {
                    if (currentNode.Index.x - 1 >= 0)
                        neighbourNodes.Add(pathGrid[currentNode.Index.x - 1, currentNode.Index.y]);
                    if (currentNode.Index.x + 1 < pathGrid.GetLength(0))
                        neighbourNodes.Add(pathGrid[currentNode.Index.x + 1, currentNode.Index.y]);
                    if (currentNode.Index.y - 1 >= 0)
                        neighbourNodes.Add(pathGrid[currentNode.Index.x, currentNode.Index.y - 1]);
                    if (currentNode.Index.y + 1 < pathGrid.GetLength(1))
                        neighbourNodes.Add(pathGrid[currentNode.Index.x, currentNode.Index.y + 1]);
                }

                foreach(Node neighbourNode in neighbourNodes)
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

        private static int CalculateDistance(Vector2Int start, Vector2Int end) => Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);

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

            return new Stack<GridCell>();
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
}
