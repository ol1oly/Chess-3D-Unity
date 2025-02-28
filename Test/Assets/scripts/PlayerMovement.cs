using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
     Rigidbody rb;
     [SerializeField] float speed = 5f;
     [SerializeField] float forceJump = 5f;
     [SerializeField] Transform groundCheck;
     [SerializeField] LayerMask ground;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
       
    }

    // Update is called once per frame
    void Update()
    {
        if(!(PauseMenu.isPaused)){
            float horizontalInput  = Input.GetAxis("Horizontal");
            float verticalInput  = Input.GetAxis("Vertical");
        
            rb.velocity = new Vector3(horizontalInput*speed, rb.velocity.y, verticalInput*speed);
        
            if(Input.GetButtonDown("Jump")  && IsTouchingFloor())
            {
                rb.velocity = new Vector3(rb.velocity.x,forceJump,rb.velocity.z);
            }
            if(Input.GetKeyDown("e"))
            {
                rb.velocity = new Vector3(horizontalInput*speed*10, rb.velocity.y, verticalInput*speed*10);
            }
             if(Input.GetKeyDown("f"))
            {
                rb.position = new Vector3(0, 3, 9);
            }
        }
       
    }
    bool IsTouchingFloor()
    {
       return Physics.CheckSphere(groundCheck.position,.1f,ground);

        
    }


}
