/// <summary>
/// MainCameraControl.cs
/// Spidermeowth
/// Jan 26, 2011
/// 
/// This is a custom Camera control
///
/// How to use:
/// Attach this to the camera in your scene
/// You can select your camera target, by dropping it in the "target" slot of this script, in the inspector tab
/// or if you leave empty the "target" slot, this script will follow by default, your player character tagged as "Player"
///
/// 
/// You will need create this Axis in the Input Manager from Unity:  
/// Mouse x----------------------------Rotate the camera horizontaly with the mouse
/// Mouse y----------------------------Rotate the camera vertically with the mouse
/// Rotate Camera Button---------------Press this button to allow us rotate the camera
/// Rotate Camera Horizontal Button----A Keyboard button to rotate the camera horizontaly
/// Rotate Camera Vertical Button------A Keyboard button to rotate the camera verticaly
/// 
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth
/// </summary>


using UnityEngine;
using System.Collections;

public class MainCameraControl : MonoBehaviour {
	
	#region Variables region
    public GameObject _target;                      // Target for our camera, You can select any part of your player, or the camera look for your player character automatically
	public float _walkDistance = 1.2f;              // Distance at which the camera will follow the player, it must be greater than _shootingWalkDistance
	public float _zoomLimit = 1.0f;                 // Limit distance at which the camera can approach the player, must be less than _shootingWalkDistance
	public float _height = 0.1f;                    // Height position for our camera 
    public float _rotationDamping = 2f;             // Damping speed for our camera will turn behind our character
	public float _heightDamping = 2.0f;             // Damping speed for our camera will folloe our character when he is jumping
	public float xManualSpeed = 250.0f;             // Horizontal speed for the camera manual rotation
	public float yManualSpeed = 120.0f;             // Vertical speed for the camera manual rotation
	
	private Vector3 _camera_Z_Offset;               // This is the offset position behind our character in which our camera will be all time in auto mode	
	private Vector3 Z_viewOffset;                   // This is the offset position that will alternate between its position when turning manually and when its in automatic movement
	private Transform myTransform;                  // transform parameter for the camera
	private float viewSlerp = 0.0f;                 // with this float we interpolates between the position of the camera in automatic mode
	private float _horizontalCorrection = 0.0f;     // is a correction that we will add to our camera to control its position on the z axis
	private float _verticalCorrection = 0.0f;       // is a correction that we will add to our camera to control its position on the y axis
	private float _obsDistance;                     // the distance to which there is an obstacle behind the camera
	private float _ground_Y;                        // the ground's height under the camera
	private float Y_aiming;                         // the calculated correction of the camera on its y axis
	private bool _setCorrection;                    // This switch will indicate when to apply our correction and when not
	
	private bool camButtonDown;             		// Bool switch for the mouse button
	private bool rotateCameraKeyPressed;    		// Bool switch for the rotate camera button
	private float _x;                               // Variable for the horizontal camera movement
	private float _y;                               // Variable for the vertical camera movement
	private float currentAngleX;                    // Temporal store for the current angle in X axis
	private float currentAngleY;                    // Temporal store for the current angle in Y axis
	private float _currentHeight;                   // The current height of our camera
	private float rotSpeed = 0.0f;                  // Speed at which our camera will adjust its position
	private float rotSlerp = 0.0f;                  // This is the speed at which the camera looks in the direction of the target
	
	public Vector3 _panToTarget;                    // Position of our shooting target
	private Vector3 lookPos;                        // Position to which the camera look at
	private Vector3 cameraPosition;                 // our camera transform position
	private Vector3 playerPosition;                 // player character transform position
	private Vector3 playerCenter;                   // the center point of our player characte	
	private float offsetTimer;                      // counter timer to return the camera to its original position
	
	public LayerMask TerrainMask;                   // LayerMask for choose your Land terrain objects
	#endregion

