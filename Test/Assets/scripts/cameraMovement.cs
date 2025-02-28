using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
     Rigidbody rb;
     private Camera mainCamera;
     [SerializeField] float speed = 5f;
     [SerializeField] float forceJump = 5f;
     [SerializeField] Transform groundCheck;
     [SerializeField] LayerMask ground;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
       
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput  = Input.GetAxis("Horizontal");
        float verticalInput  = Input.GetAxis("Vertical");

        Vector3 forwardDirection = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;
        Vector3 rightDirection = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z).normalized;
        
        Vector3 movement = Vector3.zero; 

        movement = (forwardDirection * verticalInput + rightDirection * horizontalInput).normalized * speed;

       
        //Debug.Log(rb.velocity);


       // rb.velocity = new Vector3(horizontalInput*speed, rb.velocity.y, verticalInput*speed);
        rb.velocity = new Vector3(movement.x,rb.velocity.y,movement.z);    
        

        
        if(Input.GetButtonDown("Jump")  && IsTouchingFloor())
        {
            Debug.Log("jump");
            rb.velocity = new Vector3(rb.velocity.x,forceJump,rb.velocity.z);
        }
        
        
       
    }
    bool IsTouchingFloor()
    {
       return Physics.CheckSphere(groundCheck.position,.1f,ground);

        
    }


}
