//Standard Unity/C# functionality

using UnityEngine;

//Define our main class
public class SceneManager : MonoBehaviour {
    //Variables we'll need to reference other objects in our game
    public GameObject
        _ballPrefab; //This will store the Ball Prefab we created earlier, so we can spawn a new Ball whenever we want

    public Camera
        _mainCamera; //This will reference the MainCamera in the scene, so the ARDK can leverage the device camera

    // Start is called before the first frame update
    void Start() {
        //ARSessionFactory helps create our AR Session. Here, we're telling our 'ARSessionFactory' to listen to when a new ARSession is created, then call an 'OnSessionInitialized' function when we get notified of one being created
    }

    // Update is called once per frame
    void Update() {
        //If there is no touch, we're not going to do anything
        if (Input.touchCount <= 0) {
            return;
        }

        //If we detect a new touch, call our 'TouchBegan' function
        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began) {
            TouchBegan(touch);
        }
    }

    //This function will be called when the player touches the screen. For us, we'll have this trigger the shooting of our ball from where we touch.
    private void TouchBegan(Touch touch) {
        //Let's spawn a new ball to bounce around our space
        GameObject newBall = Instantiate(_ballPrefab); //Spawn a new ball from our Ball Prefab
        newBall.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f)); //Set the rotation of our new Ball
        newBall.transform.position =
            _mainCamera.transform.position +
            _mainCamera.transform.forward; //Set the position of our new Ball to just in front of our Main Camera

        //Add velocity to our Ball, here we're telling the game to put Force behind the Ball in the direction Forward from our Camera (so, straight ahead)
        Rigidbody rigbod = newBall.GetComponent<Rigidbody>();
        rigbod.velocity = new Vector3(0f, 0f, 0f);
        float force = 300.0f;
        rigbod.AddForce(_mainCamera.transform.forward * force);
    }
}