using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Maze : MonoBehaviour
{
    public GameObject Wall;
    public float WallWidth = 1f;

    private int _xSize;
    private int _ySize;

    private Vector3 _startCellPosition;

    // collection of walls to be passed to the algorithm of choice
    private List<GameObject> _allWalls;

    // Start is called before the first frame update
    void Start()
    {
        // the camera is centered, multiplying by 2 makes sure the grid is place
        // on the full height/width
        _xSize = (int)Camera.main.orthographicSize * 4; // camera is centered, which is why * 2 is needed
        // i.e. 1024 x 768 = 3/2 - if height = 5 , 5 x 0,66
        _ySize = _xSize * (Screen.width / Screen.height); // camera is centered, which is why * 2 is needed


        //_xSize = 3;
        //_ySize = 3;


        this._allWalls = this.CreateAllWalls();

        // use a selected algorithm to destroy the walls to make the maze
        IMazeAlgorithm mazeAlgorithm = new DepthFirstMazeAlgorithm();


        int minWallX = (int)_allWalls.Min(wallObj => wallObj.transform.position.x);
        int minWallY = (int)_allWalls.Min(wallObj => wallObj.transform.position.y);
        int maxWallX = (int)_allWalls.Max(wallObj => wallObj.transform.position.x);
        int maxWallY = (int)_allWalls.Max(wallObj => wallObj.transform.position.y);

        List<Vector3> cellPositions = new List<Vector3>();
        for (int x = minWallX; x < maxWallX; x++)
        {
            for (int y = minWallY; y < maxWallY; y++)
            {
                cellPositions.Add(new Vector3(x, y, 0));
            }
        }

        if (mazeAlgorithm.TryGetWallPositionsToDestroy(cellPositions, out List<Vector3> xWallsToDestroy, out List<Vector3> yWallsToDestroy))
        {
            // we've found some walls to destroy
            List<GameObject> allRenderedXWalls = this._allWalls.Where(wall => wall.name.StartsWith("X")).ToList();
            List<GameObject> allRenderedYWalls = this._allWalls.Where(wall => wall.name.StartsWith("Y")).ToList();

            foreach(var xWallPosition in xWallsToDestroy)
            {
                // take off the (i.e. 0.5) offset used to center the wall
                Vector3 positionWithoutOffset = new Vector3(xWallPosition.x + WallWidth / 2, xWallPosition.y, xWallPosition.z);
                // try to find the X-Wall GameObject based on the position the algorithm returned
                var xWallToDestroy = allRenderedXWalls.FirstOrDefault(wall => wall.transform.position.Equals(positionWithoutOffset));
                // If it's there (as it should be), let's remove it from the scene
                if(xWallToDestroy != null)
                    Destroy(xWallToDestroy);
            }

            foreach(var yWallPosition in yWallsToDestroy)
            {
                // take off the (i.e. 0.5) offset used to center the wall
                Vector3 positionWithoutOffset = new Vector3(yWallPosition.x, yWallPosition.y + WallWidth / 2, yWallPosition.z);
                // try to find the Y-Wall GameObject based on the position the algorithm returned
                var yWallToDestroy = allRenderedYWalls.FirstOrDefault(wall => wall.transform.position.Equals(positionWithoutOffset));
                if(yWallToDestroy != null)
                    Destroy(yWallToDestroy);
            }
        }
        else
        {
            // something went terribly wrong
        }
    }

    List<GameObject> CreateAllWalls()
    {
        List<GameObject> AllWalls = new List<GameObject>();

        // X-walls - First we create all the walls needed on the x-axis
        for (int x = 0; x < _xSize; x++) // x (i.e. 3) - process 0,1,2
        {
            for (int y = 0; y <= _ySize; y++) // y (i.e. 3) - process 0,1,2,3
            {
                // instantiate wall along the x-axis
                // | | | | |
                //var xwallPos = new Vector3(x + WallWidth / 2, y, 0);
                var xwallPos = new Vector3(x - _xSize / 2 + WallWidth / 2, y - _ySize / 2, 0);
                // set name for debugging purposes
                Wall.name = $"X-Wall X:{x} Y:{y}";
                // render it and add it to the collection
                AllWalls.Add(Instantiate(Wall, xwallPos, Quaternion.identity));
            }
        }

        // Y-walls
        for (int x = 0; x <= _xSize; x++) // x (i.e. 3) - process 0,1,2,3
        {
            for (int y = 0; y < _ySize; y++) // y (i.e. 3) - process 0,1,2
            {
                // instantiate wall along the y-axis
                // create 1 less than the x-walls 
                //  _ _ _ _!= _xSize)
                //var ywallPos = new Vector3(x , y + WallWidth / 2);
                var ywallPos = new Vector3(x - _xSize / 2, y - _ySize / 2 + WallWidth / 2);
                // set name for debugging purposes
                Wall.name = $"Y-Wall X:{x} Y:{y}";
                // render it, rotate it 90 degrees and add it to the collection
                AllWalls.Add(Instantiate(Wall, ywallPos, Quaternion.Euler(0, 0, 90)));
            }
        }

        return AllWalls;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}