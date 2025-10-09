using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;


public class move_freely_Cam : MonoBehaviour
{
    public Camera mainCamera;
    [SerializeField] float speed = 5;
    [SerializeField] float verticalSpeed = 6f;

    

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    public Slider sensX;

    public Slider sensY;

    public Slider speedX;

    public Slider speedY;

    GameObject arrayManager;



    // Start is called before the first frame update
    void Start()
    {
        arrayManager = GameObject.Find("ArrayManager");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;   
        // If mainCamera is not assigned, get the main camera in the scene
        if (mainCamera == null)
        {      
            mainCamera = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool pause = arrayManager.GetComponent<chessArray>().Ui.activeSelf;
        bool end = chessArray.endOfGame;

       // bool pause =false;
       // bool end = false;
        
        if(!pause && !end && !PauseMenu.isPaused){
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;

            // Normalize the forward and right vectors to avoid changes in speed based on direction
            forward.Normalize();
            right.Normalize();

            // Calculate the movement vector
            Vector3 moveDirection = (forward*vertical + horizontal*right);
        
            moveDirection = moveDirection.normalized * speed;
        
            moveDirection.y=0;
        
            if (Input.GetKey(KeyCode.Space))
            {
                moveDirection = moveDirection + Vector3.up * verticalSpeed;
                //transform.Translate(Vector3.up * verticalSpeed * Time.deltaTime, Space.World);
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                moveDirection = moveDirection - Vector3.up * verticalSpeed;
                //transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime, Space.World);
            }
            //Debug.Log(Mathf.Abs((mainCamera.transform.position + moveDirection).magnitude));
            if(Mathf.Abs((mainCamera.transform.position + moveDirection).magnitude) < 228){
                if(mainCamera.transform.position.y + moveDirection.y >-2){
                     mainCamera.transform.position+=moveDirection;
                }
            }
             
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");

            mainCamera.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }
    public void setSensX(){
        speedV = sensX.value;
    }
    public void setSensY(){
        speedV = sensY.value;
    }
    public void setSpeedVertical(){
        verticalSpeed = speedY.value;
    }
    public void setSpeedX(){
        speed = speedX.value;
    }
    
}
