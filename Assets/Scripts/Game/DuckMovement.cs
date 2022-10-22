using UnityEngine; //Use UnityEngine to connect to Unity
using System.Collections.Generic; //Use System.Collections.Generic to allow use of Lists

public class DuckMovement : MonoBehaviour
{
    #region Variable
    //Create a variable for our move direction
    private Vector3 _moveDir;
    //Set speed of duck movement to 5
    private float _speed = 5f;
    //Create a variable for a CharacterController reference
    private CharacterController _charController;
    //Create a variable for a GameHandler reference
    private GameHandler _gameHandler;
    //Create a list to store the bullets we have fired
    [HideInInspector] public List<GameObject> bulletsInScene = new List<GameObject>();
    [Header("Prefabs")]
    [Tooltip("Add the Bullet prefab from the Resources/Prefabs folder here")]
    public GameObject bulletPrefab;
    #endregion
    #region Functions
    private void Awake()
    {
        //Assign our CharacterController from the duck
        _charController = GetComponent<CharacterController>();
        //Assign the GameHandler from the PinkBG object in the scene
        _gameHandler = GameObject.Find("PinkBG").GetComponent<GameHandler>();
    }
    void Update()
    {
        //Use player input to create a movement direction. P1 Verti is a custom axis for use in Arcade machine
        _moveDir = new Vector3(0, Input.GetAxis("P1 Verti"), 0);
        //Multiply our movement direction by our soeed
        _moveDir *= _speed;
        //Use MoveDuck function to move duck according to our _moveDir
        MoveDuck(_moveDir);
        //If we get input from one of the acceptable buttons. We will use space for PC, and P1 Blue buttons for Arcade Machine
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("P1 B1") || Input.GetButtonDown("P1 B2") || Input.GetButtonDown("P1 B3"))
        {
            //If we are using Challenge Mode
            if (_gameHandler.challengeMode)
            {
                //If our current bullet count is less than double the amount of sharks we can shoot
                if (bulletsInScene.Count < (_gameHandler.sharksInScene.Count * 2))
                {
                    //Instantiate a bullet slightly right of duck at beak level and add it to list of bullets
                    bulletsInScene.Add(Instantiate(bulletPrefab, new Vector3(-9.8f, transform.position.y + 0.365f, 2f), Quaternion.Euler(0f, 0f, 90f)));
                }
            }
            //Else we have no restrictions and can just instantiate the bullet
            else
            {
                Instantiate(bulletPrefab, new Vector3(-9.8f, transform.position.y + 0.365f, 2f), Quaternion.Euler(0f, 0f, 90f));
            }
        }
    }
    public void MoveDuck(Vector3 dir)
    {
        //Use our CharacterControllers Move function to move duck according to information given multiplied by delta time
        _charController.Move(dir * Time.deltaTime);
    }
    #endregion
}

