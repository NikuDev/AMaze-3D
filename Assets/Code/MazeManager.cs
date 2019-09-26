using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MazeManager : MonoBehaviour // rename to MazeManager
{
    // Objects for the UI
    public Canvas UICanvas;
    public Canvas InGameUICanvas;
    public TextMeshProUGUI LblRowCountValue;
    public Dropdown CbAlgorithms;
    public TextMeshPro TxtStageClear;

    // Objects for the maze
    public GameObject Wall;

    public Player PlayerObject;
    public GameObject PrizeObject;

    public AudioClip PrizeSound;
    public AudioSource PrizeSoundSource;

    // collection of walls to be passed to the algorithm of choice
    private List<GameObject> _allWalls;

    // Maze variables
    private readonly float _wallWidth = 1f;
    private int _selectedRowCount = 10;
    private int _margin = 4;

    /// <summary>
    /// Variables for the PrizeObject animation
    /// </summary>
    private bool _prizeObjectInitialised;
    private float _timeCounter = 0;
    private float _prizeObjectBaseX;
    private float _prizeObjectBaseY;


    // Start is called before the first frame update
    void Start()
    {
        // get the difficulty texts to enable/disable when needed    
        this.TxtStageClear.gameObject.SetActive(false);

        // get all the IMazeAlgorithm's to populate in the combobox
        this.InitialiseAlgorithmDropdown();


        this.UICanvas.enabled = true;
        this.InGameUICanvas.enabled = false;
        PrizeSoundSource.clip = PrizeSound;

    }

    /// <summary>
    /// The Update function is responsible for animating the PrizeObject
    /// </summary>
    void Update()
    {
        int speed = 4;

        if (this._prizeObjectInitialised)
        {
            this._timeCounter += Time.deltaTime * speed;

            float x = Mathf.Cos(this._timeCounter) / 5;
            float y = Mathf.Sin(this._timeCounter) / 5;

            this.PrizeObject.transform.position = new Vector3(this._prizeObjectBaseX + x, this._prizeObjectBaseY + y, 0);
        }
    }

    /// <summary>
    /// Populates the dropdown in the main UI to select a maze algorithm of choice
    /// </summary>
    private void InitialiseAlgorithmDropdown()
    {
        // Clear all existing items in the dropdown
        this.CbAlgorithms.ClearOptions();

        List<string> availableAlgorithms = new List<string>();

        // Get all classes that implement the IMazeAlgorithm interface
        foreach (System.Type mazeAlgImplementation in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(ass => ass.GetInterfaces().Contains(typeof(IMazeAlgorithm))))
        {
            availableAlgorithms.Add(mazeAlgImplementation.Name);
        }

        // Add the name of the IMazeAlgorithm implementations to the drop down
        this.CbAlgorithms.AddOptions(availableAlgorithms);
    }

    /// <summary>
    /// Bound to the slider in the main UI for setting the amount of rows in the maze,
    /// also sets the field of view of the main Camera, to make sure the maze fits on the gamescreen
    /// </summary>
    /// <param name="slider">Slider object in the main UI</param>
    public void UpdateRowCount(Slider slider)
    {
        // Set the camera's field of view exactly to the slider's value
        // This is to guarantee the maze fits in the view, and to create a smooth 
        // effect in the UI
        float fieldOfViewValue = (slider.value * 2) + this._margin;
        Camera.main.fieldOfView = fieldOfViewValue;

        // Since the maze works by multiplying by 2 from the centre point, we want to
        // (subtly) change the row value to the nearest even number (i.e. 5 will be rounded down to 4)
        // this will guarantee a centered maze on the screen, while keeping the smoothness when changing the camera's f.o.v.
        this._selectedRowCount = (int)System.Math.Round(slider.value / 2) * 2;
        this.LblRowCountValue.text = this._selectedRowCount.ToString();

    }


    // Maze functions
    /// <summary>
    /// Bound to the 'Generate Maze' button in the main UI,
    /// Should instantiate the Wall collection, disable the main UI, enable the in-game UI
    /// and generate the maze based on the current amount of rows, selected with the slider
    /// </summary>
    public void StartGame()
    {
        // instantiate the collection to hold the walls of the maze
        this._allWalls = new List<GameObject>();
        // disable the main UI
        this.UICanvas.enabled = false;
        // generate the maze
        this.GenerateMaze();
        // enable the in-game UI
        this.InGameUICanvas.enabled = true;
        // set the timeCounter, used for the animation of the PrizeObject, to it's initial value 0
        this._timeCounter = 0;
    }

    /// <summary>
    /// Triggered when clicking the in-game 'X' button, or when the prize is found
    /// Should clean up the generated maze, disable the in-game UI and enable the main UI
    /// </summary>
    public void CancelGame()
    {
        // disable the in-game UI
        this.InGameUICanvas.enabled = false;
        // disable the 'Stage clear' text
        this.TxtStageClear.gameObject.SetActive(false);
        // destroy the maze
        this.DestroyMaze();
        // enable the main UI
        this.UICanvas.enabled = true;
    }

    /// <summary>
    /// Generates the maze with the chosen algorithm, the size will be based
    /// on the current value of the 'row slider' in the main UI
    /// TODO: make algorithm to use variable
    /// </summary>
    void GenerateMaze()
    {
        // Let's start by determining the size of the maze

        // We'll use the value of the 'row count' slider to determine the height (y-size)
        int ySize = this._selectedRowCount;
        // .. and use the current screen ratio to determine the width (x-size)
        float gameScreenRatio = ySize * (float)Screen.width / Screen.height;
        // i.e. 1024 x 768 = 3/2 - if height = 5 , 5 x 0,66
        int xSize = System.Convert.ToInt32(gameScreenRatio);
        // check whether the horizontal size, based on the current Screen ratio, is odd.
        // If it is, add 1 to ensure a centered maze on the screen
        if (xSize % 2 != 0)
            xSize = xSize + 1;

        // now that we've determined the size, let's create all the walls of the maze
        this._allWalls = this.CreateAllWalls(xSize, ySize);
        // after creating all the walls, calculate the positions of the spaces in between (the 'cells')
        List<Vector3> allCellPositions = this.GetAllCellPositions(this._allWalls);

        // use a selected algorithm to destroy the walls to make the maze
        IMazeAlgorithm mazeAlgorithm = new DepthFirstMazeAlgorithm();

        MazeAlgorithmResult result = this.CreateMaze(mazeAlgorithm, allCellPositions);

        // The maze is created, let's add the player and the prize
        Vector3 playerStartPosition = new Vector3(result.StartPosition.x + _wallWidth / 2, result.StartPosition.y + _wallWidth / 2);
        // Set the player to the position the mazeAlgorithm provided
        PlayerObject.transform.position = playerStartPosition;
        // Add the Action to be triggered when the Player collides with the Prize
        this.PlayerObject.OnPrizeFound = HandlePrizeFound;

        Vector3 prizePosition = new Vector3(result.EndPosition.x + _wallWidth / 2, result.EndPosition.y + _wallWidth / 2);
        // Set the prize to the position the mazeAlgorithm provided
        PrizeObject.transform.position = prizePosition;
        // For the animation of the PrizeObject, we set the initial position of it
        this._prizeObjectBaseX = prizePosition.x;
        this._prizeObjectBaseY = prizePosition.y;
        this._prizeObjectInitialised = true;
    }

    /// <summary>
    /// Creates all the x- and y-walls to create the FULL grid for the maze
    /// </summary>
    /// <param name="xSize">the amount of columns along the x-axis</param>
    /// <param name="ySize">the amount of rows along the y-axis</param>
    /// <returns></returns>
    List<GameObject> CreateAllWalls(int xSize, int ySize)
    {
        // instantiate the return collection
        List<GameObject> AllWalls = new List<GameObject>();

        // X-walls - First we create all the walls needed on the x-axis
        // create 1 less than the xSize along the x-axis
        for (int x = 0; x < xSize; x++) // x (i.e. 3) - process 0,1,2
        {
            for (int y = 0; y <= ySize; y++) // y (i.e. 3) - process 0,1,2,3
            {
                // instantiate the x-walls to lay flat along the x-axis
                // _ _ _ _
                var xwallPos = new Vector3(x - xSize / 2 + _wallWidth / 2, y - ySize / 2, 0);
                // set name for debugging purposes
                Wall.name = $"X-Wall X:{x} Y:{y}";
                // render it and add it to the collection
                AllWalls.Add(Instantiate(Wall, xwallPos, Quaternion.identity));
            }
        }

        // Y-walls
        for (int x = 0; x <= xSize; x++) // x (i.e. 3) - process 0,1,2,3
        {
            // create 1 less than the ysize along the y-axis
            for (int y = 0; y < ySize; y++) // y (i.e. 3) - process 0,1,2
            {
                // instantiate the y-walls to stand upright on the x-axis
                //  | | | | |
                var ywallPos = new Vector3(x - xSize / 2, y - ySize / 2 + _wallWidth / 2);
                // set name for debugging purposes
                Wall.name = $"Y-Wall X:{x} Y:{y}";
                // render it, rotate it 90 degrees and add it to the collection
                AllWalls.Add(Instantiate(Wall, ywallPos, Quaternion.Euler(0, 0, 90)));
            }
        }

        return AllWalls;
    }

    /// <summary>
    /// Based on the walls created, this calculates the position in between the
    /// lowest x,y wall and the highest x,y wall. Assuming the walls will have a 
    /// square shape
    /// </summary>
    /// <param name="AllWalls">All instantiated walls of the maze grid</param>
    /// <returns>null if no walls are passed, a list of cell-positions otherwise</returns>
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
    
    /// <summary>
    /// Uses the given algorithm to retrieve which walls should be destroyed to create the maze
    /// Based on this result, it will Destroy the walls that need to be destroyed,
    /// and set the Player and Prize object on the calculated start- and end position respectively
    /// </summary>
    /// <param name="mazeAlgorithmToUse">Implementation of the algorithm to calculate a path in the maze</param>
    /// <param name="allCellPositions">All available cell coordinates in between the created walls</param>
    MazeAlgorithmResult CreateMaze(IMazeAlgorithm mazeAlgorithmToUse, List<Vector3> allCellPositions)
    {
        MazeAlgorithmResult result = mazeAlgorithmToUse.GenerateMaze(allCellPositions);

        if (result.HasWallsToDestroy)
        {
            // if true, we've found some walls to destroy to create a path in the maze

            // Let's get the x-walls (flat ones with rotation = 0), and the y-walls (upright ones with rotation = 90)
            List<GameObject> xWalls = this._allWalls.Where(wall => wall.transform.rotation.eulerAngles.Equals(new Vector3(0, 0, 0))).ToList();
            List<GameObject> yWalls = this._allWalls.Where(wall => wall.transform.rotation.eulerAngles.Equals(new Vector3(0, 0, 90))).ToList();

            var xWallsToDestroy = result.XWallPositionsToDestroy;
            foreach (var xWallPosition in xWallsToDestroy)
            {
                // take off the (i.e. 0.5) offset used to center the wall
                Vector3 positionWithoutOffset = new Vector3(xWallPosition.x + _wallWidth / 2, xWallPosition.y, xWallPosition.z);
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
                Vector3 positionWithoutOffset = new Vector3(yWallPosition.x, yWallPosition.y + _wallWidth / 2, yWallPosition.z);
                // try to find the Y-Wall GameObject based on the position the algorithm returned
                var yWallToDestroy = yWalls.FirstOrDefault(wall => wall.transform.position.Equals(positionWithoutOffset));
                // If it's there (as it should be), let's remove it from the scene
                if (yWallToDestroy != null)
                    Destroy(yWallToDestroy);
            }
        }
        else
        {
            // something went terribly wrong
            throw new UnassignedReferenceException("Algorithm found no walls to destroy!");
        }

        // return the result for further handling of positioning the Player and Prize
        return result;
    }

    /// <summary>
    /// Should get called when the Player object had a collision with the Prize object
    /// </summary>
    public void HandlePrizeFound()
    {
        // to enable a delay between the prize handling functions, start a Coroutine
        StartCoroutine(TriggerStageClearRoutine());
    }

    /// <summary>
    /// Routine for handling the Prize animation's and ending the game
    /// </summary>
    private IEnumerator TriggerStageClearRoutine()
    {
        // Play the soundeffect for finding the Prize
        PrizeSoundSource.Play();
        // Disable the in-game UI while the animation plays
        this.InGameUICanvas.enabled = false;
        // Adjust the fontsize of the 'Stage clear' text to fit the current camera FOV
        this.TxtStageClear.fontSize = Camera.main.fieldOfView;
        // Set the text to active to initiate the fade-in/fade-out animation
        this.TxtStageClear.gameObject.SetActive(true);
        this._prizeObjectInitialised = false;

        // set the PrizeObject to it's original position (off-screen)
        this.PrizeObject.transform.position = new Vector3(-100, 5, 0);

        // wait 6 seconds for the 'Stage clear' text to finish animating
        yield return new WaitForSeconds(6);

        // Cancel the game to destroy the maze and show the main UI again
        this.CancelGame();
    }

    /// <summary>
    /// Destroy all the (remaining) walls of the maze, and sets the Player and Prize object 
    /// to it's original position (off-screen)
    /// Should be used after the player found the prize, or when the player cancels the game
    /// by using the in-game button
    /// </summary>
    void DestroyMaze()
    {
        foreach (GameObject wall in this._allWalls)
        {
            Destroy(wall);
        }

        PlayerObject.transform.position = new Vector3(-100, 1, 0);
        PrizeObject.transform.position = new Vector3(-100, 5, 0);
    }
}