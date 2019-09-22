﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{

    class DepthFirstMazeAlgorithm : IMazeAlgorithm
    {
        public MazeAlgorithmResult GenerateMaze(List<Vector3> allCellPositions)
        {
            MazeAlgorithmResult mazeAlgorithmResult = new MazeAlgorithmResult();

            List<Vector3> xWallPositionsToDestroy = new List<Vector3>();
            List<Vector3> yWallPositionsToDestroy = new List<Vector3>();

            // create a copy of all the cell positions to alter
            Stack<Vector3> visitedPositions = new Stack<Vector3>(); ;
            List<Vector3> unvisitedCellPositions = new List<Vector3>(allCellPositions);

            /* Let's say we have a 3x3 maze:
             * 
             * we have 9 Cells in total:
             *  _ _ _
             * |_|_|_|  0,2 - 1,2 - 2,2
             * |_|_|_|  0,1 - 1,1 - 2,1
             * |_|_|_|  0,0 - 1,0 - 2,0
             * 
             * We have 24 Walls in total (3x3 = 9, 3x4 + 4x2 = 24)
             * 
             * we want to 'visit' all the cells and create a path for the maze,
             * and add the same time add which walls need to be destroyed to make this path
             */

            // double-check this, might cause issues
            var startCellX = allCellPositions.Min(cellPos => cellPos.x);
            var startCellY = allCellPositions.Min(cellPos => cellPos.y);
            var startCellZ = allCellPositions.Min(cellPos => cellPos.z);

            var endCellX = allCellPositions.Max(cellPos => cellPos.x);
            var endCellY = allCellPositions.Max(cellPos => cellPos.y);
            var endCellZ = allCellPositions.Max(cellPos => cellPos.z);

            // Set the start cell position to the bottom left (up for debate if this should be default starting cell)
            Vector3 startPosition = new Vector3(startCellX, startCellY, startCellZ);

            // Keep track of the endPosition by using the latest 'neighbour' position
            Vector3 endPosition = new Vector3(endCellX, endCellY, endCellZ);

            // Push it on the stack of visited positions
            visitedPositions.Push(startPosition);
            // Set it to the result we're going to return
            mazeAlgorithmResult.StartPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z);
            mazeAlgorithmResult.EndPosition = new Vector3(endPosition.x, endPosition.y, endPosition.z);

            int visitedCells = 1; // we've 'visited' the start cell
            int totalAmountOfCells = allCellPositions.Count;
            // Since we've added it to the visitedPositions stack, REMOVE it from the unvisited positions
            unvisitedCellPositions.Remove(startPosition);

            // As long as we haven't processed each cell position, we want to keep making a path
            while (visitedCells != totalAmountOfCells)
            {
                // use the position at the top of the stack as the current position
                var currentPosition = visitedPositions.Peek();

                if (this.TryGetRandomNeighbour(currentPosition, unvisitedCellPositions, out Vector3 randomNeighbourPosition))
                {
                    // if true, we've found a random neighbour. Let's make this the currentPosition and add the wall in between to be destroyed
                    if (currentPosition.x != randomNeighbourPosition.x)
                    {
                        // we've moved along the x-axis, so we want to destroy the y-wall (with rotation 90), add it to the list
                        if (currentPosition.x > randomNeighbourPosition.x)
                        {
                            // we've moved to the left, destroy the y-wall with the current coordinates
                            yWallPositionsToDestroy.Add(currentPosition);
                        }
                        else
                        {
                            // we've moved to the right, destroy the y-wall with the neighbours coordinates
                            yWallPositionsToDestroy.Add(randomNeighbourPosition); 
                        }
                    }
                    else
                    {
                        // we've moved along the y-axis, so we want to destroy the x-wall (with rotation 0), add it to the list
                        if (currentPosition.y > randomNeighbourPosition.y)
                        {
                            // we've moved down, destroy the x-wall with the current coordinates
                            xWallPositionsToDestroy.Add(currentPosition);
                        }
                        else
                        {
                            // we've moved up, destroy the x-wall with the neighbours coordinates
                            xWallPositionsToDestroy.Add(randomNeighbourPosition);
                        }
                    }

                    // push the random neighbour on top of the stack, effectively making it the current position
                    visitedPositions.Push(randomNeighbourPosition);
                    // Since we've added it to the visitedPositions stack, REMOVE the random neighbour (new current position) from the unvisited positions
                    unvisitedCellPositions.Remove(randomNeighbourPosition);

                    // increment the amount of visited cells for the while loop to keep track off
                    visitedCells++;
                }
                else
                {
                    // we couldn't find a neighbour, but since we're still in the while loop, we haven't
                    // visited all the cell positions yet. Let's backtrack by popping the current position
                    // of the stack and trying again
                    visitedPositions.Pop();
                }

            }

            // We've finished creating the maze, let's fill the result and return it
            mazeAlgorithmResult.XWallPositionsToDestroy = xWallPositionsToDestroy;
            mazeAlgorithmResult.YWallPositionsToDestroy = yWallPositionsToDestroy;


            return mazeAlgorithmResult;
        }

        bool TryGetRandomNeighbour(Vector3 currentPosition, List<Vector3> unvisitedCellPositions, out Vector3 randomNeighbourPosition)
        {
            randomNeighbourPosition = new Vector3();

            // Current pos + 1 x = North neighbour
            Vector3 NorthNeighbour = new Vector3(currentPosition.x + 1, currentPosition.y, currentPosition.z);
            // Current pos + 1 y = East neighbour
            Vector3 EastNeighbour = new Vector3(currentPosition.x, currentPosition.y + 1, currentPosition.z);
            // Current pos - 1 y = South neighbour
            Vector3 SouthNeighbour = new Vector3(currentPosition.x, currentPosition.y - 1, currentPosition.z);
            // Current pos - 1 x = West neighbour
            Vector3 WestNeighbour = new Vector3(currentPosition.x - 1, currentPosition.y, currentPosition.z);

            List<Vector3> neighboursToChooseFrom = new List<Vector3>();
            if (unvisitedCellPositions.Contains(NorthNeighbour))
                neighboursToChooseFrom.Add(NorthNeighbour);
            if (unvisitedCellPositions.Contains(EastNeighbour))
                neighboursToChooseFrom.Add(EastNeighbour);
            if (unvisitedCellPositions.Contains(SouthNeighbour))
                neighboursToChooseFrom.Add(SouthNeighbour);
            if (unvisitedCellPositions.Contains(WestNeighbour))
                neighboursToChooseFrom.Add(WestNeighbour);

            if (neighboursToChooseFrom.Count > 0)
            {
                // get a random position based on the amount of neighbours
                int randomPosition = (int)UnityEngine.Random.Range(0, neighboursToChooseFrom.Count);
                // set the out param
                randomNeighbourPosition = neighboursToChooseFrom[randomPosition];
                return true;
            }
            else
            {
                // there are no neighbours available, return false
                return false;
            }
        }
    }
}
