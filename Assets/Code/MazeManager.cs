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
    private TextMeshPro _txtDifficultyEasy;
    private TextMeshPro _txtDifficultyHard;

    // Maze variables
    private readonly float _wallWidth = 1f;
    private int _selectedRowCount = 10;
    private int _margin = 2;
    public bool IsBGMEnabled = false;

    // Objects for the maze
    public GameObject Wall;

    public Player PlayerObject;
    public GameObject PrizeObject;

    public Rigidbody PrizeAnimationObject;

    public AudioClip PrizeSound;
    public AudioSource PrizeSoundSource;
    public AudioClip BGMSound;
    public AudioSource BGMSoundSource;

    private LTDescr _prizeAnimationId;


    // collection of walls to be passed to the algorithm of choice
    private List<GameObject> _allWalls;

    // Start is called before the first frame update
    void Start()
    {
        // get the difficulty texts to enable/disable when needed
        this._txtDifficultyEasy = GameObject.Find("txtDifficultyEasy").GetComponent<TextMeshPro>();
        this._txtDifficultyHard = GameObject.Find("txtDifficultyHard").GetComponent<TextMeshPro>();

        // get all the IMazeAlgorithm's to populate in the combobox
        this.InitialiseAlgorithmDropdown();


        this.UICanvas.enabled = true;
        this.InGameUICanvas.enabled = false;
        PrizeSoundSource.clip = PrizeSound;

    }

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

    public void StartGame()
    {
        Debug.LogWarning("Starting Game");
        this.UICanvas.enabled = false;
        this._txtDifficultyEasy.gameObject.SetActive(false);
        this._txtDifficultyHard.gameObject.SetActive(false);
        this.GenerateMaze();

        this.InGameUICanvas.enabled = true;

        if (this.IsBGMEnabled)
        {
            BGMSoundSource.clip = BGMSound;
            BGMSoundSource.volume = 0.3f;
            BGMSoundSource.Play();
        }
    }

    /// <summary>
    /// Should get called when the Player object had a collision with the Prize object
    /// </summary>
    public void HandlePrizeFound()
    {
        PrizeSoundSource.Play();
        this.InitPrizeAnimation();
        this.CancelGame();
    }

    void InitPrizeAnimation()
    {
        int amount = 1000;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        var center = 40 / 2;

        for (int i = 0; i < amount; i++)
        {
            float randomXPos = Random.Range(-center, center);
            float randomYPos = Random.Range(30, 80);

            Vector3 startPos = new Vector3(randomXPos, randomYPos, 0);
            PrizeAnimationObject.drag = Random.Range(0.1f, 1);
            PrizeAnimationObject.mass = Random.Range(1, 100);

            float randXScale = Random.Range(0.1f, 0.6f);
            float randyScale = Random.Range(0.1f, 0.6f);
            float randzScale = Random.Range(0.1f, 0.6f);

            PrizeAnimationObject.transform.localScale = new Vector3(randXScale, randyScale, randzScale);
            Instantiate(PrizeAnimationObject, startPos, new Quaternion(45, 20, 90, 0));
        }        
    }

    public void CancelGame()
    {
        Debug.LogWarning("Cancelling Game");
        this.InGameUICanvas.enabled = false;
        this._txtDifficultyEasy.gameObject.SetActive(true);
        this._txtDifficultyHard.gameObject.SetActive(true);
        this.DestroyMaze();
        this.UICanvas.enabled = true;

    }

    // Maze functions
    void GenerateMaze()
    {
        // Let's start by determining the size of the maze

        // We'll use the value of the 'row count' slider to determine the height (y-size)
        int ySize = this._selectedRowCount;
        // .. and use the current screen ratio to determine the width (x-size)
        float gameScreenRatio = ySize * (float)Screen.width / Screen.height;
        // i.e. 1024 x 768 = 3/2 - if height = 5 , 5 x 0,66
        int xSize = System.Convert.ToInt32(gameScreenRatio);
        //int xSize = ySize * (Screen.width / Screen.height);

        // Now that we've determined the size, let's create all the walls of the maze
        this._allWalls = this.CreateAllWalls(xSize, ySize);
        List<Vector3> allCellPositions = this.GetAllCellPositions(this._allWalls);

        // use a selected algorithm to destroy the walls to make the maze
        IMazeAlgorithm mazeAlgorithm = new DepthFirstMazeAlgorithm();
        this.CreateMaze(mazeAlgorithm, allCellPositions);
    }


    List<GameObject> CreateAllWalls(int xSize, int ySize)
    {
        List<GameObject> AllWalls = new List<GameObject>();

        // X-walls - First we create all the walls needed on the x-axis
        for (int x = 0; x < xSize; x++) // x (i.e. 3) - process 0,1,2
        {
            for (int y = 0; y <= ySize; y++) // y (i.e. 3) - process 0,1,2,3
            {
                // instantiate wall along the x-axis
                // | | | | |
                //var xwallPos = new Vector3(x + WallWidth / 2, y, 0);
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
            for (int y = 0; y < ySize; y++) // y (i.e. 3) - process 0,1,2
            {
                // instantiate wall along the y-axis
                // create 1 less than the x-walls 
                //  _ _ _ _!= xSize)
                //var ywallPos = new Vector3(x , y + WallWidth / 2);
                var ywallPos = new Vector3(x - xSize / 2, y - ySize / 2 + _wallWidth / 2);
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
                if (yWallToDestroy != null)
                    Destroy(yWallToDestroy);
            }

            // The maze is created, let's add the player and the goal
            Vector3 playerStartPosition = new Vector3(result.StartPosition.x + _wallWidth/2, result.StartPosition.y + _wallWidth / 2);
            // Set the player to the position the mazeAlgorithm provided
            PlayerObject.transform.position = playerStartPosition;
            // Add the Action to be triggered when the Player collides with the Prize
            this.PlayerObject.OnPrizeFound = HandlePrizeFound;

            Vector3 prizePosition = new Vector3(result.EndPosition.x + _wallWidth / 2, result.EndPosition.y + _wallWidth / 2);
            // Set the prize to the position the mazeAlgorithm provided
            PrizeObject.transform.position = prizePosition;
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

        PlayerObject.transform.position = new Vector3(-9999, -9999, 0);
        PrizeObject.transform.position = new Vector3(-9999, 9999, 0);
    }
}