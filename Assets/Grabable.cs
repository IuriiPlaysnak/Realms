﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabable : MonoBehaviour {

	private const float VELOCITY_MULTIPLIER = 80.0f;

    public GameObject Planet;
    public float grabRadius;
    public bool keepUpright;
    public float initialSpeed;

    OVRInput.Controller grabbedBy = OVRInput.Controller.None;
    OVRInput.RawButton grabbedByButton = OVRInput.RawButton.None;
    Vector3 localGrabOffset;
    Quaternion localGrabRotation;

	private SmoothedVector3 _velocitySmoothedVector = new SmoothedVector3 (10);


    private Rigidbody _rigidbody;
    private SpringJoint _joint;

	public bool debugVelocity = false;

    private void Awake()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _joint = gameObject.GetComponent<SpringJoint>();
		_joint.minDistance = 0.55f;
		_joint.maxDistance = 1.2f;
		_joint.enableCollision = true;

		if (keepUpright)
			_rigidbody.angularDrag = 0.2f;
    }

    // Use this for initialization
    void Start()
    {
		_rigidbody.AddForce (new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)), ForceMode.Impulse);
    }
		
    // Update is called once per frame

	private float _timer = -1f;

    void Update()
    {
		Vector3 oldPosition = transform.position;
		Quaternion oldRotation = transform.rotation;

        if (_attractor != null)
        {
            transform.position = Vector3.Lerp(transform.position, _attractor.position, 0.1f);
			_velocitySmoothedVector.AddReading(transform.position - oldPosition);
			return;
        }

        CheckGrab(OVRInput.Controller.LTouch, OVRInput.RawButton.LHandTrigger);
        CheckGrab(OVRInput.Controller.RTouch, OVRInput.RawButton.RHandTrigger);

		if (grabbedBy != OVRInput.Controller.None) {

			Vector3 handPos = OVRManager.instance.transform.LocalToWorld (OVRInput.GetLocalControllerPosition (grabbedBy));
			Quaternion handRot = OVRManager.instance.transform.LocalToWorld (OVRInput.GetLocalControllerRotation (grabbedBy));
			transform.position = handPos + handRot * localGrabOffset;
			transform.rotation = handRot * localGrabRotation;

			_velocitySmoothedVector.AddReading (transform.position - oldPosition);

		} else {
			
			if (keepUpright && _rigidbody.velocity.magnitude > 0) {

				Vector3 upward = transform.position - Planet.transform.position;
				Vector3 forward = Vector3.ProjectOnPlane (_rigidbody.velocity, upward);

				Quaternion lookTo = Quaternion.LookRotation (forward, upward);

				/*
				if (_timer >= 0) {

					_timer += Time.deltaTime;

//					if (_timer >= 2.0f) {

						transform.rotation = Quaternion.RotateTowards (transform.rotation, lookTo, _rigidbody.velocity.magnitude);

						float angle = Quaternion.Angle (transform.rotation, lookTo);
						if (angle < 1)
							_timer = -1f;
//					}
				} else {

//					transform.rotation = lookTo;
					transform.rotation = Quaternion.RotateTowards (transform.rotation, lookTo, 1f);
				}
				*/

				transform.rotation = Quaternion.RotateTowards (transform.rotation, lookTo, _rigidbody.velocity.magnitude);
				if (Quaternion.Angle (transform.rotation, lookTo) <= 1f)
					transform.rotation = lookTo;
			}
		}

        if (grabbedByButton != OVRInput.RawButton.None && OVRInput.GetUp(grabbedByButton))
        {
            // release
			_timer = 0f;
            grabbedBy = OVRInput.Controller.None;
            grabbedByButton = OVRInput.RawButton.None;

			_rigidbody.AddForce(_velocitySmoothedVector.average * VELOCITY_MULTIPLIER, ForceMode.Impulse);

			Vector3 torgue = new Vector3 (_velocitySmoothedVector.average.z, _velocitySmoothedVector.average.y, -_velocitySmoothedVector.average.x);
			_rigidbody.AddTorque (torgue, ForceMode.Impulse);

			_velocitySmoothedVector.Clear();
        }
    }

    private void FixedUpdate()
    {
		if (debugVelocity)
			Debug.Log (_rigidbody.velocity);
    }

    void CheckGrab(OVRInput.Controller hand, OVRInput.RawButton button)
    {
        if (OVRInput.GetDown(button))
        {
            Vector3 touchPos = OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerPosition(hand));
            Vector3 grabOffset = touchPos - transform.position;
            if (grabOffset.sqrMagnitude < grabRadius * grabRadius)
            {
                Quaternion invTouchRot = Quaternion.Inverse(OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerRotation(hand)));

                grabbedBy = hand;
                grabbedByButton = button;
                localGrabOffset = invTouchRot * (transform.position - touchPos);
                localGrabRotation = invTouchRot * transform.rotation;
                _velocitySmoothedVector.Clear();
            }
        }
    }

    private Transform _attractor;
    public void Attract(bool isAttracting, Transform attractor = null)
    {
		_rigidbody.isKinematic = isAttracting;

		if (isAttracting == false) 
		{
			if (Vector3.Distance (transform.position, _attractor.position) > 0.2f) 
			{
				_rigidbody.AddForce(_velocitySmoothedVector.average * VELOCITY_MULTIPLIER / 10.0f, ForceMode.Impulse);
				_velocitySmoothedVector.Clear();
			}
		}

        _attractor = attractor;
    }
}
