using UnityEngine; //Use UnityEngine to connect to Unity

public class BulletHandler : MonoBehaviour
{
    #region Variables
    //Set speed of shark
    private float _speed = 9f;
    //Assign a GameHandler variable
    private GameHandler _gameHandler;
    #endregion
    #region Functions
    private void Start()
    {
        //If our gamehandler reference is empty assign it from the PinkBG object in scene
        if (_gameHandler == null)
        {
            _gameHandler = GameObject.Find("PinkBG").GetComponent<GameHandler>();
        }
    }
    void Update()
    {
        //Destroy the bullet if it makes it past right side of screen
        if (transform.position.x > 12f)
        {
            Destroy(gameObject);
        }
        //Move the bullet towards right side of screen according to speed set
        transform.position += new Vector3(_speed * Time.deltaTime, 0f, 0f);

    }
    private void OnCollisionEnter(Collision collision)
    {
        //If we collide with something destroy it, add score to our GameHandler and the destroy bullet
        Destroy(collision.gameObject);
        if (_gameHandler.challengeMode == false)
        {
            _gameHandler.score++;
        }
        else
        {
            _gameHandler.score += 3;
        }
        Destroy(gameObject);
    }
    #endregion
}
