using System.Collections; //Use System.Collections to allow use of Coroutines
using System.Collections.Generic; //Use System.Collections.Generic to allow use of Lists
using UnityEngine; //Use UnityEngine to connect to Unity
using UnityEngine.UI; //Use UnityEngine.UI to allow us to control our UI/Canvas elements
using System.IO; //Use System.IO to allow us to save and retrieve our high scores in CSV file

[System.Serializable]
public class GameHandler : MonoBehaviour
{
    #region Variables
    //Create a public score variable so it can be incremented by bullets but hide in Unity inspector
    [HideInInspector] public int score = 0;
    //Create a list to retrieve scores from our high score file
    private List<int> _highScores = new List<int>();
    //Create a list to retrieve names from our high scores file
    private List<string> _highNames = new List<string>();
    //Create a public list to store the sharks we have in scene so BulletHandler class can compare it with bullet list for challenge mode but hide it in Inspector
    [HideInInspector] public List<GameObject> sharksInScene = new List<GameObject>();
    [Header("Game Variables")]
    [Tooltip("Set the starting speed of the sharks")]
    public float sharkSpeed = 3f;
    [Tooltip("Bool check to see if user selected Challenge Mode. Will be set by menu buttons")]
    public bool challengeMode = false;
    [Header("Object References")]
    [Tooltip("Reference to Duck object in heirarchy. Doesn't need to be set as will be collected when user starts a new game or challenge")]
    public GameObject duck;
    [Tooltip("Add the SharkAnim prefab so we can Instantiate it as needed")]
    public GameObject sharkPrefab;
    [Tooltip("Add the Duck prefab here so we can instantiate it if user starts a subsequent game")]
    [SerializeField] private GameObject _duckPrefab;
    [Header("UI Elements")]
    [Tooltip("Add the ContinueButton from the Canvas object here")]
    public Button continueButton;
    [Tooltip("Add the InputField from the SaveScoreMenu here")]
    public InputField highScoreInput;
    [Tooltip("Add the Canvas object here")]
    public GameObject menuCanvas;
    [Tooltip("Add the SaveScoreMenu object here")]
    public GameObject saveScoreMenu;
    [Tooltip("Add the HighScoreMenu here")]
    public GameObject highScoreMenu;
    [Tooltip("Add the Score text fields from the HighScoreMenu in order from top to bottom")]
    public Text[] scoreBoxes;
    [Tooltip("Add the Name text fields from the HighScoreMenu in order from top to bottom")]
    public Text[] nameBoxes;
    [Header("UI Style")]
    [Tooltip("Add the skin to use to style the imGUI elements")]
    public GUISkin skin;
    public static string path = Path.Combine(Application.streamingAssetsPath, "HighScores.txt");
    #endregion
    #region Game States
    private void Update()
    {
        //If either of the menus that can be accessed on application load are active set time scale to 0 so sharks won't start functioning before User enters new game, otherwise set it to 1
        if (menuCanvas.activeInHierarchy || highScoreMenu.activeInHierarchy)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        //If time scale is 1, we can activate the pause function using the escape or P1 Start buttons
        if (Time.timeScale == 1)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Any Start"))
            {
                Pause();
            }
        }
    }
    public void NewGame()
    {
        //Stop all coroutines so we are not creating multiple sets of sharks at once
        StopAllCoroutines();
        //Make the ContinueButton interactable so we can continue game from current position if we activate pause menu
        continueButton.interactable = true;
        //Hide the cursor
        Cursor.visible = false;
        //Reset score to 0
        score = 0;
        //Reset shark speed to default starting speed
        sharkSpeed = 3f;
        //If there is no duck in scene Instantiate one at the left side of screen and set its name to Duck so next check will find it
        if (!GameObject.Find("Duck"))
        {
            duck = Instantiate(_duckPrefab, new Vector3(-11f, 0f, 2f), Quaternion.Euler(0, -90, 0));
            duck.name = "Duck";
        }
        //Clear the list of bullets in the scene
        duck.GetComponent<DuckMovement>().bulletsInScene.Clear();
        //Clear the list of sharks in the scene
        sharksInScene.Clear();
        //Create an array of Objects in the scene with the Shark tag (We have tagged bullets as sharks so they will be collected too)
        GameObject[] objectsInScene = GameObject.FindGameObjectsWithTag("Shark");
        //Destroy each object we just found so scene is clear on new game
        foreach (GameObject obj in objectsInScene)
        {
            Destroy(obj.gameObject);
        }
        //Start our Coroutines to start instantiating sharks and adjusting their speed
        StartCoroutine(LoadShark());
        StartCoroutine(AdjustSpeed());
    }
    public void Pause()
    {
        //If game wasn't already paused set tiime scale to 0 and make the cursor and menu visible
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;
            Cursor.visible = true;
            menuCanvas.SetActive(true);
        }
        //Else hide the cursor and restart time scale
        else
        {
            Time.timeScale = 1;
            Cursor.visible = false;
        }
    }
    public void EndGame()
    {
        //If the user has scored more points than the lowest score on our high score list
        if (score > _highScores[9])
        {
            //Stop time scale and show the cursor
            Time.timeScale = 0;
            Cursor.visible = true;
            //Deactivate continue button as we no longer have a game to continue on with
            continueButton.interactable = false;
            //Show the SaveScoreMenu
            saveScoreMenu.SetActive(true);
        }
        else
        {
            //Deactivate the ContinueButton as we no longer have a game to continue with
            continueButton.interactable = false;
            //Ativate the pause function to show menu and cursor and stop time scale
            Pause();
        }
    }
    public void Exit()
    {
        //If we are using Unity PlayMode exit will stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        //Quit the application
        Application.Quit();
    }
    #endregion
    #region High Scores
    public void WriteHighScores(InputField name)
    {
        int i = 0;
        foreach (int saved in _highScores)
        {
            if (score > saved)
            {
                _highScores.Insert(i, score);
                _highNames.Insert(i, name.text);
                _highNames.RemoveAt(10);
                _highScores.RemoveAt(10);
                break;
            }
            i++;
        }
        //Create a new writer that will overwrite existing file
        StreamWriter writer = new StreamWriter(path, false);
        //write each of our keys to the file with key index and value separated by a colon
        for (int n = 0; n < 10; n++)
        {
            writer.WriteLine(_highScores[n].ToString() + ":" + _highNames[n]);
        }
        //Close the writer
        writer.Close();
        //Reload the file so we have the correct version
        TextAsset asset = Resources.Load(path) as TextAsset;
    }
    public void ReadHighScores()
    {
        int i = 0;
        if (File.Exists(path))
        {
            //Create a new reader to read from the file
            StreamReader reader = new StreamReader(path);
            //Create a string from the line we are currently reading
            string line;
            //While loop to run for each line until the line is empty
            while ((line = reader.ReadLine()) != null)
            {
                //Create a string array with the first index being the string value from the file and the second index being the keycode
                string[] parts = line.Split(':');
                //If we already have keybinds in the keys dictionary change the value in the dictionary
                if (_highScores.Count == 10)
                {
                    _highScores[i] = int.Parse(parts[0]);
                    //else we need add the keys to the dictionary
                    _highNames[i] = parts[1];
                }
                else
                {
                    _highScores.Add(int.Parse(parts[0]));
                    _highNames.Add(parts[1]);
                }
                i++;
            }
            //Close the reader
            reader.Close();
        }
        else if (_highNames.Count < 1)
        {
            for (int n = 0; n < 10; n++)
            {
                _highScores.Add(0);
                _highNames.Add("DEFAULT");
            }
        }
        i = 0;
        foreach (string name in _highNames)
        {
            scoreBoxes[i].text = _highScores[i].ToString();
            nameBoxes[i].text = _highNames[i];
            i++;
        }
    }
    #endregion
    #region Game Controls
    public void SetMode(bool mode)
    {
        //Set the challenge mode bool to the value that corresponds with the button pressed by user
        challengeMode = mode;
    }
    IEnumerator LoadShark()
    {
        //While we still have a duck in the scene
        while (duck != null)
        {
            //If time is not stopped Instantiate a shark at a random Y position on the right side of screen
            if (Time.timeScale == 1)
            {
                sharksInScene.Add(Instantiate(sharkPrefab, new Vector3(11.7f, Random.Range(-6f, 6f), 2f), Quaternion.Euler(0, 90, 0)));
            }
            //Wait o.75 of a second before the next pass through loop
            yield return new WaitForSecondsRealtime(0.75f);
        }
        //Return null once our duck no longer exists
        yield return null;
    }
    IEnumerator AdjustSpeed()
    {
        //If Challenge mode we will wait for 10 seconds before increasing speed, otherwise we wait 5 seconds
        if (challengeMode)
        {
            yield return new WaitForSecondsRealtime(10f);
        }
        else
        {
            yield return new WaitForSecondsRealtime(5f);
        }
        //If time is not stopped increase speed by 0.5
        if (Time.timeScale == 1)
        {
            sharkSpeed += 0.5f;
        }
        //If duck still exists start another pass through this coroutine
        if (duck != null)
        {
            StartCoroutine(AdjustSpeed());
        }
    }
    #endregion
    #region UI
    public void HighScoreInput(string letter)
    {
        //If user presses a letter on the onscreen keyboard add its value to the text of the input field
        highScoreInput.text = highScoreInput.text + letter;
    }
    private void OnGUI()
    {
        //Apply our skin so imGUI elements are displayed correctly
        GUI.skin = skin;
        //If time is still running we will display imGUI, but it will not show while time is stopped during menus
        if (Time.timeScale == 1)
        {
            //Create a text field for current score
            GUI.TextField(new Rect(0, (Screen.height / 18) * 17, Screen.width / 5, Screen.height / 18), "Score: " + score.ToString());
            //Create a text field for the name of the game
            GUI.TextField(new Rect(Screen.width / 5, Screen.height / 18 * 17, (Screen.width / 5) * 3, Screen.height / 18), "Shark Attack 2: Revenge Of The Ducks");
            //Create a text field for the current shark speed
            GUI.TextField(new Rect((Screen.width / 5) * 4, Screen.height / 18 * 17, Screen.width / 5, Screen.height / 18), "Shark Speed: " + sharkSpeed.ToString());
        }
    }
    #endregion
    #region Test Functions
    public DuckMovement GetPlayer()
    {
        //When test calls this function return an Instantiated shark with its DuckMovement reference
        return Instantiate(_duckPrefab, new Vector3(-11f, 0f, 2f), Quaternion.Euler(0f, -90f, 0f)).GetComponent<DuckMovement>();
    }
    public void SetHighScores()
    {
        //Create a 10 entry dummy list of high scores so Test won't hit error when duck is killed
        for (int i = 0; i < 10; i++)
        {
            _highScores.Add(10);
        }
    }
    #endregion
}