	#region Camera Initialization region	
	void Start() {		
		// Set reference for camera Transform
		myTransform = transform;
		
		// Look for a Player Tag
		GameObject go = GameObject.FindGameObjectWithTag("Player");
		
		// We find the center of our character, this vector3 will serve us later
		// to launch some of our linecast
		// we will search for a character controller and rigidbody, 
		// which are the most common ways to move a character
		
		//first we try to find the center for a character controller
		CharacterController ch = go.GetComponent<CharacterController>();
		if(ch != null){
			// if we find it, we get its center
			playerCenter = ch.center;
			//Debug.Log("Ch Controller:"+playerCenter);
		}
		
		else{
			// if we don't find it, we try to look for a rigidbody
			Rigidbody rb = go.GetComponent<Rigidbody>();
			if(rb != null){
				// if we find it, we get its center
				playerCenter = rb.centerOfMass;
				//Debug.Log("Collider:"+playerCenter);
			}
		}		
		
		// if the target slot is empty
		if(_target == null){
			// Set Player Tag as target
			_target = go;
		}
		
		// if _walkDistance value is zero, set its as 1, To prevent the freeze unity3d error
		if(_walkDistance == 0f){
			_walkDistance = 1f;
		}		
		
		// Set camera the offset position
		// The player will always keep it, no matter in which direction our playeris looking at
		_camera_Z_Offset = new Vector3(0, 0, -_walkDistance);
		
		// Set initial mouse button state
		camButtonDown = false;
		// Set initial rotate camera button state
		rotateCameraKeyPressed = false;
		
		// Set intitial _setCorrection switch
		_setCorrection = true;
		
		// Set initial _panToTarget position
		_panToTarget = _target.transform.position + _target.transform.forward * 50f;
		
	}
	#endregion
	
	// Update is called once per frame
	void Update () {		
		#region update Rotate buttons surpervisor
		// If the "Rotate Camera Button" is pressed
		if(Input.GetButtonDown("Rotate Camera Button")) {
			// Set camButtonDown switch ON
			camButtonDown = true;			
		}
		// If the "Rotate Camera Button" is not pressed
		else if(Input.GetButtonUp("Rotate Camera Button")) {   
			// Set camButtonDown switch OFF
			camButtonDown = false;
		}
		
		// If the "Rotate Camera Horizontal Button" or "Rotate Camera Vertical Button" is pressed
		if(Input.GetButtonDown("Rotate Camera Horizontal Button") || Input.GetButtonDown("Rotate Camera Vertical Button")){
			// Set rotateCameraKeyPressed switch ON
			rotateCameraKeyPressed = true;
		}
		// If the "Rotate Camera Horizontal Button" or "Rotate Camera Vertical Button" is not pressed
		else if(Input.GetButtonUp("Rotate Camera Horizontal Button") || Input.GetButtonUp("Rotate Camera Vertical Button")){
			// Set rotateCameraKeyPressed switch OFF
			rotateCameraKeyPressed = false;
		}
		#endregion
	}
	
