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

	public void UpdateRotation(Vector3 originPoint, Vector3 currentPoint)
	{
		float xR = currentPoint.y - originPoint.y;
		float yR = currentPoint.x - originPoint.x;
		float zR = 0;
		transform.Rotate (new Vector3(-xR * 2, yR *2 , zR), Space.World);

	}
}
