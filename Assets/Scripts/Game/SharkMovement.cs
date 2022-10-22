using UnityEngine; //Use UnityEngine to connect to Unity

public class SharkMovement : MonoBehaviour
{
    #region Variables
    //Create a variable for our duck to use as target
    private GameObject _duck;
    //Create a variable for a GameHandler reference
    private GameHandler _gameHandler;
    //Create a variable to use as our movement direction
    private Vector3 _moveDir;
    #endregion
    #region Functions
    private void OnEnable()
    {
        //Assign the GameHandler from our PinkBG object to the reference
        _gameHandler = GameObject.Find("PinkBG").GetComponent<GameHandler>();
        //Get the duck reference from the GameHandler to use as target
        _duck = _gameHandler.duck;
    }
    private void Update()
    {
        //If duck still exists we can move
        if (_duck != null)
        {
            //Store the direction to our target duck by taking away our shark position from its position
            _moveDir = _duck.transform.position - transform.position;
            //Normalize the direction so the max distance is 1. This allows us to apply our speed correctly
            _moveDir.Normalize();
            //Move our shark towards duck according to speed set in GameHandler
            transform.position += _moveDir * _gameHandler.sharkSpeed * Time.deltaTime;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //If we run into the players duck destroy it and trigger the EndGame function
        if (collision.collider.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
            _gameHandler.EndGame();
        }
        else if (collision.collider.CompareTag("Shark"))
        {
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
    #endregion
}
