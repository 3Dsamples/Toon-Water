/// <summary>
/// MeshTree.cs
/// Spidermeowth
/// May 24, 2018
/// 
/// This script make any Trees based on 3d mesh, check if it is not overlapping, 
/// with some other object, after being instantiated
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth
/// </summary>

using UnityEngine;
using System.Collections;


public class MeshTree : MonoBehaviour {
	
	#region variables
	public LayerMask overlapCheck;                          // Layer mask To check for possible overlap, Ignore your layers of water and your terrain, So that they don't make all the trees disappear
	public Vector3 scale = new Vector3(1f,1f,1f);           // Use this to change the scale of your trees, you may want different sizes of trees
		
	private bool isOverlapped;                              // Overlapping bool switch
	private Bounds bounds;                                  // The bounding volume of the renderer component	
	private string myName;                                  // Instanced tree name reference	
	private Transform _myTransform;                         // Set transform variable
    private Collider myCollider;                            // Reference for the BoxCollider component
    private Collider[] colliders;                           // Reference for the BoxCollider component
	#endregion

	
	#region Awake region
	void Awake(){
		// Set _myTransfrom variable
		_myTransform = transform;
		
		// get gameObject name
		myName = transform.name;
		
		// Set Overlapping initial value
		isOverlapped = false;                      
	}
	#endregion

	#region Start region
	void Start () {

		// look for Collider components in the childs gameobjects
		colliders = GetComponentsInChildren<Collider>();
		
		if(colliders.Length > 0){
			// add all the bounding of the colliders of the children
			bounds = (colliders[0].GetComponent<Renderer>().bounds);
			
			for (int i = 1; i < colliders.Length; i++){
				bounds.Encapsulate(colliders[i].GetComponent<Collider>().bounds);
			}
		}		
		
		// Call Overlaping check function
		Overlaping();
		// Call Overlaping check function
		CheckSurface();
		// Set scale and rotation
		SetScaleAndRotation();
	}
	#endregion
	
	#region Update region
	// Update is called once per frame
	void Update () {	
		
	}
	#endregion
	
	#region Overlaping region
	public void Overlaping(){
		
		// make an overlapSphere and check for all the colliders in this boundry
		Collider[] cols = Physics.OverlapSphere(_myTransform.transform.position, bounds.extents.magnitude, overlapCheck.value);
		    // for all the colliders in this boundry
			foreach(Collider col in cols) {
				// if I hit myself 
				if (col.gameObject.name == myName) {
					//Ignore it, and check the next object bounds
					continue; 
				}
				// if my bounds intersect with the new object bounds
				if (bounds.Intersects(col.gameObject.GetComponent<Renderer>().bounds) || col.gameObject == null) {
					// Set isOverlapped as true
					isOverlapped = true;					
					break;
				}
			}
		
		// If I'm Overlapped, I destroy myself
		if(isOverlapped){
			Destroy (gameObject);
		}
	}
	#endregion
	
	#region Check angle surface
	public void CheckSurface(){
		// If our tree is not overlapping with anything, then we check the angle of the ground on which it is,
		// if the angle is too steep, we destroy our tree
		
		// store for our raycast's info
		RaycastHit hit;
		
		// We do a raycast to check the ground angle
		if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z), -Vector3.up, out hit, 4f, overlapCheck.value)) {
			// if is great than 60º, then i destroy myself
			if (Vector3.Angle(hit.normal, Vector3.up) >  60f){
				Destroy (gameObject);
			}
		}
	}
	#endregion
	
	#region Set scale and rotation region	
	public void SetScaleAndRotation(){
		_myTransform.localScale = scale;
		
		_myTransform.rotation = Quaternion.Euler(new Vector3(_myTransform.rotation.x, Random.Range(0, 359), _myTransform.rotation.z));
	}
	#endregion
}

