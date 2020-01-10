/// <summary>
/// DayNightControl.cs
/// Spidermeowth
/// Feb 09, 2017
/// 
/// This control the Day and Night cycle in the game.
///
/// How to use:
/// 1)On the tab Window>Lighting>Evironment Lighting set set the following boxes
///    a)In skybox material choose "Blended Sky" as meterial
///    b)in Sun choose "None"
///    c)Ambient Source choose "Color"
///    d)Ambient Color set as "R=204, G=230, B=255, A=255"
///    e)Ambient Intensity set as "1"
/// 2)Add this script to any empty gameobject in your scene and turn it into the day/night control
/// 3)Create the tag "MainLight" and tag your main light on the scene, or just drop it, in "myLight" slot in the inspector
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth 
/// </summary>

using UnityEngine;
using System.Collections;

public class DayNightControl : MonoBehaviour {
	
	#region mini state machine events
	//List fot day events
	private enum TimeOfDay {
		Idle,
		Sunrise,
		Sunset,
	}
	#endregion
	
	#region Time Variables region
	public bool _IsDay;                                      // This switch tell us if it's day or night
	public float dayCycleInMinutes = 1f;                     // Duration for our day in minutes
	public float _sunRiseHour = 6.0f;                        // Here we specify our Sunrise time
	public float _sunSetHour = 18.0f;                        // Here we specify our Sunset time
	[Range(0, 23)]public int StartTimeInHours = 12;          // Tells us what time of day, the clock starts, The value is entered on a 24-hour scale
	public float _hours;                                     // Our current hour in the game
	public float _minutes;                                   // And our current minutes in the game
	
	private float _currentHour;                              // This is our current time in 24 hrs format		
	private float _timeofDay;                                // The time of our day
	private float _dayCycleInSeconds;                        // The time duration of day in real seconds
	#endregion
	
	#region Sky blend variables region
	private TimeOfDay _tod;                                  // reference for our day events state machine
	private float t = 0f;                                    // This variable is in charge of lerp _blend value

	public float skyboxBlendModifier = 4;                    // Speed for the Skybox texture blend	
	public Color ambLightDay = new Color32(204,230,255,255); // Ambient light color for the day
	public Color ambLightNight = new Color32(13,84,158,255); // Ambient light color for the night
    public string myLightTag = "MainLight";                  // Use this name or change it for  your light label
	public Transform myLight;                                // Reference for directional light, or any other light type  in our scene
	private Light lt;                                        // Reference to access the light component of myLight
	public float _blend;                                     // Reference to public acces this value for fading _blend value
	public float _intensity;                                 // Reference to public acces this value for fading _Intensity lights value
	#endregion
	
	void Awake() {
		#region Setiing Initial values
		// Initialize State for the time day
		if(StartTimeInHours > _sunRiseHour && StartTimeInHours < _sunSetHour ) {
			// initial _tod
			_tod = DayNightControl.TimeOfDay.Sunrise;
			// set initial t for Skybox blend
			t = 0f;
		}
		
		else {
			// initial _tod
			_tod = DayNightControl.TimeOfDay.Sunset;
			// set initial t for Skybox blend
			t = 1f;
		}
		
		// if myLight slot is empty
		if(myLight == null){
			// Search for "MainLight" tag in the scene
			GameObject mL = GameObject.FindGameObjectWithTag("MainLight");
			
			if(mL == null){
				Debug.Log("Create the tag ++MainLight++ and tag your main light like that, or drop your main light in ++myLight++ slot in the inspector");
			}
			
			else {
				// set that light as my light
				myLight = mL.transform;
			}
		}
		
		// The initial value for our day or night indicator
		_IsDay = false;
		
		// Set the day cycle in seconds
		_dayCycleInSeconds = dayCycleInMinutes * 60f;
		
		// Set initial hour for our day
		_timeofDay = (StartTimeInHours * _dayCycleInSeconds)/24;	
		
		// Set initial light intensity
		if(myLight != null){
			// get acces to myLight Light component			
			lt = myLight.GetComponent<Light>();
		}
		// call Blendsky, to set initial _blend and _intensity values
		BlendSky();	
		#endregion
	}
	
	// Update is called once per frame
	void Update () {
		
		// Turn our time in seconds, to current hour
		_currentHour = ((_timeofDay*24)/_dayCycleInSeconds);
		_hours = (int)_currentHour;
		_minutes = (int)((_currentHour - _hours)*60);
		//Debug.Log(_hours+":"+_minutes+" Day:"+_IsDay+" "+_sunRiseHour+"/"+_sunSetHour+" TOD:"+_tod+" blend:"+_blend+" t:"+t);
		
		#region time clock region
		// This is our time clock, update the time of day
		_timeofDay += Time.deltaTime;
		
		// reset the day time, When our 24 hour cycle is fulfilled		
		if(_timeofDay > _dayCycleInSeconds)
			_timeofDay -= _dayCycleInSeconds;
		#endregion
		
		#region timed lights triggers
		// If it was night, and the sun has risen
		if(!_IsDay && _currentHour > _sunRiseHour && _currentHour < _sunSetHour){	
			// Use this switch, to activate anything that happens at dawn
			//It's day time
			_IsDay = true;
			//Debug.Log("Sunrise");
		}
		
		// Or if it was day, and the sun is set
		else if(_IsDay && _currentHour > _sunSetHour) {
			// Use this switch, to activate anything that happens at nightfall
			//It's night time
			_IsDay = false;
			//Debug.Log("Sunset");
		}
		#endregion
		
		#region BlendSky triggers
		// Blend Sky for the Sunrise		
		if(_IsDay){
			// Set time of day state
		 	_tod = DayNightControl.TimeOfDay.Sunrise;
			// and change the Sky
		  	BlendSky();
		}
		
		else if (!_IsDay) {
			// Set time of day state
		    _tod = DayNightControl.TimeOfDay.Sunset;
			// and change the Sky
	        BlendSky();
		}
		#endregion		
	}
	
	#region Blend Sky region
	private void BlendSky() {		
		// if myLight exist
		if(myLight != null){
			// Initial _intensity value
			_intensity = 1f;
		}
		
		// This mini state machine will fade our Skybox, ambient light and light intensity
		switch(_tod) {			
			// For Sunrise	
			case TimeOfDay.Sunrise:
				// Moves the value from t to zero		
				if(t > 0.0f){
					t= 1-(_currentHour - _sunRiseHour);
				}
				break;
				
			// For Sunset	
			case TimeOfDay.Sunset:
				// Return the t value to one
				if(t < 1.0f){
					t = _currentHour -_sunSetHour;
				}			
				break;		
		}

		// Limit the range that we can increase t
		t = Mathf.Clamp(t, 0.0f, 1.0f);		
		// Lerp _blend based on the value of t
		_blend = Mathf.Lerp(0f, 1.0f, t);
		
		// Change light intensity if myLight is not null
		if(myLight != null){
			// Change the intensity of our scene light
			_intensity = 1 - _blend;			
			// Limit the max and min intensity 
			_intensity = Mathf.Clamp(_intensity, 0.4f, 1.0f);
		}
		
		// Change the Skybox, by update the _Blend value in SkyBoxBlended shader
		RenderSettings.skybox.SetFloat("_Blend",_blend);		
		// Change the ambient Light, by interpolates between day and night colors
		RenderSettings.ambientLight = Color.Lerp(ambLightDay, ambLightNight, _blend);		
		
		// check if myLight exist
		if(myLight != null){
			// if exist change the light intensity it			
			lt.intensity = _intensity;
		}		
	}
	#endregion
	
	
}