	void LateUpdate() {
		#region Setting the limits and positions of different variables of our camera
		// this limit the _y value, To apply it to the rotation of our camera, so don't rotate indefinitely
		_y = Mathf.Clamp(_y, -2.0f, 5.0f);
		// limit the viewSlerp value
		viewSlerp = Mathf.Clamp(viewSlerp, 0.0f, 1.0f);
		
		// Here we will establish some of the positions of our camera, 
		// with which its different movements will be carried out		
		// This Vector3 will indicate the limit to which the camera can approach our player		
		Vector3 virtualLimit = ((playerPosition+playerCenter) - _target.transform.forward * _zoomLimit);
		// This Vector3 would be a virtual indicator of the position of the camera in it standard position behind the player
		Vector3 cameraVirtualPos = ((playerPosition+playerCenter) - _target.transform.forward * _walkDistance);
		// this will be the distance we will have between our virtualLimit and our cameraVirtualPos
		float closeViewDist = Vector3.Distance(virtualLimit, cameraVirtualPos);
		
		// this float This will be the difference between the distance we have between our normal position behind the player, 
		// and the virtual limit from which we can not get closer.
		float viewOffset_Z = _walkDistance-closeViewDist;
		
		
		// with the Z_viewOffset, we interpolates between the position of the camera in automatic mode 
		// and the one it occupies when activating the manual rotation
		Z_viewOffset = Vector3.Lerp(_camera_Z_Offset, new Vector3(0f, 0f,-viewOffset_Z), viewSlerp*2f);
		//Debug.Log(Z_viewOffset);
		#endregion
				
		# region For manual rotation of our camera	
		// Check if Horizontal and/or vertical Manual Rotate Camera Buttons is/are pressed
		if(rotateCameraKeyPressed || camButtonDown){
			// we increase the value of viewSlerp
			viewSlerp += Time.deltaTime;
			
			// if the rotation of the camera is controlled with the keyboard
			if(rotateCameraKeyPressed){			
				// Set _x and _y valor to rotate our camera
				_x += Input.GetAxis("Rotate Camera Horizontal Button") * xManualSpeed * 0.02f;
				_y += Input.GetAxis("Rotate Camera Vertical Button") * yManualSpeed * 0.001f;
				// call rotate camera function
				RotateCamera(_x,_y);
			}
			
			// if the rotation of the camera is controlled with the mouse
			else if(camButtonDown){				
				// Set _x and _y valor to rotate our camera
				_x += Input.GetAxis("Mouse X") * xManualSpeed * 0.008f;
				_y += Input.GetAxis("Mouse Y") * yManualSpeed * 0.0008f;
				// call rotate camera function		
				RotateCamera(_x,_y);
			}
		}
		#endregion
		
		#region for auto rotation of our camera
		// if the camera moves automatically
		else {
			
			// we decrease the value of viewSlerp
			viewSlerp -= Time.deltaTime;
			
			// first reset _x and _y valor to 0, for the "RotateCamera" function
			_x=0;
			_y=0;
			
			#region Smooth rotation
			// To smooth our camera rotation:
			// Get the current angle of the our camera over y axis
			float _currentAnglex = myTransform.eulerAngles.y;
			// Get the current angle of the our camera over x axis
			float _currentAngley = myTransform.eulerAngles.x;

			// Get the angle of the target
			float _desiredAngle = _target.transform.eulerAngles.y;
			// Lerp between the angle of the camera and the angle of the target		
			float _horizontalAngle;
			float angle = 120f;
			
			// This vector is the relative position where we should look
			Vector3 relativePos = (myTransform.position - _target.transform.position).normalized;
			
			// we get the angle between camera and our objective, over the horizontal plane
			float camAngle = Vector3.Angle(_target.transform.forward, new Vector3(relativePos.x, _target.transform.position.y ,relativePos.z));
			
			//Debug.Log(camAngle);
			
			// if target is directly facing the camera, we increase rotSpeed
			if(camAngle < angle){
				// we increase rotSpeed
				rotSpeed += Time.deltaTime*0.1f;
				// we increase rotSlerp
				rotSlerp += Time.deltaTime*0.15f;
			}
			
			// or if target is with his back to the camera (as he'playerPosition normally moving), we use our usual speed			
			else{
				//we decrease rotSpeed slowly
				rotSpeed -= Time.deltaTime*0.05f;
				//we decrease rotSlerp slowly
				rotSlerp -= Time.deltaTime*0.1f;
			}
			
			// we clamp rotSpeed value
			rotSpeed =  Mathf.Clamp(rotSpeed, 0.1f, 0.1f*_rotationDamping);
			// we clamp rotSlerp value
			rotSlerp =  Mathf.Clamp(rotSlerp, 0.07f, 0.5f);
			

			//Debug.Log(rotSpeed);
			//Debug.Log(rotSlerp);
			
			// and finally we Lerp our _horizontalAngle
			_horizontalAngle = Mathf.LerpAngle(_currentAnglex, _desiredAngle, rotSpeed);
			
			// Then we turn it into our desired rotation
			Quaternion _desiredRotation = Quaternion.Euler(0, _horizontalAngle, 0);
			// multiply the _desiredRotation by the Z offset 
			//Vector3 _currentRotation = _desiredRotation * _camera_Z_Offset;
			Vector3 _currentRotation = _desiredRotation * Z_viewOffset;

			// Make the current angle of our camera, the same angle to start manual rotating
			currentAngleX = _currentAnglex;			
			currentAngleY = _currentAngley;
			#endregion			
			
			# region set the new rotation and position of our camera			
			// The camera always look at target, this vector is the relative to our current camera _target
			lookPos = ((_target.transform.position - myTransform.transform.position) + _target.transform.forward.normalized)/2f;
			
			// We get the new rotation, at we want to look at, in this case towards our target
			Quaternion rotation = Quaternion.LookRotation(lookPos.normalized);			
			
			// We make a slerp, to turn smoothly and avoid disorienting us
			myTransform.rotation = Quaternion.Slerp(myTransform.rotation, rotation, rotSlerp);
			
			// and we restrict the the z-axis rotations, to avoid any weird camera movements
			Quaternion z = myTransform.rotation;
			z.eulerAngles = new Vector3(z.eulerAngles.x, z.eulerAngles.y, 0f);
			myTransform.rotation = z;
			
			// We add the current rotation to the position of the target and use our smooth height to the position of our camera 
			myTransform.position = new Vector3(_target.transform.position.x + _currentRotation.x, _currentHeight, _target.transform.position.z + _currentRotation.z);						
			#endregion
		}
		#endregion
		
		
		#region call different funtions region
		// Here we are going to call a series of void, to activate and/or deactivate 
		// different functions of our camera
		// We will use different functions, just to make it easier to locate what happens in each one, 
		// and not to make this part of the script very long and confusing
		
		// first let's set, the variables that we will used in general in ourfunction
		// Set our cameraPosition posiition
		cameraPosition = myTransform.position;
		// Set our playerPosition position
		playerPosition = _target.transform.position;
		
		// prevent wall collision
		WallCorrection();

		// Smooth current camera height
		CurrentHeight();
		
		// keep camera`s height
		KeepHeight();
		#endregion
	}
	
