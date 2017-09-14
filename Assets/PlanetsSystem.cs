using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetsSystem : MonoBehaviour {


	private bool _isMoving = false;
	private Vector3 _moveTo;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (_isMoving) {

			transform.position = Vector3.Lerp (transform.position, _moveTo, 0.1f);

			if (Vector3.Distance (transform.position, _moveTo) < 0.1f) {
				transform.position = _moveTo;
				_isMoving = false;
			}
		}
	}

	public void UpdatePosition(Transform planet)
	{
		_moveTo = -planet.localPosition;
		_isMoving = Vector3.Distance (transform.position, _moveTo) > 0.1f;
	}
}
