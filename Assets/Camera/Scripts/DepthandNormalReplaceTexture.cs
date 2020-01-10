/// Spidermeowth
/// Mar 20, 2019
///
///
/// This script is used to replace the normal and depth texture. 
/// this generates some as we can call like a fake Depth and Normal Texture.
/// it stores the output depth to a global shader texture named "_CameraDepthTexture" available to all shaders.
/// and the normal to a global shader texture named "_CameraNormalsTexture", available to all shaders too.
/// with this is possible use of Depth and Normal Texture within the Unity free version
/// I tested it in my free version of unity 5.2.1f and it worked like charm, allowing me to create a shoreline
/// using this fake Depth texture, which otherwise is only available for the paid version of unity
///
/// How this work, just apply this script to the Camera object in the scene. 
/// As well, drag the "Depth Shader" into the Depth shader slot.
/// and the "Normals Shader" into the Normals shader slot. 
/// If you run the scene, you'll see that two new cameras, "Depth Camera" and "Normals Camera" automatically spawned 
/// the two as a child to the main camera. If you select any of this objects, you can see the Depth or the Normal texture being rendered 
/// in the Camera preview of the object.
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth

using UnityEngine;
using System.Collections;

public class DepthandNormalReplaceTexture : MonoBehaviour {

	public Shader depthShader;             // This is the reference for our Depth Shader
	public Shader normalsShader;           // This is the reference for our Normals Shader
	public LayerMask cullingMask;          // Camera Culling mask,with this we can select which layer we want to render in our texture
	private RenderTexture depthTexture;    // Our Depth RenderTexture object.
	private RenderTexture normalTexture;   // Our Normal RenderTexture object.
    private Camera depthCamera;            // reference for our new Depth Camera    
    private Camera normalCamera;           // reference for our new Normal Camera

    private void Start() {		
        Camera thisCamera = GetComponent<Camera>();

        // Create a render texture matching the main camera's current dimensions.
        depthTexture = new RenderTexture(thisCamera.pixelWidth, thisCamera.pixelHeight, 24);
		// Create a render texture matching the main camera's current dimensions.
        normalTexture = new RenderTexture(thisCamera.pixelWidth, thisCamera.pixelHeight, 24);
		
        // Surface the render texture as a global variable, available to all shaders.
        Shader.SetGlobalTexture("_CameraDepthTexture", depthTexture);
		// Surface the render texture as a global variable, available to all shaders.
        Shader.SetGlobalTexture("_CameraNormalsTexture", normalTexture);

        // Setup a copy of the camera to render the scene using the normals shader.
        GameObject depthCopy = new GameObject("Depth Camera");
        depthCamera = depthCopy.AddComponent<Camera>();
        depthCamera.CopyFrom(thisCamera);
        depthCamera.transform.SetParent(transform);
        depthCamera.targetTexture = depthTexture;
        depthCamera.SetReplacementShader(depthShader, "RenderType");
        depthCamera.depth = thisCamera.depth - 1;
		depthCamera.cullingMask = cullingMask.value;		
		depthCamera.clearFlags = CameraClearFlags.SolidColor;
		depthCamera.backgroundColor = Color.white;

        // Setup a copy of the camera to render the scene using the normals shader.
        GameObject normalCopy = new GameObject("Normals Camera");
        normalCamera = normalCopy.AddComponent<Camera>();
        normalCamera.CopyFrom(thisCamera);
        normalCamera.transform.SetParent(transform);
        normalCamera.targetTexture = normalTexture;
        normalCamera.SetReplacementShader(normalsShader, "RenderType");
        normalCamera.depth = thisCamera.depth - 1;
		normalCamera.cullingMask = cullingMask.value;
    }
}

