using System.Collections;
using System.Collections.Generic;
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
        _xSize = (int)Camera.main.orthographicSize * 2; // camera is centered, which is why * 2 is needed
        // i.e. 1024 x 768 = 3/2 - if height = 5 , 5 x 0,66
        _ySize = _xSize * (Screen.width / Screen.height); // camera is centered, which is why * 2 is needed


        this._allWalls = this.CreateAllWalls();
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
                var xwallPos = new Vector3(x - _xSize/2 + WallWidth/2, y - _ySize/2, 0);
                // set name for debugging purposes
                Wall.name = $"X-Wall X:{x} Y:{y}";
                // add it to the collection
                AllWalls.Add(Wall);
                // render it
                Instantiate(Wall, xwallPos, Quaternion.identity);
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
                var ywallPos = new Vector3(x - _xSize/2, y - _ySize/2 + WallWidth/2);
                // set name for debugging purposes
                Wall.name = $"Y-Wall X:{x} Y:{y}";
                // add it to the collection
                AllWalls.Add(Wall);
                // render it and rotate it 90 degrees
                Instantiate(Wall, ywallPos, Quaternion.Euler(0, 0, 90));

            }
        }

        return AllWalls;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
