using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MazeManager : MonoBehaviour // rename to MazeManager
{
    // Objects for the UI
    public Canvas UICanvas;
    public Canvas InGameUICanvas;
    private Slider _sldRowCount;
    private Text _lblRowCountValue;
    private int _selectedRowCount;

    // Objects for the maze
    public GameObject Wall;
    public Rigidbody Player;
    public GameObject Prize;

    private readonly float WallWidth = 1f;

    private int _margin = 2;
    private int _xSize;
    private int _ySize;

    private Vector3 _startCellPosition;

    // collection of walls to be passed to the algorithm of choice
    private List<GameObject> _allWalls;

    // Start is called before the first frame update
    void Start()
    {
        this._sldRowCount = GameObject.Find("sldRowCount").GetComponent<Slider>();
        this._lblRowCountValue = GameObject.Find("lblRowCountValue").GetComponent<Text>();
        //this._btnCancel = GameObject.Find("btnCancel").GetComponent<Button>();

        this.UICanvas.enabled = true;
        this.InGameUICanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        this._selectedRowCount = (int)this._sldRowCount.value;
        this._lblRowCountValue.text = this._selectedRowCount.ToString();
    }

    public void StartGame()
    {
        Debug.LogWarning("Starting Game");
        this.UICanvas.enabled = false;
        this.GenerateMaze();
        this.InGameUICanvas.enabled = true;
    }

    public void CancelGame()
    {
        Debug.LogWarning("Cancelling Game");
        this.InGameUICanvas.enabled = false;
        this.DestroyMaze();
        this.UICanvas.enabled = true;

    }

    void GenerateMaze()
    {
        var mazeSize = this.CalculateMazeSize(this._selectedRowCount);
        this._xSize = mazeSize.Key;
        this._ySize = mazeSize.Value;

        this._allWalls = this.CreateAllWalls();
        List<Vector3> allCellPositions = this.GetAllCellPositions(this._allWalls);

        // use a selected algorithm to destroy the walls to make the maze
        IMazeAlgorithm mazeAlgorithm = new DepthFirstMazeAlgorithm();
        this.CreateMaze(mazeAlgorithm, allCellPositions);
    }

    // Maze.cs functions
    KeyValuePair<int, int> CalculateMazeSize(int rows)
    {
        // First let's get the size of the maze. y-size (vertical) is the ortographicSize of the camera * 2 (since the camera is centered)
        // let's take 1 unit as margin around the maze
        Camera.main.orthographicSize = (rows + this._margin)/2;
        int ySize = (int)(Camera.main.orthographicSize - 1) * 2;
        
        // Let's calculate the width of the maze by getting the size of the game-screen,
        // this way when we change the resolution of the gamescreen, the size of the maze adjusts itself
        float gameScreenRatio = ySize * (float)Screen.width / Screen.height;
        // i.e. 1024 x 768 = 3/2 - if height = 5 , 5 x 0,66
        int xSize = System.Convert.ToInt32(gameScreenRatio);

        return new KeyValuePair<int, int>(xSize, ySize);
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

        // The walls are created, destroy the initial wall which we've used to clone all
        // the other ones
        //Destroy(Wall);

        return AllWalls;
    }

    List<Vector3> GetAllCellPositions(List<GameObject> AllWalls)
    {
        // check if we have walls to calculate our cell positions from
        if (!AllWalls.Any())
            return null;

        // Retrieve the lowest X, Y and highest X, Y positions of the walls
        int minWallX = (int)AllWalls.Min(wallObj => wallObj.transform.position.x);
        int minWallY = (int)AllWalls.Min(wallObj => wallObj.transform.position.y);
        int maxWallX = (int)AllWalls.Max(wallObj => wallObj.transform.position.x);
        int maxWallY = (int)AllWalls.Max(wallObj => wallObj.transform.position.y);

        List<Vector3> cellPositions = new List<Vector3>();

        // based on the lowest/highest X, Y coordinates, add all cell positions in between
        for (int x = minWallX; x < maxWallX; x++)
        {
            for (int y = minWallY; y < maxWallY; y++)
            {
                cellPositions.Add(new Vector3(x, y, 0));
            }
        }

        return cellPositions;
    }

    void CreateMaze(IMazeAlgorithm mazeAlgorithmToUse, List<Vector3> allCellPositions)
    {
        MazeAlgorithmResult result = mazeAlgorithmToUse.GenerateMaze(allCellPositions);

        if (result.HasWallsToDestroy)
        {
            // if true, we've found some walls to destroy

            // Let's get the x-walls (flat ones with rotation = 0), and the y-walls (upright ones with rotation = 90)
            List<GameObject> xWalls = this._allWalls.Where(wall => wall.transform.rotation.eulerAngles.Equals(new Vector3(0, 0, 0))).ToList();
            List<GameObject> yWalls = this._allWalls.Where(wall => wall.transform.rotation.eulerAngles.Equals(new Vector3(0, 0, 90))).ToList();

            var xWallsToDestroy = result.XWallPositionsToDestroy;
            foreach (var xWallPosition in xWallsToDestroy)
            {
                // take off the (i.e. 0.5) offset used to center the wall
                Vector3 positionWithoutOffset = new Vector3(xWallPosition.x + WallWidth / 2, xWallPosition.y, xWallPosition.z);
                // try to find the X-Wall GameObject based on the position the algorithm returned
                var xWallToDestroy = xWalls.FirstOrDefault(wall => wall.transform.position.Equals(positionWithoutOffset));
                // If it's there (as it should be), let's remove it from the scene
                if (xWallToDestroy != null)
                    Destroy(xWallToDestroy);
            }

            var yWallsToDestroy = result.YWallPositionsToDestroy;
            foreach (var yWallPosition in yWallsToDestroy)
            {
                // take off the (i.e. 0.5) offset used to center the wall
                Vector3 positionWithoutOffset = new Vector3(yWallPosition.x, yWallPosition.y + WallWidth / 2, yWallPosition.z);
                // try to find the Y-Wall GameObject based on the position the algorithm returned
                var yWallToDestroy = yWalls.FirstOrDefault(wall => wall.transform.position.Equals(positionWithoutOffset));
                if (yWallToDestroy != null)
                    Destroy(yWallToDestroy);
            }

            // The maze is created, let's add the player and the goal
            Vector3 playerStartPosition = new Vector3(result.StartPosition.x + WallWidth/2, result.StartPosition.y + WallWidth / 2);
            // Set the player to the position the mazeAlgorithm provided
            Player.transform.position = playerStartPosition;

            Vector3 prizePosition = new Vector3(result.EndPosition.x + WallWidth / 2, result.EndPosition.y + WallWidth / 2);
            // Set the prize to the position the mazeAlgorithm provided
            Prize.transform.position = prizePosition;
        }
        else
        {
            // something went terribly wrong
            Debug.LogError("Algorithm found no walls to destroy!");
        }
    }

    void DestroyMaze()
    {
        foreach (GameObject wall in this._allWalls)
        {
            Destroy(wall);
        }

        Player.transform.position = new Vector3(-9999, -9999, 0);
        Prize.transform.position = new Vector3(-9999, 9999, 0);
    }
}