	#region Rotate camera function
	// Set the rotation and position for the camera
	private void RotateCamera(float x, float y){
		float X = x;
		float Y = y;
		
		// this limit the Y value, To apply it to the rotation of our camera, so don't rotate indefinitely 
		Y = Mathf.Clamp(Y, -2.0f, 5.0f);

		// This Quaternion give us the starting angle to beging rotation		
		Quaternion rotation = Quaternion.Euler(currentAngleY, X + currentAngleX, 0);		
		// This Vector3 give us the starting position to beging rotation
		Vector3 position = rotation * Z_viewOffset + _target.transform.position;
		
		// This vector3 is the direction in which our camera will focus
		Vector3 lookPos = new Vector3(_target.transform.position.x, _currentHeight + Y, _target.transform.position.z) - myTransform.transform.position;
		// We get the new rotation at we want to look at, in this case towards our target
		Quaternion lookRot = Quaternion.LookRotation(lookPos.normalized);	
		
		// Update the position and rotation, when the camera is rotating
		myTransform.rotation = Quaternion.Slerp(rotation, lookRot, viewSlerp);
		myTransform.position = new Vector3(position.x, _currentHeight, position.z);						
	}
	#endregion
	
	#region Smooth current height
	public void CurrentHeight(){		
		// To smooth our camera height:
		// Get the height of our camera
		_currentHeight = myTransform.position.y;
		// Get the height of our target
		float _desiredHeight;
		// if the height of our target is less than the point under our camera 
		if(_ground_Y > _target.transform.position.y){
			// we use the ground height as _desiredHeight
			_desiredHeight = _ground_Y + _height + _verticalCorrection;
		}
		
		// if not
		else {
			// we use the _target height as _desiredHeight
			_desiredHeight = _target.transform.position.y + _height + _verticalCorrection;
		}
		
		// lerp between the height of the camera and the height of the target		
		_currentHeight = Mathf.Lerp (_currentHeight, _desiredHeight, _heightDamping * Time.deltaTime );				
	}
	#endregion
	
	
	#region prevent wall collision
	public void WallCorrection(){
		#region prevent wall collision
		// this linecast we will always do, regardless of whether we are pointing to something or not
		// This will tell us if we should move the position of our camera, to avoid that there is any wall or tree, 
		// or any other element that does not allow us to have a clear vision
		// this basically works, as if it were a rod that measures how deep we are behind some obstacle
		// this Linecast starts from our player position, and ends in the position of our camera, 
		// in the event that it never moved from its original offset
		
		// Set the hypothetical position of our camera
		Vector3 cameraV3 = new Vector3(_target.transform.position.x, _target.transform.position.y + _height, _target.transform.position.z) + ((new Vector3(myTransform.position.x, _target.transform.position.y + _height, myTransform.position.z) - new Vector3(_target.transform.position.x, _target.transform.position.y + _height, _target.transform.position.z)).normalized * _walkDistance);

		// we found the central point of our player character,
		// with this we avoid that the camera approaches with the smallest inclination of the ground,
		// what we are looking for, is that the camera moves only when there is something higher behind us
		Vector3 playerV3 = playerPosition + playerCenter;
		
		//Debug.DrawLine(playerV3, cameraV3, Color.red);
		//Debug.DrawLine(cameraPosition, cameraPosition - myTransform.forward, Color.yellow);
		//Debug.DrawLine(cameraPosition, cameraPosition - Vector3.up *10f, Color.green);
		
		//the output to the information generated by the hit of our linecats
		RaycastHit Obstacle;
		// we check if we hit something
		if(Physics.Linecast(playerV3, cameraV3, out Obstacle, TerrainMask.value)){
			// we get the distance from our obstacle
			_obsDistance = Obstacle.distance;
			
			// Once detected some obstacle, we do a second linecast towards the back of the camera,
			// simply to know if the camera is already on the other side of this
			RaycastHit hit;
			if(Physics.Linecast(cameraPosition, cameraPosition - myTransform.forward, out hit, TerrainMask.value)){
				// if we detect something we stop the increase of the _horizontalCorrection
				// by set the _setCorrection switch, Off			
				_setCorrection = false;
			}
			
			else {
				// if not, we simply let the variable increase continuously
				// by keep the _setCorrection switch, On
				_setCorrection = true;
			}
			
			// we change the _horizontalCorrection value, only if the _setCorrection switch is on			
			if(_setCorrection){
				_horizontalCorrection += Time.deltaTime * 100f;
			}
		}
		
		// in case we don't hit anything
		else {
			// set the distance from our obstacle as zero
			_obsDistance = 0f;
			
			// return the _setCorrection switch to On, to keep it ready for the next time we need it
			_setCorrection = true;
			
			// if _horizontalCorrection has been increased before
			if(_horizontalCorrection > 0){
				// we decrease _horizontalCorrection value, to return it to zero
				_horizontalCorrection -= Time.deltaTime;
			}
			
		}
		
		// we clamp the _horizontalCorrection max and min values
		_horizontalCorrection =  Mathf.Clamp(_horizontalCorrection, 0.0f, _walkDistance - (_obsDistance + 0.1f));
		// we establish our _verticalCorrection based on our _horizontalCorrection
		_verticalCorrection = _horizontalCorrection * 0.4f;
		#endregion
	}
	#endregion
	
	#region Keep Camera's height
	public void KeepHeight(){
		#region keep camera`s height
		//the output to the information generated by the hit of our Raycats
		RaycastHit Ground;
		// we do a Raycast in the ground direction
		if(Physics.Raycast(cameraPosition, -Vector3.up, out Ground, Mathf.Infinity, TerrainMask.value)){
			// and we get the Y position of the point below our camera
			_ground_Y = Ground.point.y;
			//Debug.Log(Ground.transform.name+" distance ="+Ground.distance+" "+_ground_Y);
		}
		#endregion
	}
	#endregion
}
