/// <summary>
/// MainCameraControl.cs
/// Spidermeowth
/// Sep 25, 2011
/// 
/// This script moves the player character, for this demo
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth


using UnityEngine;
using System.Collections;




public class Move : MonoBehaviour {

    CharacterController characterController;

    public float speed = 6.0f;
    //public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;	
	public GameObject leftWheel;
	public GameObject rightWheel;
	public GameObject backWheel;
	public float wheelsSpeed = 160f;
    private Vector3 moveDirection = Vector3.zero;
	

    void Start() {
		// Acces to CharacterController component
        characterController = GetComponent<CharacterController>();
    }

    void Update(){
		
        if (characterController.isGrounded) {
            // if we are grounded,
           			
			// Make it rotate
			transform.Rotate(0, Input.GetAxis("Horizontal") * speed * 0.5f, 0);
            // Move in axes direction
			moveDirection = new Vector3(0.0f, 0.0f, Input.GetAxis("Vertical"));
			moveDirection = transform.TransformDirection(moveDirection).normalized;
            moveDirection *= speed;
			

            //if (Input.GetButton("Jump")) {
				// Make it jump
                //moveDirection.y = jumpSpeed;
            //}
        }

        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
		
		// Wheels's rotation		
		if(Input.GetAxis("Vertical")!= 0 && Input.GetAxis("Horizontal")== 0){
			leftWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
			rightWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
			backWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
		}
		
		else if(Input.GetAxis("Vertical")== 0 && Input.GetAxis("Horizontal")!= 0){
			leftWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Horizontal"), 0, 0);
			rightWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * -Input.GetAxis("Horizontal"), 0, 0);
			backWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
		}	
		
		else if(Input.GetAxis("Vertical")>= 0 && Input.GetAxis("Horizontal")<= 0){
			leftWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical") * 0.5f, 0, 0);
			rightWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
			backWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
		}
		
		else if(Input.GetAxis("Vertical")>= 0 && Input.GetAxis("Horizontal")>= 0){
			leftWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
			rightWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical") * 0.5f, 0, 0);
			backWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
		}
		
		else if(Input.GetAxis("Vertical")<= 0 && Input.GetAxis("Horizontal")<= 0){
			leftWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
			rightWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical") * 0.5f, 0, 0);
			backWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
		}
		
		else if(Input.GetAxis("Vertical")<= 0 && Input.GetAxis("Horizontal")>= 0){
			leftWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical") * 0.5f, 0, 0);
			rightWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
			backWheel.transform.Rotate(Time.deltaTime* wheelsSpeed * Input.GetAxis("Vertical"), 0, 0);
		}	
		
    }
}
