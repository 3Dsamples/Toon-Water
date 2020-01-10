/// <summary>
/// WaterAnimation.cs
/// Spidermeowth
/// Jul 03, 2019
/// 
/// This is responsible for the animation and simulation of water volume,
/// this works together with the "ProjectorLight + Altitude/, "ToonShading + Altitude Fog" and "ToonWater" custom shaders
/// How use:
/// 1) Just add this script to your main camera or your player character
/// 2) Only adjust the parameters and color in the inspector as best suits you
/// 3) add a number and the textures you need in the frames slots for surface animation
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WaterAnimation : MonoBehaviour {
	
	#region Variables
	public float _WaveSpeed = 1f;                             // Wave Speed
	public float _WaveAmp = 0.1f;                             // Wave Amplitude
	private float vertex_Y;                                   // vextex y value
	private float x = 0f;                                     // x value	
	public float _FogMaxHeight = 0.0f;                        // initial Max height to use in shaders
	public float _FogMinHeight = -1f;                         // initial Min height to use in shaders
	public Color _FogColor = new Color32(94, 173, 235, 255);  // Fog color to use in shaders
	private List<GameObject> _Waters;                         // GameObjects List, Of possible water
	private GameObject _water;                                // Set reference for the water mesh
	private bool _waterOn;                                    // This bool tells us if there is water or not
	private float waterheight;                                // Defines the height at which the water surface is located
	private  float _heighDifference;                          // store for height difference	
	private Vector3 _max;                                     // Store for collider max value
	private Vector3 _min;                                     // Store for collider min value
	private List<GameObject> _Animation;                      // a copy of the list of water plans
	public Texture2D[] frames;                                // Texture images
	public float fps = 20.0f;                                 // frames per seconds
	private int frameIndex;                                   // Frames index	
	#endregion
	

	// Use this for initialization
	void Start () {
		// we initialize our list
		_Waters = new List<GameObject>();
		// we search for Search for any water object
		_Waters = new List<GameObject>(GameObject.FindGameObjectsWithTag("Water"));
		
		// we initialize our list
		_Animation = new List<GameObject>();
		// we copy the contents of the list
		_Animation = new List<GameObject>(_Waters);
		
		// we always need a negative value
		if(_FogMinHeight > 0){
			_FogMinHeight *= -1f;
		}
		
		// Store initial Height difference value
		_heighDifference = _FogMaxHeight - _FogMinHeight;
		
		
		// if we find any
		//if (_Waters.Length > 0){
		if (_Waters.Count > 0){	
			// Water exist
			_waterOn = true;
			
			//for(int i = 0; i < _Waters.Count; i++){	
				//Debug.Log(_Waters[i].name+" Level:"+_Waters[i].transform.position.y);
			//}
			
			// Set global _FogMaxHeight in shaders
			Shader.SetGlobalFloat("_FogMaxHeight", _FogMaxHeight);
			// Set global _FogMinHeight in shaders
			Shader.SetGlobalFloat("_FogMinHeight", _FogMaxHeight - _heighDifference);
			// Set global _FogColor in shaders
			Shader.SetGlobalColor("_FogColor", _FogColor);
		}
		
		//if we can't find any
		else {
			// we don't have water
			_waterOn = false;
			
			Debug.Log("No water, you don't need underwater effect");
			
			// Set global _FogMaxHeight in shaders
			Shader.SetGlobalFloat("_FogMaxHeight", -10000f);
			// Set global _FogMinHeight in shaders
			Shader.SetGlobalFloat("_FogMinHeight", -10000f);
			// Set global _FogColor in shaders
			Shader.SetGlobalColor("_FogColor", new Color32(128, 128, 128, 255));
		}
		
		// Set initial min values
		_min = new Vector3(-10000f,0f,-10000f);
		// Set initial max values
		_max = new Vector3(10000f,0f,10000f);
		
		
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
	
	// Update is called once per frame
	void Update () {
		
		// if we find any water 
		if (_waterOn){
			#region Create Wave animation
			x += Time.deltaTime * 0.1f;
			vertex_Y = Mathf.Sin(x * _WaveSpeed) * _WaveAmp;
			
			// Set global float _vertex_Y in shaders
			Shader.SetGlobalFloat("_vertex_Y", vertex_Y);
			#endregion

			#region look for the closets water surface
			// if we have more than one water plane
			if(_Waters.Count > 1f){
				// We ordered our list based on two criteria, first comparing its vertical distance with us, 
				// and second comparing its horizontal distance, the closest ones based on these two parameters
				// will go to the top of the list, This is the reason why we add it to the camera or player			 
				_Waters = _Waters.OrderBy(waters =>  Vector3.Distance(transform.position, new Vector3(transform.position.x, waters.transform.position.y, transform.position.z))).ThenBy(waters =>  Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), new Vector3(waters.transform.position.x, 0f, waters.transform.position.z))).ToList();	
				// Set the closet one as water
				_water = _Waters[0];
			}
			
			else{
				// or juat set as _water, our only plane
				_water = _Waters[0];
			}
			
			// set his y position as waterheight
			waterheight = _water.transform.position.y;
			
			// Get max and min
			_max = _water.GetComponent<Renderer>().bounds.max;
			_min = _water.GetComponent<Renderer>().bounds.min;
			
			// update fog heights
			_FogMaxHeight = waterheight;
			_FogMinHeight = waterheight - _heighDifference;
			
			// Set globals in shaders
			Shader.SetGlobalFloat("_FogMaxHeight", _FogMaxHeight);
			Shader.SetGlobalFloat("_FogMinHeight", _FogMinHeight);			
			Shader.SetGlobalVector("_max",new Vector4(_max.x ,_max.y, _max.z, 0f));
			Shader.SetGlobalVector("_min",new Vector4(_min.x ,_min.y, _min.z, 0f));
			#endregion
		}
	}
	
	void NextFrame(){
		
		// We animate each item on our list
		for (int i = 0; i < _Animation.Count; i++) {
			// Animate texture
			_Animation[i].GetComponent<Renderer>().material.SetTexture("_SurfaceFoam", frames [frameIndex]);
		}
	
		// Update next frameIndex value
		frameIndex = (frameIndex + 1) % frames.Length;
	}
}
