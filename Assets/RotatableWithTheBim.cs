using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatableWithTheBim : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateRotation(Vector3 rotation) 
	{
		transform.Rotate (new Vector3(rotation.y * 2, -rotation.x * 2 , 0), Space.World);
	}
}
