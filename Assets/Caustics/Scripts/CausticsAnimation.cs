/// <summary>
/// CausticsAnimation.cs
/// Spidermeowth
/// Jul 10, 2019
/// 
/// This script use projector component to to project caustic lights underwater
/// How use:
/// 1) Just add this script to an empty gameobject that will be your projector
/// 2) Add Caustics frames, and adjust the parameters in the inspector as best suits you
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Projector))]
 
public class CausticsAnimation: MonoBehaviour {
 
	public float _height = 5f;        // Projector's height over the player character
	public float _fieldOfView = 90f;  // Projector's field of view value
	public float fps = 30.0f;         // frames per seconds
	public Texture2D[] frames;        // Caustics images
	 
	private int frameIndex;           // Frames index
	private Projector projector;      // Projector GameObject
	
	private GameObject _player;       // Reference for Player Character
	private Transform _myTransform;   // Reference for Transform
 
	void Start(){
		// Search for Player
		_player = GameObject.FindGameObjectWithTag("Player");
		
		// Set reference for Transform
		_myTransform = transform;
		
		// Set Projector component reference
		projector = GetComponent<Projector> ();
		// Set Projector´s field of view
		projector.fieldOfView = _fieldOfView;
		
		if(frames.Length > 0){
			// call NextFrame function
			NextFrame();
			// Invokes NextFrame function in 1 second, then repeatedly every 1 seconds.
			InvokeRepeating("NextFrame", 1f/fps, 1f /fps);
		}
		
		else{
			Debug.Log("You need add frames textures to frames array in inspector");
		}
	}
	 
	void NextFrame(){
		// Set Projector Material
		projector.material.SetTexture ("_ShadowTex", frames [frameIndex]);
		
		// Update next frameIndex value
		frameIndex = (frameIndex + 1) % frames.Length;
	}
	
	
	void LateUpdate() {
		// Make projector follow the player character
		_myTransform.position = new Vector3(_player.transform.position.x,_player.transform.position.y + _height,_player.transform.position.z);
		// and look at player character
		_myTransform.LookAt(_player.transform);
	}
 
}
