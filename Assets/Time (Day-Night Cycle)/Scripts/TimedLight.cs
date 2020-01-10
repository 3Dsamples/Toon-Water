/// <summary>
/// TimedLight.cs
/// Spidermeowth
/// Feb 20, 2017
/// 
/// This script control any timed light on our game, This works in conjunction with the DayNightControl script
///
/// How to use: Just add this script to any light you want it to turn on at sunset and turn off at dawn
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth 
/// </summary>

using UnityEngine;
using System.Collections;

public class TimedLight : MonoBehaviour {
	
	#region variables region
	private DayNightControl _DayNightControl;      // Reference for DayNightControl Script
	private bool _light;                           // On/Off light switch
	private bool _switchLights;                    // Activator for the lights switcher
	#endregion
	
	// Use this for initialization
	void Awake () {
		#region set initialization  variables		
		// Find our time control
		GameObject Tc = GameObject.FindGameObjectWithTag("Time");
		
		// If Day night control no exist
		 if(Tc == null){
			// Set Light Component true
			GetComponent<Light>().enabled = true; 
			// Turns Off the lights switcher
			_switchLights = false;
			// Set initial light switch state
			_light = true;
			// Send an advice
			Debug.Log("Day/Night control is not present");
		 }
		 
		 else {
			 // Set Light Component
			GetComponent<Light>().enabled = false;
			// Set reference for the DayTame script
			_DayNightControl = Tc.GetComponent<DayNightControl>();
			// Turns On the lights switcher
			_switchLights = true;
			// Set initial light switch state
			_light = !_DayNightControl._IsDay;
		 }		
		#endregion
	}
	
	// Update is called once per frame
	void Update () {
		#region Turning lights on and off
		if(_switchLights){
			// Update our light switch state
			_light = !_DayNightControl._IsDay;
			// Turn On/Off the lights
			GetComponent<Light>().enabled = _light;
		}
		#endregion
	}
}
