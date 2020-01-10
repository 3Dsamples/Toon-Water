/// <summary>
/// SpawnTrees.cs
/// Spidermeowth
/// Nov 30, 2016
/// 
/// This script instantiate any GameObject randomly over the surface of any mesh, While this have the Renderer component 
/// In this case can be used to populate trees, or any other element, over the surface of your custom mesh terrain
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth
/// </summary>

using UnityEngine;
using System.Collections;

public class SpawnTrees : MonoBehaviour {
	
	#region Add Prefabs variables region
	
	public GameObject[] prefabsList;        // Set number in the inspector to hold all the prefabs we want instantiated	
	
	public float minHeightToSpawn = 0.0f;   // Minimum height to instantiate a prefab
	public float maxHeightToSpawn = 100.0f; // Maximum height to instantiate a prefab

	public int numberOfObjects;             // Indicates how many prefabs are we going to instantiate
    private int currentObjects;             // Count the number of successfully placed prefabs
 
	private RaycastHit hit;                 // Store the information back from our raycast
	
    private float randomX;                  // Store a random point on the x-axis, within the surface of our mesh
    private float randomZ;                  // Store a random point on the x-axis, within the surface of our mesh
	
    private Renderer r;                     // Reference for the Renderer component
	#endregion

	#region Start region
	void Start () {
		
		//Call AddTrees function
		AddTrees();
	}
	#endregion
	
	#region Add GameObjects in random position region
	public void AddTrees(){
		
		// If tree slot is empty, just give an advise
		//if(tree == null) {
		if(prefabsList.Length == 0) {
			Debug.Log("No prefabs to spam (^v^)");
		}
		
		else{
			// Acces to our Custom Mesh Renderer component
			r = GetComponent<Renderer>();		
		
			// Add trees, as many as we need
			//for(currentObjects = 0; currentObjects < numberOfObjects; currentObjects++){
			for(int i = 0; currentObjects < numberOfObjects; i++){
				
				// Choose random prefab from the list
				GameObject prefab = prefabsList[Random.Range(0,prefabsList.Length)];
				
				//Choose a random point on the x-axis and z-axis, within the surface of our mesh
				randomX = Random.Range(r.bounds.min.x, r.bounds.max.x);
				randomZ = Random.Range(r.bounds.min.z, r.bounds.max.z);
				
				// Make a raycast in our randomX and randomZ position, To locate the y position of our prefab
				if (Physics.Raycast(new Vector3(randomX, r.bounds.max.y + 5f, randomZ), -Vector3.up, out hit)) {
					
					// Instantiate a random prefab, within the limit range of minimum and maximum heights
					if(hit.point.y > minHeightToSpawn && hit.point.y < maxHeightToSpawn && hit.collider.name == this.gameObject.name){						
						// Instantiate a prefab in that position as child of our mesh
						GameObject Clone = Instantiate(prefab, hit.point, transform.rotation)as GameObject;
						Clone.transform.parent = transform;
						
						if(hit.collider.name != this.gameObject.name){
							Debug.Log(hit.collider.name);
						}
						
						currentObjects ++;
					}
				}
			}
		}		   
	}
	#endregion
	
	// Update is called once per frame
	void Update () {
		// Uncomment the Debug.Log, to know the total of instantiated prefabs
		//Debug.Log(transform.childCount);
	}
}
