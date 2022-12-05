using System.Collections; //Use System.Collections to allow use of IEnumerators
using NUnit.Framework; //Use NUnit.Framework to allow use of Testing functions such as Setup, Teardown and Assert
using UnityEngine; //Use UnityEngine to connect to unity
using UnityEngine.UI; //Use UnityEngine.UI because we will need to reattach some UI elements to GameHandler once it is instantiated
using UnityEngine.TestTools; //Use UnityEngine.TestTools so we can set up each Unity test

public class Tests
{
    #region Variables
    //Assign a variable to store the GameHandler script
    GameHandler _gameHandler;
    //We will need a menu variable so we can use it to make some connections from GameHandler
    GameObject _menu;
    //Assign a variable so we can setup and remove a camera for each test as needed
    Object _camera;
    #endregion
    #region Setup & Teardown
    //Setup before each test
    [SetUp]
    public void Setup()
    {
        //Instantiate PinkBG prefab in scene and retrieve its GameHandler component
        _gameHandler = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/PinkBG")).GetComponent<GameHandler>();
        //Rename object as PinkBG to allow connections in other scripts to be made correctly
        _gameHandler.gameObject.name = "PinkBG";
        //Instantiate the Canvas prefab in menu so we can connect the ContinueButton
        _menu = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Canvas"));
        //Connect the GameHandlers continueButton variable to its relevant object in the menu to avoid error
        _gameHandler.continueButton = GameObject.Find("ContinueButton").GetComponent<Button>();
        //Assign menu we instantiated to the various menu references in Game Handler to avoid null references. Can use same canvas for all as we are not actually using them
        _gameHandler.menuCanvas = _menu;
        _gameHandler.highScoreMenu = _menu;
        _gameHandler.saveScoreMenu = _menu;
        //Deactivate the menu canvas as we don't need to see it
        _menu.SetActive(false);
    }
    //Destroy after each test
    [TearDown]
    public void TearDown()
    {
        //Destroy the GameHandler object
        Object.Destroy(_gameHandler.gameObject);
        //Destroy the menu object
        Object.Destroy(_menu.gameObject);
    }
    #endregion
    #region Tests
    //Test 1 - Shark instantiates
    [UnityTest]
    public IEnumerator InstantiateShark()
    {
        //Instantiate the SharkAnim prefab at the right side of screen
        GameObject shark = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/SharkAnim"), new Vector3(13.5f, Random.Range(-6,6), 2f), Quaternion.Euler(0f, 90f, 0f));
        //Wait for half a second to allow adequate time to check
        yield return new WaitForSecondsRealtime(0.5f);
        //Try to find the SharkAnim(Clone) object in scene (Unity adds (Clone) when instantiating objects)
        GameObject sharkTest = GameObject.Find("SharkAnim(Clone)");
        //Use assert to see if sharkTest successfully found the shark in the scene
        Assert.IsNotNull(sharkTest);
        //Use Assert.IsTrue to check if shark resides within the range -6 to 6 on the y axis
        Assert.IsTrue(sharkTest.transform.position.y <= 6f);
        Assert.IsTrue(sharkTest.transform.position.y >= -6f);
        //Use Assert.AreEqual to check if shark is in the correct location on the x and z axis
        Assert.AreEqual(sharkTest.transform.position.x, 13.5f);
        Assert.AreEqual(shark.transform.position.z, 2f);
        //Destroy the shark
        Object.Destroy(shark.gameObject);
    }
    //Test 2 - Bullet kills shark
    [UnityTest]
    public IEnumerator BulletDestroysShark()
    {
        //Instantiate a shark object in the scene at right side of screen
        GameObject shark = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/SharkAnim"), new Vector3(11.7f, 0f, 2f), Quaternion.Euler(0f, 90f, 0f));
        //Instantiate a bullet object slightly left of shark
        GameObject bullet = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Bullet"), new Vector3(9f, 0f, 2f), Quaternion.Euler(0f, 0f, 90f));
        //Store the current value of GameHandlers score so we can see if bullet adds score when killing shark
        int _previousScore = _gameHandler.score;
        //Wait for a second to allow time for bullet to hit shark
        yield return new WaitForSecondsRealtime(1f);
        //Use assert to check if we can find our shark in the scene
        Assert.IsNull(GameObject.Find("SharkAnim(Clone)"));
        //Use assert to check that our score has incremented
        Assert.Greater(_gameHandler.score, _previousScore);
        //Destroy the objects we created
        Object.Destroy(shark.gameObject);
        Object.Destroy(bullet.gameObject);
    }
    //Test 3 - Shark Kills Duck
    [UnityTest]
    public IEnumerator SharkKillsDuck()
    {
        //Setup camera so we can visually see what is happening
        _camera = Camera.Instantiate(Resources.Load("Prefabs/Main Camera"), new Vector3(0, 0, -10), Quaternion.Euler(0, 0, 0));
        //Use GetPlayer function to instantiate a duck object in scene
        DuckMovement _duck = _gameHandler.GetPlayer();
        //Assign our created duck to the GameHandler so shark has correct target
        _gameHandler.duck = _duck.gameObject;
        //Instantiate a shark object at right side of screen
        GameObject _shark = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/SharkAnim"), new Vector3(11f, 0f, 2f), Quaternion.Euler(0f, 90f, 0f));
        //Use GameHandlers SetHighScores function to build a dummy list of high scores so killing duck won't trigger an error
        _gameHandler.SetHighScores();
        //Use assert to check we have a duck in the scene
        Assert.IsNotNull(GameObject.Find("Duck(Clone)"));
        //Store current positions of our game objects
        Vector3 _sharkPosition = _shark.transform.position;
        Vector3 _duckPosition = _duck.transform.position;
        //Store a moveDirection to use to move duck up on the y axis. Divide it by deltaTime so we can get adequate movement as MoveDuck function multiplies by deltaTime
        Vector3 _moveDirection = new Vector3(0f, 3f, 0f) / Time.deltaTime;
        //Use the MoveDuck function to move our duck
        _duck.MoveDuck(_moveDirection);
        //Use an assert to check our duck has moved towards top of screen
        Assert.Greater(_duck.transform.position.y, _duckPosition.y);
        //Store our new position so we can run down test
        _duckPosition = _duck.transform.position;
        //Store another moveDirection to use to move duck down on the y axis.
        _moveDirection = new Vector3(0f, -6f, 0f) / Time.deltaTime;
        //Use the MoveDuck function to move our duck
        _duck.MoveDuck(_moveDirection);
        //Use an assert to check our duck has moved towards bottom of screen
        Assert.Less(_duck.transform.position.y, _duckPosition.y);
        //Wait ten seconds to allow shark adequate time to get to shark
        yield return new WaitForSecondsRealtime(10f);
        //Use an assert to check if shark has moved towards ducks side of screen
        Assert.Less(_shark.transform.position.x, _sharkPosition.x);
        //Use an assert to check if duck has been destroyed by shark. Using IsTrue instead of IsNull as Destroy function makes duck pseudo null instead of null which will fail the IsNull test
        Assert.IsTrue(_duck == null);
        //Destroy objects we created in scene
        Object.Destroy(_duck);
        Object.Destroy(_shark.gameObject);
        Object.Destroy(_camera);
    }
    #endregion
}
